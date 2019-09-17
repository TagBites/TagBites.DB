using TBS.Data.DB;
using Xunit;

namespace TBS.Data.UnitTests.DB.SQLite
{
    public class DbQueryCursorTests : DbTestBase
    {
        private class Model
        {
            public double Number { get; set; }
            public string Text { get; set; }
        }

        [Fact]
        public void CursorTest()
        {
            if (!SQLiteProvider.IsCursorSupported)
                return;

            int openCount = 0;
            SQLiteProvider.ContextCreated += (sender, args) =>
            {
                args.LinkContext.ConnectionOpen += (s2, e2) =>
                {
                    ++openCount;
                };
            };

            var q = new Query("SELECT 1 UNION SELECT 2");

            using (var cursorManager = SQLiteProvider.CreateCursorManager())
            {
                Assert.Equal(0, cursorManager.CursorCount);

                for (var i = 0; i < 2; i++)
                    using (var cursor = cursorManager.CreateCursor(q))
                    {
                        Assert.Equal(1, cursorManager.CursorCount);
                        Assert.Equal(2, cursor.RecordCount);

                        var result = cursor.Execute(0, 1);
                        Assert.Equal(1, result.RowCount);
                        Assert.Equal((long)1, result.GetValue<long>(0, 0));

                        result = cursor.Execute(1, 1);
                        Assert.Equal(1, result.RowCount);
                        Assert.Equal((long)2, result.GetValue<long>(0, 0));
                    }
            }

            Assert.Equal(SQLiteProvider.UsePooling ? 1 : 2, openCount);
        }

        [Fact]
        public void IteratorTest()
        {
            if (!SQLiteProvider.IsCursorSupported)
                return;

            var q = new Query("WITH RECURSIVE t(n) AS (SELECT 1 UNION ALL SELECT n+1 FROM t) SELECT n AS Number, 'This is text.' AS Text FROM t LIMIT 100");

            using (var cursorManager = SQLiteProvider.CreateCursorManager())
            using (var cursor = cursorManager.CreateCursor(q))
            {
                Assert.Equal(100, cursor.RecordCount);

                int i = 0;
                foreach (var item in cursor.Iterate<Model>())
                {
                    Assert.Equal(item.Number, ++i);
                    Assert.Equal("This is text.", item.Text);
                }
            }
        }
    }
}
