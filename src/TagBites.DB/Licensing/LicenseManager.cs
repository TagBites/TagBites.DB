using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace TagBites.DB.Licensing
{
    class LicenseManager
    {
        private static bool? s_hasLicense;
        private static readonly object s_synchRoot = new object();

        public static bool HasLicense
        {
            get
            {
                if (!s_hasLicense.HasValue)
                    lock (s_synchRoot)
                        if (!s_hasLicense.HasValue)
                            s_hasLicense = TryLoadLicense();

                return s_hasLicense.Value;
            }
        }


        private static bool TryLoadLicense()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var directory = Path.GetDirectoryName(assembly.Location);
            var licenseFile = Path.Combine(directory, assembly.GetName().Name + "-License.xml");

            if (!File.Exists(licenseFile))
                return false;

            var licenseXml = File.ReadAllText(licenseFile);
            VerifyLicense(licenseXml, assembly);
            return true;
        }
        private static void VerifyLicense(string licenseXml, Assembly assembly)
        {
            // Load license
            var instance = License.FromXml(licenseXml);

            // Get license verify key blob
            var licenseVerifyKey = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(x => x.Key == "LicenseVerifyKey")?.Value;
            if (string.IsNullOrEmpty(licenseVerifyKey))
                throw new Exception("AssemblyMetadataAttribute with key LicenseVerifyKey not found.");

            // Validate signature
            if (!instance.VerifySignature(licenseVerifyKey))
                throw new Exception("Invalid license.");

            // Product name
            var productName = GetAssemblyProductName(assembly);
            if (instance.ProductName != productName)
                throw new Exception("This license is for other product.");

            // Check date
            var versionDateTime = GetAssemblyBuildDate(assembly);
            if (versionDateTime.HasValue && instance.SubscriptionUntil < versionDateTime.Value || instance.ValidUntil.HasValue && instance.ValidUntil.Value < DateTime.Today)
                throw new Exception("This license has expired.");
        }

        private static string GetAssemblyProductName(Assembly assembly) => assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        private static DateTime? GetAssemblyBuildDate(Assembly assembly)
        {
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (version == null)
                return null;

            var date = version.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => DateTime.TryParse(x, out var d) ? (DateTime?)d : null)
                .FirstOrDefault(x => x != null);
            return date;
        }
    }
}
