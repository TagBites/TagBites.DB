using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using TBS.Data.DB;
using TBS.Data.DB.PostgreSql;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbLinkCursorTest : DbTestBase
    {
        private class Model
        {
            public double Number { get; set; }
            public string Text { get; set; }
        }

        [TestMethod]
        public void CursorSwitchTest()
        {
            if (!NpgsqlProvider.IsCursorSupported)
                return;

            var q = new Query("SELECT 1 UNION SELECT 2");

            using (var cursorManager = NpgsqlProvider.CreateCursorManager())
            {
                cursorManager.TransactionTimeout = 2000; // 2s

                for (var i = 0; i < 2; i++)
                {
                    var c1 = cursorManager.CreateCursor(q);
                    cursorManager.CreateCursor(q);

                    Thread.Sleep(1000);
                    var c2 = cursorManager.CreateCursor(q);
                    cursorManager.CreateCursor(q);

                    Assert.AreNotEqual(c1.Owner, c2.Owner);
                    Assert.AreEqual(2, cursorManager.ConnectionCount);

                    Thread.Sleep(1500);
                    Assert.AreEqual(1, cursorManager.ConnectionCount);

                    Thread.Sleep(1000);
                    Assert.AreEqual(0, cursorManager.ConnectionCount);
                }
            }
        }

        [TestMethod]
        public void CursorTest()
        {
            if (!NpgsqlProvider.IsCursorSupported)
                return;

            int openCount = 0;
            NpgsqlProvider.ContextCreated += (sender, args) =>
            {
                args.LinkContext.ConnectionOpen += (s2, e2) =>
                {
                    ++openCount;
                };
            };

            var q = new Query("SELECT * FROM (SELECT 1 AS id UNION SELECT 2) AS t ORDER BY id");

            using (var cursorManager = NpgsqlProvider.CreateCursorManager())
            {
                Assert.AreEqual(cursorManager.CursorCount, 0);

                for (var i = 0; i < 2; i++)
                    using (var cursor = cursorManager.CreateCursor(q))
                    {
                        Assert.AreEqual(1, cursorManager.CursorCount);
                        Assert.AreEqual(2, cursor.RecordCount);

                        var result = cursor.Execute(0, 1);
                        Assert.AreEqual(1, result.RowCount);
                        Assert.AreEqual(1, result.GetValue<int>(0, 0));

                        result = cursor.Execute(1, 1);
                        Assert.AreEqual(1, result.RowCount);
                        Assert.AreEqual(2, result.GetValue<int>(0, 0));
                    }
            }

            Assert.AreEqual(1, openCount);
        }

        [TestMethod]
        public void CursorSearchTest()
        {
            if (!NpgsqlProvider.IsCursorSupported)
                return;

            var q = new Query("SELECT * FROM (SELECT 1 AS id UNION SELECT 2 UNION SELECT 3 UNION SELECT 4) AS t ORDER BY id");

            using (var cursorManager = NpgsqlProvider.CreateCursorManager())
            {
                Assert.AreEqual(cursorManager.CursorCount, 0);

                using (var cursor = cursorManager.CreateCursor(q, "id", 3))
                {
                    var recordCount = cursor.RecordCount;
                    var searchIndex = cursor.SearchResultPosition;

                    var data = cursor.Execute(0, recordCount);
                    var ids = data.ToColumnScalars<int>();

                    Assert.AreEqual(ids.Count, recordCount);
                    Assert.AreEqual(ids.IndexOf(3), searchIndex);
                }
            }
        }

        [TestMethod]
        public void IteratorTest()
        {
            if (!NpgsqlProvider.IsCursorSupported)
                return;

            var q = new Query("WITH RECURSIVE t(n) AS (SELECT 1 UNION ALL SELECT n+1 FROM t) SELECT n AS Number, 'This is text.' AS Text FROM t LIMIT 100");

            using (var cursorManager = NpgsqlProvider.CreateCursorManager())
            using (var cursor = cursorManager.CreateCursor(q))
            {
                Assert.AreEqual(cursor.RecordCount, 100);

                int i = 0;
                foreach (var item in cursor.Iterate<Model>())
                {
                    Assert.AreEqual(item.Number, ++i);
                    Assert.AreEqual(item.Text, "This is text.");
                }
            }
        }

        [TestMethod]
        public void CursorActionTest()
        {
            if (!NpgsqlProvider.IsCursorSupported)
                return;

            var beforeHitCount = 0;
            var afterHitCount = 0;
            int? pid = null;
            var q = new Query("SELECT * FROM (SELECT 1 AS id UNION SELECT 2 UNION SELECT 3 UNION SELECT 4) AS t ORDER BY id");
            Action<IDbLink> before = link =>
            {
                ++beforeHitCount;
                link.ExecuteNonQuery("Select 1");
                pid = ((PgSqlLinkContext)link.ConnectionContext).ProcessId;
            };
            Action<IDbLink> after = link =>
            {
                ++afterHitCount;
                link.ExecuteNonQuery("Select 1");
                Assert.AreEqual(pid, ((PgSqlLinkContext)link.ConnectionContext).ProcessId);
            };

            using (var cursorManager = NpgsqlProvider.CreateCursorManager())
            {
                Assert.AreEqual(cursorManager.CursorCount, 0);

                for (int i = 0; i < 2; i++)
                    using (var cursor = cursorManager.CreateCursor(q, null, "id", 3, before, after))
                    {
                        Assert.AreEqual(4, cursor.RecordCount);
                        Assert.AreEqual(2, cursor.SearchResultPosition);
                    }
            }

            Assert.AreEqual(2, beforeHitCount);
            Assert.AreEqual(2, afterHitCount);
        }
    }
}
