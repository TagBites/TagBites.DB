using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TagBites.DB.Tests.DB.Core;
using Xunit;

namespace TagBites.DB.Tests.DB.Common
{
    public class DbLinkPoolingTest : DbTestBase
    {
        [Fact]
        public void PoolUsedTest()
        {
            if (DefaultProvider.UsePooling && DefaultProvider.MinPoolSize >= 1)
            {
                using (var link = DefaultProvider.CreateLink())
                    link.ConnectionContext.Bag["a"] = 2;

                using (var link = CreateLink())
                    Assert.Equal(2, link.ConnectionContext.Bag["a"]);
            }
        }

        [Fact]
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
