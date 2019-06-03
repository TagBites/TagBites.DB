using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TBS.Data.DB;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbLinkPoolingTest : DbTestBase
    {
        [TestMethod]
        public void PoolUsedTest()
        {
            if (DefaultProvider.UsePooling && DefaultProvider.MinPoolSize >= 1)
            {
                using (var link = DefaultProvider.CreateLink())
                    link.ConnectionContext.Bag["a"] = 2;

                using (var link = CreateLink())
                    Assert.AreEqual(2, link.ConnectionContext.Bag["a"]);
            }
        }

        [TestMethod]
        public void NoDeadlockTest()
        {
            var tasks = Enumerable.Range(1, 30).Select(x => Task.Run(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    using (var link2 = CreateLink())
                    {
                        Thread.Sleep(1);

                        using (var link = CreateLink())
                        using (var transaction = link.Begin())
                        {
                            Thread.Sleep(1);
                            link.ExecuteNonQuery("Select 1");
                            transaction.Rollback();
                        }
                    }
                }
            }));
            Task.WaitAll(tasks.ToArray());
        }
    }
}
