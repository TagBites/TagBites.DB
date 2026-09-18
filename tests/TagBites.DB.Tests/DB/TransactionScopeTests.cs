using System;
using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;
using TagBites.DB.Configuration;
using Xunit;

namespace TagBites.DB
{
    public class TransactionScopeTests : DbTests
    {
        [Fact]
        public void UncommitedTransactionScopeTest()
        {
            var provider = CreateDefaultProvider();
            provider.Configuration.AllowMissingRollbackInNestedTransaction = true;

            using (var ts = new TransactionScope())
            {
                using (var link = provider.CreateLink())
                {
                    using (var t2 = link.Begin())
                    {
                        using (var t = link.Begin())
                            link.ExecuteNonQuery("SELECT 1");
                    }

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
            var provider = CreateDefaultProvider();
            provider.Configuration.UseSystemTransactions = true;

            using (new TransactionScope())
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (var link = provider.CreateLink())
                        Assert.Equal(DbLinkTransactionStatus.None, link.TransactionStatus);
                }

                using (var link = provider.CreateLink())
                    Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);
            }

            // Two Links
            provider.Configuration.LinkCreateOnDifferentSystemTransaction = DbLinkCreateOnDifferentSystemTransaction.CreateLinkWithNewContextOrAssigedToCurrentTransaction;

            using (new TransactionScope())
            using (var link = provider.CreateLink())
            {
                var tr = Transaction.Current;

                using (new TransactionScope(TransactionScopeOption.Suppress))
                using (var link2 = provider.CreateLink())
                {
                    Assert.NotEqual(link.ConnectionContext, link2.ConnectionContext);

                    using (new TransactionScope(tr.DependentClone(DependentCloneOption.BlockCommitUntilComplete)))
                    using (var link3 = provider.CreateLink())
                    {
                        Assert.Equal(link.ConnectionContext, link3.ConnectionContext);
                    }
                }
            }

            // Two Links 2
            provider.Configuration.LinkCreateOnDifferentSystemTransaction = DbLinkCreateOnDifferentSystemTransaction.TryToMoveTransactionOrThrowException;

            using (new TransactionScope())
            using (var link = provider.CreateLink())
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    using (var link2 = provider.CreateLink())
                    {
                        Assert.Equal(link.ConnectionContext, link2.ConnectionContext);
                        Assert.NotNull(Transaction.Current);
                    }
                }
            }

            // Two Links 3
            provider.Configuration.LinkCreateOnDifferentSystemTransaction = DbLinkCreateOnDifferentSystemTransaction.ThrowException;

            using (new TransactionScope())
            using (var link = provider.CreateLink())
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                    try
                    {
                        using (var link2 = provider.CreateLink())
                            Assert.True(false, "Should Throw Exception");
                    }
                    catch { }
            }
        }

        [Fact]
        public void TransactionScopeTest()
        {
            var provider = CreateDefaultProvider();
            provider.Configuration.UseSystemTransactions = true;

            using (var scope = new TransactionScope())
            {
                using (var link = provider.CreateLink())
                using (var transaction = link.Begin())
                {
                    Assert.Equal(provider.Configuration.ForceOnLinkCreate || provider.Configuration.ForceOnTransactionBegin ? DbLinkTransactionStatus.Open : DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    link.ExecuteNonQuery("Select 1");
                    transaction.Commit();
                    Assert.Equal(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                using (var link = provider.CreateLink())
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
                using (var link = provider.CreateLink())
                {
                    Assert.Equal(DbLinkTransactionStatus.Pending, link.TransactionStatus);
                    link.ExecuteNonQuery("Select 1");
                    Assert.Equal(DbLinkTransactionStatus.Open, link.TransactionStatus);
                }

                using (var link = provider.CreateLink())
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

        [PostgresFact]
        public async Task TransactionScopeWithConcurrentTasksTest()
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                IDbLinkContext c1 = null;
                IDbLinkContext c2 = null;

                var task1 = Task.Run(async () =>
                {
                    using (var link2 = DefaultProvider.CreateExclusiveLink())
                    using (var transaction2 = link2.Begin())
                    {
                        c1 = link2.ConnectionContext;

                        for (var i = 0; i < 5; i++)
                        {
                            await Task.Delay(100);
                            link2.Execute("Select 1");
                        }
                        transaction2.Commit();
                    }
                });
                var task2 = Task.Run(async () =>
                {
                    using (var link2 = DefaultProvider.CreateExclusiveLink())
                    using (var transaction2 = link2.Begin())
                    {
                        c2 = link2.ConnectionContext;

                        for (var i = 0; i < 5; i++)
                        {
                            if (c1 == c2)
                                throw new Exception();

                            await Task.Delay(100);
                            link2.Execute("Select 1");
                        }
                        transaction2.Commit();
                    }
                });

                await Task.WhenAll(task1, task2);
                scope.Complete();
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
