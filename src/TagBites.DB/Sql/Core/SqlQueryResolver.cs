using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using TagBites.DB;
using TagBites.DB.Configuration;
using TagBites.Utils;

namespace TagBites.Sql
{
    public class SqlQueryResolver
    {
        private static SqlQueryResolver s_defaultToStringResolver = new SqlQueryResolver();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SqlQueryResolver DefaultToStringResolver
        {
            get => s_defaultToStringResolver;
            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                s_defaultToStringResolver = value;
            }
        }

        private readonly Dictionary<Type, Func<object, object>> _customParameterResolvers = new Dictionary<Type, Func<object, object>>();

        public virtual bool SupportReturningClause => true;
        public bool InlineParameters { get; set; }
        /// <summary>
        /// Number of values sets to include names to.
        /// </summary>
        public int NamedValuesNumber { get; set; }
        public bool UseMultilineTrackingComment { get; set; } = true;

        protected virtual string TrueLiteral => "true";
        protected virtual string FalseLiteral => "false";
        protected virtual string NullLiteral => "null";


        public void RegisterParameterResolver<T>(Func<T, string> parameterResolver)
        {
            Guard.ArgumentNotNull(parameterResolver, nameof(parameterResolver));
            _customParameterResolvers[typeof(T)] = x => parameterResolver((T)x);
        }
        public void RegisterParameterResolver<T>(Func<T, SqlExpression> parameterResolver)
        {
            Guard.ArgumentNotNull(parameterResolver, nameof(parameterResolver));
            _customParameterResolvers[typeof(T)] = x => parameterResolver((T)x);
        }

        public virtual Query GetQuery(IQuerySource querySource)
        {
            return querySource as Query ?? querySource.CreateQuery(this, new SqlQueryBuilder());
        }

        public virtual void Visit(object expression, SqlQueryBuilder builder)
        {
            switch (expression)
            {
                case null:
                    builder.Append(NullLiteral);
                    break;
                case SqlExpression sql:
                    sql.Accept(this, builder);
                    break;
                case ISqlElement element:
                    element.Accept(this, builder);
                    break;
                case SqlQueryBase queryBase:
                    builder.Append('(');
                    queryBase.Accept(this, builder);
                    builder.Append(')');
                    break;
                case Query query:
                    builder.Append('(');
                    builder.Append(query.GetUnsafeEscapeString(this));
                    builder.Append(')');
                    break;
                default:
                    VisitParameter(null, expression, builder);
                    break;
            }
        }
        protected virtual void VisitParameter(object parameterOwner, object parameter, SqlQueryBuilder builder)
        {
            if (parameter == null)
                builder.Append(NullLiteral);
            else
            {
                if (_customParameterResolvers.TryGetValue(parameter.GetType(), out var customParameterResolver))
                {
                    var result = customParameterResolver(parameter);
                    if (result is SqlExpression)
                        Visit(result, builder);
                    else
                        builder.Append((string)result);
                }
                else
                {
                    parameter = DbLinkDataConverter.Default.ToDbType(parameter);
                    if (parameter is SqlExpression)
                        Visit(parameter, builder);
                    else
                    {
                        var text = ToParameterString(parameter, !builder.SupportParameters || InlineParameters);
                        if (text == null)
                            builder.AppendParameter(parameterOwner, parameter);
                        else
                            builder.Append(text);
                    }
                }
            }
        }

        protected internal virtual void VisitExpression(SqlLiteral expression, SqlQueryBuilder builder)
        {
            builder.Append(expression.Value);
        }
        protected internal virtual void VisitExpression(SqlLiteralExpression expression, SqlQueryBuilder builder)
        {
            if (expression.Args.Length == 0)
                builder.Append(expression.Format);
            else
            {
                var args = new object[expression.Args.Length];
                for (var i = 0; i < args.Length; i++)
                {
                    builder.Push();
                    Visit(expression.Args[i], builder);
                    args[i] = builder.Pop();
                }

                builder.Append(string.Format(expression.Format, args));
            }
        }
        protected internal virtual void VisitExpression(SqlArgument expression, SqlQueryBuilder builder)
        {
            VisitParameter(expression, expression.Value, builder);
        }
        protected internal virtual void VisitExpression(SqlTable expression, SqlQueryBuilder builder)
        {
            if (!string.IsNullOrEmpty(expression.Alias))
                builder.Append(QuoteIdentifierIfNeeded(expression.Alias));
        }
        protected internal virtual void VisitExpression(SqlColumn expression, SqlQueryBuilder builder)
        {
            var quoteIdentifierIfNeeded = !string.IsNullOrEmpty(expression.Table.Alias)
                ? $"{QuoteIdentifierIfNeeded(expression.Table.Alias)}.{QuoteIdentifierIfNeeded(expression.ColumnName)}"
                : QuoteIdentifierIfNeeded(expression.ColumnName);
            builder.Append(quoteIdentifierIfNeeded);
        }
        protected internal virtual void VisitExpression(SqlExpressionFunctionCall expression, SqlQueryBuilder builder)
        {
            builder.Append(GetBuildInFunctionName(expression.FunctionName));
            builder.Append('(');
            for (var i = 0; i < expression.Operands.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                Visit(expression.Operands[i], builder);
            }
            builder.Append(')');
        }
        protected internal virtual void VisitExpression(SqlExpressionWithAlias expression, SqlQueryBuilder builder)
        {
            Visit(expression.Expression, builder);
        }
        protected internal virtual void VisitExpression(SqlConditionExpression expression, SqlQueryBuilder builder)
        {
            Visit(expression.Value, builder);
        }
        protected internal virtual void VisitExpression(SqlConditionGroupOperator expression, SqlQueryBuilder builder)
        {
            if (expression.Operands.Count == 1)
                Visit(expression.Operands[0], builder);
            else
            {
                var connect = expression.OperatorType == SqlConditionGroupOperatorType.And ? ") AND (" : ") OR (";
                builder.Append('(');

                for (var i = 0; i < expression.Operands.Count; i++)
                {
                    if (i > 0)
                        builder.Append(connect);
                    Visit(expression.Operands[i], builder);
                }

                builder.Append(')');
            }
        }
        protected internal virtual void VisitExpression(SqlConditionBinaryOperator expression, SqlQueryBuilder builder)
        {
            if (expression.OperatorType < SqlConditionBinaryOperatorType.Like || expression.OperatorType > SqlConditionBinaryOperatorType.EndsWith)
            {
                builder.Append('(');
                Visit(expression.OperandLeft, builder);
                builder.Append(')');

                builder.Append(GetOperatorTypeString(expression.OperatorType));

                builder.Append('(');
                Visit(expression.OperandRight, builder);
                builder.Append(')');
            }
            else
            {
                builder.Append('(');
                Visit(expression.OperandLeft, builder);
                builder.Append(") LIKE (");

                if (expression.OperatorType == SqlConditionBinaryOperatorType.Contains || expression.OperatorType == SqlConditionBinaryOperatorType.EndsWith)
                    builder.Append("'%' || ");

                builder.Append('(');
                Visit(expression.OperandRight, builder); // TODO SQL escape _ and %
                builder.Append(')');

                if (expression.OperatorType == SqlConditionBinaryOperatorType.Contains || expression.OperatorType == SqlConditionBinaryOperatorType.StartsWith)
                    builder.Append(" || '%'");

                builder.Append(')');
            }
        }
        protected internal virtual void VisitExpression(SqlConditionInOperator expression, SqlQueryBuilder builder)
        {
            if (expression.Values.Count == 0)
                builder.Append(FalseLiteral);
            else
            {
                Visit(expression.Operand, builder);
                builder.Append(" IN (");

                if (expression.Values is IList<int>)
                    for (var i = 0; i < expression.Values.Count; i++)
                    {
                        if (i > 0)
                            builder.Append(", ");
                        builder.Append(expression.Values[i].ToString());
                    }
                else
                    for (var i = 0; i < expression.Values.Count; i++)
                    {
                        if (i > 0)
                            builder.Append(", ");
                        Visit(expression.Values[i], builder);
                    }

                builder.Append(')');
            }
        }
        protected internal virtual void VisitExpression(SqlConditionUnaryOperator expression, SqlQueryBuilder builder)
        {
            switch (expression.OperatorType)
            {
                case SqlConditionUnaryOperatorType.Not:
                    builder.Append("NOT ("); Visit(expression.Operand, builder); builder.Append(')');
                    break;
                case SqlConditionUnaryOperatorType.Exists:
                    builder.Append("EXISTS ("); Visit(expression.Operand, builder); builder.Append(')');
                    break;
                case SqlConditionUnaryOperatorType.NotExists:
                    builder.Append("NOT EXISTS ("); Visit(expression.Operand, builder); builder.Append(')');
                    break;
                case SqlConditionUnaryOperatorType.IsNull:
                    builder.Append('('); Visit(expression.Operand, builder); builder.Append(") IS NULL");
                    break;
                case SqlConditionUnaryOperatorType.IsNotNull:
                    builder.Append('('); Visit(expression.Operand, builder); builder.Append(") IS NOT NULL");
                    break;
                default:
                    throw new NotSupportedException($"Operator type {expression.OperatorType} is not supported.");
            }
        }
        protected internal virtual void VisitExpression(SqlExpressionBinaryOperator expression, SqlQueryBuilder builder)
        {
            builder.Append('(');
            Visit(expression.OperandLeft, builder);
            builder.Append(')');
            builder.Append(GetOperatorTypeString(expression.OperatorType));
            builder.Append('(');
            Visit(expression.OperandRight, builder);
            builder.Append(')');
        }
        protected internal virtual void VisitExpression(SqlExpressionCastOperator expression, SqlQueryBuilder builder)
        {
            builder.Append("CAST(");
            Visit(expression.Operand, builder);
            builder.Append(" AS ");

            if (expression.NetType != null)
                builder.Append(GetTypeName(expression.NetType));
            else if (expression.DbType.HasValue)
                builder.Append(GetTypeName(expression.DbType.Value));
            else
                builder.Append(expression.DbTypeName);

            builder.Append(')');
        }
        protected internal virtual void VisitExpression(SqlExpressionUnaryOperator expression, SqlQueryBuilder builder)
        {
            builder.Append(GetOperatorTypeString(expression.OperatorType));
            builder.Append('(');
            Visit(expression.Operand, builder);
            builder.Append(')');
        }
        protected internal virtual void VisitExpression(SqlExpressionIndexerOperator expression, SqlQueryBuilder builder)
        {
            builder.Append('(');
            Visit(expression.Operand, builder);
            builder.Append(")[");
            Visit(expression.Index, builder);
            builder.Append(']');
        }
        protected internal virtual void VisitExpression(SqlExpressionSelect expression, SqlQueryBuilder builder)
        {
            Visit(expression.Select, builder);
        }
        protected internal virtual void VisitExpression(SqlExpressionQuery expression, SqlQueryBuilder builder)
        {
            Visit(expression.Query, builder);
        }

        protected internal virtual void VisitQuery(SqlQuerySelect query, SqlQueryBuilder builder)
        {
            if (builder.ValidationEnabled && query.Select.Count == 0)
                throw new Exception("Select clause does not contain any column.");

            VisitClause(query.With, "WITH", builder);

            builder.AppendKeyword("SELECT");
            VisitClause(query.Distinct, builder);
            VisitClause(query.Select, null, builder);

            VisitClause(query.From, "FROM", builder);
            VisitClause(query.Join, builder);
            VisitClause(query.Where, "WHERE", builder);
            VisitClause(query.GroupBy, "GROUP BY", builder);
            VisitClause(query.Having, "HAVING", builder);
            VisitClause(query.OrderBy, "ORDER BY", builder);
            VisitClause(query.Union, builder);
            VisitLimitOffset(query.Limit, query.Offset, builder);
        }
        protected internal virtual void VisitQuery(SqlQueryValues query, SqlQueryBuilder builder)
        {
            if (builder.ValidationEnabled && query.Values.Count == 0)
                throw new Exception("Values clause does not contain any values.");

            VisitClause(query.Values, "VALUES", builder, null);
            VisitClause(query.OrderBy, "ORDER BY", builder);
            VisitClause(query.Union, builder);
            VisitLimitOffset(query.Limit, query.Offset, builder);
        }
        protected internal virtual void VisitQuery(SqlQueryInsertBase query, SqlQueryBuilder builder)
        {
            VisitClause(query.With, "WITH", builder);
            VisitTableClause(query.Into, "INSERT INTO", builder);
            VisitClause(query.Columns, builder);

            if (query is SqlQueryInsertSelect select)
            {
                builder.Append(" ");

                VisitQuery(select.Select, builder);
                PostVisitQuery(select.Select, builder);
            }
            else
                VisitClause(((SqlQueryInsertValues)query).Values, "VALUES", builder, query.Columns);

            VisitClause(query.Returning, "RETURNING", builder);
        }
        protected internal virtual void VisitQuery(SqlQueryUpdate query, SqlQueryBuilder builder)
        {
            if (query.Set.Count == 0)
                throw new Exception("Set clause does not contain any column.");

            VisitClause(query.With, "WITH", builder);
            VisitTableClause(query.Table, "UPDATE", builder);
            VisitClause(query.Set, "SET", builder);
            VisitClause(query.From, "FROM", builder);
            VisitClause(query.Join, builder);
            VisitClause(query.Where, "WHERE", builder);
            VisitClause(query.Returning, "RETURNING", builder);
        }
        protected internal virtual void VisitQuery(SqlQueryDelete query, SqlQueryBuilder builder)
        {
            VisitClause(query.With, "WITH", builder);
            VisitTableClause(query.From, "DELETE FROM", builder);
            VisitClause(query.Using, "USING", builder);
            VisitClause(query.Join, builder);
            VisitClause(query.Where, "WHERE", builder);
            VisitClause(query.Returning, "RETURNING", builder);
        }
        internal void PostVisitQuery(SqlQueryBase query, SqlQueryBuilder builder)
        {
            VisitQueryTrackingComment(query, builder);
        }
        private void VisitQueryTrackingComment(SqlQueryBase query, SqlQueryBuilder builder)
        {
            if (!string.IsNullOrWhiteSpace(query.TrackingComment))
            {
                if (!builder.IsEmpty)
                    builder.Append(' ');

                if (UseMultilineTrackingComment)
                builder.Append($"/* {query.TrackingComment.Trim()} */");
                else
                    builder.Append($"-- {query.TrackingComment.Trim().Replace("\r", "").Replace("\n", " -- ")}");
            }
        }

        protected internal virtual void VisitClause(SqlClauseWith clause, string keyword, SqlQueryBuilder builder)
        {
            var recursive = false;
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < clause.Count; i++)
                recursive |= clause[i].RecursiveQuery != null;

            for (var i = 0; i < clause.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                else if (keyword != null)
                {
                    builder.AppendKeyword(keyword);

                    if (recursive)
                        builder.AppendKeyword("RECURSIVE");
                }

                VisitClauseEntry(clause[i], builder);
            }
        }
        protected internal virtual void VisitClause(SqlClauseSelect clause, string keyword, SqlQueryBuilder builder)
        {
            for (var i = 0; i < clause.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                else if (keyword != null)
                    builder.AppendKeyword(keyword);

                VisitExpressionWithAlias(builder, clause[i]);
            }
        }
        protected internal virtual void VisitClause(SqlClauseValues clause, string keyword, SqlQueryBuilder builder, SqlClauseColumns columns)
        {
            for (var i = 0; i < clause.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                else if (keyword != null)
                    builder.AppendKeyword(keyword);

                VisitClauseEntry(clause[i], builder, NamedValuesNumber == -1 || i < NamedValuesNumber ? columns : null);
            }
        }
        protected internal virtual void VisitClause(SqlClauseFrom clause, string keyword, SqlQueryBuilder builder)
        {
            for (var i = 0; i < clause.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                else if (keyword != null)
                    builder.AppendKeyword(keyword);

                VisitClauseEntry(clause[i], builder);
            }
        }
        protected internal virtual void VisitClause(SqlClauseConditionals clause, string keyword, SqlQueryBuilder builder)
        {
            if (clause.Count > 0)
            {
                if (keyword != null)
                    builder.AppendKeyword(keyword);

                Visit(SqlExpression.And(clause.Select(x => x)), builder);
            }
        }
        protected internal virtual void VisitClause(SqlClauseGroupBy clause, string keyword, SqlQueryBuilder builder)
        {
            for (var i = 0; i < clause.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                else if (keyword != null)
                    builder.AppendKeyword(keyword);

                Visit(clause[i].Expression, builder);
            }
        }
        protected internal virtual void VisitClause(SqlClauseOrderBy clause, string keyword, SqlQueryBuilder builder)
        {
            for (var i = 0; i < clause.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                else if (keyword != null)
                    builder.AppendKeyword(keyword);

                VisitClauseEntry(clause[i], builder);
            }
        }
        protected internal virtual void VisitClause(SqlClauseSet clause, string keyword, SqlQueryBuilder builder)
        {
            for (var i = 0; i < clause.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");
                else if (keyword != null)
                    builder.AppendKeyword(keyword);

                VisitClauseEntry(clause[i], builder);
            }
        }
        protected internal virtual void VisitClause(SqlClauseDistinct clause, SqlQueryBuilder builder)
        {
            if (clause.Enabled)
            {
                if (clause.Count == 0)
                    builder.Append("DISTINCT ");
                else
                {
                    builder.Append("DISTINCT ON (");
                    {
                        for (var i = 0; i < clause.Count; i++)
                        {
                            if (i > 0)
                                builder.Append(", ");

                            Visit(clause[i], builder);
                        }
                    }
                    builder.Append(')');
                }
            }
        }
        protected internal virtual void VisitClause(SqlClauseColumns clause, SqlQueryBuilder builder)
        {
            if (clause.Count > 0)
            {
                builder.Append('(');
                for (var i = 0; i < clause.Count; i++)
                {
                    if (i > 0)
                        builder.Append(", ");

                    builder.Append(clause[i]);
                }
                builder.Append(')');
            }
        }
        protected internal virtual void VisitClause(SqlClauseJoin clause, SqlQueryBuilder builder)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < clause.Count; i++)
                VisitClauseEntry(clause[i], builder);
        }
        protected internal virtual void VisitClause(SqlClauseUnion clause, SqlQueryBuilder builder)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < clause.Count; i++)
                VisitClauseEntry(clause[i], builder);
        }

        protected internal virtual void VisitClauseEntry(SqlClauseWithEntry entry, SqlQueryBuilder builder)
        {
            // Name
            builder.Append(QuoteIdentifierIfNeeded(entry.Name));

            // Columns
            if (entry.Columns.Length > 0)
            {
                builder.Append('(');
                for (var i = 0; i < entry.Columns.Length; i++)
                {
                    if (i > 0)
                        builder.Append(", ");
                    builder.Append(entry.Columns[i]);
                }
                builder.Append(')');
            }

            // View
            builder.Append(" AS ");

            if (!(entry.Query is SqlQueryBase) && !(entry.Query is SqlExpressionQuery) || entry.RecursiveQuery != null)
                builder.Append('(');

            Visit(entry.Query, builder);

            if (entry.RecursiveQuery != null)
            {
                builder.AppendKeyword(entry.RecursiveUnionType == SqlClauseUnionEntryType.Default ? "UNION" : "UNION ALL");

                if (!(entry.RecursiveQuery is SqlQueryBase) && !(entry.RecursiveQuery is SqlExpressionQuery))
                    builder.Append(" )");

                Visit(entry.RecursiveQuery, builder);

                if (!(entry.RecursiveQuery is SqlQueryBase) && !(entry.RecursiveQuery is SqlExpressionQuery))
                    builder.Append(" )");
            }

            if (!(entry.Query is SqlQueryBase) && !(entry.Query is SqlExpressionQuery) || entry.RecursiveQuery != null)
                builder.Append(" )");
        }
        protected internal virtual void VisitClauseEntry(SqlClauseValuesEntry entry, SqlQueryBuilder builder, SqlClauseColumns columns)
        {
            builder.Append('(');
            for (var j = 0; j < entry.Values.Count; j++)
            {
                if (j > 0)
                    builder.Append(", ");

                if (columns != null && j < columns.Count)
                    builder.Append($"/*{columns[j]}*/ ");

                Visit(entry.Values[j], builder);
            }
            builder.Append(')');
        }
        protected internal virtual void VisitClauseEntry(SqlClauseFromEntry entry, SqlQueryBuilder builder)
        {
            var item = entry.Table;
            VisitTableDeclaration(builder, item, true);

            if (entry.ColumnNames.Length > 0)
            {
                builder.Append("(");

                for (var j = 0; j < entry.ColumnNames.Length; j++)
                {
                    if (j > 0)
                        builder.Append(", ");

                    builder.Append(QuoteIdentifierIfNeeded(entry.ColumnNames[j]));
                }

                builder.Append(")");
            }
        }
        protected internal virtual void VisitClauseEntry(SqlClauseOrderByEntry entry, SqlQueryBuilder builder)
        {
            Visit(entry.Expression, builder);

            if (entry.OrderType == SqlClauseOrderByEntryType.Descending)
                builder.Append(" DESC");

            if (entry.NullsOrderType == SqlClauseOrderByEntryNullsOrderType.NullsFirst)
                builder.Append(" NULLS FIRST");
            else if (entry.NullsOrderType == SqlClauseOrderByEntryNullsOrderType.NullsLast)
                builder.Append(" NULLS LAST");

        }
        protected internal virtual void VisitClauseEntry(SqlClauseSetEntry entry, SqlQueryBuilder builder)
        {
            builder.Append(QuoteIdentifierIfNeeded(entry.ColumnName));
            builder.Append(" = ");
            Visit(entry.Expression, builder);
        }
        protected internal virtual void VisitClauseEntry(SqlClauseJoinEntry entry, SqlQueryBuilder builder)
        {
            // Type
            switch (entry.JoinType)
            {
                case SqlClauseJoinEntryType.OuterJoin:
                    builder.AppendKeyword("FULL JOIN");
                    break;
                case SqlClauseJoinEntryType.LeftJoin:
                    builder.AppendKeyword("LEFT JOIN");
                    break;
                case SqlClauseJoinEntryType.RightJoin:
                    builder.AppendKeyword("RIGHT JOIN");
                    break;
                default:
                    builder.AppendKeyword("JOIN");
                    break;
            }

            // Table
            VisitTableDeclaration(builder, entry.Table, true);

            // Condition
            switch (entry.ConditionType)
            {
                case SqlClauseJoinEntryConditionType.Using:
                    builder.Append(" USING (");
                    Visit(entry.Condition, builder);
                    builder.Append(")");
                    break;

                case SqlClauseJoinEntryConditionType.On:
                    builder.Append(" ON (");
                    Visit(entry.Condition, builder);
                    builder.Append(")");
                    break;

                default:
                    throw new NotSupportedException($"Condition join type {entry.ConditionType} is not supported.");
            }
        }
        protected internal virtual void VisitClauseEntry(SqlClauseUnionEntry entry, SqlQueryBuilder builder)
        {
            builder.AppendKeyword(entry.Type == SqlClauseUnionEntryType.Default ? "UNION" : "UNION ALL");
            Visit(entry.Select, builder);
        }

        protected virtual void VisitLimitOffset(int? limit, int? offset, SqlQueryBuilder builder)
        {
            if (limit.HasValue)
            {
                builder.Append(" LIMIT ");
                builder.Append(limit.Value.ToString());
            }

            if (offset.HasValue)
            {
                builder.Append(" OFFSET ");
                builder.Append(offset.Value.ToString());
            }
        }
        protected virtual void VisitTableClause(SqlTable table, string keyword, SqlQueryBuilder builder)
        {
            if (keyword != null)
                builder.AppendKeyword(keyword);

            VisitTableDeclaration(builder, table, false);
        }
        protected virtual void VisitTableDeclaration(SqlQueryBuilder builder, SqlTable table, bool allowExpressions, bool includeAlias = true)
        {
            if (table.Table is string name)
                builder.Append(QuoteTableNameIfNeeded(name));
            else if (!allowExpressions)
                throw new Exception("Can not delete/update/insert from/to query.");
            else
                Visit(table.Table, builder);

            if (!string.IsNullOrEmpty(table.Alias) && includeAlias)
            {
                builder.Append(" AS ");
                builder.Append(QuoteIdentifierIfNeeded(table.Alias));
            }
        }
        protected virtual void VisitExpressionWithAlias(SqlQueryBuilder builder, SqlExpressionWithAlias expressionWithAlias)
        {
            Visit(expressionWithAlias.Expression, builder);

            if (!string.IsNullOrEmpty(expressionWithAlias.Alias))
            {
                builder.Append(" AS ");
                builder.Append(QuoteIdentifierIfNeeded(expressionWithAlias.Alias));
            }
        }

        public virtual string ToParameterString(object parameter, bool force)
        {
            if (parameter == null)
                return NullLiteral;

            var type = parameter.GetType();
            var code = DataHelper.GetTypeCode(type);

            if (force)
            {
                if (parameter is SqlExpression sqlExpression)
                    return ToExpressionParameterString(sqlExpression);

                if (_customParameterResolvers.TryGetValue(type, out var resolver))
                {
                    var result = resolver(parameter);
                    return result is SqlExpression expression
                        ? ToExpressionParameterString(expression)
                        : (string)result;
                }
            }

            switch (code)
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return NullLiteral;

                case TypeCode.Char:
                    return $"'{parameter}'";

                case TypeCode.Boolean:
                    return (bool)parameter ? TrueLiteral : FalseLiteral;

                case TypeCode.DateTime:
                    return ToDateTimeParameterString((DateTime)parameter);

                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return ((IFormattable)parameter).ToString(null, CultureInfo.InvariantCulture);

                case TypeCode.String:
                    {
                        var sValue = (string)parameter;

                        if (sValue.Length == 0)
                            return "''";

                        if (force)
                            return ToEscapedString(sValue);

                        return null;
                    }

                case TypeCode.Object:
                    {
                        if (parameter is TimeSpan span)
                            return ToTimeSpanParameterString(span);

                        return force
                            ? ToEscapedString(Convert.ToString(parameter, CultureInfo.InvariantCulture))
                            : null;
                    }

                default:
                    return $"{parameter:D}";
            }
        }
        protected virtual string ToDateTimeParameterString(DateTime value)
        {
            return GetCastString($"{value:o}", GetTypeName(typeof(DateTime)));
        }
        protected virtual string ToTimeSpanParameterString(TimeSpan value)
        {
            return GetCastString($"{value:c}", GetTypeName(typeof(TimeSpan)));
        }
        protected virtual string ToEscapedString(string value)
        {
            return '\'' + value.Replace("'", "''") + '\'';
        }
        private string ToExpressionParameterString(SqlExpression expression)
        {
            var builder = SqlQueryBuilder.CreateToStringBuilder();
            Visit(expression, builder);
            return builder.Query;
        }

        public virtual string GetTypeName(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Boolean:
                    return "BOOL";

                case DbType.Byte:
                case DbType.SByte:
                case DbType.Int16:
                case DbType.UInt16:
                    return "INT2";

                case DbType.Int32:
                case DbType.UInt32:
                    return "INT4";

                case DbType.Int64:
                case DbType.UInt64:
                    return "INT8";

                case DbType.Single:
                    return "FLOAT4";

                case DbType.Double:
                    return "FLOAT8";

                case DbType.Decimal:
                case DbType.VarNumeric:
                    return "NUMERIC";

                case DbType.Currency:
                    return "MONEY";

                case DbType.DateTime:
                case DbType.DateTime2:
                    return "TIMESTAMP";

                case DbType.DateTimeOffset:
                    return "TIMESTAMPTZ";

                case DbType.Date:
                    return "DATE";

                case DbType.Time:
                    return "TIME";

                case DbType.Guid:
                    return "UUID";

                case DbType.Xml:
                    return "XML";

                case DbType.Binary:
                    return "BYTEA";

                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return "TEXT";

                default:
                    return "UNKNOWN";
            }
        }
        public virtual string GetTypeName(Type netType)
        {
            var nullableType = Nullable.GetUnderlyingType(netType);
            if (nullableType != null)
                netType = nullableType;

            switch (DataHelper.GetTypeCode(netType))
            {
                case TypeCode.Empty:
                case (TypeCode)2:
                    return "NULL";

                case TypeCode.Boolean:
                    return "BOOL";

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return "INT";

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return "INT8";

                case TypeCode.Single:
                    return "FLOAT4";
                case TypeCode.Double:
                    return "FLOAT8";
                case TypeCode.Decimal:
                    return "NUMERIC";

                case TypeCode.DateTime:
                    return "TIMESTAMP";

                case TypeCode.Char:
                    return "CHAR";
                case TypeCode.String:
                    return "TEXT";

                default:
                    {
                        if (netType == typeof(TimeSpan))
                            return "INTERVAL";

                        return "UNKNOWN";
                    }
            }
        }
        protected virtual string GetCastString(object value, string typeName)
        {
            return value is string
                ? $"CAST('{value}' AS {typeName})"
                : $"CAST(({value}) AS {typeName})";
        }
        protected virtual string GetBuildInFunctionName(string functionName)
        {
            return functionName;
        }

        protected virtual string GetOperatorTypeString(SqlExpressionUnaryOperatorType operatorType)
        {
            switch (operatorType)
            {
                case SqlExpressionUnaryOperatorType.Nagate: return "-";
                case SqlExpressionUnaryOperatorType.BitwiseComplement: return "~";
                default: throw new NotSupportedException($"Operator type {operatorType} is not supported.");
            }
        }
        protected virtual string GetOperatorTypeString(SqlExpressionBinaryOperatorType operatorType)
        {
            switch (operatorType)
            {
                case SqlExpressionBinaryOperatorType.Plus: return "+";
                case SqlExpressionBinaryOperatorType.Minus: return "-";
                case SqlExpressionBinaryOperatorType.Divide: return "/";
                case SqlExpressionBinaryOperatorType.Modulo: return "%";
                case SqlExpressionBinaryOperatorType.Multiply: return "*";
                case SqlExpressionBinaryOperatorType.BitwiseAnd: return "&";
                case SqlExpressionBinaryOperatorType.BitwiseOr: return "|";
                case SqlExpressionBinaryOperatorType.BitwiseXor: return "^";
                case SqlExpressionBinaryOperatorType.BitwiseLeftShift: return "<<";
                case SqlExpressionBinaryOperatorType.BitwiseRightShift: return ">>";
                case SqlExpressionBinaryOperatorType.Concat: return "||";
                default: throw new NotSupportedException($"Operator type {operatorType} is not supported.");
            }
        }
        protected virtual string GetOperatorTypeString(SqlConditionBinaryOperatorType operatorType)
        {
            switch (operatorType)
            {
                case SqlConditionBinaryOperatorType.Distinct: return " IS DISTINCT FROM ";
                case SqlConditionBinaryOperatorType.NotDistinct: return " IS NOT DISTINCT FROM ";
                case SqlConditionBinaryOperatorType.Equal: return "=";
                case SqlConditionBinaryOperatorType.NotEqual: return "<>";
                case SqlConditionBinaryOperatorType.Greater: return ">";
                case SqlConditionBinaryOperatorType.GreaterOrEqual: return ">=";
                case SqlConditionBinaryOperatorType.Less: return "<";
                case SqlConditionBinaryOperatorType.LessOrEqual: return "<=";
                case SqlConditionBinaryOperatorType.Like: return " LIKE ";
                default: throw new NotSupportedException($"Operator type {operatorType} is not supported.");
            }
        }

        internal static bool NeedQuoteIdentifier(string name)
        {
            // TODO verify
            return (name.Length > 0 && char.IsNumber(name[0]))
                   || name.Any(x => char.IsUpper(x) || (!char.IsLetterOrDigit(x) && !(x == '_' || x == '[' || x == ']')))
                   || (name.StartsWith("\"") && name.EndsWith("\""));
        }
        internal static string QuoteIdentifierIfNeeded(string name)
        {
            return NeedQuoteIdentifier(name)
                ? "\"" + name + "\""
                : name;
        }
        internal static string QuoteTableNameIfNeeded(string tableName)
        {
            var index = tableName.IndexOf('.');
            if (index == -1)
                return QuoteIdentifierIfNeeded(tableName);

            return QuoteIdentifierIfNeeded(tableName.Substring(0, index))
                   + "."
                   + QuoteIdentifierIfNeeded(tableName.Substring(index + 1));
        }

        public static string QuoteIdentifier(string identifier)
        {
            return "[" + identifier.Replace("]", "]]") + "]";
        }
    }
}
