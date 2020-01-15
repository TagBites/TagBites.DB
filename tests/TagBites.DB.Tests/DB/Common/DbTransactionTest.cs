using TagBites.DB.Tests.DB.Core;
using Xunit;

namespace TagBites.DB.Tests.DB.Common
{
    public class DbTransactionTest : DbTestBase
    {
        [Fact]
        public void BeforeCommitTest()
        {
            var ok = false;

            using (var link = DefaultProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.TransactionContext.TransactionBeforeCommit += (s, e) => ok = true;
                transaction.Commit();
            }

            Assert.False(ok);

            using (var link = DefaultProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.Force();
                link.TransactionContext.TransactionBeforeCommit += (s, e) => ok = true;
                transaction.Commit();
            }

            Assert.True(ok);
        }
    }
}
