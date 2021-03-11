using System;
using System.Text;
using System.Transactions;
using TagBites.DB.Configuration;
using TagBites.DB.Tests.DB.Core;
using Xunit;

namespace TagBites.DB.Tests.DB
{
    public class DbLinkTest : DbTestBase
    {
        private class Model
        {
            public double Number { get; set; }
            public string Text { get; set; }
        }

        [Fact]
        public void ConnectionTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                var result = link.Execute("SELECT 1 AS a, 2 AS b UNION ALL SELECT 2 AS a, 1 AS b");

                Assert.Null(link.ExecuteScalar<int?>("SELECT null"));
                Assert.Equal(0, link.ExecuteScalar<int>("SELECT null"));
                Assert.Equal(1, link.ExecuteScalar<int>("SELECT 1"));
                Assert.Equal(1, link.ExecuteScalar<int>("SELECT {0}", 1));
            }

            using (var link = NpgsqlProvider.CreateLink())
            {
                var result = link.Execute("SELECT 1 AS a, 2 AS b UNION ALL SELECT 2 AS a, 1 AS b");
                Assert.Equal(2, result.ColumnCount);
                Assert.Equal(2, result.RowCount);
                Assert.Equal(result.GetValue<int>(0, 0), result.GetValue<int>(1, "b"));
            }

            using (var link = NpgsqlProvider.CreateExclusiveLink())
            {
                var result = link.Execute<Model>("SELECT 1.5 AS Number, 'This is text.' AS Text");
                Assert.Single(result);
                Assert.Equal(1.5, result[0].Number);
                Assert.Equal("This is text.", result[0].Text);
            }
        }

        [Fact]
        public void ExecuteManyTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                var result = link.BatchExecute("SELECT 1; SELECT 2, 3");
                Assert.Equal(2, result.Length);
                Assert.Equal(1, result[0].GetValue<int>(0, 0));
                Assert.Equal(2, result[1].GetValue<int>(0, 0));
                Assert.Equal(3, result[1].GetValue<int>(0, 1));
            }
        }

        [Fact]
        public void NestedTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                Assert.Equal(1, link.ExecuteScalar<int>("SELECT 1"));

                using (var link2 = NpgsqlProvider.CreateLink())
                {
                    Assert.NotEqual(link, link2);
                    Assert.Equal(link.ConnectionContext, link2.ConnectionContext);
                    Assert.Equal(1, link2.ExecuteScalar<int>("SELECT 1"));
                }

                Assert.Equal(1, link.ExecuteScalar<int>("SELECT 1"));
            }

            using (var link = NpgsqlProvider.CreateLink())
            {
                using (var link2 = NpgsqlProvider.CreateExclusiveLink())
                {
                    Assert.NotEqual(link.ConnectionContext, link2.ConnectionContext);
                }
            }
        }

        [Fact]
        public void TransactionTest()
        {
            NpgsqlProvider.Configuration.ImplicitCreateTransactionScopeIfNotExists = false;
            NpgsqlProvider.Configuration.ForceOnTransactionBegin = false;

            using (var link = NpgsqlProvider.CreateLink())
            {
                Assert.Equal(DbLinkTransactionStatus.None, link.TransactionStatus);
                using (var transaction = link.Begin())
                {
                    Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);

                    using (var link2 = NpgsqlProvider.CreateLink())
                    {
                        Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);

                        using (var transaction2 = link2.Begin())
                        {
                            Assert.NotEqual(transaction, transaction2);
                            Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                            transaction2.Commit();
                            Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                        }

                        Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    }

                    Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    transaction.Commit();
                    Assert.Equal(DbLinkTransactionStatus.Committing, link.TransactionStatus);
                }

                Assert.Equal(DbLinkTransactionStatus.None, link.TransactionStatus);
                using (var transaction = link.Begin())
                {
                    link.ExecuteNonQuery("SELECT 1");
                    Assert.Equal(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                Assert.Equal(DbLinkTransactionStatus.None, link.TransactionStatus);
                using (var transaction = link.Begin())
                {
                    try
                    {
                        link.ExecuteNonQuery("SELECT a");
                        Assert.True(false);
                    }
                    catch { }
                    Assert.Equal(DbLinkTransactionStatus.RollingBack, link.TransactionStatus);
                }

                Assert.Equal(DbLinkTransactionStatus.None, link.TransactionStatus);
            }
        }

        [Fact]
        public void RollbackTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.ExecuteNonQuery("SELECT 1");

                try
                {
                    using (var link2 = NpgsqlProvider.CreateLink())
                    using (var transaction2 = link2.Begin())
                    {
                        link.ExecuteNonQuery("SELECT 2");
                    }
                }
                catch (OperationCanceledException)
                { }

                try
                {
                    link.ExecuteNonQuery("SELECT 3");
                    Assert.True(false, "Can not execute command while transaction is in process of rollback!");
                }
                catch { }

                try
                {
                    transaction.Commit();
                    Assert.True(false, "Can not commit already rollback transaction!");
                }
                catch (InvalidOperationException)
                { }

                Assert.Equal(DbLinkTransactionStatus.RollingBack, link.TransactionStatus);
            }
        }

        [Fact]
        public void RollbackTest2()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                using (var transaction = link.Begin())
                {
                    transaction.Rollback();
                    Assert.Equal(DbLinkTransactionStatus.RollingBack, link.TransactionStatus);
                }

                using (var transaction = link.Begin())
                {
                    link.ExecuteNonQuery("SELECT 1");
                }
            }
        }

        [Fact]
        public void RollbackTest3()
        {
            NpgsqlProvider.Configuration.ImplicitCreateTransactionScopeIfNotExists = true;
            NpgsqlProvider.Configuration.LinkCreateOnDifferentSystemTransaction = DbLinkCreateOnDifferentSystemTransaction.CreateLinkWithNewContextOrAssigedToCurrentTransaction;

            try
            {
                using (var link = NpgsqlProvider.CreateLink())
                using (var t = link.Begin())
                {
                    Transaction.Current.TransactionCompleted +=
                        (sender, args) => Assert.Equal(TransactionStatus.Aborted, args.Transaction.TransactionInformation.Status);

                    for (var i = 0; i < 3; i++)
                    {
                        try
                        {
                            using (var linki = NpgsqlProvider.CreateLink())
                            using (var ti = linki.Begin())
                            {
                                if (i == 1)
                                    linki.ExecuteNonQuery("a");

                                if (i >= 1)
                                    Assert.True(false);

                                link.BatchExecute("SELECT 1");
                                ti.Commit();
                            }
                        }
                        catch
                        {
                            Assert.True(i >= 1);
                        }
                    }

                    t.Commit();
                    Assert.True(false);
                }
            }
            catch { /* Ignored */ }
        }

        [Fact]
        public void RollbackOnEmptyActionTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                using (var transaction = link.Begin())
                {
                }
            }
        }

        [Fact]
        public void EventFireOrderTest()
        {
            bool firstOpen = false;
            bool firstClose = false;
            bool firstBeforeBeginTransaction = false;
            bool firstBeginTransaction = false;
            bool firstCloseTransaction = false;

            using (var link = NpgsqlProvider.CreateExclusiveLink())
            {
                link.ConnectionContext.ConnectionOpened += (s, e) => { Assert.False(firstOpen); firstOpen = true; };
                link.ConnectionContext.ConnectionOpened += (s, e) => { Assert.True(firstOpen); };

                link.ConnectionContext.ConnectionClosed += (s, e) => { Assert.True(firstClose); };
                link.ConnectionContext.ConnectionClosed += (s, e) => { Assert.False(firstClose); firstClose = true; };

                link.ConnectionContext.TransactionBeginning += (s, e) => { Assert.False(firstBeforeBeginTransaction); firstBeforeBeginTransaction = true; };
                link.ConnectionContext.TransactionBeginning += (s, e) => { Assert.True(firstBeforeBeginTransaction); };

                link.ConnectionContext.TransactionBegan += (s, e) => { Assert.False(firstBeginTransaction); firstBeginTransaction = true; };
                link.ConnectionContext.TransactionBegan += (s, e) => { Assert.True(firstBeginTransaction); };

                link.ConnectionContext.TransactionClosed += (s, e) => { Assert.True(firstCloseTransaction); };
                link.ConnectionContext.TransactionClosed += (s, e) => { Assert.False(firstCloseTransaction); firstCloseTransaction = true; };
            }
        }

        [Fact]
        public void EventTest()
        {
            var contextCreateCounter = 0;
            var openCounter = 0;
            var closeCounter = 0;
            var beginTransactionCounter = 0;
            var closeTransactionCounter = 0;
            var beginTransactionContextCounter = 0;
            var closeTransactionContextCounter = 0;

            var cp = DbManager.CreateNpgsqlProvider(false);
            cp.ContextCreated += (sender, args) => ++contextCreateCounter;

            using (var link = cp.CreateLink())
            {
                link.ConnectionContext.ConnectionOpened += (s, e) => { ++openCounter; };
                link.ConnectionContext.ConnectionClosed += (s, e) => { ++closeCounter; };
                link.ConnectionContext.TransactionContextBegan += (s, e) => { ++beginTransactionContextCounter; };
                link.ConnectionContext.TransactionContextClosed += (s, e) => { ++closeTransactionContextCounter; };
                link.Force();

                using (var link2 = cp.CreateLink())
                {
                    link2.ConnectionContext.ConnectionOpened += (s, e) => { Assert.True(false); };
                    link2.ConnectionContext.TransactionBegan += (s, e) => { ++beginTransactionCounter; };
                    link2.ConnectionContext.TransactionClosed += (s, e) =>
                    {
                        ++closeTransactionCounter;
                        if (e.CloseReason != DbLinkTransactionCloseReason.Rollback)
                            Assert.True(false);
                    };
                    link2.Force();

                    using (var transaction = link2.Begin())
                    { }
                    using (var transaction = link2.Begin())
                    {
                        link2.Force();
                        transaction.Rollback();
                    }
                }
            }

            Assert.True(1 == contextCreateCounter, "context create count");
            Assert.True((cp.Configuration.ForceOnLinkCreate ? 0 : 1) == openCounter, "open count");
            Assert.True(1 == closeCounter, "close count");
            Assert.True((cp.Configuration.ForceOnTransactionBegin ? 2 : 1) == beginTransactionCounter, "t. begin count");
            Assert.True((cp.Configuration.ForceOnTransactionBegin ? 2 : 1) == closeTransactionCounter, "t. close count");
            Assert.True(2 == beginTransactionContextCounter, "begin context count");
            Assert.True(2 == closeTransactionContextCounter, "close context count");
        }

        [Fact]
        public void ExecuteOnConnectionOpenTest()
        {
            for (int i = 0; i < 2; i++)
            {
                var order = new StringBuilder();

                NpgsqlProvider.Configuration.PostponeTransactionBeginOnConnectionOpenEvent = i == 0;
                NpgsqlProvider.Configuration.ForceOnLinkCreate = false;
                NpgsqlProvider.Configuration.ForceOnTransactionBegin = false;

                using (var link = NpgsqlProvider.CreateExclusiveLink())
                using (var transaction = link.Begin())
                {
                    link.ConnectionContext.ConnectionOpened += (s, e) =>
                    {
                        Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                        order.Append(link.ConnectionContext.ExecuteScalar<int>(new Query("Select 1")));
                        Assert.Equal(NpgsqlProvider.Configuration.PostponeTransactionBeginOnConnectionOpenEvent ? DbLinkTransactionStatus.Pending : DbLinkTransactionStatus.Open, link.TransactionStatus);
                    };

                    Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    order.Append(link.ExecuteScalar<int>("Select 2"));
                    Assert.Equal("12", order.ToString());

                    transaction.Commit();
                }
            }
        }
    }
}
