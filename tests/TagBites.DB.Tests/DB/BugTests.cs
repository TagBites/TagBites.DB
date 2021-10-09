using System;
using Xunit;

namespace TagBites.DB
{
    public class BugTests : DbTests
    {
        [Fact]
        public void TwoStatementsInOneQueryWithBugInSecondOne()
        {
            using (var link = DefaultProvider.CreateLink())
            using (var t = link.Begin())
            {
                Assert.ThrowsAny<Exception>(() => link.Execute("Select 1; select a"));
                Assert.Equal(DbLinkTransactionStatus.RollingBack, t.Context.Status);
                Assert.NotNull(t.Context.Exception);
            }
        }
    }
}
