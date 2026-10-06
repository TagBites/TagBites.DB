namespace TagBites.DB;

public class QueryTests
{
    [Fact]
    public void ConcatKeepsFirstQueryParametersTest()
    {
        var query = Query.Concat(new Query("SELECT {0}, {1};", 10, 20), new Query("SELECT 1"));

        Assert.Equal("SELECT @1, @2\n;SELECT 1", query.Command);
        Assert.Equal([10, 20], query.Parameters.Select(x => x.Value));
    }

    [Fact]
    public void ConcatReusesParameterWithSameValueTest()
    {
        var query = Query.Concat(new Query("SELECT {0}", 10), new Query("SELECT {0}, {1}", 10, 30));

        Assert.Equal("SELECT @1\n;SELECT @1, @2", query.Command);
        Assert.Equal([10, 30], query.Parameters.Select(x => x.Value));
    }

    [Fact]
    public void ConcatRenamesCollidingParameterTest()
    {
        var query = Query.Concat(new Query("SELECT {0}", 10), new Query("SELECT {0}", 20));

        Assert.Equal("SELECT @1\n;SELECT @2", query.Command);
        Assert.Equal(["@1", "@2"], query.Parameters.Select(x => x.Name));
    }

    [Fact]
    public void ConcatReusesNullParameterTest()
    {
        var query = Query.Concat(new Query("SELECT {0}", new object[] { null }), new Query("SELECT {0}", new object[] { null }));

        Assert.Equal("SELECT @1\n;SELECT @1", query.Command);
        Assert.Single(query.Parameters);
    }

    [Fact]
    public void ConcatKeepsPlaceholderWithoutParameterTest()
    {
        var query = Query.Concat(new Query("SELECT {0}", 10), new Query("SELECT @x, {0}", 20));

        Assert.Equal("SELECT @1\n;SELECT @x, @2", query.Command);
    }

    [Fact]
    public void ConcatGivesRepeatedPlaceholderOneNameTest()
    {
        var second = new Query("SELECT @1 + @1", new List<QueryParameter> { new("@1", 20) });
        var query = Query.Concat(new Query("SELECT {0}", 10), second);

        Assert.Equal("SELECT @1\n;SELECT @2 + @2", query.Command);
        Assert.Equal(2, query.Parameters.Count);
    }

    [Fact]
    public void ConcatSkipsTakenParameterNameTest()
    {
        var first = new Query("SELECT @1, @3", new List<QueryParameter> { new("@1", 10), new("@3", 30) });
        var query = Query.Concat(first, new Query("SELECT {0}", 50));

        Assert.Equal("SELECT @1, @3\n;SELECT @4", query.Command);
    }
}
