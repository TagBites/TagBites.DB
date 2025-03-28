﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using TagBites.DB.Configuration;
using TagBites.DB.Postgres;
using Xunit;

namespace TagBites.DB
{
    public class VendoTests : DbTests
    {
        protected override void InitializeConnectionProvider(PgSqlLinkProvider connectionProvider)
        {
            var transactionSemaphore = new Semaphore(1, 1);

            connectionProvider.Configuration.ForceOnLinkCreate = false;
            connectionProvider.Configuration.ForceOnTransactionBegin = false;
            connectionProvider.Configuration.ImplicitCreateTransactionScopeIfNotExists = true;
            connectionProvider.Configuration.LinkCreateOnDifferentSystemTransaction = DbLinkCreateOnDifferentSystemTransaction.CreateLinkWithNewContextOrAssigedToCurrentTransaction;

            NpgsqlProvider.ContextCreated += (sender, args) =>
            {
                //args.LinkContext.Force();
                args.LinkContext.TransactionContextBegan += (s, e) => transactionSemaphore.WaitOne();
                args.LinkContext.TransactionContextClosed += (s, e) => transactionSemaphore.Release(1);
                args.LinkContext.TransactionBegan += (s, e) =>
                {
                    var context = s as PgSqlLinkContext;
                    if (context != null)
                    {
                        context.Listen("a");
                    }
                };
                args.LinkContext.TransactionClosed += (s, e) =>
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

        [Fact]
        public void OneTransactionAtTheTimeTest()
        {
            if (!DefaultProvider.Configuration.UseSystemTransactions)
                return;

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
                for (var i = 0; i < 10; i++)
                {
                    using (var sc = new TransactionScope())
                    {
                        using (var link = NpgsqlProvider.CreateLink())
                        using (var t = link.Begin())
                        {
                            Assert.True(isNewTransactionBlocked());
                            t.Commit();
                        }
                        sc.Complete();
                    }
                }
            }
            )).ToArray());
        }

        [Fact]
        public void VendoNotifyTest()
        {
            var notifyCounter = 0;

            using (var link = NpgsqlProvider.CreateLink())
            {
                link.ConnectionContext.Notification += (sender, args) => { if (args.Channel == "a") ++notifyCounter; };

                using (var transaction = link.Begin())
                {
                    link.Notify("a", "1");
                    transaction.Commit();
                }
            }

            Assert.Equal(1, notifyCounter);
        }
    }
}
