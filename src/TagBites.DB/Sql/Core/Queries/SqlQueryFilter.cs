#nullable enable
using System;
using System.ComponentModel;
using System.Linq;

namespace TagBites.Sql;

public class SqlQueryFilter : SqlClauseConditionals
{
    public SqlQueryBase? SourceQuery { get; }
    public SqlTable Root => From.FirstOrDefault()?.Table ?? throw new InvalidOperationException("Root table is missing.");

    [EditorBrowsable(EditorBrowsableState.Never)]
    public SqlClauseWith With { get; } = new();
    public SqlClauseFrom From { get; } = new();
    public SqlClauseJoin Join { get; } = new();

    public SqlQueryFilter() { }
    public SqlQueryFilter(SqlTable root) => From.Add(root);
    public SqlQueryFilter(SqlQuerySelect select)
    {
        SourceQuery = select;
        From.AddRange(select.From);
        Join.AddRange(select.Join);
    }
}
