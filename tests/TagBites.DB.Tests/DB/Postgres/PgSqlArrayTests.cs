using Xunit;

namespace TagBites.DB.Postgres;

public class PgSqlArrayTests : DbTests
{
    [Theory]
    [InlineData("{ \"a\": 1 }", false)]
    [InlineData("{ \"a\": }", false)]
    [InlineData("{ \"a\"1 }", false)]
    [InlineData("{ \"a\", \"a\"1 }", false)]
    [InlineData("{ \"a\" }", true)]
    [InlineData("{ \"a \\\" \" }", true)]
    [InlineData("{ \"a\", \"a\" }", true)]
    [InlineData("{ a, b }", true)]
    [InlineData("{ a:1, b:2 }", true)]
    public void ArrayParseTest(string value, bool result)
    {
        Assert.Equal(result, PgSqlArray.TryParse(value, out _));
    }
}
