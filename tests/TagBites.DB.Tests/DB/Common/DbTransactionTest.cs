using TagBites.DB.Tests.DB.Core;
using Xunit;

// ReSharper disable AccessToModifiedClosure

namespace TagBites.DB.Tests.DB.Common
{
    public class DbTransactionTest : DbTestBase
    {
        [Fact]
        public void BeforeCommitTest()
        {
            var beforeBegin = 0;
            var begin = 0;
            var beforeCommit = 0;
            var close = 0;

            using (DefaultProvider.CreateLink())
            {
                using (var link = DefaultProvider.CreateLink())
                using (var transaction = link.Begin())
                {
                    link.TransactionContext.TransactionBeforeBegin += (s, e) => beforeBegin++;
                    link.TransactionContext.TransactionBegin += (s, e) => begin++;
                    link.TransactionContext.TransactionBeforeCommit += (s, e) => beforeCommit++; ;
                    link.TransactionContext.TransactionClose += (s, e) => close++;

                    transaction.Commit();
                }


                Assert.Equal(0, beforeBegin);
                Assert.Equal(0, begin);
                Assert.Equal(0, beforeCommit);
                Assert.Equal(0, close);

                using (var link = DefaultProvider.CreateLink())
                using (var transaction = link.Begin())
                {
                    link.TransactionContext.TransactionBeforeBegin += (s, e) => beforeBegin++;
                    link.TransactionContext.TransactionBeforeBegin += (s, e) => beforeBegin++;
                    link.TransactionContext.TransactionBegin += (s, e) => begin++;
                    link.TransactionContext.TransactionBegin += (s, e) => begin++;
                    link.TransactionContext.TransactionBeforeCommit += (s, e) => beforeCommit++;
                    link.TransactionContext.TransactionBeforeCommit += (s, e) => beforeCommit++;
                    link.TransactionContext.TransactionClose += (s, e) => close++;
                    link.TransactionContext.TransactionClose += (s, e) => close++;

                    link.Force();
                    transaction.Commit();
                }

                Assert.Equal(2, beforeBegin);
                Assert.Equal(2, begin);
                Assert.Equal(2, beforeCommit);
                Assert.Equal(2, close);

                beforeBegin = 0;
                begin = 0;
                beforeCommit = 0;
                close = 0;

                using (var link = DefaultProvider.CreateLink())
                using (var transaction = link.Begin())
                {
                    link.Force();
                    transaction.Commit();
                }

                Assert.Equal(0, beforeBegin);
                Assert.Equal(0, begin);
                Assert.Equal(0, beforeCommit);
                Assert.Equal(0, close);
            }
        }
    }
}
