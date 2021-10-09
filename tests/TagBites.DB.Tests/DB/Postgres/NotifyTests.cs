using System;
using System.Threading.Tasks;
using Xunit;

namespace TagBites.DB.Postgres
{
    public class NotifyTests : DbTests
    {
        [Fact]
        public void NotifyWithSingleLink()
        {
            using (var link = NpgsqlProvider.CreateExclusiveLink())
            {
                var x0 = 0;
                link.ConnectionContext.Notification += (s, e) => { ++x0; };

                link.Listen("x0");

                link.Notify("x0", "1");
                link.ExecuteNonQuery("SELECT 1");
                Assert.Equal(1, x0);

                link.Notify("x0", "2");
                link.ExecuteNonQuery("SELECT 1");
                Assert.Equal(2, x0);

                link.Unlisten("x0");
                link.Notify("x0", "4");
                link.ExecuteNonQuery("SELECT 1");
                Assert.Equal(2, x0);
            }
        }

        [Fact]
        public void NotifyWithTwoLinks()
        {
            using (var sender = NpgsqlProvider.CreateExclusiveLink())
            using (var receiver = NpgsqlProvider.CreateExclusiveLink())
            {
                var x1 = 0;
                receiver.ConnectionContext.Notification += (s, e) => { ++x1; };

                receiver.Listen("x1");

                sender.Notify("x1", "1");
                sender.Notify("x1", "2");

                receiver.ExecuteNonQuery("SELECT 1");
                Assert.Equal(2, x1);
            }
        }

        [Fact]
        public void NotifyAtTheEndOfTransaction()
        {
            var x2 = 0;

            using (var link = NpgsqlProvider.CreateExclusiveLink())
            using (var transaction = link.Begin())
            {
                link.ConnectionContext.Notification += (s, e) => { ++x2; };
                link.Listen("x2");

                link.Notify("x2", "1");
                link.Notify("x2", "2");
                link.Notify("x2", "3");

                transaction.Commit();
            }

            Assert.Equal(3, x2);
        }

        [Fact]
        public void NotifyAtTheEndOfTransactionWithSeparateConnections()
        {
            NpgsqlProvider.Configuration.ImplicitCreateTransactionScopeIfNotExists = false;

            var x3 = 0;

            using (var sender = NpgsqlProvider.CreateExclusiveLink())
            using (var receiver = NpgsqlProvider.CreateExclusiveLink())
            {
                receiver.ConnectionContext.Notification += (s, e) => { ++x3; };
                receiver.Listen("x3");

                using (var transaction = sender.Begin())
                {
                    sender.Notify("x3", "1");
                    Assert.Equal(0, x3);

                    sender.Notify("x3", "2");
                    Assert.Equal(0, x3);

                    receiver.ExecuteNonQuery("SELECT 1");
                    Assert.Equal(0, x3);

                    transaction.Commit();
                }

                receiver.ExecuteNonQuery("SELECT 1");
                Assert.Equal(2, x3);
            }
        }

        [Fact]
        public async Task NotifyManager()
        {
            using (var notifyManager = new PgSqlNotifyListener(NpgsqlProvider))
            {
                var x4 = 0;
                var y4 = 0;

                notifyManager.Notification += (s, e) =>
                {
                    switch (e.Channel)
                    {
                        case "a4":
                        case "x4":
                            Assert.Equal("1", e.Message);
                            ++x4;
                            break;
                        case "b4":
                        case "y4":
                            Assert.Equal("2", e.Message);
                            ++y4;
                            break;
                    }
                };

                await notifyManager.ListenAsync("a4").ConfigureAwait(false);
                await notifyManager.ListenAsync("b4").ConfigureAwait(false);
                Assert.Equal(0, x4);
                Assert.Equal(0, y4);

                using (var link = NpgsqlProvider.CreateExclusiveLink())
                {
                    link.Notify("a4", "1");
                    link.Notify("a4", "1");
                    link.Notify("b4", "2");
                    link.Notify("b4", "2");

                    await notifyManager.ListenAsync("x4", "y4").ConfigureAwait(false);
                    await notifyManager.UnlistenAsync("a4", "b4").ConfigureAwait(false);
                    await Task.Delay(1000);
                    Assert.Equal(2, x4);
                    Assert.Equal(2, y4);

                    link.Notify("x4", "1");
                    link.Notify("y4", "2");
                    link.Notify("x4", "1");
                    link.Notify("y4", "2");
                }

                await WaitForCounter(4, () => x4);
                await WaitForCounter(4, () => y4);

                Assert.Equal(4, x4);
                Assert.Equal(4, y4);
            }
        }

        [Fact]
        public async Task NotifyManagerWithTransaction()
        {
            var expectedX5 = 20;
            var expectedY5 = 100;

            var x5 = 0;
            var y5 = 0;

            using (var notifyManager = new PgSqlNotifyListener(NpgsqlProvider))
            {
                notifyManager.Notification += (s, e) =>
                {
                    switch (e.Channel)
                    {
                        case "x5": ++x5; break;
                        case "y5": ++y5; break;
                    }
                };

                await notifyManager.ListenAsync("x5");
                await notifyManager.ListenAsync("y5");

                using (var link = NpgsqlProvider.CreateLink())
                using (var transaction = link.Begin())
                {
                    for (var i = 0; i < expectedX5; i++)
                        link.Notify("x5", i.ToString());

                    for (var i = 0; i < expectedY5; i++)
                        link.Notify("y5", i.ToString());
                    link.Notify("y5", "0"); // Postgres ignores duplicates

                    transaction.Commit();
                }

                await WaitForCounter(expectedX5, () => x5);
                await WaitForCounter(expectedY5, () => y5);

                Assert.Equal(expectedX5, x5);
                Assert.Equal(expectedY5, y5);
            }
        }

        [Fact]
        public async Task NotifyManagerConnectionLost()
        {
            var x6 = 0;
            var x7 = 0;
            var has3 = false;
            var has4 = false;
            var has5 = false;

            using var notifyManager = new PgSqlNotifyListener(NpgsqlProvider);
            notifyManager.Notification += (s, e) =>
            {
                switch (e.Channel)
                {
                    case "x6":
                        {
                            ++x6;
                            switch (e.Message)
                            {
                                case "3": has3 = true; break;
                                case "4": has4 = true; break;
                                case "5": has5 = true; break;
                            }
                            break;
                        }
                    case "x7":
                        ++x7;
                        break;
                }
            };
            await notifyManager.ListenAsync("fake").ConfigureAwait(false);
            await notifyManager.ListenAsync("x6").ConfigureAwait(false);

            using (var link = NpgsqlProvider.CreateExclusiveLink())
            {
                var p1 = notifyManager.ProcessId;
                Assert.NotNull(p1);

                link.Notify("x6", "1");
                link.Notify("x6", "2");

                await WaitForCounter(2, () => x6);
                Assert.Equal(2, x6);

                link.Notify("x6", "3");
                link.ExecuteNonQuery(@"SELECT pg_terminate_backend({0})", notifyManager.ProcessId);

                link.Notify("x6", "4");
                await WaitForCounter(4, () => x6);  // 3 and 4 can be lost, usually only 3 is lost

                link.Notify("x6", "5");
                await WaitForCounter(5, () => x6);

                Assert.True(has5 && (x6 < 4 || has3 || has4)); // 3 and 4 can be lost

                var p2 = notifyManager.ProcessId;
                Assert.NotNull(p2);
                Assert.NotEqual(p1, p2);

                await notifyManager.ListenAsync("x7").ConfigureAwait(false);

                link.Notify("x7", "1");
                await WaitForCounter(1, () => x7);

                Assert.Equal(1, x7);
            }
        }

        private static async Task WaitForCounter(int counterExpected, Func<int> counterActual, int times = 100)
        {
            for (var checks = 0; counterActual() < counterExpected && checks < times; ++checks)
                await Task.Delay(10).ConfigureAwait(false);
        }
    }
}
