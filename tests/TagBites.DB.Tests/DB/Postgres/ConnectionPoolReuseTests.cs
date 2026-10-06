using System.Diagnostics;

namespace TagBites.DB.Postgres
{
    public class ConnectionPoolReuseTests
    {
        private const int MinPoolSize = 1;
        private const int MaxPoolSize = 4;


        [PostgresFact]
        public void PoolRetainsEveryReleasedContextTest()
        {
            var provider = DbManager.CreateNpgsqlProvider(true, MinPoolSize, MaxPoolSize);

            UseLinksSimultaneously(provider, MaxPoolSize);

            Assert.Equal(MaxPoolSize, provider.PoolConnectionsCount);
        }

        [PostgresFact]
        public void PoolReusesReleasedContextsTest()
        {
            var provider = DbManager.CreateNpgsqlProvider(true, MinPoolSize, MaxPoolSize);
            var createdContexts = 0;
            provider.ContextCreated += (_, _) => createdContexts++;

            UseLinksSimultaneously(provider, MaxPoolSize);
            Assert.Equal(MaxPoolSize, createdContexts);

            UseLinksSimultaneously(provider, MaxPoolSize);
            Assert.Equal(MaxPoolSize, createdContexts);
        }

        [PostgresFact]
        public async Task PoolClosesIdleContextsDownToMinPoolSizeTestAsync()
        {
            const int burstSize = 6;

            var provider = DbManager.CreateNpgsqlProvider(true, MinPoolSize, burstSize, 1, 1);
            var closeTimes = new List<TimeSpan>();
            var stopwatch = Stopwatch.StartNew();
            provider.ContextCreated += (_, e) => e.LinkContext.ConnectionClosed += (_, _) =>
            {
                lock (closeTimes)
                    closeTimes.Add(stopwatch.Elapsed);
            };

            UseLinksSimultaneously(provider, burstSize);
            Assert.Equal(burstSize, provider.PoolConnectionsCount);

            while (provider.PoolConnectionsCount > MinPoolSize && stopwatch.Elapsed < TimeSpan.FromSeconds(20))
                await Task.Delay(100);

            await Task.Delay(1500);

            Assert.Equal(MinPoolSize, provider.PoolConnectionsCount);
            Assert.Equal(burstSize - MinPoolSize, closeTimes.Count);
            Assert.True(closeTimes.Max() - closeTimes.Min() < TimeSpan.FromSeconds(2), $"Closes spread over {closeTimes.Max() - closeTimes.Min()}.");
        }

        [PostgresFact]
        public void ExclusiveContextReturnsToPoolTest()
        {
            var provider = DbManager.CreateNpgsqlProvider(true, MinPoolSize, MaxPoolSize);
            DbLinkContext exclusiveContext;

            using (var link = provider.CreateExclusiveLink(x => x["ApplicationName"] = "exclusive"))
            {
                link.ExecuteScalar<int>("SELECT 1");
                exclusiveContext = (DbLinkContext)link.ConnectionContext;
            }

            Assert.Equal(1, provider.PoolConnectionsCount);
            Assert.False(exclusiveContext.IsDisposed);
        }

        [PostgresFact]
        public async Task NotifyContextDoesNotReturnToPoolTestAsync()
        {
            var provider = DbManager.CreateNpgsqlProvider(true, MinPoolSize, MaxPoolSize);
            DbLinkContext notifyContext = null;
            provider.ContextCreated += (_, e) =>
            {
                if (e.LinkContext.Bag[PgSqlBagKeys.IsNotifyContext] is true)
                    notifyContext = (DbLinkContext)e.LinkContext;
            };

            using (var listener = new PgSqlNotifyListener(provider))
                Assert.True(await listener.ListenAsync("pool_test"));

            Assert.NotNull(notifyContext);
            Assert.True(notifyContext.IsDisposed);
            Assert.Equal(0, provider.PoolConnectionsCount);

            using var shared = provider.CreateLink();
            Assert.NotSame(notifyContext, shared.ConnectionContext);
        }

        [PostgresFact]
        public async Task PoolKeepsContextMarkedToStayOpenWhenIdleTestAsync()
        {
            const int burstSize = 4;

            var provider = DbManager.CreateNpgsqlProvider(true, MinPoolSize, burstSize, 1, 1);
            var links = new List<IDbLink>();

            for (var i = 0; i < burstSize; i++)
            {
                var link = provider.CreateLink(DbLinkCreateOption.RequiresNew);
                link.ExecuteScalar<int>("SELECT 1");
                links.Add(link);
            }

            var keptContext = (DbLinkContext)links[^1].ConnectionContext;
            keptContext.Bag[DbLinkBagKeys.KeepOpenWhenIdle] = true;

            for (var i = links.Count - 1; i >= 0; i--)
                links[i].Dispose();

            var stopwatch = Stopwatch.StartNew();
            while (provider.PoolConnectionsCount > MinPoolSize && stopwatch.Elapsed < TimeSpan.FromSeconds(20))
                await Task.Delay(100);

            Assert.Equal(MinPoolSize, provider.PoolConnectionsCount);
            Assert.False(keptContext.IsDisposed);

            using var reused = provider.CreateLink();
            Assert.Same(keptContext, reused.ConnectionContext);
        }

        [PostgresFact]
        public async Task PoolClosesIdleContextsOutsideReleasingExecutionContextTestAsync()
        {
            const int burstSize = 3;

            var provider = DbManager.CreateNpgsqlProvider(true, MinPoolSize, burstSize, 1, 1);
            var releasingOperation = new AsyncLocal<string>();
            var operationsSeenOnClose = new List<string>();
            provider.ContextCreated += (_, e) => e.LinkContext.ConnectionClosed += (_, _) =>
            {
                lock (operationsSeenOnClose)
                    operationsSeenOnClose.Add(releasingOperation.Value);
            };

            releasingOperation.Value = "invoice-posting";
            UseLinksSimultaneously(provider, burstSize);

            var stopwatch = Stopwatch.StartNew();
            while (operationsSeenOnClose.Count < burstSize - MinPoolSize && stopwatch.Elapsed < TimeSpan.FromSeconds(20))
                await Task.Delay(100);

            Assert.Equal(burstSize - MinPoolSize, operationsSeenOnClose.Count);
            Assert.All(operationsSeenOnClose, Assert.Null);
        }

        [PostgresFact]
        public async Task PoolClosesIdleContextsReleasedWithSuppressedFlowTestAsync()
        {
            const int burstSize = 3;

            var provider = DbManager.CreateNpgsqlProvider(true, MinPoolSize, burstSize, 1, 1);

            using (ExecutionContext.SuppressFlow())
                UseLinksSimultaneously(provider, burstSize);

            var stopwatch = Stopwatch.StartNew();
            while (provider.PoolConnectionsCount > MinPoolSize && stopwatch.Elapsed < TimeSpan.FromSeconds(20))
                await Task.Delay(100);

            Assert.Equal(MinPoolSize, provider.PoolConnectionsCount);
            Assert.Equal(0, provider.UsingConnectionsCount);
        }

        private static void UseLinksSimultaneously(DbLinkProvider provider, int count)
        {
            var links = new List<IDbLink>();

            try
            {
                for (var i = 0; i < count; i++)
                {
                    var link = provider.CreateLink(DbLinkCreateOption.RequiresNew);
                    links.Add(link);
                    link.ExecuteScalar<int>("SELECT 1");
                }

                Assert.Equal(count, provider.UsingConnectionsCount);
            }
            finally
            {
                for (var i = links.Count - 1; i >= 0; i--)
                    links[i].Dispose();
            }
        }
    }
}
