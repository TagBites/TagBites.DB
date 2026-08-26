using TagBites.Sql.Resolvers;

namespace TagBites.Sql;

public class SqlInjectionTests
{
    [Theory]
    [InlineData("simple", "simple")]
    [InlineData("Upper", "\"Upper\"")]
    [InlineData("with space", "\"with space\"")]
    [InlineData("1leading_digit", "\"1leading_digit\"")]
    public void QuotesOnlyWhenNeeded(string name, string expected)
    {
        Assert.Equal(expected, SqlQueryResolver.QuoteIdentifierIfNeeded(name));
    }

    [Theory]
    [InlineData("a\"b", "\"a\"\"b\"")]
    [InlineData("\"", "\"\"\"\"")]
    [InlineData("x\" FROM t; DROP TABLE t --", "\"x\"\" FROM t; DROP TABLE t --\"")]
    public void DoublesEmbeddedQuotes(string name, string expected)
    {
        Assert.Equal(expected, SqlQueryResolver.QuoteIdentifierIfNeeded(name));
    }

    [Theory]
    [InlineData("a[1]", "\"a[1]\"")]
    [InlineData("[dbo]", "\"[dbo]\"")]
    public void QuotesBrackets(string name, string expected)
    {
        Assert.Equal(expected, SqlQueryResolver.QuoteIdentifierIfNeeded(name));
    }

    [Theory]
    [InlineData("plain", "'plain'::text")]
    [InlineData("a'b", "'a''b'::text")]
    [InlineData("x'); DROP TABLE t --", "'x''); DROP TABLE t --'::text")]
    public void PostgresCastEscapesValue(string value, string expected)
    {
        Assert.Equal(expected, new PostgresResolver().GetCastString(value, "text"));
    }

    [Theory]
    [InlineData("a'b", "CAST('a''b' AS text)")]
    public void SqliteCastEscapesValue(string value, string expected)
    {
        Assert.Equal(expected, new SqliteResolver().GetCastString(value, "text"));
    }

    [Theory]
    [InlineData("a'b", "CONVERT(text, 'a''b')")]
    public void TransactSqlCastEscapesValue(string value, string expected)
    {
        Assert.Equal(expected, new TransactSqlResolver().GetCastString(value, "text"));
    }
}
