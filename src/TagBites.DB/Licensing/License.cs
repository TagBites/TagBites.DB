using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace TagBites.DB.Licensing
{
    class License
    {
        private KeyValuePair<string, string>[] _metadata;

        /// <summary>
        /// Gets license ID.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets type of license e.g. "User" or "Company".
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Gets product name.
        /// </summary>
        public string ProductName { get; private set; }
        /// <summary>
        /// Gets date to which the subscription is valid.
        /// </summary>
        public DateTime SubscriptionUntil { get; private set; }
        /// <summary>
        /// Gets date to which the license is valid. Null means no limits.
        /// </summary>
        public DateTime? ValidUntil { get; private set; }
        /// <summary>
        /// Gets full name of the license owner.
        /// </summary>
        public string RegisteredTo { get; private set; }

        private byte[] Hash { get; set; }
        private byte[] Signature { get; set; }

        /// <summary>
        /// Gets license metadata, properties are included. 
        /// </summary>
        /// <param name="name">Name of the metadata.</param>
        public string this[string name]
        {
            get
            {
                switch (name)
                {
                    case nameof(Id): return Id;
                    case nameof(Type): return Type;
                    case nameof(ProductName): return ProductName;
                    case nameof(SubscriptionUntil): return SubscriptionUntil.ToString("yyyy-MM-dd");
                    case nameof(RegisteredTo): return RegisteredTo;
                    default:
                        if (_metadata != null)
                        {
                            for (var i = _metadata.Length - 1; i >= 0; i--)
                                if (_metadata[i].Key == name)
                                    return _metadata[i].Value;
                        }
                        return null;
                }
            }
        }

        private License()
        { }


        /// <summary>
        /// Verifies license signature.
        /// </summary>
        /// <param name="licenseVerifyKey">License key.</param>
        public bool VerifySignature(string licenseVerifyKey)
        {
            if (Signature == null)
                throw new InvalidOperationException("License was not signed.");

            using (var dsa = new DSACryptoServiceProvider())
            {
                var blob = Convert.FromBase64String(licenseVerifyKey);
                dsa.ImportCspBlob(blob);
                return dsa.VerifySignature(Hash, Signature);
            }
        }

        /// <summary>
        /// Creates license form xml.
        /// </summary>
        /// <param name="xml">License saved as xml string.</param>
        public static License FromXml(string xml)
        {
            var license = new License();

            // Parse
            var doc = XDocument.Parse(xml);
            var data = doc.Root?.Element("Data");

            // Data
            {
                List<KeyValuePair<string, string>> metadata = null;

                if (data != null)
                {
                    foreach (var element in data.Elements())
                    {
                        switch (element.Name.LocalName)
                        {
                            case nameof(Id): license.Id = (string)element; break;
                            case nameof(Type): license.Type = (string)element; break;
                            case nameof(ProductName): license.ProductName = (string)element; break;
                            case nameof(RegisteredTo): license.RegisteredTo = (string)element; break;
                            case nameof(SubscriptionUntil):
                                {
                                    if (!DateTime.TryParse((string)element, out var subscriptionUntil))
                                        ThrowInvalidLicenseException();

                                    license.SubscriptionUntil = subscriptionUntil;
                                    break;
                                }
                            case nameof(ValidUntil):
                                {
                                    if (!DateTime.TryParse((string)element, out var validUntil))
                                        ThrowInvalidLicenseException();

                                    license.ValidUntil = validUntil;
                                    break;
                                }
                            default:
                                if (metadata == null)
                                    metadata = new List<KeyValuePair<string, string>>();

                                metadata.Add(new KeyValuePair<string, string>(element.Name.LocalName, (string)element));
                                break;
                        }
                    }
                }

                if (data == null
                    || string.IsNullOrEmpty(license.Id)
                    || string.IsNullOrEmpty(license.Type)
                    || string.IsNullOrEmpty(license.ProductName)
                    || string.IsNullOrEmpty(license.RegisteredTo)
                    || license.SubscriptionUntil == default)
                {
                    ThrowInvalidLicenseException();
                    return null;
                }

                if (metadata != null)
                    license._metadata = metadata.ToArray();
            }

            // Signature
            {
                var signature = (string)doc.Root?.Element("Signature");
                if (string.IsNullOrEmpty(signature))
                    ThrowInvalidLicenseException();
                else
                    license.Signature = Convert.FromBase64String(signature);
            }

            // Hash
            using (var sha1 = SHA1.Create())
            {
                var dataString = data.ToString(SaveOptions.DisableFormatting);
                var buffer = Encoding.UTF8.GetBytes(dataString);
                license.Hash = sha1.ComputeHash(buffer);
            }

            return license;
        }
        /// <summary>
        /// Creates license form compressed xml saved as base64.
        /// </summary>
        /// <param name="compressedBase64Xml">License saved as base64 of compressed xml.</param>
        public static License FromCompressedBase64Xml(string compressedBase64Xml)
        {
            try
            {
                using (var source = new MemoryStream(Convert.FromBase64String(compressedBase64Xml)))
                using (var destination = new MemoryStream())
                {
                    using (var gz = new GZipStream(source, CompressionMode.Decompress))
                        gz.CopyTo(destination);

                    compressedBase64Xml = Encoding.UTF8.GetString(destination.ToArray());
                }
            }
            catch
            {
                ThrowInvalidLicenseException();
            }

            return FromXml(compressedBase64Xml);
        }

        private static void ThrowInvalidLicenseException() => throw new Exception("Invalid license.");
    }
}
