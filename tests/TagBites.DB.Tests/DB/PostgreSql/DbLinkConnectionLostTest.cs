﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using TBS.Data.DB;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbLinkConnectionLostTest : DbTestBase
    {
        [TestMethod]
        public void ReconnectAfterBreakWithAttempsTest()
        {
            var onConnectionLost = (DbLinkConnectionLostEventHandler)((s, e) =>
            {
                if (e.ReconnectAttempts < 6)
                {
                    Thread.Sleep(2000);
                    e.Reconnect = true;
                }
            });

            using (var link = NpgsqlProvider.CreateLink())
            {
                link.ConnectionContext.ConnectionLost += onConnectionLost;
                var result = link.ExecuteScalar<bool>(
                    "SELECT (CASE WHEN now() < {0} THEN pg_terminate_backend(pg_backend_pid()) ELSE FALSE END)",
                    DateTime.Now.AddSeconds(1));
                Assert.AreEqual(false, result);
            }
        }

        [TestMethod]
        public void ReconnectAfterBreakWithTryCatchTest()
        {
            var openCount = 0;
            var lostCount = 0;
            var closeCount = 0;

            using (var link = NpgsqlProvider.CreateLink())
            {
                link.ConnectionContext.ConnectionOpen += (s, e) => ++openCount;
                link.ConnectionContext.ConnectionLost += (s, e) => { if (lostCount == 0) e.Reconnect = true; ++lostCount; };
                link.ConnectionContext.ConnectionClose += (s, e) => ++closeCount;
                link.ConnectionContext.Bag["a"] = 1;

                Assert.AreEqual(1, link.ExecuteScalar<int>("SELECT 1"));

                using (var breakLink = NpgsqlProvider.CreateLink())
                    try
                    {
                        string q = @"SELECT pg_terminate_backend(pg_backend_pid())";
                        breakLink.Execute(q);
                        Assert.Fail();
                    }
                    catch
                    {
                        // Ignored
                    }

                // Check is connection ok
                Assert.AreEqual(1, link.ExecuteScalar<int>("SELECT 1"));
                // Check if bag has values after reconnect 
                Assert.AreEqual(1, link.ConnectionContext.Bag["a"]);

                Assert.AreEqual(3, openCount);
                Assert.AreEqual(2, lostCount);
                Assert.AreEqual(0, closeCount);
            }
        }

        [TestMethod]
        public void ReconnectAfterBreakOnDifferentConnectionTest()
        {
            int reconnectAttempts = 0;

            using (var link = NpgsqlProvider.CreateExclusiveLink())
            using (var link2 = NpgsqlProvider.CreateExclusiveLink())
            {
                link.ConnectionContext.ConnectionLost += (sender, args) => args.Reconnect = ++reconnectAttempts == 1;

                link.Force();
                var linkProcessId = link.ConnectionContext.ProcessId;

                Assert.AreEqual(true, link2.ExecuteScalar<bool>($"SELECT pg_terminate_backend({linkProcessId})"));
                Assert.AreEqual(1, link.ExecuteScalar<int>("SELECT 1"));
                Assert.AreEqual(1, reconnectAttempts);
            }
        }

        [TestMethod]
        public void ConnectionIsNotBreakTest()
        {
            using (var link = NpgsqlProvider.CreateExclusiveLink())
            {
                link.ConnectionContext.ConnectionLost += (sender, args) => Assert.Fail();

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
