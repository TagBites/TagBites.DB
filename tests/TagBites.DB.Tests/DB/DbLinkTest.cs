using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Transactions;
using TBS.Data.DB;
using TBS.Data.DB.Configuration;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbLinkTest : DbTestBase
    {
        private class Model
        {
            public double Number { get; set; }
            public string Text { get; set; }
        }

        [TestMethod]
        public void ConnectionTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                var result = link.Execute("SELECT 1 AS a, 2 AS b UNION ALL SELECT 2 AS a, 1 AS b");

                Assert.AreEqual(null, link.ExecuteScalar<int?>("SELECT null"));
                Assert.AreEqual(0, link.ExecuteScalar<int>("SELECT null"));
                Assert.AreEqual(1, link.ExecuteScalar<int>("SELECT 1"));
                Assert.AreEqual(1, link.ExecuteScalar<int>("SELECT {0}", 1));
            }

            using (var link = NpgsqlProvider.CreateLink())
            {
                var result = link.Execute("SELECT 1 AS a, 2 AS b UNION ALL SELECT 2 AS a, 1 AS b");
                Assert.AreEqual(result.ColumnCount, 2);
                Assert.AreEqual(result.RowCount, 2);
                Assert.AreEqual(result.GetValue<int>(0, 0), result.GetValue<int>(1, "b"));
            }

            using (var link = NpgsqlProvider.CreateExclusiveLink())
            {
                var result = link.Execute<Model>("SELECT 1.5 AS Number, 'This is text.' AS Text");
                Assert.AreEqual(result.Count, 1);
                Assert.AreEqual(result[0].Number, 1.5);
                Assert.AreEqual(result[0].Text, "This is text.");
            }
        }

        [TestMethod]
        public void ExecuteManyTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                var result = link.BatchExecute("SELECT 1; SELECT 2, 3");
                Assert.AreEqual(2, result.Length);
                Assert.AreEqual(1, result[0].GetValue<int>(0, 0));
                Assert.AreEqual(2, result[1].GetValue<int>(0, 0));
                Assert.AreEqual(3, result[1].GetValue<int>(0, 1));
            }
        }

        [TestMethod]
        public void NestedTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                Assert.AreEqual(1, link.ExecuteScalar<int>("SELECT 1"));

                using (var link2 = NpgsqlProvider.CreateLink())
                {
                    Assert.AreNotEqual(link, link2);
                    Assert.AreEqual(link.ConnectionContext, link2.ConnectionContext);
                    Assert.AreEqual(1, link2.ExecuteScalar<int>("SELECT 1"));
                }

                Assert.AreEqual(1, link.ExecuteScalar<int>("SELECT 1"));
            }

            using (var link = NpgsqlProvider.CreateLink())
            {
                using (var link2 = NpgsqlProvider.CreateExclusiveLink())
                {
                    Assert.AreNotEqual(link.ConnectionContext, link2.ConnectionContext);
                }
            }
        }

        [TestMethod]
        public void TransactionTest()
        {
            NpgsqlProvider.Configuration.ImplicitCreateTransactionScopeIfNotExists = false;
            NpgsqlProvider.Configuration.ForceOnTransactionBegin = false;

            using (var link = NpgsqlProvider.CreateLink())
            {
                Assert.AreEqual(link.TransactionStatus, DbLinkTransactionStatus.None);
                using (var transaction = link.Begin())
                {
                    Assert.AreEqual(DbLinkTransactionStatus.Pending, link.TransactionStatus);

                    using (var link2 = NpgsqlProvider.CreateLink())
                    {
                        Assert.AreEqual(DbLinkTransactionStatus.Pending, link.TransactionStatus);

                        using (var transaction2 = link2.Begin())
                        {
                            Assert.AreNotEqual(transaction, transaction2);
                            Assert.AreEqual(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                            transaction2.Commit();
                            Assert.AreEqual(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                        }

                        Assert.AreEqual(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    }

                    Assert.AreEqual(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    transaction.Commit();
                    Assert.AreEqual(DbLinkTransactionStatus.Committing, link.TransactionStatus);
                }

                Assert.AreEqual(link.TransactionStatus, DbLinkTransactionStatus.None);
                using (var transaction = link.Begin())
                {
                    link.ExecuteNonQuery("SELECT 1");
                    Assert.AreEqual(link.TransactionStatus, DbLinkTransactionStatus.Open);
                }

                Assert.AreEqual(link.TransactionStatus, DbLinkTransactionStatus.None);
                using (var transaction = link.Begin())
                {
                    try
                    {
                        link.ExecuteNonQuery("SELECT a");
                        Assert.Fail();
                    }
                    catch { }
                    Assert.AreEqual(link.TransactionStatus, DbLinkTransactionStatus.RollingBack);
                }

                Assert.AreEqual(link.TransactionStatus, DbLinkTransactionStatus.None);
            }
        }

        [TestMethod]
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
                    Assert.Fail("Can not execute command while transaction is in process of rollback!");
                }
                catch { }

                try
                {
                    transaction.Commit();
                    Assert.Fail("Can not commit already rollback transaction!");
                }
                catch (InvalidOperationException)
                { }

                Assert.AreEqual(link.TransactionStatus, DbLinkTransactionStatus.RollingBack);
            }
        }

        [TestMethod]
        public void RollbackTest2()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                using (var transaction = link.Begin())
                {
                    transaction.Rollback();
                    Assert.AreEqual(DbLinkTransactionStatus.RollingBack, link.TransactionStatus);
                }

                using (var transaction = link.Begin())
                {
                    link.ExecuteNonQuery("SELECT 1");
                }
            }
        }

        [TestMethod]
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
                        (sender, args) => Assert.AreEqual(TransactionStatus.Aborted, args.Transaction.TransactionInformation.Status);

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
                                    Assert.Fail();

                                link.BatchExecute("SELECT 1");
                                ti.Commit();
                            }
                        }
                        catch
                        {
                            Assert.IsTrue(i >= 1);
                        }
                    }

                    t.Commit();
                    Assert.Fail();
                }
            }
            catch { /* Ignored */ }
        }

        [TestMethod]
        public void RollbackOnEmptyActionTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                using (var transaction = link.Begin())
                {
                }
            }
        }

        [TestMethod]
        public void EventFireOrderTest()
        {
            bool firstOpen = false;
            bool firstClose = false;
            bool firstBeforeBeginTransaction = false;
            bool firstBeginTransaction = false;
            bool firstCloseTransaction = false;

            using (var link = NpgsqlProvider.CreateExclusiveLink())
            {
                link.ConnectionContext.ConnectionOpen += (s, e) => { Assert.IsFalse(firstOpen); firstOpen = true; };
                link.ConnectionContext.ConnectionOpen += (s, e) => { Assert.IsTrue(firstOpen); };

                link.ConnectionContext.ConnectionClose += (s, e) => { Assert.IsTrue(firstClose); };
                link.ConnectionContext.ConnectionClose += (s, e) => { Assert.IsFalse(firstClose); firstClose = true; };

                link.ConnectionContext.TransactionBeforeBegin += (s, e) => { Assert.IsFalse(firstBeforeBeginTransaction); firstBeforeBeginTransaction = true; };
                link.ConnectionContext.TransactionBeforeBegin += (s, e) => { Assert.IsTrue(firstBeforeBeginTransaction); };

                link.ConnectionContext.TransactionBegin += (s, e) => { Assert.IsFalse(firstBeginTransaction); firstBeginTransaction = true; };
                link.ConnectionContext.TransactionBegin += (s, e) => { Assert.IsTrue(firstBeginTransaction); };

                link.ConnectionContext.TransactionClose += (s, e) => { Assert.IsTrue(firstCloseTransaction); };
                link.ConnectionContext.TransactionClose += (s, e) => { Assert.IsFalse(firstCloseTransaction); firstCloseTransaction = true; };
            }
        }

        [TestMethod]
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
                link.ConnectionContext.ConnectionOpen += (s, e) => { ++openCounter; };
                link.ConnectionContext.ConnectionClose += (s, e) => { ++closeCounter; };
                link.ConnectionContext.TransactionContextBegin += (s, e) => { ++beginTransactionContextCounter; };
                link.ConnectionContext.TransactionContextClose += (s, e) => { ++closeTransactionContextCounter; };
                link.Force();

                using (var link2 = cp.CreateLink())
                {
                    link2.ConnectionContext.ConnectionOpen += (s, e) => { Assert.Fail(); };
                    link2.ConnectionContext.TransactionBegin += (s, e) => { ++beginTransactionCounter; };
                    link2.ConnectionContext.TransactionClose += (s, e) =>
                    {
                        ++closeTransactionCounter;
                        if (e.CloseReason != DbLinkTransactionCloseReason.Rollback)
                            Assert.Fail();
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

            Assert.AreEqual(1, contextCreateCounter, "context create count");
            Assert.AreEqual(cp.Configuration.ForceOnLinkCreate ? 0 : 1, openCounter, "open count");
            Assert.AreEqual(1, closeCounter, "close count");
            Assert.AreEqual(cp.Configuration.ForceOnTransactionBegin ? 2 : 1, beginTransactionCounter, "t. begin count");
            Assert.AreEqual(cp.Configuration.ForceOnTransactionBegin ? 2 : 1, closeTransactionCounter, "t. close count");
            Assert.AreEqual(2, beginTransactionContextCounter, "begin context count");
            Assert.AreEqual(2, closeTransactionContextCounter, "close context count");
        }

        [TestMethod]
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
                    link.ConnectionContext.ConnectionOpen += (s, e) =>
                    {
                        Assert.AreEqual(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                        order.Append(link.ConnectionContext.ExecuteScalar<int>(new Query("Select 1")));
                        Assert.AreEqual(NpgsqlProvider.Configuration.PostponeTransactionBeginOnConnectionOpenEvent ? DbLinkTransactionStatus.Pending : DbLinkTransactionStatus.Open, link.TransactionStatus);
                    };

                    Assert.AreEqual(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    order.Append(link.ExecuteScalar<int>("Select 2"));
                    Assert.AreEqual("12", order.ToString());

                    transaction.Commit();
                }
            }
        }
    }
}
