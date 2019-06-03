using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbLinkPoolTest : DbTestBase
    {
        [TestMethod]
        public void ConnectionsPoolTest()
        {
            if (DefaultProvider.UsePooling)
            {
                Assert.AreEqual(0, DefaultProvider.ConnectionsCount, 0);
                Assert.AreEqual(0, DefaultProvider.UsingConnectionsCount, 0);
                Assert.AreEqual(0, DefaultProvider.ActiveConnectionsCount, 0);

                using (var link = DefaultProvider.CreateLink())
                {
                    Assert.AreEqual(1, DefaultProvider.ConnectionsCount);
                    Assert.AreEqual(1, DefaultProvider.UsingConnectionsCount);
                    Assert.AreEqual(DefaultProvider.Configuration.ForceOnLinkCreate ? 1 : 0, DefaultProvider.ActiveConnectionsCount);

                    link.Force();
                    Assert.AreEqual(1, DefaultProvider.ConnectionsCount);
                    Assert.AreEqual(1, DefaultProvider.UsingConnectionsCount);
                    Assert.AreEqual(1, DefaultProvider.ActiveConnectionsCount);
                }

                Assert.AreEqual(1, DefaultProvider.ConnectionsCount);
                Assert.AreEqual(0, DefaultProvider.UsingConnectionsCount);
                Assert.AreEqual(1, DefaultProvider.ActiveConnectionsCount);
            }
        }
    }
}
