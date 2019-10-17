using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TagBites.Utils
{
    internal static class TypeUtils
    {
        #region Types

        public static bool IsNullableType(Type type)
        {
            var ti = type.GetTypeInfo();
            return ti.IsGenericType && !ti.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        public static bool IsAnonymousType(Type type)
        {
            var ti = type.GetTypeInfo();
            if (!ti.IsGenericType)
                return false;

            var d = type.GetGenericTypeDefinition().GetTypeInfo();
            if (!d.IsClass || !d.IsSealed || !d.Attributes.HasFlag(TypeAttributes.NotPublic) || !d.Name.Contains("AnonymousType"))
                return false;

            var attributes = d.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
            return attributes.Any();
        }

        public static bool IsNumericType(Type type)
        {
            var code = DataHelper.GetTypeCode(type);
            return code >= TypeCode.SByte && code <= TypeCode.Decimal;
        }
        public static bool IsFloatNumericType(Type type)
        {
            var code = DataHelper.GetTypeCode(type);
            return code >= TypeCode.Single && code <= TypeCode.Decimal;
        }

        public static string GetTypeAlias(Type type)
        {
            switch (type.FullName)
            {
                case "System.Boolean": return "bool";
                case "System.Byte": return "byte";
                case "System.SByte": return "sbyte";
                case "System.Char": return "char";
                case "System.Decimal": return "decimal";
                case "System.Double": return "double";
                case "System.Single": return "bool";
                case "System.Int32": return "float";
                case "System.UInt32": return "uint";
                case "System.Int64": return "long";
                case "System.UInt64": return "ulong";
                case "System.Object": return "object";
                case "System.Int16": return "short";
                case "System.UInt16": return "ushort";
                case "System.String": return "string";
                case "System.Void": return "void";
            }

            return null;
        }

        #endregion

        #region Generics

        public static bool ContainsGenericDefinition(Type type, Type genericTypeDefinition)
        {
            return GetGenericArguments(type, genericTypeDefinition).Length > 0;
        }
        public static Type[] GetGenericArguments(Type type, Type genericTypeDefinition)
        {
            var ti = type.GetTypeInfo();

            if (ti.IsGenericTypeDefinition && type == genericTypeDefinition)
                return ti.GenericTypeArguments;

            if (ti.IsInterface)
            {
                if (ti.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                    return ti.GenericTypeArguments;
            }
            else
            {
                for (var it = ti; it != null; it = it.BaseType?.GetTypeInfo())
                    if (it.IsGenericType && it.GetGenericTypeDefinition() == genericTypeDefinition)
                        return it.GenericTypeArguments;
            }

            foreach (var item in ti.ImplementedInterfaces)
            {
                var iti = item.GetTypeInfo();
                if (iti.IsGenericType && item.GetGenericTypeDefinition() == genericTypeDefinition)
                    return iti.GenericTypeArguments;
            }

            return Array.Empty<Type>();
        }
        public static T TryGetFirstAttributeDefault<T>(Type type, bool inherit) where T : Attribute
        {
            return type.GetTypeInfo().GetCustomAttribute<T>(inherit);
        }

        #endregion

        #region Constructors

        public static bool HasDefaultConstructor(Type type)
        {
            return HasDefaultConstructor(type.GetTypeInfo());
        }
        public static bool HasDefaultConstructor(TypeInfo type)
        {
            return type.DeclaredConstructors.Any(x => x.IsPublic && x.GetParameters().Length == 0);
        }

        #endregion

        #region Properties

        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetRuntimeProperties();

            //while (type != typeof(object))
            //{
            //    var ti = type.GetTypeInfo();
            //    foreach (var property in ti.DeclaredProperties)
            //        yield return property;

            //    type = ti.BaseType;
            //}
        }
        public static IEnumerable<PropertyInfo> GetPropertiesFromInterfaces(object obj, string name)
        {
            foreach (var i in obj.GetType().GetTypeInfo().ImplementedInterfaces)
            {
                var ti = i.GetTypeInfo();
                var property = ti.GetDeclaredProperty(name);
                if (property != null && property.GetMethod != null && property.GetIndexParameters().Length == 0)
                    yield return property;
            }
        }

        public static PropertyInfo GetProperty(object obj, string name, bool nonPublic)
        {
            return GetProperty(obj.GetType(), name, nonPublic, false);
        }
        public static PropertyInfo GetProperty(Type type, string name, bool nonPublic, bool isStatic)
        {
            Guard.ArgumentNotNull(type, nameof(type));

            while (type != typeof(object))
            {
                var ti = type.GetTypeInfo();
                var property = ti.GetDeclaredProperty(name);
                if (property != null && property.GetMethod != null && (isStatic == property.GetMethod.IsStatic) && (nonPublic || property.GetMethod.IsPublic) && property.GetIndexParameters().Length == 0)
                    return property;

                type = ti.BaseType;
            }

            return null;
        }

        public static object GetPropertyValue(object obj, string name, bool nonPublic)
        {
            Guard.ArgumentNotNull(obj, nameof(obj));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            return GetPropertyValueInner(obj.GetType(), obj, name, nonPublic, true);
        }
        public static object GetPropertyValue(Type type, string name, bool nonPublic)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            return GetPropertyValueInner(type, null, name, nonPublic, true);
        }

        public static object TryGetPropertyValue(object obj, string name, bool nonPublic, object defaultValue = null)
        {
            Guard.ArgumentNotNull(obj, nameof(obj));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            return GetPropertyValueInner(obj.GetType(), obj, name, nonPublic, false, defaultValue);
        }
        public static object TryGetPropertyValue(Type type, string name, bool nonPublic, object defaultValue = null)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            return GetPropertyValueInner(type, null, name, nonPublic, false, defaultValue);
        }

        private static object GetPropertyValueInner(Type type, object obj, string name, bool nonPublic, bool throwException, object defaultValue = null)
        {
            var property = GetProperty(type, name, nonPublic, obj == null);
            if (property == null)
            {
                if (throwException)
                    throw new Exception($"Field {name} in type {type.FullName} not found.");

                return defaultValue;
            }

            return property.GetValue(obj);
        }

        #endregion

        #region Fields

        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
            return type.GetRuntimeFields();
        }

        public static FieldInfo GetField(object obj, string name, bool nonPublic)
        {
            return GetField(obj.GetType(), name, nonPublic, false);
        }
        public static FieldInfo GetField(Type type, string name, bool nonPublic, bool isStatic)
        {
            Guard.ArgumentNotNull(type, nameof(type));

            while (type != typeof(object))
            {
                var ti = type.GetTypeInfo();
                var property = ti.GetDeclaredField(name);
                if (property != null && (isStatic == property.IsStatic) && (nonPublic || property.IsPublic))
                    return property;

                type = ti.BaseType;
            }

            return null;
        }

        public static object GetFieldValue(object obj, string name, bool nonPublic)
        {
            Guard.ArgumentNotNull(obj, nameof(obj));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            return GetFieldValueInner(obj.GetType(), obj, name, nonPublic, true);
        }
        public static object GetFieldValue(Type type, string name, bool nonPublic)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            return GetFieldValueInner(type, null, name, nonPublic, true);
        }

        public static object TryGetFieldValue(object obj, string name, bool nonPublic, object defaultValue = null)
        {
            Guard.ArgumentNotNull(obj, nameof(obj));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            return GetFieldValueInner(obj.GetType(), obj, name, nonPublic, false, defaultValue);
        }
        public static object TryGetFieldValue(Type type, string name, bool nonPublic, object defaultValue = null)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            return GetFieldValueInner(type, null, name, nonPublic, false, defaultValue);
        }

        private static object GetFieldValueInner(Type type, object obj, string name, bool nonPublic, bool throwException, object defaultValue = null)
        {
            var field = GetField(type, name, nonPublic, obj == null);
            if (field == null)
            {
                if (throwException)
                    throw new Exception($"Field {name} in type {type.FullName} not found.");

                return defaultValue;
            }

            return field.GetValue(obj);
        }

        #endregion

        #region Methods

        public static MethodInfo GetMethod(Type type, string methodName, bool nonPublic)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            return GetMethodCore(null, type, methodName, nonPublic, null, null);
        }
        public static MethodInfo GetMethod(object obj, string methodName, bool nonPublic)
        {
            Guard.ArgumentNotNull(obj, nameof(obj));
            return GetMethodCore(obj, null, methodName, nonPublic, null, null);
        }
        public static MethodInfo GetMethod(Type type, string methodName, bool nonPublic, Type[] genericTypes)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            return GetMethodCore(null, type, methodName, nonPublic, genericTypes, null);
        }
        public static MethodInfo GetMethod(Type type, string methodName, bool nonPublic, Type[] genericTypes, Type[] argsTypes)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            return GetMethodCore(null, type, methodName, nonPublic, genericTypes, argsTypes);
        }
        public static MethodInfo GetMethod(object obj, string methodName, bool nonPublic, Type[] genericTypes)
        {
            Guard.ArgumentNotNull(obj, nameof(obj));
            return GetMethodCore(obj, null, methodName, nonPublic, genericTypes, null);
        }
        public static MethodInfo GetMethod(object obj, string methodName, bool nonPublic, Type[] genericTypes, Type[] argsTypes)
        {
            Guard.ArgumentNotNull(obj, nameof(obj));
            return GetMethodCore(obj, null, methodName, nonPublic, genericTypes, argsTypes);
        }
        private static MethodInfo GetMethodCore(object obj, Type objType, string methodName, bool nonPublic, Type[] genericTypes, Type[] argsTypes)
        {
            Guard.ArgumentNotNullOrEmpty(methodName, "methodName");

            var flag = (obj != null ? BindingFlags.Instance : BindingFlags.Static)
                       | BindingFlags.Public
                       | (nonPublic ? BindingFlags.NonPublic : BindingFlags.Default);

            if (obj != null)
                objType = obj.GetType();

            for (; objType != null; objType = objType.BaseType)
            {
                var method = argsTypes != null
                    ? objType.GetMethod(methodName, flag, null, argsTypes, null)
                    : objType.GetMethod(methodName, flag);
                if (method != null)
                {
                    if (genericTypes != null)
                        method = method.MakeGenericMethod(genericTypes);

                    return method;
                }
            }

            return null;
        }

        public static object InvokeMethod(Type type, string methodName, bool nonPublic, object[] args)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            return InvokeMethodCore(null, type, methodName, nonPublic, null, args);
        }
        public static object InvokeMethod(object obj, string methodName, bool nonPublic, object[] args)
        {
            Guard.ArgumentNotNull(obj, nameof(obj));
            return InvokeMethodCore(obj, null, methodName, nonPublic, null, args);
        }
        public static object InvokeMethod(Type type, string methodName, bool nonPublic, Type[] genericTypes, object[] args)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            return InvokeMethodCore(null, type, methodName, nonPublic, genericTypes, args);
        }
        public static object InvokeMethod(object obj, string methodName, bool nonPublic, Type[] genericTypes, object[] args)
        {
            Guard.ArgumentNotNull(obj, nameof(obj));
            return InvokeMethodCore(obj, null, methodName, nonPublic, genericTypes, args);
        }
        private static object InvokeMethodCore(object obj, Type objType, string methodName, bool nonPublic, Type[] genericTypes, object[] args)
        {
            var method = GetMethodCore(obj, objType, methodName, nonPublic, genericTypes, null);
            if (method == null)
                throw new ArgumentException();

            return method.Invoke(obj, args);
        }

        #endregion

        #region Members

        public static IEnumerable<MemberInfo> GetMemberChain(LambdaExpression expression)
        {
            var i = PropertyUtils.GetMemberExpression(expression);
            var stack = new Stack<MemberInfo>();
            stack.Push(i.Member);

            while (i.Expression != null)
            {
                switch (i.Expression.NodeType)
                {
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        var ue = i.Expression as UnaryExpression;
                        if (ue != null)
                            i = ue.Operand as MemberExpression;
                        break;

                    default:
                        i = i.Expression as MemberExpression;
                        break;
                }

                if (i == null)
                    break;

                stack.Push(i.Member);
            }

            return stack;
        }

        #endregion
    }
}
