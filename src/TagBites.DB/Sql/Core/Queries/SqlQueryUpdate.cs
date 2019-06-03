using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Sql
{
    public class SqlQueryUpdate : SqlQueryBase
    {
        public SqlClauseWith With { get; } = new SqlClauseWith();
        public SqlTable Table { get; }
        public SqlClauseSet Set { get; } = new SqlClauseSet();
        public SqlClauseFrom From { get; } = new SqlClauseFrom();
        public SqlClauseJoin Join { get; } = new SqlClauseJoin();
        public SqlClauseConditionals Where { get; } = new SqlClauseConditionals();
        public SqlClauseSelect Returning { get; } = new SqlClauseSelect();

        public SqlQueryUpdate(SqlTable table)
        {
            Guard.ArgumentNotNull(table, nameof(table));
            Table = table;
        }
        public SqlQueryUpdate(string tableName)
            : this(tableName, null)
        { }
        public SqlQueryUpdate(string tableName, string tableAlias)
        {
            Guard.ArgumentNotNullOrEmpty(tableName, nameof(tableAlias));
            Table = new SqlTable(tableName, string.IsNullOrEmpty(tableAlias) ? "tu_0" : tableAlias);
        }


        protected internal override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitQuery(this, builder);
        }
    }
}