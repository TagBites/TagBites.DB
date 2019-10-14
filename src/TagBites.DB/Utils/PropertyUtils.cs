using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TagBites.Utils
{
    internal static class PropertyUtils
    {
        public static object GetValue(Type type, string propertyName, bool nonPublic)
        {
            Guard.ArgumentNotNull(type, "type");
            Guard.ArgumentNotNull(propertyName, "propertyName");

            return GetValueInner(null, type, propertyName, nonPublic);
        }
        public static object GetValue(object obj, string propertyName, bool nonPublic)
        {
            Guard.ArgumentNotNull(obj, "obj");
            Guard.ArgumentNotNull(propertyName, "propertyName");

            return GetValueInner(obj, obj.GetType(), propertyName, nonPublic);
        }
        private static object GetValueInner(object obj, Type type, string propertyName, bool nonPublic)
        {
            PropertyInfo property = GetProperty(type, propertyName, obj == null, nonPublic);

            return property == null
                ? null
                : property.GetValue(obj, null);
        }

        public static PropertyInfo GetProperty(object obj, string name, bool nonPublic)
        {
            return GetProperty(obj.GetType(), name, false, nonPublic);
        }
        public static PropertyInfo GetProperty(Type type, string name, bool isStatis, bool nonPublic)
        {
            var rootType = type;

            while (type != typeof(object) && type != null)
            {
                var ti = type.GetTypeInfo();
                var property = ti.GetDeclaredProperty(name);
                if (property != null && property.GetMethod != null && (isStatis == property.GetMethod.IsStatic) && (nonPublic || property.GetMethod.IsPublic))
                    return property;

                type = ti.BaseType;
            }

            foreach (var interfaceType in rootType.GetTypeInfo().ImplementedInterfaces)
            {
                var property = interfaceType.GetTypeInfo().GetDeclaredProperty(name);
                if (property != null && property.GetMethod != null && (isStatis == property.GetMethod.IsStatic) && (nonPublic || property.GetMethod.IsPublic))
                    return property;
            }

            return null;
        }
        public static PropertyInfo GetProperty(LambdaExpression expression)
        {
            return (PropertyInfo)GetMemberExpression(expression).Member;
        }

        public static string GetMemberExpressionName(LambdaExpression expression)
        {
            return GetMemberExpression(expression).Member.Name;
        }
        public static MemberExpression GetMemberExpression(LambdaExpression expression)
        {
            MemberExpression me = null;

            switch (expression.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = expression.Body as UnaryExpression;
                    if (ue != null)
                        me = ue.Operand as MemberExpression;
                    break;

                default:
                    me = expression.Body as MemberExpression;
                    break;
            }

            if (me == null)
                throw new ArgumentException("Expression is not member expression.", nameof(expression));

            return me;
        }

        public static bool IsPropertyMethod(MethodInfo method)
        {
            Guard.ArgumentNotNull(method, nameof(method));

            if (method.IsSpecialName)
            {
                var properties = TypeUtils.GetProperties(method.DeclaringType);
                return properties.Any(p => p.GetMethod == method || p.SetMethod == method);
            }

            return false;
        }


        public static Func<object, object> BuildGetAccessor(PropertyInfo propertyInfo)
        {
            //return (Func<object, object>)propertyInfo.GetMethod.CreateDelegate(typeof(Func<object, object>));
            if (!propertyInfo.CanRead)
                return null;

            var method = propertyInfo.GetMethod;

            // GetRuntimeBaseDefinition - czy nie czasem zwraca zawsze najbardziej bazowy i nie ma potrzeby użycia for?
            for (var baseMethod = method.GetRuntimeBaseDefinition(); baseMethod != null && baseMethod != method; baseMethod = baseMethod.GetRuntimeBaseDefinition())
                method = baseMethod;

            var obj = Expression.Parameter(typeof(object), "o");

            var expr = Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.Call(
                        Expression.Convert(obj, method.DeclaringType),
                        method),
                    typeof(object)),
                obj);

            return expr.Compile();
        }
        public static Func<object, T> BuildGetAccessor<T>(PropertyInfo propertyInfo)
        {
            //return (Func<object, T>)propertyInfo.GetMethod.CreateDelegate(typeof(Func<object, T>));
            if (!propertyInfo.CanRead)
                return null;

            var method = propertyInfo.GetMethod;

            for (var baseMethod = method.GetRuntimeBaseDefinition(); baseMethod != null && baseMethod != method; baseMethod = baseMethod.GetRuntimeBaseDefinition())
                method = baseMethod;

            var obj = Expression.Parameter(typeof(object), "o");

            var expr = Expression.Lambda<Func<object, T>>(
                Expression.Call(
                    Expression.Convert(obj, method.DeclaringType),
                    method),
                obj);

            return expr.Compile();
        }

        public static Action<object, object> BuildSetAccessor(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
                return null;

            var method = propertyInfo.SetMethod;

            // TODO GetRuntimeBaseDefinition need for?
            for (var baseMethod = method.GetRuntimeBaseDefinition(); baseMethod != null && baseMethod != method; baseMethod = baseMethod.GetRuntimeBaseDefinition())
                method = baseMethod;

            var obj = Expression.Parameter(typeof(object), "o");
            var val = Expression.Parameter(typeof(object), "v");

            var expr = Expression.Lambda<Action<object, object>>(
                Expression.Call(
                    Expression.Convert(obj, method.DeclaringType),
                    method,
                    Expression.Convert(val, method.GetParameters()[0].ParameterType)),
                obj, val);

            return expr.Compile();
        }
        public static Action<object, T> BuildSetAccessor<T>(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
                return null;

            var method = propertyInfo.SetMethod;

            for (var baseMethod = method.GetRuntimeBaseDefinition(); baseMethod != null && baseMethod != method; baseMethod = baseMethod.GetRuntimeBaseDefinition())
                method = baseMethod;

            var obj = Expression.Parameter(typeof(object), "o");
            var val = Expression.Parameter(typeof(T), "v");

            var expr = Expression.Lambda<Action<object, T>>(
                Expression.Call(
                    Expression.Convert(obj, method.DeclaringType),
                    method,
                    val),
                obj, val);

            return expr.Compile();
        }
    }
}
