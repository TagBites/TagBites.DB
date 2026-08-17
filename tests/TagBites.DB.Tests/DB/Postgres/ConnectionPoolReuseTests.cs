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
