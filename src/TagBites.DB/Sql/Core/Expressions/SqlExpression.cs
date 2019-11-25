using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TagBites.Utils;

namespace TagBites.Sql
{
    public abstract class SqlExpression
    {
        public static readonly SqlExpression Null = new SqlLiteral("null");
        public static readonly SqlExpression One = Argument(1);
        public static readonly SqlExpression Zero = Argument(0);
        public static readonly SqlExpression EmptyString = Argument(string.Empty);
        public static readonly SqlCondition True = ToCondition(Argument(true));
        public static readonly SqlCondition False = ToCondition(Argument(false));

        public SqlExpression this[int index] => new SqlExpressionIndexerOperator(this, Argument(index));
        public SqlExpression this[SqlExpression index] => new SqlExpressionIndexerOperator(this, index ?? throw new ArgumentNullException(nameof(index)));

        internal SqlExpression()
        { }


        public SqlCondition ToCondition() => ToCondition(this);

        public abstract void Accept(ISqlExpressionVisitor visitor);
        protected internal abstract void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder);
        public override string ToString()
        {
            var builder = SqlQueryBuilder.CreateToStringBuilder();
            Accept(SqlQueryResolver.DefaultToStringResolver, builder);
            return builder.Query;
        }

        public static SqlExpression Argument(object argumentValue)
        {
            return argumentValue == null
                ? Null
                : new SqlArgument(argumentValue);
        }
        public static SqlExpression Column(SqlTable table, string columnName)
        {
            return new SqlColumn(table, columnName);
        }
        public static SqlExpression Column(string tableAlias, string columnName)
        {
            return new SqlLiteral($"{SqlQueryResolver.QuoteIdentifierIfNeeded(tableAlias)}.{SqlQueryResolver.QuoteIdentifierIfNeeded(columnName)}");
        }
        public static SqlExpression Function(string name)
        {
            return new SqlExpressionFunctionCall(name);
        }
        public static SqlExpression Function(string name, params SqlExpression[] args)
        {
            return new SqlExpressionFunctionCall(name, args);
        }
        public static SqlExpression Literal(string sqlLiteral)
        {
            return new SqlLiteral(sqlLiteral);
        }
        public static SqlExpression LiteralExpression(string sqlLiteralExpressionFormat, params SqlExpression[] args)
        {
            return new SqlLiteralExpression(sqlLiteralExpressionFormat, args);
        }

        public static SqlExpression Negate(SqlExpression operand)
        {
            return new SqlExpressionUnaryOperator(SqlExpressionUnaryOperatorType.Nagate, operand);
        }
        public static SqlExpression Minus(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.Minus, leftOperand, rightOperand);
        }
        public static SqlExpression Plus(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.Plus, leftOperand, rightOperand);
        }
        public static SqlExpression Multiply(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.Multiply, leftOperand, rightOperand);
        }
        public static SqlExpression Divide(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.Divide, leftOperand, rightOperand);
        }
        public static SqlExpression Modulo(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.Modulo, leftOperand, rightOperand);
        }
        public static SqlExpression BitwiseOr(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.BitwiseOr, leftOperand, rightOperand);
        }
        public static SqlExpression BitwiseAnd(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.BitwiseAnd, leftOperand, rightOperand);
        }
        public static SqlExpression BitwiseXor(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.BitwiseXor, leftOperand, rightOperand);
        }
        public static SqlExpression BitwiseLeftShift(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.BitwiseLeftShift, leftOperand, rightOperand);
        }
        public static SqlExpression BitwiseRightShift(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.BitwiseRightShift, leftOperand, rightOperand);
        }
        public static SqlExpression BitwiseLeftShift(SqlExpression leftOperand, int rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.BitwiseLeftShift, leftOperand, Argument(rightOperand));
        }
        public static SqlExpression BitwiseRightShift(SqlExpression leftOperand, int rightOperand)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.BitwiseRightShift, leftOperand, Argument(rightOperand));
        }
        public static SqlExpression BitwiseComplement(SqlExpression operand)
        {
            return new SqlExpressionUnaryOperator(SqlExpressionUnaryOperatorType.BitwiseComplement, operand);
        }
        public static SqlExpression FlagBit(SqlExpression integerExpression, int bitIndex)
        {
            return BitwiseAnd(BitwiseRightShift(integerExpression, Argument(bitIndex)), One);
        }
        public static SqlExpression FlagBitRange(SqlExpression integerExpression, int bitIndex, int bitCount)
        {
            var mask = 0;
            while (--bitCount >= 0)
                mask = (mask << 1) | 1;

            return BitwiseAnd(BitwiseRightShift(integerExpression, Argument(bitIndex)), Argument(mask));
        }
        public static SqlExpression FlagSetBit(SqlExpression integerExpression, int bitIndex, bool bitValue)
        {
            return bitValue
                ? BitwiseOr(integerExpression, BitwiseLeftShift(One, Argument(bitIndex)))
                : BitwiseAnd(integerExpression, BitwiseComplement(BitwiseLeftShift(One, Argument(bitIndex))));
        }
        public static SqlExpression FlagSetBit(SqlExpression integerExpression, int bitIndex, SqlExpression bitValueExpression)
        {
            var filter = ~(1 << bitIndex);
            var value = BitwiseAnd(bitValueExpression, One) << bitIndex;

            return BitwiseOr(BitwiseAnd(integerExpression, Argument(filter)), Argument(value));
        }
        public static SqlExpression FlagSetBitRange(SqlExpression integerExpression, int bitIndex, int bitCount, int bitRangeValue)
        {
            var mask = 0;
            while (--bitCount >= 0)
                mask = (mask << 1) | 1;

            var filter = ~(mask << bitIndex);
            var value = (bitRangeValue & mask) << bitIndex;

            return BitwiseOr(BitwiseAnd(integerExpression, Argument(filter)), Argument(value));
        }
        public static SqlExpression FlagSetBitRange(SqlExpression integerExpression, int bitIndex, int bitCount, SqlExpression bitRangeValueExpression)
        {
            var mask = 0;
            while (--bitCount >= 0)
                mask = (mask << 1) | 1;

            var filter = ~(mask << bitIndex);
            var value = BitwiseAnd(bitRangeValueExpression, Argument(mask)) << bitIndex;

            return BitwiseOr(BitwiseAnd(integerExpression, Argument(filter)), value);
        }
        public static SqlExpression Concat(SqlExpression operand1, SqlExpression operand2)
        {
            return new SqlExpressionBinaryOperator(SqlExpressionBinaryOperatorType.Concat, operand1, operand2);
        }
        public static SqlExpression Concat(params SqlExpression[] operands)
        {
            Guard.ArgumentPositive(operands.Length, nameof(operands.Length));

            var ex = operands[0];
            for (var i = 1; i < operands.Length; i++)
                ex = Concat(ex, operands[i]);

            return ex;
        }
        public static SqlExpression Cast(SqlExpression operand, Type netType)
        {
            return new SqlExpressionCastOperator(operand, netType);
        }
        public static SqlExpression Cast(SqlExpression operand, DbType dbType)
        {
            return new SqlExpressionCastOperator(operand, dbType);
        }
        public static SqlExpression Cast(SqlExpression operand, string dbTypeName)
        {
            return new SqlExpressionCastOperator(operand, dbTypeName);
        }
        public static SqlExpression Coalesce(SqlExpression operand1, SqlExpression operand2)
        {
            return Function("COALESCE", operand1, operand2);
        }
        public static SqlExpression Coalesce(params SqlExpression[] operands)
        {
            Guard.ArgumentNotNullOrEmptyWithNotNullItems(operands, nameof(operands));

            if (operands.Length == 1)
                return operands[0];

            return Function("COALESCE", operands);
        }
        public static SqlExpression Greatest(params SqlExpression[] operands)
        {
            Guard.ArgumentNotNullOrEmptyWithNotNullItems(operands, nameof(operands));

            if (operands.Length == 1)
                return operands[0];

            // TODO return special expression type (different null handling for databases)
            return Function("GREATEST", operands);
        }
        public static SqlExpression Least(params SqlExpression[] operands)
        {
            Guard.ArgumentNotNullOrEmptyWithNotNullItems(operands, nameof(operands));

            if (operands.Length == 1)
                return operands[0];

            // TODO return special expression type (different null handling for databases)
            return Function("LEAST", operands);
        }
        public static SqlExpression When(SqlCondition whenCondition, SqlExpression operandWhenTrue)
        {
            Guard.ArgumentNotNull(whenCondition, nameof(whenCondition));
            Guard.ArgumentNotNull(operandWhenTrue, nameof(operandWhenTrue));

            return LiteralExpression("CASE WHEN {0} THEN {1} END", whenCondition, operandWhenTrue);
        }
        public static SqlExpression When(SqlCondition whenCondition, SqlExpression operandWhenTrue, SqlExpression operandWhenFalse)
        {
            Guard.ArgumentNotNull(whenCondition, nameof(whenCondition));
            Guard.ArgumentNotNull(operandWhenTrue, nameof(operandWhenTrue));
            Guard.ArgumentNotNull(operandWhenFalse, nameof(operandWhenFalse));

            return LiteralExpression("CASE WHEN {0} THEN {1} ELSE {2} END", whenCondition, operandWhenTrue, operandWhenFalse);
        }
        public static SqlExpression WhenNotEqualsTo(SqlExpression resultOperand, SqlExpression notEqualsToOperand)
        {
            Guard.ArgumentNotNull(resultOperand, nameof(resultOperand));
            Guard.ArgumentNotNull(notEqualsToOperand, nameof(notEqualsToOperand));

            return Function("NULLIF", resultOperand, notEqualsToOperand);
        }

        public static SqlCondition LiteralCondition(string sqlLiteral)
        {
            return ToCondition(Literal(sqlLiteral));
        }
        public static SqlCondition LiteralConditionExpression(string sqlLiteralExpressionFormat, params SqlExpression[] args)
        {
            return ToCondition(LiteralExpression(sqlLiteralExpressionFormat, args));
        }
        public static SqlCondition Not(SqlCondition condition)
        {
            if (condition == null)
                return null;

            return new SqlConditionUnaryOperator(SqlConditionUnaryOperatorType.Not, condition);
        }
        public static SqlCondition IsNull(SqlExpression expression)
        {
            return new SqlConditionUnaryOperator(SqlConditionUnaryOperatorType.IsNull, expression);
        }
        public static SqlCondition IsNotNull(SqlExpression expression)
        {
            return new SqlConditionUnaryOperator(SqlConditionUnaryOperatorType.IsNotNull, expression);
        }
        public static SqlCondition Exists(SqlExpression expression)
        {
            return new SqlConditionUnaryOperator(SqlConditionUnaryOperatorType.Exists, expression);
        }
        public static SqlCondition NotExists(SqlExpression expression)
        {
            return new SqlConditionUnaryOperator(SqlConditionUnaryOperatorType.NotExists, expression);
        }

        public static SqlCondition AreDistinct(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.Distinct, leftOperand, rightOperand);
        }
        public static SqlCondition AreNotDistinct(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.NotDistinct, leftOperand, rightOperand);
        }
        public static SqlCondition AreEquals(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.Equal, leftOperand, rightOperand);
        }
        public static SqlCondition AreNotEquals(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.NotEqual, leftOperand, rightOperand);
        }
        public static SqlCondition AreBitRangeEquals(SqlExpression integerExpression, int bitIndex, int bitCount, int bitRangeValue)
        {
            return AreEquals(FlagBitRange(integerExpression, bitIndex, bitCount), Argument(bitRangeValue));
        }
        public static SqlCondition AreBitRangeNotEquals(SqlExpression integerExpression, int bitIndex, int bitCount, int bitRangeValue)
        {
            return AreNotEquals(FlagBitRange(integerExpression, bitIndex, bitCount), Argument(bitRangeValue));
        }
        public static SqlCondition IsGreater(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.Greater, leftOperand, rightOperand);
        }
        public static SqlCondition IsGreaterOrEqual(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.GreaterOrEqual, leftOperand, rightOperand);
        }
        public static SqlCondition IsLess(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.Less, leftOperand, rightOperand);
        }
        public static SqlCondition IsLessOrEqual(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.LessOrEqual, leftOperand, rightOperand);
        }
        public static SqlCondition IsLike(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.Like, leftOperand, rightOperand);
        }
        public static SqlCondition IsBitOn(SqlExpression integerExpression, int bitIndex)
        {
            return AreEquals(FlagBit(integerExpression, bitIndex), One);
        }
        public static SqlCondition IsBitOff(SqlExpression integerExpression, int bitIndex)
        {
            return AreEquals(FlagBit(integerExpression, bitIndex), Zero);
        }

        public static SqlCondition In(SqlExpression expression, int[] values)
        {
            return new SqlConditionInOperator(expression, values);
        }
        public static SqlCondition In(SqlExpression expression, params SqlExpression[] values)
        {
            return new SqlConditionInOperator(expression, values);
        }
        public static SqlCondition In(SqlExpression expression, IEnumerable<SqlExpression> values)
        {
            return new SqlConditionInOperator(expression, values.ToList());
        }
        public static SqlCondition IntervalIntersects(SqlExpression intervalAStart, SqlExpression intervalAEnd, SqlExpression intervalBStart, SqlExpression intervalBEnd)
        {
            return And(IsGreater(intervalAEnd, intervalBStart), IsLess(intervalAStart, intervalBEnd));
        }
        public static SqlCondition IntervalCompleteIntersects(SqlExpression intervalAStart, SqlExpression intervalAEnd, SqlExpression intervalBStart, SqlExpression intervalBEnd)
        {
            return And(IsGreaterOrEqual(intervalAEnd, intervalBStart), IsLessOrEqual(intervalAStart, intervalBEnd));
        }
        public static SqlCondition IntervalLeftCompleteIntersects(SqlExpression intervalAStart, SqlExpression intervalAEnd, SqlExpression intervalBStart, SqlExpression intervalBEnd)
        {
            return And(IsGreater(intervalAEnd, intervalBStart), IsLessOrEqual(intervalAStart, intervalBEnd));
        }
        public static SqlCondition Between(SqlExpression expression, SqlExpression min, SqlExpression max)
        {
            return And(IsLessOrEqual(min, expression), IsGreaterOrEqual(max, expression));
        }
        public static SqlCondition Like(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.Like, leftOperand, rightOperand);
        }
        public static SqlCondition Contains(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.Contains, leftOperand, rightOperand);
        }
        public static SqlCondition StartsWith(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.StartsWith, leftOperand, rightOperand);
        }
        public static SqlCondition EndsWith(SqlExpression leftOperand, SqlExpression rightOperand)
        {
            return new SqlConditionBinaryOperator(SqlConditionBinaryOperatorType.EndsWith, leftOperand, rightOperand);
        }

        public static SqlSearchFilerRule SearchFilterRule(string adjustFunctionName, SqlExpression expression)
        {
            return new SqlSearchFilerRule(expression, adjustFunctionName);
        }
        public static SqlCondition SearchFilter(string filter, IEnumerable<SqlSearchFilerRule> columns)
        {
            if (string.IsNullOrWhiteSpace(filter) || !columns.Any())
                return null;

            SqlCondition condition = null;

            foreach (var item in filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().OrderByDescending(x => x.Length))
            {
                var word = item;
                condition = And(
                    condition,
                    Or(columns.Select(
                        x => new SqlConditionExpression(string.IsNullOrEmpty(x.AdjustFunction)
                            ? LiteralExpression("{0} LIKE ('%' || {1} || '%')", Cast(x.ColumnExpression, typeof(string)), Argument(word))
                            : LiteralExpression(x.AdjustFunction + "({0}) LIKE ('%' || " + x.AdjustFunction + "({1}) || '%')", Cast(x.ColumnExpression, typeof(string)), Argument(word))))));
            }

            return condition;
        }

        public static SqlCondition And(SqlCondition leftOperand, SqlCondition rightOperand)
        {
            return Combine(SqlConditionGroupOperatorType.And, leftOperand, rightOperand);
        }
        public static SqlCondition And(params SqlCondition[] conditions)
        {
            return Combine(SqlConditionGroupOperatorType.And, (IEnumerable<SqlCondition>)conditions);
        }
        public static SqlCondition And(IEnumerable<SqlCondition> conditions)
        {
            return Combine(SqlConditionGroupOperatorType.And, conditions);
        }
        public static SqlCondition Or(SqlCondition leftOperand, SqlCondition rightOperand)
        {
            return Combine(SqlConditionGroupOperatorType.Or, leftOperand, rightOperand);
        }
        public static SqlCondition Or(params SqlCondition[] conditions)
        {
            return Combine(SqlConditionGroupOperatorType.Or, (IEnumerable<SqlCondition>)conditions);
        }
        public static SqlCondition Or(IEnumerable<SqlCondition> conditions)
        {
            return Combine(SqlConditionGroupOperatorType.Or, conditions);
        }
        private static SqlCondition Combine(SqlConditionGroupOperatorType operatorType, IEnumerable<SqlCondition> conditions)
        {
            SqlCondition condition = null;

            if (conditions != null)
                foreach (var item in conditions)
                    condition = Combine(operatorType, condition, item);

            return condition;
        }
        private static SqlCondition Combine(SqlConditionGroupOperatorType operatorType, SqlCondition left, SqlCondition right)
        {
            if (left == null)
                return right;

            if (right == null)
                return left;

            var groupLeft = left as SqlConditionGroupOperator;
            if (groupLeft != null && groupLeft.OperatorType != operatorType)
                groupLeft = null;

            var groupRight = right as SqlConditionGroupOperator;
            if (groupRight != null && groupRight.OperatorType != operatorType)
                groupRight = null;

            var operands = new SqlCondition[(groupLeft == null ? 1 : groupLeft.Operands.Count) + (groupRight == null ? 1 : groupRight.Operands.Count)];
            var operandsIndex = 0;

            if (groupLeft == null)
                operands[operandsIndex++] = left;
            else
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < groupLeft.Operands.Count; i++)
                    operands[operandsIndex++] = groupLeft.Operands[i];
            }

            if (groupRight == null)
                operands[operandsIndex] = right;
            else
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < groupRight.Operands.Count; i++)
                    operands[operandsIndex++] = groupRight.Operands[i];
            }

            return new SqlConditionGroupOperator(operatorType, operands);
        }

        internal static SqlExpression InternalLiteralExpression(string sqlLiteralExpressionFormat, params object[] args)
        {
            return new SqlLiteralExpression(sqlLiteralExpressionFormat, args.Select(ToExpression).ToArray());
        }
        internal static SqlExpression ToExpression(object value)
        {
            return value is SqlExpression expression ? expression : Argument(value);
        }
        internal static SqlExpression Function(string name, params object[] args)
        {
            return Function(name, args.Select(ToExpression).ToArray());
        }
        public static SqlCondition ToCondition(object value)
        {
            return value is SqlConditionExpression expression ? expression : new SqlConditionExpression(ToExpression(value));
        }
        internal static SqlCondition IsNull(object column)
        {
            return IsNull(ToExpression(column));
        }
        internal static SqlCondition IsNotNull(object column)
        {
            return IsNull(ToExpression(column));
        }
        internal static object Combine(SqlConditionGroupOperatorType operatorType, params object[] conditions)
        {
            return Combine(operatorType, (IEnumerable<object>)conditions);
        }
        internal static object Combine(SqlConditionGroupOperatorType operatorType, IEnumerable<object> conditions)
        {
            object condition = null;

            if (conditions != null)
                foreach (var item in conditions)
                    condition = Combine(operatorType, condition, item);

            return condition;
        }
        internal static object Combine(SqlConditionGroupOperatorType operatorType, object left, object right)
        {
            if (left == null)
                return right;

            if (right == null)
                return left;

            return Combine(operatorType, ToCondition(left), ToCondition(right));
        }

        [Obsolete("Please use LiteralExpression")]
        public static SqlExpression Expression(string sqlLiteralExpressionFormat, params object[] args)
        {
            return LiteralExpression(sqlLiteralExpressionFormat, args.Select(ToExpression).ToArray());
        }
        [Obsolete("Please use LiteralExpression")]
        public static SqlExpression ConditionExpression(string sqlLiteralExpressionFormat, params object[] args)
        {
            return LiteralConditionExpression(sqlLiteralExpressionFormat, args.Select(ToExpression).ToArray());
        }

        public static SqlExpression operator -(SqlExpression expression) { return Negate(expression); }
        public static SqlExpression operator ~(SqlExpression expression) { return BitwiseComplement(expression); }
        public static SqlExpression operator -(SqlExpression left, SqlExpression right) { return Minus(left, right); }
        public static SqlExpression operator +(SqlExpression left, SqlExpression right) { return Plus(left, right); }
        public static SqlExpression operator *(SqlExpression left, SqlExpression right) { return Multiply(left, right); }
        public static SqlExpression operator /(SqlExpression left, SqlExpression right) { return Divide(left, right); }
        public static SqlExpression operator %(SqlExpression left, SqlExpression right) { return Modulo(left, right); }
        public static SqlExpression operator |(SqlExpression left, SqlExpression right) { return BitwiseOr(left, right); }
        public static SqlExpression operator &(SqlExpression left, SqlExpression right) { return BitwiseAnd(left, right); }
        public static SqlExpression operator ^(SqlExpression left, SqlExpression right) { return BitwiseXor(left, right); }
        public static SqlExpression operator <<(SqlExpression left, int right) { return BitwiseLeftShift(left, Argument(right)); }
        public static SqlExpression operator >>(SqlExpression left, int right) { return BitwiseRightShift(left, Argument(right)); }

        public static SqlCondition operator <(SqlExpression left, SqlExpression right) { return IsLess(left, right); }
        public static SqlCondition operator <=(SqlExpression left, SqlExpression right) { return IsLessOrEqual(left, right); }
        public static SqlCondition operator >(SqlExpression left, SqlExpression right) { return IsGreater(left, right); }
        public static SqlCondition operator >=(SqlExpression left, SqlExpression right) { return IsGreaterOrEqual(left, right); }

        public static explicit operator SqlExpression(SqlQuerySelect select)
        {
            return new SqlExpressionSelect(select);
        }
    }
}
