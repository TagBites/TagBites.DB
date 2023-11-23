using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TagBites.DB.Postgres
{
    [Collection("Cursors")]
    public class CursorTests : DbTests
    {
        //[Fact]
        public async Task CursorSwitchTest()
        {
            if (!NpgsqlProvider.IsCursorSupported)
                return;

            using var cursorManager = NpgsqlProvider.CreateCursorManager();
            cursorManager.TransactionTimeout = 500;

            for (var i = 0; i < 2; i++)
            {
                var q = new Query("SELECT * FROM generate_series(1, 2000)");

                var c1 = cursorManager.CreateCursor(q);
                await Task.Delay(100);
                var c2 = cursorManager.CreateCursor(q);

                Assert.NotEqual(c1.Owner, c2.Owner);
                Assert.Equal(2, cursorManager.ConnectionCount);

                for (var j = 0; j < 60; j++)
                    if (cursorManager.ConnectionCount > 0)
                        await Task.Delay(200);

                Assert.Equal(0, cursorManager.ConnectionCount);
            }
        }

        [Fact]
        public void CursorTest()
        {
            if (!NpgsqlProvider.IsCursorSupported)
                return;

            var q = new Query("SELECT * FROM (SELECT 1 AS id UNION SELECT 2) AS t ORDER BY id");

            using (var cursorManager = NpgsqlProvider.CreateCursorManager())
            {
                Assert.Equal(0, cursorManager.CursorCount);

                for (var i = 0; i < 2; i++)
                    using (var cursor = cursorManager.CreateCursor(q))
                    {
                        Assert.Equal(1, cursorManager.CursorCount);
                        Assert.Equal(2, cursor.RecordCount);

                        var result = cursor.Execute(0, 1);
                        Assert.Equal(1, result.RowCount);
                        Assert.Equal(1, result.GetValue<int>(0, 0));

                        result = cursor.Execute(1, 1);
                        Assert.Equal(1, result.RowCount);
                        Assert.Equal(2, result.GetValue<int>(0, 0));
                    }

                Assert.Equal(0, cursorManager.ConnectionCount);
            }
        }

        [Fact]
        public void CursorSearchTest()
        {
            if (!NpgsqlProvider.IsCursorSupported)
                return;

            var q = new Query("SELECT * FROM (SELECT 1 AS id UNION SELECT 2 UNION SELECT 3 UNION SELECT 4) AS t ORDER BY id");

            using (var cursorManager = NpgsqlProvider.CreateCursorManager())
            {
                Assert.Equal(0, cursorManager.CursorCount);

                using (var cursor = cursorManager.CreateCursor(q, "id", 3))
                {
                    var recordCount = cursor.RecordCount;
                    var searchIndex = cursor.SearchResultPosition;

                    var data = cursor.Execute(0, recordCount);
                    var ids = data.ToColumnScalars<int>();

                    Assert.Equal(ids.Count, recordCount);
                    Assert.Equal(ids.IndexOf(3), searchIndex);
                }
            }
        }

        [Fact]
        public void IteratorTest()
        {
            if (!NpgsqlProvider.IsCursorSupported)
                return;

            var q = new Query("WITH RECURSIVE t(n) AS (SELECT 1 UNION ALL SELECT n+1 FROM t) SELECT n AS Double, 'This is text.' AS Text FROM t LIMIT 100");

            using (var cursorManager = NpgsqlProvider.CreateCursorManager())
            using (var cursor = cursorManager.CreateCursor(q))
            {
                Assert.Equal(100, cursor.RecordCount);

                var i = 0;
                foreach (var item in cursor.AsList<Model>())
                {
                    Assert.Equal(item.Double, ++i);
                    Assert.Equal("This is text.", item.Text);
                }
            }
        }

        [Fact]
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
                Assert.Equal(pid, ((PgSqlLinkContext)link.ConnectionContext).ProcessId);
            };

            using (var cursorManager = NpgsqlProvider.CreateCursorManager())
            {
                Assert.Equal(0, cursorManager.CursorCount);

                for (var i = 0; i < 2; i++)
                    using (var cursor = cursorManager.CreateCursor(q, null, "id", 3, before, after))
                    {
                        Assert.Equal(4, cursor.RecordCount);
                        Assert.Equal(2, cursor.SearchResultPosition);
                    }
            }

            Assert.Equal(2, beforeHitCount);
            Assert.Equal(2, afterHitCount);
        }

        [Fact]
        public async Task ConcurrentCursorsTestAsync()
        {
            if (!NpgsqlProvider.IsCursorSupported)
                return;

            var provider = DbManager.CreateNpgsqlProvider(true, 1, 8);
            var q = new Query("SELECT * FROM generate_series(1, 2000)");

            using (var cursorManager = provider.CreateCursorManager())
            {
                cursorManager.TransactionTimeout = 200 * 2;
                cursorManager.ActiveTransactionLimit = 4;

                var ts = new List<Task>();
                var r = new Random();

                for (var i = 0; i < 10; i++)
                {
                    var t = Task.Run(async () =>
                    {
                        for (var j = 0; j < 10; j++)
                            // ReSharper disable once AccessToDisposedClosure
                            using (var cursor = cursorManager.CreateCursor(q))
                            {
                                await Task.Delay(r.Next(1, 10));
                                Assert.True(cursor.RecordCount > 0);
                            }
                    });
                    ts.Add(t);
                }

                await Task.WhenAll(ts);

                Assert.Equal(0, cursorManager.CursorCount);
            }
        }
    }
}
