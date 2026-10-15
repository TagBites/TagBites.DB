using TagBites.DB;
using static TagBites.Sql.SqlExpression;

namespace TagBites.Sql;

public class SqlQueryUpdateTests : DbTests
{
    [Fact]
    public void ColumnTarget() => AssertSet("plain_column = 5", x => x.Column("plain_column"), Argument(5));

    [Fact]
    public void ArrayElementTarget() => AssertSet("array_column[3] = null", x => x.Column("array_column")[3], Null);

    [Fact]
    public void ArraySliceTarget() => AssertSet("array_column[2:3] = null", x => x.Column("array_column").Slice(2, 3), Null);

    [Fact]
    public void FieldTarget() => AssertSet("complex_column.field = 5", x => x.Column("complex_column").Field("field"), Argument(5));

    [Fact]
    public void FieldOfArrayElementTarget() => AssertSet("complex_array[1].field = 5", x => x.Column("complex_array")[1].Field("field"), Argument(5));

    [Fact]
    public void QuotedTarget() => AssertSet("\"Complex_Array\"[1].\"Field\" = 5", x => x.Column("Complex_Array")[1].Field("Field"), Argument(5));

    [Fact]
    public void ExpressionTargetIsRejected()
    {
        var q = new SqlQueryUpdate("test_table");

        Assert.Throws<ArgumentException>(() => q.Set.Add(q.Table.Column("plain_column") + One, Argument(5)));
    }

    [Fact]
    public void SliceAndFieldInSelect()
    {
        var q = new SqlQuerySelect();
        var t = q.From.Add("test_table", "t");
        q.Select.Add(t.Column("array_column").Slice(2, 3));
        q.Select.Add(t.Column("complex_column").Field("field"));

        Assert.Equal("(t.array_column)[2:3], (t.complex_column).field", q.Select.ToString());
    }

    [PostgresFact]
    public void TargetsUpdateDatabase()
    {
        using var link = CreateLink();
        using var transaction = link.Begin();

        link.ExecuteNonQuery("CREATE TEMP TABLE test_complex (field1 int, field2 int)");
        link.ExecuteNonQuery("CREATE TEMP TABLE test_table (array_column int[], slice_column int[], complex_column test_complex, complex_array test_complex[])");
        link.ExecuteNonQuery("INSERT INTO test_table VALUES (ARRAY[1, 2, 3], ARRAY[1, 2, 3], ROW(1, 2), ARRAY[ROW(1, 2)::test_complex])");

        var q = new SqlQueryUpdate("test_table");
        q.Set.Add(q.Table.Column("array_column")[3], Argument(30));
        q.Set.Add(q.Table.Column("slice_column").Slice(1, 2), LiteralExpression("ARRAY[10, 20]"));
        q.Set.Add(q.Table.Column("complex_column").Field("field2"), Argument(5));
        q.Set.Add(q.Table.Column("complex_array")[1].Field("field1"), Argument(7));
        link.ExecuteNonQuery(q);

        var row = link.Execute("SELECT array_column::text AS array_column, slice_column::text AS slice_column, complex_column::text AS complex_column, complex_array::text AS complex_array FROM test_table");
        Assert.Equal("{1,2,30}", row.GetValue<string>(0, "array_column"));
        Assert.Equal("{10,20,3}", row.GetValue<string>(0, "slice_column"));
        Assert.Equal("(1,5)", row.GetValue<string>(0, "complex_column"));
        Assert.Equal("{\"(7,2)\"}", row.GetValue<string>(0, "complex_array"));
    }

    private static void AssertSet(string expected, Func<SqlTable, SqlExpression> target, SqlExpression value)
    {
        var q = new SqlQueryUpdate("test_table");
        q.Set.Add(target(q.Table), value);

        Assert.Equal(expected, q.Set.ToString());
    }
}
