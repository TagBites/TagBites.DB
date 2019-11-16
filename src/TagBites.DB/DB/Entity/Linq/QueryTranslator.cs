using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TagBites.Sql;
using TagBites.Utils;

namespace TagBites.DB.Entity
{
    internal class QueryTranslatorLocalExecutionInfo
    {
        public MethodInfo Method { get; }
        public Type LocalType { get; }
        public object[] Parameters { get; }

        public QueryTranslatorLocalExecutionInfo(MethodInfo method, Type type, object[] parameters)
        {
            Method = method;
            LocalType = type;
            Parameters = parameters;
        }
    }

    internal class QueryTranslator : ExpressionVisitor
    {
        #region Exceptions

        private const string ComparerNotSupported = "The overloads with IComparer<T> parameter are not supported, because apparently IComparer<T> has no equivalent SQL translation.";
        private const string EqualityComparerNotSupported = "The overloads with IEqualityComparer<T> parameter are not supported, because apparently IEqualityComparer<T> has no equivalent SQL translation.";
        private const string IndexesNotSupported = "The indexed overloads are not supported.";
        private const string MethodNotSupported = "The query operator '{0}' is not supported.";

        #endregion

        #region Private members

        private IList<SqlExpressionNode> m_sqlExpressionNodeList = new List<SqlExpressionNode>();
        private Stack<object> m_objectStack = new Stack<object>();
        private Stack<SqlQuerySelect> m_querySelectStack = new Stack<SqlQuerySelect>();

        #endregion

        #region Properties

        private QueryTranslatorLocalExecutionInfo m_localExecutionInfo;

        public QueryObjectInitializer Initializer { get; private set; }
        public QueryTranslatorLocalExecutionInfo LocalExecutionInfo
        {
            get => m_localExecutionInfo;
            set
            {
                if (m_localExecutionInfo != null)
                    throw new Exception($"{nameof(LocalExecutionInfo)} can be set only once");

                m_localExecutionInfo = value;
            }
        }

        private SqlQuerySelect CurrentSelect => m_querySelectStack.Peek();

        #endregion

        #region Public methods

        public SqlQuerySelect Translate(Expression expression)
        {
            Visit(expression);
            var querySelect = m_querySelectStack.Pop();
            var indexes = new HashSet<int>();
            if (Initializer?.Parameters != null)
                foreach (var item in Initializer.Parameters.Select(x => x.ColumnIndex))
                    indexes.Add(item);

            var columns = new List<SqlExpressionWithAlias>();
            for (int i = 0; i < querySelect.Select.Count; i++)
                if (indexes.Contains(i))
                    columns.Add(querySelect.Select[i]);
            if (columns.Count > 0)
            {
                querySelect.Select.Clear();
                querySelect.Select.AddRange(columns);
            }

            return querySelect;
        }

        #endregion

        #region Binary expression

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var currentNode = base.VisitBinary(node);

            var rightObject = GetObject();
            var leftObject = GetObject();
            var result = VisitBinaryInner(node.NodeType, leftObject, rightObject);
            PushObject(result);

            return currentNode;
        }
        private object VisitBinaryInner(ExpressionType type, object leftObject, object rightObject)
        {
            if (type == ExpressionType.AndAlso || type == ExpressionType.OrElse)
                return SqlExpression.Combine(
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
        private Func<SqlExpression, SqlExpression, SqlExpression> GetFunction(ExpressionType type)
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

        #endregion

        #region Constant expression

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var currentNode = base.VisitConstant(node);
            VisitConstantInner(node);
            return node;
        }
        private void VisitConstantInner(ConstantExpression node)
        {
            if (node.Value is IQueryable queryable)
            {
                var tableInfo = EntityTableInfo.GetTableByType(queryable.ElementType);
                m_querySelectStack.Push(new SqlQuerySelect());
                var expressioNode = new SqlExpressionNode()
                {
                    TableInfo = tableInfo,
                    TableClause = CurrentSelect.From.Add(tableInfo.TableFullName)
                };
                m_sqlExpressionNodeList.Add(expressioNode);
                var properties = tableInfo.Columns.Select(x => new QueryObjectProperty(GetOrAddSelectColumn(expressioNode.TableClause.Column(x.Name)), tableInfo.Type.GetRuntimeProperty(x.PropertyName), null));
                Initializer = new QueryObjectInitializer(tableInfo.Type, null, properties.ToArray());
            }
            else
                PushObject((node.Value));
        }

        #endregion

        #region Member expression

        protected override Expression VisitMember(MemberExpression node)
        {
            var currentNode = base.VisitMember(node);
            VisitMemberInner(node);
            return currentNode;
        }
        private void VisitMemberInner(MemberExpression node)
        {
            // Remote
            var sqlNode = GetOrCreateSqlExpressionNode(node);
            if (sqlNode != null)
                return;

            //// Select remote
            //if (Initializer != null)
            //{
            //    var property = CollectionHelper.GetRecursive(Initializer.Properties, x => x.Initializer?.Properties).FirstOrDefault(t => t.PropertyInfo == node.Member);
            //    if (property != null)
            //    {
            //        m_stack.Push(property);
            //        return;
            //    }

            //    var parameter = CollectionHelper.GetRecursive(Initializer.Parameters, x => x.Initializer?.Parameters).FirstOrDefault(t => t.ParameterInfo.Name == node.Member.Name);
            //    if (parameter != null)
            //    {
            //        m_stack.Push(parameter);
            //        return;
            //    }
            //}

            // Local
            if (node.Expression != null)
                m_objectStack.Pop();
            var newValue = Expression.Lambda(node).Compile().DynamicInvoke();
            PushObject((newValue));
        }

        #endregion

        #region Member assignment expression

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var current = base.VisitMemberAssignment(node);
            VisitMemberAssignmentInner(node);
            return current;
        }
        private void VisitMemberAssignmentInner(MemberAssignment node)
        {
            PushObject((CreateProperty(node.Member as PropertyInfo, m_objectStack.Pop())));
        }

        #endregion

        #region Member init expression

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var current = base.VisitMemberInit(node);
            VisitMemberInitInner(node);
            return current;
        }
        private void VisitMemberInitInner(MemberInitExpression node)
        {
            PushObject((CreateInitializer(node.Type, node.NewExpression.Arguments.Count, node.Bindings.Count)));
        }

        #endregion

        #region Method call expression

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var currentNode = base.VisitMethodCall(node);
            VisitMethodCallInner(node);
            return currentNode;
        }
        private void VisitMethodCallInner(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable))
                VisitQueryableMethodCallInner(node);
            else if ((node.Method.DeclaringType == typeof(string)))
                PushObject((VisitStringRemoteMethod(node.Method.Name, GetStackObject(node.Arguments.Count))));
            else if ((node.Method.DeclaringType == typeof(Math)))
                PushObject((VisitMathRemoteMethod(node.Method.Name, GetStackObject(node.Arguments.Count))));
            else if ((node.Method.DeclaringType == typeof(DateTime)))
                PushObject((VisitDatetimeRemoteMethod(node.Method.Name, GetStackObject(node.Arguments.Count))));
            else if ((node.Method.DeclaringType == typeof(TimeSpan)))
                PushObject((VisitTimeSpanRemoteMethod(node.Method.Name, GetStackObject(node.Arguments.Count))));
            else
                PushObject((VisitLocalMethod(node.Method.Name, GetStackObject(node.Arguments.Count))));
        }
        private void VisitQueryableMethodCallInner(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                //
                // Generation
                //
                case nameof(Queryable.DefaultIfEmpty):
                    SetLocalExecutionInfo(node, node.Arguments.Count - 1);
                    break;
                //
                // Filtering
                //
                case nameof(Queryable.Where):
                    if (((LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand).Parameters.Count == 2)
                        throw new NotSupportedException(IndexesNotSupported);

                    CurrentSelect.Where.Add(GetObject<SqlCondition>());
                    break;
                case nameof(Queryable.OfType):
                    throw new Exception("not implemented");
                //
                // Mapping
                //
                case nameof(Queryable.Select):
                    if (((LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand).Parameters.Count == 2)
                        throw new NotSupportedException(IndexesNotSupported);

                    if (m_objectStack.Peek() is QueryObjectInitializer)
                        Initializer = m_objectStack.Pop() as QueryObjectInitializer;
                    else
                    {
                        var element = GetObject<SqlExpression>();
                        var propertyInfo = PropertyUtils.GetProperty((LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand);
                        var property = CreateProperty(propertyInfo, element);
                        Initializer = new QueryObjectInitializer(propertyInfo.PropertyType, null, new QueryObjectProperty[] { property });
                    }

                    break;
                //
                // Join
                //
                case nameof(Queryable.Join):
                case nameof(Queryable.GroupJoin):
                    throw new Exception("not implemented");
                case nameof(Queryable.SelectMany):
                    throw new Exception("not implemented");
                //
                // Concatenation
                //
                case "Concat":
                    var concatSecondQuery = m_querySelectStack.Pop();
                    var concatFirstQuery = m_querySelectStack.Pop();
                    concatFirstQuery.Union.Add(concatSecondQuery, SqlClauseUnionEntryType.All);
                    var concatQuery = new SqlQuerySelect();
                    concatQuery.From.Add(concatFirstQuery);
                    concatQuery.Select.AddAll();
                    m_querySelectStack.Push(concatQuery);
                    break;
                //
                // Set
                //
                case nameof(Queryable.Distinct):
                    if (node.Arguments.Count > 1)
                        throw new NotSupportedException(EqualityComparerNotSupported);

                    CurrentSelect.Distinct.Enabled = true;
                    break;
                case nameof(Queryable.GroupBy):
                    CurrentSelect.OrderBy.Add(GetObject<SqlExpression>(), GetSqlClauseOrderByEntryType(false));
                    SetLocalExecutionInfo(node, node.Arguments.Count - 1);
                    break;
                //throw new Exception("not implemented");
                case nameof(Queryable.Union):
                case nameof(Queryable.Intersect):
                case nameof(Queryable.Except):
                    SetLocalExecutionInfo(node, node.Arguments.Count - 1);
                    break;
                //
                // Convolutution
                //
                case nameof(Queryable.Zip):
                    throw new NotSupportedException(string.Format(MethodNotSupported, node.Method.Name));
                //
                // Partitioning
                //
                case nameof(Queryable.Take):
                    CurrentSelect.Limit = m_objectStack.Pop() as int?;
                    break;
                case nameof(Queryable.Skip):
                    CurrentSelect.Offset = m_objectStack.Pop() as int?;
                    break;
                case nameof(Queryable.TakeWhile):
                case nameof(Queryable.SkipWhile):
                    throw new NotSupportedException(string.Format(MethodNotSupported, node.Method.Name));
                //
                // Ordering
                //
                case nameof(Queryable.OrderBy):
                    if (node.Arguments.Count > 2)
                        throw new NotSupportedException(ComparerNotSupported);

                    CurrentSelect.OrderBy.Insert(0, GetObject<SqlExpression>(), GetSqlClauseOrderByEntryType(false));
                    break;
                case nameof(Queryable.ThenBy):
                    if (node.Arguments.Count > 2)
                        throw new NotSupportedException(ComparerNotSupported);

                    CurrentSelect.OrderBy.Add(GetObject<SqlExpression>(), GetSqlClauseOrderByEntryType(false));
                    break;
                case nameof(Queryable.OrderByDescending):
                    if (node.Arguments.Count > 2)
                        throw new NotSupportedException(ComparerNotSupported);

                    CurrentSelect.OrderBy.Insert(0, GetObject<SqlExpression>(), GetSqlClauseOrderByEntryType(true));
                    break;
                case nameof(Queryable.ThenByDescending):
                    if (node.Arguments.Count > 2)
                        throw new NotSupportedException(ComparerNotSupported);

                    CurrentSelect.OrderBy.Add(GetObject<SqlExpression>(), GetSqlClauseOrderByEntryType(true));
                    break;
                case nameof(Queryable.Reverse):
                    throw new NotSupportedException(string.Format(MethodNotSupported, node.Method.Name));
                //
                // Conversion
                //
                case nameof(Queryable.Cast):
                    throw new Exception("Not implemented");
                case nameof(Queryable.AsQueryable):
                    throw new Exception("Not implemented");
                //
                // Element
                //
                case nameof(Queryable.First):
                case nameof(Queryable.FirstOrDefault):
                    if (node.Arguments.Count == 2)
                        CurrentSelect.Where.Add(GetObject<SqlCondition>());

                    CurrentSelect.Limit = 1;
                    SetLocalExecutionInfo(node);
                    break;
                case nameof(Queryable.Last):
                case nameof(Queryable.LastOrDefault):
                case nameof(Queryable.ElementAt):
                case nameof(Queryable.ElementAtOrDefault):
                    throw new NotSupportedException(string.Format(MethodNotSupported, node.Method.Name));
                case nameof(Queryable.Single):
                case nameof(Queryable.SingleOrDefault):
                    if (node.Arguments.Count == 2)
                        CurrentSelect.Where.Add(GetObject<SqlCondition>());

                    CurrentSelect.Limit = 2;
                    SetLocalExecutionInfo(node);
                    break;
                //
                // Aggregation
                //
                case nameof(Queryable.Aggregate):
                    throw new NotSupportedException(string.Format(MethodNotSupported, node.Method.Name));
                case nameof(Queryable.Count):
                case nameof(Queryable.LongCount):
                    var countPrevoiusQuery = m_querySelectStack.Pop();
                    if (node.Arguments.Count == 2)
                        countPrevoiusQuery.Where.Add(GetObject<SqlCondition>());

                    var countCurrentQuery = new SqlQuerySelect();
                    countCurrentQuery.From.Add(countPrevoiusQuery);
                    countCurrentQuery.Select.Add(SqlExpression.Literal("COUNT(*)"));
                    m_querySelectStack.Push(countCurrentQuery);
                    break;
                case nameof(Queryable.Min):
                    if (node.Arguments.Count != 2)
                        throw new NotSupportedException("Min method has to have argument");

                    CurrentSelect.Select.Clear();
                    CurrentSelect.Select.Add(SqlFunction.Min(SqlExpression.Argument(m_objectStack.Pop())));
                    break;
                case nameof(Queryable.Max):
                    if (node.Arguments.Count != 2)
                        throw new NotSupportedException("Max method has to have argument");

                    CurrentSelect.Select.Clear();
                    CurrentSelect.Select.Add(SqlFunction.Max(SqlExpression.Argument(m_objectStack.Pop())));
                    break;
                case nameof(Queryable.Sum):
                    CurrentSelect.Select.Clear();
                    CurrentSelect.Select.Add(SqlFunction.Sum(SqlExpression.Argument(m_objectStack.Pop())));
                    break;
                case nameof(Queryable.Average):
                    CurrentSelect.Select.Clear();
                    CurrentSelect.Select.Add(SqlFunction.Avg(SqlExpression.Argument(m_objectStack.Pop())));
                    break;
                // 
                // Qualifier
                //
                case nameof(Queryable.Any):
                    var anypreviousQuery = m_querySelectStack.Pop();
                    anypreviousQuery.Select.Clear();
                    anypreviousQuery.Select.Add(SqlExpression.Argument(1));
                    if (node.Arguments.Count == 2)
                        anypreviousQuery.Where.Add(GetObject<SqlCondition>());
                    Initializer = null;

                    var anyQuery = new SqlQuerySelect();
                    anyQuery.Select.Add(SqlExpression.Exists((SqlExpression)anypreviousQuery));
                    m_querySelectStack.Push(anyQuery);
                    break;
                case nameof(Queryable.All):
                    var allpreviousQuery = m_querySelectStack.Pop();
                    allpreviousQuery.Select.Clear();
                    allpreviousQuery.Select.Add(SqlExpression.Argument(1));
                    allpreviousQuery.Where.Add(SqlExpression.Not(GetObject<SqlCondition>()));
                    Initializer = null;

                    var allQuery = new SqlQuerySelect();
                    allQuery.Select.Add(SqlExpression.NotExists((SqlExpression)allpreviousQuery));
                    m_querySelectStack.Push(allQuery);
                    break;
                case nameof(Queryable.Contains):
                    if (node.Arguments.Count > 2)
                        throw new NotSupportedException(EqualityComparerNotSupported);

                    if (node.Arguments[1].Type.GetTypeInfo().IsPrimitive || node.Arguments[1].Type.Equals(typeof(string)))
                    {
                        var containsPreviousQuery = m_querySelectStack.Pop();
                        var column = containsPreviousQuery.Select[Initializer.Properties.FirstOrDefault().ColumnIndex];
                        containsPreviousQuery.Select.Clear();
                        containsPreviousQuery.Select.Add(SqlExpression.Argument(1));
                        containsPreviousQuery.Where.AddEquals(column, SqlExpression.Argument(m_objectStack.Pop()));
                        Initializer = null;

                        var containsQuery = new SqlQuerySelect();
                        containsQuery.Select.Add(SqlExpression.Exists((SqlExpression)containsPreviousQuery));
                        m_querySelectStack.Push(containsQuery);
                    }
                    else
                        SetLocalExecutionInfo(node);

                    break;
                //
                // Equality
                //
                case nameof(Queryable.SequenceEqual):
                    throw new NotSupportedException(string.Format(MethodNotSupported, node.Method.Name));
                default:
                    throw new NotSupportedException(string.Format(MethodNotSupported, node.Method.Name));
            }
        }
        private void SetLocalExecutionInfo(MethodCallExpression expression, int parameterCount = 0)
        {
            // Generic types
            var genericTypes = new List<Type>();
            genericTypes.Add(CollectionHelper.GetItemType(GetLocalMethodArguments(expression.Arguments[0])));
            foreach (var item in expression.Type.GenericTypeArguments)
                if (item != genericTypes.First())
                    genericTypes.Add(item);

            // Method info
            var methodInfo = typeof(Enumerable)
                .GetTypeInfo()
                .GetDeclaredMethods(expression.Method.Name)
                .FirstOrDefault(x => x.GetParameters().Length == parameterCount + 1)
                ?.MakeGenericMethod(genericTypes.ToArray());
            if (methodInfo == null)
                throw new Exception($"Local method for {expression.Method.Name} does not exist.");

            var arguments = expression.Arguments.Skip(1).Take(parameterCount)
                .Select(GetLocalMethodArguments)
                .ToArray();

            for (int i = 0; i < expression.Type.GenericTypeArguments.Length; i++)
                if (expression.Type.GenericTypeArguments[i] != arguments[i]?.GetType())
                    throw new Exception();

            LocalExecutionInfo = new QueryTranslatorLocalExecutionInfo(methodInfo, genericTypes.First(), arguments);
        }
        private object GetLocalMethodArguments(Expression expression)
        {
            switch (expression)
            {
                case ConstantExpression constant:
                    return constant.Value;
                case UnaryExpression unary:
                    return unary.Operand;
                default:
                    break;
            }

            throw new NotImplementedException();
        }

        private SqlLiteralExpression VisitLocalMethod(string methodName, object[] args)
        {
            throw new NotImplementedException();
        }
        private SqlExpression VisitStringRemoteMethod(string methodName, object[] args)
        {
            switch (methodName)
            {
                case "CompareTo":
                    return SqlExpression.InternalLiteralExpression("{0} = {1}", args);
                case "Concat":
                    throw new NotImplementedException();
                case "Contains":
                    return SqlExpression.InternalLiteralExpression("{0} LIKE %{1}%", args);
                case "EndsWith":
                    return SqlExpression.InternalLiteralExpression("{0} LIKE %{1}", args);
                case "Equals":
                    return SqlExpression.InternalLiteralExpression("{0} = {1}", args);
                case "IndexOf":
                    return SqlExpression.InternalLiteralExpression("position({0})", args);
                case "Insert":
                    throw new NotImplementedException();
                case "LastIndexOf":
                case "Length":
                    return SqlExpression.InternalLiteralExpression("length({0})", args);
                case "PadLeft":
                    return SqlLiteralExpression.Function("lpad", args);
                case "PadRight":
                    return SqlLiteralExpression.Function("rpad", args);
                case "Remove":
                    throw new NotImplementedException();
                case "Replace":
                    return SqlExpression.InternalLiteralExpression("replace({0}, {0}, {1})", args);
                case "StartsWith":
                    return SqlExpression.InternalLiteralExpression("{0} LIKE {1}%", args);
                case "Substring":
                    return SqlExpression.InternalLiteralExpression("substr({0}, {1}, {2})", args);
                case "ToLower":
                    return SqlExpression.InternalLiteralExpression("lower({0})", args);
                case "ToUpper":
                    return SqlExpression.InternalLiteralExpression("upper({0})", args);
                case "Trim":
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
        private SqlExpression VisitMathRemoteMethod(string methodName, object[] args)
        {
            switch (methodName)
            {
                case "Abs":
                    return SqlExpression.InternalLiteralExpression("abs({0})", args);
                case "Acos":
                    return SqlExpression.InternalLiteralExpression("acos({0})", args);
                case "Asin":
                    return SqlExpression.InternalLiteralExpression("asin({0})", args);
                case "Atan":
                    return SqlExpression.InternalLiteralExpression("atan({0})", args);
                case "Atan2":
                    return SqlExpression.InternalLiteralExpression("atan2({0}, {1})", args);
                case "Ceiling":
                    return SqlExpression.InternalLiteralExpression("ceil({0})", args);
                case "Cos":
                    return SqlExpression.InternalLiteralExpression("cos({0})", args);
                case "Exp":
                    return SqlExpression.InternalLiteralExpression("exp({0})", args);
                case "Floor":
                    return SqlExpression.InternalLiteralExpression("floor({0})", args);
                case "Log":
                    return args.Length == 2
                        ? SqlExpression.InternalLiteralExpression("log({1}, {0})", args)
                        : SqlExpression.InternalLiteralExpression("ln({0})", args); ;
                case "Log10":
                    return SqlExpression.InternalLiteralExpression("log({0})", args);
                case "Pow":
                    return SqlExpression.InternalLiteralExpression("power({0},{1})", args);
                case "Round":
                    return SqlExpression.InternalLiteralExpression("round({0})", args);
                case "Sign":
                    return SqlExpression.InternalLiteralExpression("sign({0})", args);
                case "Sin":
                    return SqlExpression.InternalLiteralExpression("sin({0})", args);
                case "Sqrt":
                    return SqlExpression.InternalLiteralExpression("sqrt({0})", args);
                case "Tan":
                    return SqlExpression.InternalLiteralExpression("tan({0})", args);
                case "Truncate":
                    return SqlExpression.InternalLiteralExpression("trunc({0})", args);
                default:
                    throw new NotImplementedException();
            }
        }
        private SqlExpression VisitTimeSpanRemoteMethod(string methodName, object[] args)
        {
            switch (methodName)
            {
                case "Add":
                case "Subtract":
                case "Duration":
                case "Negate":
                case "Compare":
                    return SqlExpression.AreEquals(SqlExpression.ToExpression(args[0]), SqlExpression.ToExpression(args[1]));
                default:
                    throw new NotImplementedException();
            }
        }
        private SqlLiteralExpression VisitDatetimeRemoteMethod(string methodName, object[] args)
        {
            switch (methodName)
            {
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region New expression

        protected override Expression VisitNew(NewExpression node)
        {
            var currentNode = base.VisitNew(node);
            VisitNewInner(node);
            return currentNode;
        }
        private void VisitNewInner(NewExpression node)
        {
            if (node.Type == typeof(DateTime))
            {
                if (node.Arguments.Count == 3)
                    PushObject((SqlExpression.Function("make_date", GetStackObjectAndCkeckTypes(node.Arguments.Count, typeof(int), typeof(int), typeof(int)))));
                else if (node.Arguments.Count == 6)
                    PushObject((SqlExpression.Function("make_timestamp", GetStackObjectAndCkeckTypes(node.Arguments.Count, typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int)))));
                else
                    throw new NotSupportedException();
                // TODO: BJ: add 7 arguments, miliseconds
            }
            else if (node.Type == typeof(TimeSpan))
            {
                if (node.Arguments.Count == 0)
                    PushObject((SqlExpression.Literal("make_interval()")));
                else if (node.Arguments.Count == 1)
                    PushObject((SqlExpression.Function("make_interval", GetStackObjectAndCkeckTypes(node.Arguments.Count, typeof(long)))));
                else if (node.Arguments.Count == 3)
                    PushObject((SqlExpression.Function("make_interval", GetStackObjectAndCkeckTypes(node.Arguments.Count, typeof(int), typeof(int), typeof(int)))));
                else if (node.Arguments.Count == 4)
                    PushObject((SqlExpression.Function("make_interval", GetStackObjectAndCkeckTypes(node.Arguments.Count, typeof(int), typeof(int), typeof(int), typeof(int)))));
                else if (node.Arguments.Count == 5)
                    PushObject((SqlExpression.Function("make_interval", GetStackObjectAndCkeckTypes(node.Arguments.Count, typeof(int), typeof(int), typeof(int), typeof(int), typeof(int)))));
                else
                    throw new NotSupportedException();
                // TODO: BJ: add 7 arguments, miliseconds
            }
            else
            {
                var parameters = GetParameterInfos(node.Type, node.Arguments.Select(x => x.Type).ToArray());
                var objects = GetStackObject(node.Arguments.Count);

                for (int i = 0; i < objects.Length; i++)
                    PushObject((CreateParameter(parameters[i], objects[i])));

                if (TypeUtils.IsAnonymousType(node.Type))
                    PushObject((CreateInitializer(node.Type, parameters.Length, 0)));
            }
        }

        #endregion

        #region Unary expression

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var currentNode = base.VisitUnary(node);
            if (node.NodeType != ExpressionType.Quote)
                VisitUnaryInner(node.NodeType);
            return currentNode;
        }
        private void VisitUnaryInner(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Not:
                    {
                        var element = m_objectStack.Pop();
                        PushObject((SqlExpression.Not(element as SqlCondition)));
                        break;
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        #region Helpers

        private T GetObject<T>() => (T)m_objectStack.Pop();
        private object GetObject() => m_objectStack.Pop();
        private void PushObject(object argument)
        {
            m_objectStack.Push(argument);
        }

        private SqlClauseOrderByEntryType GetSqlClauseOrderByEntryType(bool descending)
        {
            return descending
                ? SqlClauseOrderByEntryType.Descending
                : SqlClauseOrderByEntryType.Ascending;
        }

        private SqlExpressionNode GetOrCreateNode(Type type)
        {
            for (int i = 0; i < m_sqlExpressionNodeList.Count; i++)
                if (m_sqlExpressionNodeList[i].TableInfo.Type == type)
                    return m_sqlExpressionNodeList[i];

            return m_sqlExpressionNodeList.Last();
        }
        private SqlExpressionNode GetOrCreateSqlExpressionNode(MemberExpression expression)
        {
            SqlExpressionNode parent = null;
            if (expression.Expression is ParameterExpression)
                parent = GetOrCreateNode(expression.Expression.Type);
            else if (expression.Expression is MemberExpression)
                parent = GetOrCreateSqlExpressionNode((MemberExpression)expression.Expression);
            else
                return null;

            if (parent == null)
                return null;

            var memberName = expression.Member.Name;
            var column = parent.TableInfo.GetColumnByPropertyName(memberName);
            if (column != null)
            {
                var sqlColumn = new SqlColumn(parent.TableClause, column.Name);
                PushObject((sqlColumn));

                return parent;
            }

            throw new Exception();

            //// Existing ForeignKey
            //for (int i = 0; i < m_sqlExpressionNodeList.Count; i++)
            //    if (m_sqlExpressionNodeList[i].ParentNode == parent && m_sqlExpressionNodeList[i].ParentMember == memberName)
            //        return m_sqlExpressionNodeList[i];

            //// New ForeignKey
            //var foreignKey = parent.TableInfo.GetForeignKeyPropertyName(memberName);
            //if (foreignKey == null)
            //    throw new NotSupportedException();

            //var tableInfo = EntityTableInfo.GetTableByType(expression.Type);
            //var joinTable = m_sqlExpressionNodeList.Last().QueryBuilder.Join.AddOn(SqlClauseJoinEntryType.LeftJoin,
            //                foreignKey.TableInfo.TableFullName,
            //                foreignKey.TableInfo.PrimaryKey[0].Name,
            //                parent.TableClause,
            //                foreignKey.Column.Name);

            //var expressioNode = new SqlExpressionNode()
            //{
            //    ParentNode = parent,
            //    ParentMember = memberName,
            //    TableInfo = tableInfo,
            //    TableClause = joinTable
            //};
            //m_sqlExpressionNodeList.Add(expressioNode);

            //return expressioNode;
        }

        private int GetOrAddSelectColumn(SqlColumn column)
        {
            var columnIndex = CurrentSelect.Select.IndexOf(column);
            if (columnIndex != -1)
                return columnIndex;

            CurrentSelect.Select.Add(column, null);
            return CurrentSelect.Select.Count - 1;
        }
        private int GetOrAddSelectExpression(SqlExpression expression)
        {
            var columnIndex = CurrentSelect.Select.IndexOf(expression);
            if (columnIndex != -1)
                return columnIndex;

            CurrentSelect.Select.Add(expression);
            return CurrentSelect.Select.Count - 1;
        }

        private object[] GetStackObject(int count)
        {
            var argumentArray = new object[count];
            for (int i = count - 1; i >= 0; i--)
                argumentArray[i] = m_objectStack.Pop();

            return argumentArray;
        }
        private object[] GetStackObjectAndCkeckTypes(int count, params Type[] types)
        {
            var argumentArray = new object[count];
            for (int i = count - 1; i >= 0; i--)
            {
                var value = m_objectStack.Pop();
                if (types[i] != value.GetType())
                    throw new NotSupportedException();

                argumentArray[i] = value;
            }

            return argumentArray;
        }

        private ParameterInfo[] GetParameterInfos(Type type, Type[] argumentTypes)
        {
            var constructor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.GetParameters().Select(y => y.ParameterType).SequenceEqual(argumentTypes));
            if (constructor == null)
                throw new Exception();

            return constructor.GetParameters();
        }
        private QueryObjectInitializer CreateInitializer(Type type, int parameterCount, int membersCount)
        {
            var members = GetStackObject(membersCount).Cast<QueryObjectProperty>().ToArray();
            var parameters = GetStackObject(parameterCount).Cast<QueryObjectParameter>().ToArray();
            return new QueryObjectInitializer(type, parameters, members);
        }
        private QueryObjectParameter CreateParameter(ParameterInfo paremeterInfo, object value)
        {
            Guard.ArgumentNotNull(paremeterInfo, nameof(paremeterInfo));

            if (value is SqlLiteralExpression)
                return new QueryObjectParameter(GetOrAddSelectExpression(value as SqlLiteralExpression), paremeterInfo, null);
            else if (value is SqlColumn)
                return new QueryObjectParameter(GetOrAddSelectColumn(value as SqlColumn), paremeterInfo, null);
            else
                return new QueryObjectParameter(value, paremeterInfo, null);
        }
        private QueryObjectProperty CreateProperty(PropertyInfo propertyInfo, object value)
        {
            Guard.ArgumentNotNull(propertyInfo, nameof(propertyInfo));

            if (value is SqlLiteralExpression)
                return new QueryObjectProperty(GetOrAddSelectExpression(value as SqlLiteralExpression), propertyInfo, null);
            else if (value is SqlColumn)
                return new QueryObjectProperty(GetOrAddSelectColumn(value as SqlColumn), propertyInfo, null);
            else if (value is QueryObjectInitializer)
                return new QueryObjectProperty(null, propertyInfo, value as QueryObjectInitializer);
            else
                return new QueryObjectProperty(value, propertyInfo, null);
        }

        #endregion

        #region Classes

        private class SqlExpressionNode
        {
            //public SqlExpressionNode ParentNode;
            //public string ParentMember;

            public EntityTableInfo TableInfo;
            public SqlTable TableClause;
        }

        #endregion
    }
}
