using TagBites.DB.Tests.DB.Core;
using Xunit;

// ReSharper disable AccessToModifiedClosure

namespace TagBites.DB.Tests.DB.Common
{
    public class DbTransactionTest : DbTestBase
    {
        [Fact]
        public void CommitingEventTest()
        {
            using (var link = DefaultProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.Execute("SELECT 1");

                transaction.Context.TransactionCommiting += (s, e) =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    link.Execute("SELECT 1");
                };

                transaction.Commit();
            }
        }

        [Fact]
        public void CommittedEventTest()
        {
            using (var link = DefaultProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.Execute("SELECT 1");

                transaction.Context.TransactionClosed += (s, e) =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    using (var t2 = link.Begin())
                    {
                        link.Execute("SELECT 1");
                        t2.Commit();
                    }
                };

                transaction.Commit();
            }
        }

        [Fact]
        public void TransactionContextClosedEventTest()
        {
            using (var link = DefaultProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.Execute("SELECT 1");

                transaction.ConnectionContext.TransactionContextClosed += (s, e) =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    link.Execute("SELECT 1");
                };

                transaction.Commit();
            }
        }

        [Fact]
        public void RecursiveTransactionContextClosedEventTest()
        {
            var count1 = 0;
            var count2 = 0;
            var innerCount = 0;

            using (var link = DefaultProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.Force();
                transaction.Context.TransactionContextClosed += OnDbLinkTransactionContextCloseEventHandler;
                transaction.Context.TransactionContextClosed += OnConnectionContextOnTransactionContextClose;
                transaction.Commit();

                void OnDbLinkTransactionContextCloseEventHandler(object s, DbLinkTransactionContextCloseEventArgs e)
                {
                    ++count1;
                }
                void OnConnectionContextOnTransactionContextClose(object s, DbLinkTransactionContextCloseEventArgs e)
                {
                    ++count2;

                    using (var link2 = DefaultProvider.CreateLink())
                    using (var transaction2 = link2.Begin())
                    {
                        link2.Force();

                        transaction2.Context.TransactionContextClosed += (_, _) =>
                        {
                            ++innerCount;
                        };

                        transaction2.Commit();
                    }
                }
            };

            Assert.Equal(1, count1);
            Assert.Equal(1, count2);
            Assert.Equal(1, innerCount);
        }

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
                    link.TransactionContext.TransactionBeginning += (s, e) => beforeBegin++;
                    link.TransactionContext.TransactionBegan += (s, e) => begin++;
                    link.TransactionContext.TransactionCommiting += (s, e) => beforeCommit++; ;
                    link.TransactionContext.TransactionClosed += (s, e) => close++;

                    transaction.Commit();
                }


                Assert.Equal(0, beforeBegin);
                Assert.Equal(0, begin);
                Assert.Equal(0, beforeCommit);
                Assert.Equal(0, close);

                using (var link = DefaultProvider.CreateLink())
                using (var transaction = link.Begin())
                {
                    link.TransactionContext.TransactionBeginning += (s, e) => beforeBegin++;
                    link.TransactionContext.TransactionBeginning += (s, e) => beforeBegin++;
                    link.TransactionContext.TransactionBegan += (s, e) => begin++;
                    link.TransactionContext.TransactionBegan += (s, e) => begin++;
                    link.TransactionContext.TransactionCommiting += (s, e) => beforeCommit++;
                    link.TransactionContext.TransactionCommiting += (s, e) => beforeCommit++;
                    link.TransactionContext.TransactionClosed += (s, e) => close++;
                    link.TransactionContext.TransactionClosed += (s, e) => close++;

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
