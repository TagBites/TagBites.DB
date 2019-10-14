using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagBites.DB.Postgres;
using TagBites.DB.Tests.DB.Core;
using Xunit;

namespace TagBites.DB.Tests.DB.PostgreSql
{
    public class DbLinkNotifyTest : DbTestBase
    {
        [Fact]
        public void NotifyWithSingleLink()
        {
            using (var link = NpgsqlProvider.CreateExclusiveLink())
            {
                var hitCount = 0;
                link.ConnectionContext.Notification += (s, e) => { ++hitCount; };

                link.Listen("x0");

                link.Notify("x0", "1");
                DbLinkExtensions.ExecuteNonQuery(link, "SELECT 1");
                Assert.Equal(1, hitCount);

                link.Notify("x0", "2");
                DbLinkExtensions.ExecuteNonQuery(link, "SELECT 1");
                Assert.Equal(2, hitCount);

                link.Unlisten("x0");
                link.Notify("x0", "4");
                DbLinkExtensions.ExecuteNonQuery(link, "SELECT 1");
                Assert.Equal(2, hitCount);
            }
        }

        [Fact]
        public void NotifyWithTwoLinks()
        {
            using (var sender = NpgsqlProvider.CreateExclusiveLink())
            using (var receiver = NpgsqlProvider.CreateExclusiveLink())
            {
                var hitCount = 0;
                receiver.ConnectionContext.Notification += (s, e) => { ++hitCount; };

                receiver.Listen("x1");

                sender.Notify("x1", "1");
                sender.Notify("x1", "2");

                DbLinkExtensions.ExecuteNonQuery(receiver, "SELECT 1");
                Assert.Equal(2, hitCount);
            }
        }

        [Fact]
        public void NotifyAtTheEndOfTransaction()
        {
            var hitCount = 0;

            using (var link = NpgsqlProvider.CreateExclusiveLink())
            using (var transaction = link.Begin())
            {
                link.ConnectionContext.Notification += (s, e) => { ++hitCount; };
                link.Listen("x2");

                link.Notify("x2", "1");
                link.Notify("x2", "2");
                link.Notify("x2", "3");

                transaction.Commit();
            }

            Assert.Equal(3, hitCount);
        }

        [Fact]
        public void NotifyAtTheEndOfTransactionWithSeparateConnections()
        {
            NpgsqlProvider.Configuration.ImplicitCreateTransactionScopeIfNotExists = false;

            var hitCount = 0;

            using (var sender = NpgsqlProvider.CreateExclusiveLink())
            using (var receiver = NpgsqlProvider.CreateExclusiveLink())
            {
                receiver.ConnectionContext.Notification += (s, e) => { ++hitCount; };
                receiver.Listen("x3");

                using (var transaction = sender.Begin())
                {
                    sender.Notify("x3", "1");
                    Assert.Equal(0, hitCount);

                    sender.Notify("x3", "2");
                    Assert.Equal(0, hitCount);

                    DbLinkExtensions.ExecuteNonQuery(receiver, "SELECT 1");
                    Assert.Equal(0, hitCount);

                    transaction.Commit();
                }

                DbLinkExtensions.ExecuteNonQuery(receiver, "SELECT 1");
                Assert.Equal(2, hitCount);
            }
        }

        [Fact]
        public async Task NotifyManager()
        {
            using (var notifyManager = new PgSqlNotifyListener(NpgsqlProvider))
            {
                var hitD1 = 0;
                var hitD2 = 0;

                notifyManager.Notification += (s, e) =>
                {
                    switch (e.Channel)
                    {
                        case "a4":
                        case "x4":
                            Assert.Equal("1", e.Message);
                            ++hitD1;
                            break;
                        case "b4":
                        case "y4":
                            Assert.Equal("2", e.Message);
                            ++hitD2;
                            break;
                    }
                };

                await notifyManager.ListenAsync("a4").ConfigureAwait(false);
                await notifyManager.ListenAsync("b4").ConfigureAwait(false);
                Assert.Equal(0, hitD1);
                Assert.Equal(0, hitD2);

                using (var link = NpgsqlProvider.CreateExclusiveLink())
                {
                    link.Notify("a4", "1");
                    link.Notify("a4", "1");
                    link.Notify("b4", "2");
                    link.Notify("b4", "2");

                    await notifyManager.ListenAsync("x4", "y4").ConfigureAwait(false);
                    await notifyManager.UnlistenAsync("a4", "b4").ConfigureAwait(false);
                    Assert.Equal(2, hitD1);
                    Assert.Equal(2, hitD2);

                    link.Notify("x4", "1");
                    link.Notify("y4", "2");
                    link.Notify("x4", "1");
                    link.Notify("y4", "2");
                }

                var checks = 0;
                while ((hitD1 < 4 || hitD2 < 4) && ++checks < 200)
                    await Task.Delay(10);

                Assert.Equal(4, hitD1);
                Assert.Equal(4, hitD2);
            }
        }

        [Fact]
        public async Task NotifyManagerWithTransaction()
        {
            var hitD1 = 0;
            var hitD2 = 0;

            using (var notifyManager = new PgSqlNotifyListener(NpgsqlProvider))
            {
                notifyManager.Notification += (s, e) =>
                {
                    switch (e.Channel)
                    {
                        case "a5": ++hitD1; break;
                        case "b5": ++hitD2; break;
                    }
                };

                await notifyManager.ListenAsync("a5");
                await notifyManager.ListenAsync("b5");

                using (var link = NpgsqlProvider.CreateLink())
                using (var transaction = link.Begin())
                {
                    link.Notify("a5", "1");
                    link.Notify("a5", "2");
                    link.Notify("a5", "3");

                    link.Notify("b5", "4");
                    link.Notify("b5", "5");
                    link.Notify("b5", "6");
                    link.Notify("b5", "7");
                    link.Notify("b5", "7"); // Postgres ignores duplicates

                    transaction.Commit();
                }

                var checks = 0;
                while ((hitD1 < 3 || hitD2 < 4) && ++checks < 200)
                    await Task.Delay(10);

                Assert.Equal(3, hitD1);
                Assert.Equal(4, hitD2);
            }
        }
    }
}
