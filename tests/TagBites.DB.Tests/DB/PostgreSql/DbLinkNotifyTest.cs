using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using TBS.Data.DB;
using TBS.Data.DB.PostgreSql;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbLinkNotifyTest : DbTestBase
    {
        [TestMethod]
        public void NotifyWithSingleLink()
        {
            using (var link = NpgsqlProvider.CreateExclusiveLink())
            {
                var hitCount = 0;
                link.ConnectionContext.Notification += (s, e) => { ++hitCount; };

                link.Listen("x0");

                link.Notify("x0", "1");
                link.ExecuteNonQuery("SELECT 1");
                Assert.AreEqual(hitCount, 1);

                link.Notify("x0", "2");
                link.ExecuteNonQuery("SELECT 1");
                Assert.AreEqual(hitCount, 2);

                link.Unlisten("x0");
                link.Notify("x0", "4");
                link.ExecuteNonQuery("SELECT 1");
                Assert.AreEqual(hitCount, 2);
            }
        }

        [TestMethod]
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

                receiver.ExecuteNonQuery("SELECT 1");
                Assert.AreEqual(hitCount, 2);
            }
        }

        [TestMethod]
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

            Assert.AreEqual(3, hitCount);
        }

        [TestMethod]
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
                    Assert.AreEqual(hitCount, 0);

                    sender.Notify("x3", "2");
                    Assert.AreEqual(hitCount, 0);

                    receiver.ExecuteNonQuery("SELECT 1");
                    Assert.AreEqual(hitCount, 0);

                    transaction.Commit();
                }

                receiver.ExecuteNonQuery("SELECT 1");
                Assert.AreEqual(hitCount, 2);
            }
        }

        [TestMethod]
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
                            Assert.AreEqual(e.Message, "1");
                            ++hitD1;
                            break;
                        case "b4":
                        case "y4":
                            Assert.AreEqual(e.Message, "2");
                            ++hitD2;
                            break;
                    }
                };

                await notifyManager.ListenAsync("a4").ConfigureAwait(false);
                await notifyManager.ListenAsync("b4").ConfigureAwait(false);
                Assert.AreEqual(0, hitD1);
                Assert.AreEqual(0, hitD2);

                using (var link = NpgsqlProvider.CreateExclusiveLink())
                {
                    link.Notify("a4", "1");
                    link.Notify("a4", "1");
                    link.Notify("b4", "2");
                    link.Notify("b4", "2");

                    await notifyManager.ListenAsync("x4", "y4").ConfigureAwait(false);
                    await notifyManager.UnlistenAsync("a4", "b4").ConfigureAwait(false);
                    Assert.AreEqual(2, hitD1);
                    Assert.AreEqual(2, hitD2);

                    link.Notify("x4", "1");
                    link.Notify("y4", "2");
                    link.Notify("x4", "1");
                    link.Notify("y4", "2");
                }

                var checks = 0;
                while ((hitD1 < 4 || hitD2 < 4) && ++checks < 200)
                    await Task.Delay(10);

                Assert.AreEqual(4, hitD1);
                Assert.AreEqual(4, hitD2);
            }
        }

        [TestMethod]
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

                Assert.AreEqual(3, hitD1);
                Assert.AreEqual(4, hitD2);
            }
        }
    }
}
