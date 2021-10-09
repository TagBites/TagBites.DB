using System;
using Xunit;

namespace TagBites.Sql
{
    public class SqlDiagnosticsTests : DbTests
    {
        [Fact]
        public void QueryTrackingCommentTest()
        {
            var q = new SqlQuerySelect
            {
                TrackingComment = "My SQL",
                Select = { SqlExpression.One }
            };
            var qs = q.ToString();

            Assert.True(qs.Contains(q.TrackingComment, StringComparison.Ordinal));
        }

        [Fact]
        public void NamedValuesTest()
        {
            var resolver = new SqlQueryResolver() { NamedValuesNumber = 2 };

            var q = new SqlQueryInsertValues("t")
            {
                Columns = { "a", "b" },
                Values = { { 1, 2 }, { 3, 4 }, { 5, 6 } }
            };
            var qs = q.ToString(resolver);
            var qst = qs.Replace(" ", "").Replace("/*", "").Replace("*/", "");

            Assert.True(qst.Contains("a1", StringComparison.Ordinal));
            Assert.True(qst.Contains("b2", StringComparison.Ordinal));
            Assert.True(qst.Contains("a3", StringComparison.Ordinal));
            Assert.True(qst.Contains("b4", StringComparison.Ordinal));
            Assert.False(qst.Contains("a5", StringComparison.Ordinal));
            Assert.False(qst.Contains("b6", StringComparison.Ordinal));
        }
    }
}
