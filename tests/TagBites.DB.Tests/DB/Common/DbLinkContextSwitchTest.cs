using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using TBS.Data.DB;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbLinkContextSwitchTest : DbTestBase
    {
        [TestMethod]
        public void ContextSwitchTest()
        {
            using (var link = DefaultProvider.CreateLink())
            {
                // Not suppressed
                using (var link2 = DefaultProvider.CreateLink())
                    Assert.AreEqual(link.ConnectionContext, link2.ConnectionContext);

                // Standard suppress
                using (var link2 = DefaultProvider.CreateLink(DbLinkCreateOption.RequiresNew))
                {
                    Assert.AreNotEqual(link.ConnectionContext, link2.ConnectionContext);

                    using (var link3 = DefaultProvider.CreateLink())
                        Assert.AreEqual(link2.ConnectionContext, link3.ConnectionContext);
                }

                // Suppress using switch and activating other context
                using (var link2 = DefaultProvider.CreateExclusiveLink())
                using (new DbLinkContextSwitch(link2, DbLinkContextSwitchMode.Activate))
                {
                    Assert.AreNotEqual(link.ConnectionContext, link2.ConnectionContext);

                    // Temporrary bring back first context
                    using (new DbLinkContextSwitch(link, DbLinkContextSwitchMode.Activate))
                    using (var link3 = DefaultProvider.CreateLink())
                        Assert.AreEqual(link.ConnectionContext, link3.ConnectionContext);

                    Assert.AreNotEqual(link.ConnectionContext, link2.ConnectionContext);
                }

                // Not suppressed
                using (var link2 = DefaultProvider.CreateLink())
                    Assert.AreEqual(link.ConnectionContext, link2.ConnectionContext);
            }
        }

        [TestMethod]
        public async Task ContextSwitchOnTasks()
        {
            using (var link = DefaultProvider.CreateLink())
            {
                await Task.Run(() =>
                {
                    using (var link2 = DefaultProvider.CreateLink())
                        Assert.AreEqual(link.ConnectionContext, link2.ConnectionContext);
                });

                using (var link2 = DefaultProvider.CreateLink())
                    Assert.AreEqual(link.ConnectionContext, link2.ConnectionContext);

                using (new DbLinkContextSwitch(link, DbLinkContextSwitchMode.Suppress))
                {
                    await Task.Run(() =>
                    {
                        using (var link2 = DefaultProvider.CreateLink())
                            Assert.AreNotEqual(link.ConnectionContext, link2.ConnectionContext);

                        using (new DbLinkContextSwitch(link, DbLinkContextSwitchMode.Activate))
                        using (var link2 = DefaultProvider.CreateLink())
                            Assert.AreEqual(link.ConnectionContext, link2.ConnectionContext);
                    });

                    using (var link2 = DefaultProvider.CreateLink())
                        Assert.AreNotEqual(link.ConnectionContext, link2.ConnectionContext);
                }

                using (var link2 = DefaultProvider.CreateLink())
                    Assert.AreEqual(link.ConnectionContext, link2.ConnectionContext);
            }
        }
    }
}
