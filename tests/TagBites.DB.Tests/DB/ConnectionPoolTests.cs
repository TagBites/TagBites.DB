using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TagBites.DB
{
    public class ConnectionPoolTests : DbTests
    {
        [Fact]
        public void ConnectionsPoolTest()
        {
            if (DefaultProvider.UsePooling)
            {
                Assert.Equal(0, DefaultProvider.ConnectionsCount);
                Assert.Equal(0, DefaultProvider.UsingConnectionsCount);
                Assert.Equal(0, DefaultProvider.ActiveConnectionsCount);

                using (var link = DefaultProvider.CreateLink())
                {
                    Assert.Equal(1, DefaultProvider.ConnectionsCount);
                    Assert.Equal(1, DefaultProvider.UsingConnectionsCount);
                    Assert.Equal(DefaultProvider.Configuration.ForceOnLinkCreate ? 1 : 0, DefaultProvider.ActiveConnectionsCount);

                    link.Force();
                    Assert.Equal(1, DefaultProvider.ConnectionsCount);
                    Assert.Equal(1, DefaultProvider.UsingConnectionsCount);
                    Assert.Equal(1, DefaultProvider.ActiveConnectionsCount);
                }

                Assert.Equal(1, DefaultProvider.ConnectionsCount);
                Assert.Equal(0, DefaultProvider.UsingConnectionsCount);
                Assert.Equal(1, DefaultProvider.ActiveConnectionsCount);
            }
        }

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
        public async Task NoDeadlockTest()
        {
            var tasks = Enumerable.Range(1, 30).Select(x => Task.Run(async () =>
            {
                for (var i = 0; i < 50; i++)
                {
                    using (var link2 = CreateLink())
                    {
                        await Task.Delay(1);

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
            await Task.WhenAll(tasks.ToArray());
        }
    }
}
