using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.DB.Tests.DB.Core;
using Xunit;

namespace TagBites.DB.Tests.DB.Common
{
    public class DbLinkPoolTest : DbTestBase
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
    }
}
