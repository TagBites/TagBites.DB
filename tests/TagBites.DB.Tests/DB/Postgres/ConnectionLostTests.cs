﻿using System;
using Xunit;

namespace TagBites.DB.Postgres
{
    public class ConnectionLostTests : DbTests
    {
        [Fact]
        public void ReconnectAfterBreakWithAttemptsTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                link.Force();
                link.ConnectionContext.ConnectionLost += ConnectionContextOnConnectionLost;

                var result = link.ExecuteScalar<bool>($"SELECT pg_terminate_backend({link.ConnectionContext.ProcessId})");
                Assert.False(result);

                result = link.ExecuteScalar<bool>("SELECT TRUE");
                Assert.True(result);
            }

            void ConnectionContextOnConnectionLost(object sender, DbLinkConnectionLostEventArgs e)
            {
                if (e.ReconnectAttempts < 1)
                    e.Reconnect = true;
            }
        }

        [Fact]
        public void ReconnectAfterBreakWithTryCatchTest()
        {
            var openCount = 0;
            var lostCount = 0;
            var closeCount = 0;

            using (var link = NpgsqlProvider.CreateLink())
            {
                link.ConnectionContext.ConnectionOpened += (s, e) => ++openCount;
                link.ConnectionContext.ConnectionLost += (s, e) => { if (lostCount == 0) e.Reconnect = true; ++lostCount; };
                link.ConnectionContext.ConnectionClosed += (s, e) => ++closeCount;
                link.ConnectionContext.Bag["a"] = 1;

                Assert.Equal(1, link.ExecuteScalar<int>("SELECT 1"));

                using (var breakLink = NpgsqlProvider.CreateLink())
                    try
                    {
                        var q = @"SELECT pg_terminate_backend(pg_backend_pid())";
                        breakLink.Execute(q);
                        Assert.True(false);
                    }
                    catch
                    {
                        // Ignored
                    }

                // Check is connection ok
                Assert.Equal(1, link.ExecuteScalar<int>("SELECT 1"));
                // Check if bag has values after reconnect 
                Assert.Equal(1, link.ConnectionContext.Bag["a"]);

                Assert.Equal(3, openCount);
                Assert.Equal(2, lostCount);
                Assert.Equal(0, closeCount);
            }
        }

        [Fact]
        public void ReconnectAfterBreakOnDifferentConnectionTest()
        {
            var reconnectAttempts = 0;

            using (var link = NpgsqlProvider.CreateExclusiveLink())
            using (var link2 = NpgsqlProvider.CreateExclusiveLink())
            {
                link.ConnectionContext.ConnectionLost += (sender, args) => args.Reconnect = ++reconnectAttempts == 1;

                link.Force();
                var linkProcessId = link.ConnectionContext.ProcessId;

                Assert.True(link2.ExecuteScalar<bool>($"SELECT pg_terminate_backend({linkProcessId})"));
                Assert.Equal(1, link.ExecuteScalar<int>("SELECT 1"));
                Assert.Equal(1, reconnectAttempts);
            }
        }

        [Fact]
        public void ConnectionIsNotBreakTest()
        {
            using (var link = NpgsqlProvider.CreateExclusiveLink())
            {
                link.ConnectionContext.ConnectionLost += (sender, args) => Assert.True(false);

                try
                {
                    link.ExecuteNonQuery("fake query");
                }
                catch { }

                try
                {
                    link.ExecuteScalar<DateTime>("fake query");
                }
                catch { }
            }
        }
    }
}
