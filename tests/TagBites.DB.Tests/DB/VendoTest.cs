using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using TBS.Data.DB;
using System.Threading;
using System.Linq;
using System.Transactions;
using TBS.Data.DB.Configuration;
using TBS.Data.DB.PostgreSql;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class VendoTest : DbTestBase
    {
        protected override void InitializeConnectionProvider(PgSqlLinkProvider connectionProvider)
        {
            Semaphore transactionSemaphore = new Semaphore(1, 1);

            connectionProvider.Configuration.ForceOnLinkCreate = false;
            connectionProvider.Configuration.ForceOnTransactionBegin = false;
            connectionProvider.Configuration.ImplicitCreateTransactionScopeIfNotExists = true;
            connectionProvider.Configuration.LinkCreateOnDifferentSystemTransaction = DbLinkCreateOnDifferentSystemTransaction.CreateLinkWithNewContextOrAssigedToCurrentTransaction;

            NpgsqlProvider.ContextCreated += (sender, args) =>
            {
                //args.LinkContext.Force();
                args.LinkContext.TransactionContextBegin += (s, e) => transactionSemaphore.WaitOne();
                args.LinkContext.TransactionContextClose += (s, e) => transactionSemaphore.Release(1);
                args.LinkContext.TransactionBegin += (s, e) =>
                {
                    var context = s as PgSqlLinkContext;
                    if (context != null)
                    {
                        context.Listen("a");
                    }
                };
                args.LinkContext.TransactionClose += (s, e) =>
                {
                    var context = s as PgSqlLinkContext;
                    if (context != null)
                    {
                        if (e.CloseReason == DbLinkTransactionCloseReason.Commit)
                            context.UnlistenAll();
                    }
                };
            };
        }

        [TestMethod]
        public void OneTransactionAtTheTimeTest()
        {
            Func<bool> isNewTransactionBlocked = () =>
            {
                using (ExecutionContext.SuppressFlow())
                {
                    var tst = Task.Factory.StartNew(() =>
                    {
                        using (var sc = new TransactionScope())
                        {
                            using (var linkt = NpgsqlProvider.CreateLink())
                            using (var transactiont = linkt.Begin())
                            {
                                transactiont.Commit();
                            }
                            sc.Complete();
                        }
                    });
                    return !tst.Wait(100);
                }
            };

            Task.WaitAll(Enumerable.Range(0, 4).Select(x => Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    using (var sc = new TransactionScope())
                    {
                        using (var link = NpgsqlProvider.CreateLink())
                        using (var t = link.Begin())
                        {
                            Assert.IsTrue(isNewTransactionBlocked());
                            t.Commit();
                        }
                        sc.Complete();
                    }
                }
            }
            )).ToArray());
        }

        [TestMethod]
        public void VendoNotifyTest()
        {
            int notifyCounter = 0;

            using (var link = NpgsqlProvider.CreateLink())
            {
                link.ConnectionContext.Notification += (sender, args) => { if (args.Channel == "a") ++notifyCounter; };

                using (var transaction = link.Begin())
                {
                    link.Notify("a", "1");
                    transaction.Commit();
                }
            }

            Assert.AreEqual(1, notifyCounter);
        }
    }
}
