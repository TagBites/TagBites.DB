using TagBites.DB;
using static TagBites.Sql.SqlExpression;

namespace TagBites.Sql;

public class SqlQueryInsertTests : DbTests
{
    [Fact]
    public void ColumnTargetsKeepOrder()
    {
        var q = new SqlQueryInsertValues("test_table");
        q.Columns.Add("plain_column");
        q.Columns.Add(q.Into.Column("array_column")[3]);
        q.Columns.Add(q.Into.Column("complex_column").Field("field"));
        q.Columns.Add("other_column");
        q.Values.Add(1, 2, 3, 4);

        Assert.Equal("INSERT INTO test_table AS iit_0(plain_column, array_column[3], complex_column.field, other_column) VALUES (1, 2, 3, 4)", q.ToString());
    }

    [Fact]
    public void ColumnTargetsFollowListChanges()
    {
        var q = new SqlQueryInsertValues("test_table");
        var columns = q.Columns;
        columns.Add("plain_column");
        columns.Add(q.Into.Column("array_column").Slice(1, 2));
        columns.Add(q.Into.Column("complex_column").Field("field"));

        columns.Insert(0, "id_column");
        columns.Remove("plain_column");
        columns[2] = "complex_column";

        Assert.Equal("(id_column, array_column[1:2], complex_column)", columns.ToString());
        Assert.Equal("array_column", columns[1]);
        Assert.Null(columns.GetEntry(2).Target);
    }

    [Fact]
    public void ExpressionColumnTargetIsRejected()
    {
        var q = new SqlQueryInsertValues("test_table");

        Assert.Throws<ArgumentException>(() => q.Columns.Add(q.Into.Column("plain_column") + One));
    }

    [PostgresFact]
    public void ColumnTargetsInsertIntoDatabase()
    {
        using var link = CreateLink();
        using var transaction = link.Begin();

        link.ExecuteNonQuery("CREATE TEMP TABLE test_complex (field1 int, field2 int)");
        link.ExecuteNonQuery("CREATE TEMP TABLE test_table (plain_column int, array_column int[], slice_column int[], complex_column test_complex, complex_array test_complex[])");

        var q = new SqlQueryInsertValues("test_table");
        q.Columns.Add(q.Into.Column("array_column")[3]);
        q.Columns.Add("plain_column");
        q.Columns.Add(q.Into.Column("slice_column").Slice(1, 2));
        q.Columns.Add(q.Into.Column("complex_column").Field("field2"));
        q.Columns.Add(q.Into.Column("complex_array")[1].Field("field1"));
        q.Values.Add(30, 1, LiteralExpression("ARRAY[10, 20]"), 5, 7);
        link.ExecuteNonQuery(q);

        var row = link.Execute("SELECT plain_column, array_column::text AS array_column, slice_column::text AS slice_column, complex_column::text AS complex_column, complex_array::text AS complex_array FROM test_table");
        Assert.Equal(1, row.GetValue<int>(0, "plain_column"));
        Assert.Equal("[3:3]={30}", row.GetValue<string>(0, "array_column"));
        Assert.Equal("{10,20}", row.GetValue<string>(0, "slice_column"));
        Assert.Equal("(,5)", row.GetValue<string>(0, "complex_column"));
        Assert.Equal("{\"(7,)\"}", row.GetValue<string>(0, "complex_array"));
    }
}
