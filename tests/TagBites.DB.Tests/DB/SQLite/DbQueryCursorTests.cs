using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBS.Data.DB;

namespace TBS.Data.UnitTests.DB.SQLite
{
    [TestClass]
    public class DbQueryCursorTests : DbTestBase
    {
        private class Model
        {
            public double Number { get; set; }
            public string Text { get; set; }
        }

        [TestMethod]
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
                Assert.AreEqual(cursorManager.CursorCount, 0);

                for (var i = 0; i < 2; i++)
                    using (var cursor = cursorManager.CreateCursor(q))
                    {
                        Assert.AreEqual(1, cursorManager.CursorCount);
                        Assert.AreEqual(2, cursor.RecordCount);

                        var result = cursor.Execute(0, 1);
                        Assert.AreEqual(1, result.RowCount);
                        Assert.AreEqual((long)1, result.GetValue<long>(0, 0));

                        result = cursor.Execute(1, 1);
                        Assert.AreEqual(1, result.RowCount);
                        Assert.AreEqual((long)2, result.GetValue<long>(0, 0));
                    }
            }

            Assert.AreEqual(SQLiteProvider.UsePooling ? 1 : 2, openCount);
        }

        [TestMethod]
        public void IteratorTest()
        {
            if (!SQLiteProvider.IsCursorSupported)
                return;

            var q = new Query("WITH RECURSIVE t(n) AS (SELECT 1 UNION ALL SELECT n+1 FROM t) SELECT n AS Number, 'This is text.' AS Text FROM t LIMIT 100");

            using (var cursorManager = SQLiteProvider.CreateCursorManager())
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
    }
}
