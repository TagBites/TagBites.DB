using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TagBites.DB.Configuration;
using TagBites.DB.Tests.DB.Core;
using Xunit;

namespace TagBites.DB.Tests.DB.Common
{
    public class DbLinkWithTransactionScope : DbTestBase
    {
        [Fact]
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
                        Assert.True(false);
                    }
                    catch { }
                }
            }
        }

        [Fact]
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

        [Fact]
        public void SuppressTransactionScopeTest()
        {
            using (new TransactionScope())
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (var link = DefaultProvider.CreateLink())
                        Assert.Equal(DbLinkTransactionStatus.None, link.TransactionStatus);
                }

                using (var link = DefaultProvider.CreateLink())
                    Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);
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
                    Assert.NotEqual(link.ConnectionContext, link2.ConnectionContext);

                    using (new TransactionScope(tr.DependentClone(DependentCloneOption.BlockCommitUntilComplete)))
                    using (var link3 = DefaultProvider.CreateLink())
                    {
                        Assert.Equal(link.ConnectionContext, link3.ConnectionContext);
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
                        Assert.Equal(link.ConnectionContext, link2.ConnectionContext);
                        Assert.NotNull(Transaction.Current);
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
                            Assert.True(false, "Should Throw Exception");
                    }
                    catch { }
            }
        }

        [Fact]
        public void TransactionScopeTest()
        {
            using (var scope = new TransactionScope())
            {
                using (var link = DefaultProvider.CreateLink())
                using (var transaction = link.Begin())
                {
                    Assert.Equal(DefaultProvider.Configuration.ForceOnLinkCreate || DefaultProvider.Configuration.ForceOnTransactionBegin ? DbLinkTransactionStatus.Open : DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    link.ExecuteNonQuery("Select 1");
                    transaction.Commit();
                    Assert.Equal(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                using (var link = DefaultProvider.CreateLink())
                using (var transaction = link.Begin())
                {
                    Assert.Equal(DbLinkTransactionStatus.Open, link.TransactionStatus);
                    link.ExecuteNonQuery("Select 1");
                    transaction.Commit();
                    Assert.Equal(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                scope.Complete();
            }

            using (var scope = new TransactionScope())
            {
                using (var link = DefaultProvider.CreateLink())
                {
                    Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    link.ExecuteNonQuery("Select 1");
                    Assert.Equal(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                using (var link = DefaultProvider.CreateLink())
                {
                    Assert.Equal(DbLinkTransactionStatus.Open, link.TransactionStatus);
                    link.ExecuteNonQuery("Select 1");
                    Assert.Equal(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                scope.Complete();
            }
        }

        [Fact]
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
                        Assert.True(false);
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
                        Assert.True(false);
                }
                catch {/* Ignored */}
            }
        }

        [Fact]
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
                        Assert.True(false);
                }
                catch (Exception)
                {
                    if (DefaultProvider.Configuration.LinkCreateOnDifferentSystemTransaction != DbLinkCreateOnDifferentSystemTransaction.ThrowException)
                        Assert.True(false);
                }
            }
        }

        [Fact]
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
                    Assert.True(e is DbException);
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
