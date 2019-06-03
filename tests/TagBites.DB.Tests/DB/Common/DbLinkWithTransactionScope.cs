using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;
using TBS.Data.DB;
using TBS.Data.DB.Configuration;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbLinkWithTransactionScope : DbTestBase
    {
        [TestMethod]
        public void UncommitedTransactionScopeTest()
        {
            using (var ts = new TransactionScope())
            {
                using (var link = DefaultProvider.CreateLink())
                {
                    using (var t = link.Begin())
                        link.ExecuteNonQuery("SELECT 1");

                    try
                    {
                        link.ExecuteNonQuery("SELECT 1");
                        Assert.Fail();
                    }
                    catch { }
                }
            }
        }

        [TestMethod]
        public void UncommitedTransactionScopeTest2()
        {
            using (var link2 = DefaultProvider.CreateLink())
            {
                using (var ts = new TransactionScope())
                using (var link = DefaultProvider.CreateLink())
                {
                    link.Force();
                }
            }
        }

        [TestMethod]
        public void SuppressTransactionScopeTest()
        {
            using (new TransactionScope())
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (var link = DefaultProvider.CreateLink())
                        Assert.AreEqual(DbLinkTransactionStatus.None, link.TransactionStatus);
                }

                using (var link = DefaultProvider.CreateLink())
                    Assert.AreEqual(DbLinkTransactionStatus.Pending, link.TransactionStatus);
            }

            // Two Links
            DefaultProvider.Configuration.LinkCreateOnDifferentSystemTransaction = DbLinkCreateOnDifferentSystemTransaction.CreateLinkWithNewContextOrAssigedToCurrentTransaction;

            using (new TransactionScope())
            using (var link = DefaultProvider.CreateLink())
            {
                var tr = Transaction.Current;

                using (new TransactionScope(TransactionScopeOption.Suppress))
                using (var link2 = DefaultProvider.CreateLink())
                {
                    Assert.AreNotEqual(link.ConnectionContext, link2.ConnectionContext);

                    using (new TransactionScope(tr.DependentClone(DependentCloneOption.BlockCommitUntilComplete)))
                    using (var link3 = DefaultProvider.CreateLink())
                    {
                        Assert.AreEqual(link.ConnectionContext, link3.ConnectionContext);
                    }
                }
            }

            // Two Links 2
            DefaultProvider.Configuration.LinkCreateOnDifferentSystemTransaction = DbLinkCreateOnDifferentSystemTransaction.TryToMoveTransactionOrThrowException;

            using (new TransactionScope())
            using (var link = DefaultProvider.CreateLink())
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (var link2 = DefaultProvider.CreateLink())
                    {
                        Assert.AreEqual(link.ConnectionContext, link2.ConnectionContext);
                        Assert.IsNotNull(Transaction.Current);
                    }
                }
            }

            // Two Links 3
            DefaultProvider.Configuration.LinkCreateOnDifferentSystemTransaction = DbLinkCreateOnDifferentSystemTransaction.ThrowException;

            using (new TransactionScope())
            using (var link = DefaultProvider.CreateLink())
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                    try
                    {
                        using (var link2 = DefaultProvider.CreateLink())
                            Assert.Fail("Should Throw Exception");
                    }
                    catch { }
            }
        }

        [TestMethod]
        public void TransactionScopeTest()
        {
            using (var scope = new TransactionScope())
            {
                using (var link = DefaultProvider.CreateLink())
                using (var transaction = link.Begin())
                {
                    Assert.AreEqual(DefaultProvider.Configuration.ForceOnLinkCreate || DefaultProvider.Configuration.ForceOnTransactionBegin ? DbLinkTransactionStatus.Open : DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    link.ExecuteNonQuery("Select 1");
                    transaction.Commit();
                    Assert.AreEqual(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                using (var link = DefaultProvider.CreateLink())
                using (var transaction = link.Begin())
                {
                    Assert.AreEqual(DbLinkTransactionStatus.Open, link.TransactionStatus);
                    link.ExecuteNonQuery("Select 1");
                    transaction.Commit();
                    Assert.AreEqual(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                scope.Complete();
            }

            using (var scope = new TransactionScope())
            {
                using (var link = DefaultProvider.CreateLink())
                {
                    Assert.AreEqual(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    link.ExecuteNonQuery("Select 1");
                    Assert.AreEqual(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                using (var link = DefaultProvider.CreateLink())
                {
                    Assert.AreEqual(DbLinkTransactionStatus.Open, link.TransactionStatus);
                    link.ExecuteNonQuery("Select 1");
                    Assert.AreEqual(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                scope.Complete();
            }
        }

        [TestMethod]
        public void DifferentTransactionScopeTest()
        {
            using (var scope = new TransactionScope())
            using (var link = DefaultProvider.CreateLink())
            {
                link.Force();

                using (var scope2 = new TransactionScope())
                using (var link2 = DefaultProvider.CreateLink())
                { }
            }

            using (var scope = new TransactionScope())
            using (var link = DefaultProvider.CreateLink())
            {
                using (var scope2 = new TransactionScope(TransactionScopeOption.Required))
                using (var link2 = DefaultProvider.CreateLink())
                { }
            }

            using (var scope = new TransactionScope())
            using (var link = DefaultProvider.CreateLink())
            {
                try
                {
                    using (var scope2 = new TransactionScope(TransactionScopeOption.Suppress))
                    using (var link2 = DefaultProvider.CreateLink())
                        Assert.Fail();
                }
                catch {/* Ignored */}
            }

            using (var scope = new TransactionScope())
            using (var link = DefaultProvider.CreateLink())
            {
                try
                {
                    using (var scope2 = new TransactionScope(TransactionScopeOption.RequiresNew))
                    using (var link2 = DefaultProvider.CreateLink())
                        Assert.Fail();
                }
                catch {/* Ignored */}
            }
        }

        [TestMethod]
        public void TransactionScopeWithTasksTest()
        {
            using (var scope = new TransactionScope())
            using (var link = DefaultProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                try
                {
                    var task = new Task(() =>
                    {
                        using (var link2 = DefaultProvider.CreateLink())
                        using (var transaction2 = link2.Begin())
                        {
                            transaction2.Commit();
                        }
                    });
                    task.Start();
                    task.Wait();

                    if (DefaultProvider.Configuration.LinkCreateOnDifferentSystemTransaction == DbLinkCreateOnDifferentSystemTransaction.ThrowException)
                        Assert.Fail();
                }
                catch (Exception)
                {
                    if (DefaultProvider.Configuration.LinkCreateOnDifferentSystemTransaction != DbLinkCreateOnDifferentSystemTransaction.ThrowException)
                        Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void ExceptionOnCommit()
        {
            using (var link = DefaultProvider.CreateLink())
            {
                try
                {
                    using (var scope = new TransactionScope())
                    {
                        link.ExecuteNonQuery("INSERT INTO testerror VALUES(-4)");
                        scope.Complete();
                    }
                }
                catch (Exception e)
                {
                    Assert.IsTrue(e is DbException);
                }

                using (var scope = new TransactionScope())
                {
                    link.ExecuteNonQuery("Select 1");
                    scope.Complete();
                }
            }
        }
    }
}
