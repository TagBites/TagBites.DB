using System;
using System.Reflection;
using System.Text;

namespace TagBites.Utils
{
    internal static class MemberUtils
    {
        public static T TryGetFirstAttribute<T>(MemberInfo memberInfo, bool inherit) where T : Attribute
        {
            return memberInfo.GetCustomAttribute<T>(inherit);
        }
        public static T TryGetFirstAttribute<T>(Assembly assembly) where T : Attribute
        {
            return assembly.GetCustomAttribute<T>();
        }

        public static string GetFullGenericName(MemberInfo member)
        {
            if (member is TypeInfo)
                return GetFullGenericName(((TypeInfo)member).AsType());

            if (member is MethodInfo)
                return GetFullGenericName((MethodInfo)member);

            return member.Name;
        }
        public static string GetFullGenericName(Type type)
        {
            var ti = type.GetTypeInfo();
            if (!ti.IsGenericType)
                return type == typeof(void) ? "void" : GetFullName(type);

            return GetFullGenericNameInner(GetFullName(type), ti.GenericTypeArguments);
        }
        private static string GetFullGenericName(MethodInfo method)
        {
            var baseName = GetFullGenericName(method.DeclaringType) + "." + method.Name;

            if (!method.IsGenericMethod)
                return baseName;

            return GetFullGenericNameInner(baseName, method.GetGenericArguments());
        }
        private static string GetFullGenericNameInner(string name, Type[] types)
        {
            var sb = new StringBuilder();
            sb.Append(name.Substring(0, name.LastIndexOf('`')));
            sb.Append('<');
            sb.Append(GetFullGenericName(types[0]));
            for (var i = 1; i < types.Length; i++)
            {
                sb.Append(',');
                sb.Append(GetFullGenericName(types[i]));
            }
            sb.Append('>');

            return sb.ToString();
        }
        private static string GetFullName(Type type)
        {
            return (type.FullName ?? type.Name).Replace("+", ".");
        }

        public static string GetFullGenericNameOrAlias(MemberInfo member)
        {
            if (member is TypeInfo)
                return GetFullGenericNameOrAlias(((TypeInfo)member).AsType());

            if (member is MethodInfo)
                return GetFullGenericNameOrAlias((MethodInfo)member);

            return member.Name;
        }
        public static string GetFullGenericNameOrAlias(Type type)
        {
            var ti = type.GetTypeInfo();
            var baseName = TypeUtils.GetTypeAlias(type) ?? GetFullName(type);

            if (!ti.IsGenericType)
                return baseName;

            return GetFullGenericNameOrAliasInner(baseName, ti.GenericTypeArguments);
        }
        public static string GetFullGenericNameOrAlias(MethodInfo method)
        {
            var baseName = GetFullGenericNameOrAlias(method.DeclaringType) + "." + method.Name;

            if (!method.IsGenericMethod)
                return baseName;

            return GetFullGenericNameOrAliasInner(baseName, method.GetGenericArguments());
        }
        private static string GetFullGenericNameOrAliasInner(string name, Type[] types)
        {
            var sb = new StringBuilder();
            sb.Append(name.Substring(0, name.LastIndexOf('`')));
            sb.Append('<');
            sb.Append(GetFullGenericNameOrAlias(types[0]));
            for (var i = 1; i < types.Length; i++)
            {
                sb.Append(',');
                sb.Append(GetFullGenericNameOrAlias(types[i]));
            }
            sb.Append('>');

            return sb.ToString();
        }
    }
}
