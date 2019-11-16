using System;
using System.Linq.Expressions;
using TagBites.Sql;

namespace TagBites.DB.Entity.Linq
{
    internal class Context
    {
        public SqlQuerySelect Select { get; }
        public object Value { get; set; }
        public SqlExpression Expression { get; set; }
    }
    internal class EntityExpressionVisitor
    {

        public SqlQuerySelect Translate(Expression expression)
        {
            var context = Visit(new Context(), expression);
            return context.Select;
        }

        public Context Visit(Context context, Expression node)
        {
            switch (node)
            {
                case BinaryExpression expression: return VisitExpression(context, expression);
                case BlockExpression blockExpression: return VisitExpression(context, blockExpression);
                case ConditionalExpression conditionalExpression: return VisitExpression(context, conditionalExpression);
                case ConstantExpression constantExpression: return VisitExpression(context, constantExpression);
                //case DebugInfoExpression infoExpression: return VisitExpression(context, infoExpression);
                //case DefaultExpression expression: return  VisitExpression(context, expression);
                //case DynamicExpression expression: return VisitExpression(context, expression);
                //case GotoExpression expression: return VisitExpression(context, expression);
                //case IndexExpression expression: return VisitExpression(context, expression);
                //case InvocationExpression expression: return VisitExpression(context, expression);
                //case LabelExpression expression: return VisitExpression(context, expression);
                //case LambdaExpression expression: return VisitExpression(context, expression);
                //case ListInitExpression expression: return VisitExpression(context, expression);
                //case LoopExpression expression: return VisitExpression(context, expression);
                case MemberExpression expression: return VisitExpression(context, expression);
                case MemberInitExpression expression: return VisitExpression(context, expression);
                case MethodCallExpression expression: return VisitExpression(context, expression);
                //case NewArrayExpression expression: return  VisitExpression(context, expression); ;
                case NewExpression expression: return VisitExpression(context, expression);
                //case ParameterExpression expression: return VisitExpression(context, expression);
                //case RuntimeVariablesExpression expression: return VisitExpression(context, expression);
                //case SwitchExpression expression: return VisitExpression(context, expression);
                //case TryExpression expression: return VisitExpression(context, expression);
                //case TypeBinaryExpression expression: return VisitExpression(context, expression);
                case UnaryExpression expression: return VisitExpression(context, expression);
                default:
                    throw new NotSupportedException();
            }
        }

        public Context VisitExpression(Context context, BinaryExpression expression)
        {
            var left = Visit(context, expression.Left);
            var right = Visit(context, expression.Right);




            throw new NotImplementedException();
        }
        public Context VisitExpression(Context context, BlockExpression expression)
        {
            throw new NotImplementedException();
        }
        public Context VisitExpression(Context context, ConditionalExpression expression)
        {
            throw new NotImplementedException();
        }
        public Context VisitExpression(Context context, ConstantExpression expression)
        {
            throw new NotImplementedException();
        }
        public Context VisitExpression(Context context, MemberExpression expression)
        {
            throw new NotImplementedException();
        }
        //public SqlQuerySelect VisitExpression(Context context, MemberAssignment expression)
        //{
        //    throw new NotImplementedException();
        //}
        public Context VisitExpression(Context context, MemberInitExpression expression)
        {
            throw new NotImplementedException();
        }
        public Context VisitExpression(Context context, MethodCallExpression expression)
        {
            throw new NotImplementedException();
        }
        public Context VisitExpression(Context context, NewExpression expression)
        {
            throw new NotImplementedException();
        }
        public Context VisitExpression(Context context, UnaryExpression expression)
        {
            throw new NotImplementedException();
        }




        /*private string GetValue(object input)
    {
        var type = input.GetType();
        //if it is not simple value
        if (type.IsClass && type != typeof(string))
        {
            //proper order of selected names provided by means of Stack structure
            var fieldName = _fieldNames.Pop();
            var fieldInfo = type.GetField(fieldName);
            object value;
            if (fieldInfo != null)            
               //get instance of order    
                value = fieldInfo.GetValue(input);
            else
                //get value of "Customer" property on order
                value = type.GetProperty(fieldName).GetValue(input);
            return GetValue(value);
        }                    
        else
        {
            //our predefined _typeConverters
            if (_typeConverters.ContainsKey(type))
                return _typeConverters[type](input);
            else
            //rest types
                return input.ToString();
        }
    }*/
    }

    internal static class EntityVisitiorHelper
    {
        internal static SqlExpression GetBinaryExpression(ExpressionType type, SqlExpression leftObject, SqlExpression rightObject)
        {
            if (type == ExpressionType.AndAlso || type == ExpressionType.OrElse)
                return (SqlCondition)SqlExpression.Combine(
                    type == ExpressionType.AndAlso ? SqlConditionGroupOperatorType.And : SqlConditionGroupOperatorType.Or,
                    leftObject,
                    rightObject);
            if (type == ExpressionType.Equal && (rightObject == null || leftObject == null))
                return (SqlExpression.IsNull(rightObject ?? leftObject));
            if (type == ExpressionType.NotEqual && (rightObject == null || leftObject == null))
                return (SqlExpression.IsNotNull(rightObject ?? leftObject));

            var function = GetFunction(type);

            return function(SqlExpression.ToExpression(leftObject), SqlExpression.ToExpression(rightObject));
        }
        private static Func<SqlExpression, SqlExpression, SqlExpression> GetFunction(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Add: return SqlExpression.Plus;
                case ExpressionType.Subtract: return SqlExpression.Minus;
                case ExpressionType.Divide: return SqlExpression.Divide;
                case ExpressionType.Modulo: return SqlExpression.Modulo;
                case ExpressionType.Multiply: return SqlExpression.Multiply;
                case ExpressionType.And: return SqlExpression.BitwiseAnd;
                case ExpressionType.Or: return SqlExpression.BitwiseOr;
                case ExpressionType.ExclusiveOr: return SqlExpression.BitwiseXor;

                case ExpressionType.Equal: return SqlExpression.AreEquals;
                case ExpressionType.NotEqual: return SqlExpression.AreNotEquals;
                case ExpressionType.GreaterThan: return SqlExpression.IsGreater;
                case ExpressionType.GreaterThanOrEqual: return SqlExpression.IsGreaterOrEqual;
                case ExpressionType.LessThan: return SqlExpression.IsLess;
                case ExpressionType.LessThanOrEqual: return SqlExpression.IsLessOrEqual;
                //case ExpressionType.li: return SqlConditionBinaryOperatorType.Like;
                // TODO: BJ: Like operator
                default: throw new Exception($"Operator {type} not supported");
            }
        }
    }
}
