using System;
using System.Linq;

namespace TagBites.Sql
{
    public class SqlClauseJoin : SqlClauseCollectionBase<SqlClauseJoinEntry>
    {
        public SqlClauseJoinEntry Find(SqlClauseJoinEntryType type, SqlColumn joinColumn, SqlColumn otherTableColumn)
        {
            return this.FirstOrDefault(x => x.JoinType == type
                                            && Equals(x.Table, joinColumn.Table)
                                            && x.Condition is SqlConditionBinaryOperator { OperatorType: SqlConditionBinaryOperatorType.Equal, OperandLeft: SqlColumn c1, OperandRight: SqlColumn c2 }
                                            && (c1.ColumnName == joinColumn.ColumnName && c2.ColumnName == otherTableColumn.ColumnName && Equals(c2.Table.Table, otherTableColumn.Table.Table)
                                                || c2.ColumnName == joinColumn.ColumnName && c1.ColumnName == otherTableColumn.ColumnName && Equals(c1.Table.Table, otherTableColumn.Table.Table)));
        }

        public SqlTable AddOnOrGet(SqlClauseJoinEntryType type, SqlColumn joinColumn, SqlColumn otherTableColumn)
        {
            var t = Find(type, joinColumn, otherTableColumn);
            if (t != null)
            {
                joinColumn.Table.Alias = t.Table.Alias;
                return t.Table;
            }

            return AddOn(type, joinColumn, otherTableColumn);
        }
        public TSqlTable AddOnOrGet<TSqlTable>(SqlClauseJoinEntryType type, Func<TSqlTable, SqlColumn> joinTableColumnSelector, SqlColumn otherTableColumn) where TSqlTable : SqlTable, new()
        {
            var table = new TSqlTable();
            AddOnOrGet(type, joinTableColumnSelector(table), otherTableColumn);
            return table;
        }

        public SqlTable AddOnExpression(SqlClauseJoinEntryType joinType, SqlTable joinTable, SqlCondition expression)
        {
            foreach (var entry in this)
                if (joinTable.Alias == entry.Table.Alias)
                {
                    if (SqlClauseJoinEntryConditionType.On == entry.ConditionType && joinType == entry.JoinType && Equals(joinTable, entry.Table) && Equals(expression, entry.Condition))
                        return entry.Table;

                    // ReSharper disable once LocalizableElement
                    throw new ArgumentException($"Join with alias '{joinTable.Alias}' already exists and has different condition.", nameof(joinTable));
                }

            joinTable.Alias ??= GetNextAlias();

            Add(new SqlClauseJoinEntry(joinType, joinTable, SqlClauseJoinEntryConditionType.On, expression));
            return joinTable;
        }
        public SqlTable AddOnExpression(SqlClauseJoinEntryType joinType, string joinTableName, string joinTableAlias, SqlCondition expression)
        {
            return AddOnExpression(joinType, new SqlTable(joinTableName, joinTableAlias), expression);
        }
        public Func<SqlTable> LazyAddOnExpression(SqlClauseJoinEntryType joinType, string joinTableName, string joinTableAlias, SqlCondition expression)
        {
            SqlTable joinTable = null;
            return () => joinTable ??= AddOnExpression(joinType, joinTableName, joinTableAlias, expression);
        }

        public SqlTable AddOn(SqlClauseJoinEntryType joinType, SqlColumn joinTableColumn, SqlColumn otherTableColumn)
        {
            AddOnExpression(joinType, joinTableColumn.Table, SqlExpression.AreEquals(joinTableColumn, otherTableColumn));
            return joinTableColumn.Table;
        }
        public SqlTable AddOn(SqlClauseJoinEntryType joinType, SqlTable joinTable, string joinTableColumnName, SqlColumn otherTableColumn)
        {
            AddOnExpression(joinType, joinTable, SqlExpression.AreEquals(joinTable.Column(joinTableColumnName), otherTableColumn));
            return joinTable;
        }
        public SqlTable AddOn(SqlClauseJoinEntryType joinType, SqlTable joinTable, string joinTableColumnName, SqlTable otherTable, string otherTableColumnName)
        {
            AddOnExpression(joinType, joinTable, SqlExpression.AreEquals(joinTable.Column(joinTableColumnName), otherTable.Column(otherTableColumnName)));
            return joinTable;
        }
        public SqlTable AddOn(SqlClauseJoinEntryType joinType, string joinTableName, string joinTableColumnName, SqlColumn otherTableColumn)
        {
            return AddOn(joinType, new SqlTable(joinTableName, GetNextAlias()), joinTableColumnName, otherTableColumn);
        }
        public SqlTable AddOn(SqlClauseJoinEntryType joinType, string joinTableName, string joinTableColumnName, SqlTable otherTable, string otherTableColumnName)
        {
            return AddOn(joinType, new SqlTable(joinTableName, GetNextAlias()), joinTableColumnName, otherTable, otherTableColumnName);
        }
        public SqlTable AddOn(SqlClauseJoinEntryType joinType, string joinTableName, string joinTableAlias, string joinTableColumnName, SqlColumn otherTableColumn)
        {
            return AddOn(joinType, new SqlTable(joinTableName, joinTableAlias), joinTableColumnName, otherTableColumn);
        }
        public SqlTable AddOn(SqlClauseJoinEntryType joinType, string joinTableName, string joinTableAlias, string joinTableColumnName, SqlTable otherTable, string otherTableColumnName)
        {
            return AddOn(joinType, new SqlTable(joinTableName, joinTableAlias), joinTableColumnName, otherTable, otherTableColumnName);
        }
        public Func<SqlTable> LazyAddOn(SqlClauseJoinEntryType joinType, string joinTableName, string joinTableColumnName, SqlColumn otherTableColumn)
        {
            SqlTable joinTable = null;
            return () => joinTable ??= AddOn(joinType, joinTableName, joinTableColumnName, otherTableColumn);
        }
        public Func<SqlTable> LazyAddOn(SqlClauseJoinEntryType joinType, string joinTableName, string joinTableColumnName, SqlTable otherTable, string otherTableColumnName)
        {
            SqlTable joinTable = null;
            return () => joinTable ??= AddOn(joinType, joinTableName, joinTableColumnName, otherTable, otherTableColumnName);
        }
        public Func<SqlTable> LazyAddOn(SqlClauseJoinEntryType joinType, string joinTableName, string joinTableAlias, string joinTableColumnName, SqlColumn otherTableColumn)
        {
            SqlTable joinTable = null;
            return () => joinTable ??= AddOn(joinType, joinTableName, joinTableAlias, joinTableColumnName, otherTableColumn);
        }
        public Func<SqlTable> LazyAddOn(SqlClauseJoinEntryType joinType, string joinTableName, string joinTableAlias, string joinTableColumnName, SqlTable otherTable, string otherTableColumnName)
        {
            SqlTable joinTable = null;
            return () => joinTable ??= AddOn(joinType, joinTableName, joinTableAlias, joinTableColumnName, otherTable, otherTableColumnName);
        }

        public TSqlTable AddOn<TSqlTable>(SqlClauseJoinEntryType joinType, Func<TSqlTable, SqlColumn> joinTableColumnSelector, SqlColumn otherTableColumn) where TSqlTable : SqlTable, new()
        {
            return (TSqlTable)AddOn(joinType, joinTableColumnSelector(new TSqlTable() { Alias = GetNextAlias() }), otherTableColumn);
        }
        public Func<TSqlTable> LazyAddOn<TSqlTable>(SqlClauseJoinEntryType joinType, Func<TSqlTable, SqlColumn> joinTableColumnSelector, SqlColumn otherTableColumn) where TSqlTable : SqlTable, new()
        {
            TSqlTable joinTable = null;
            return () => joinTable ??= (TSqlTable)AddOn(joinType, joinTableColumnSelector(new TSqlTable() { Alias = GetNextAlias() }), otherTableColumn);
        }

        private string GetNextAlias()
        {
            return $"tj_{Count + 1}";
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, builder);
        }
    }
}
