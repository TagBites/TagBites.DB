using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace TBS.Utils
{
    [DebuggerStepThrough]
    internal static class Guard
    {
        public static void ArgumentIsValidMemberName(string value, string name)
        {
            if (string.IsNullOrEmpty(value) || !char.IsLetter(value[0]) && value[0] != '_')
                ThrowArgumentException(name, value);

            for (var i = 1; i < value.Length; i++)
                if (!char.IsLetterOrDigit(value[i]) && value[i] != '_')
                    ThrowArgumentException(name, value);
        }
        public static void ArgumentIsEnumValue<T>(T value, string name) where T : struct
        {
            if (!Enum.IsDefined(typeof(T), value))
                throw new ArgumentOutOfRangeException(name, String.Format("Argument {0} is out of enum {1} range.", name, typeof(T).Name));
        }

        public static void ArgumentIsType(object value, string name, Type type)
        {
            if (value == null || !type.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))
                throw new ArgumentException(String.Format("Argument is not type of {0}.", type.Name), name);
        }
        public static void ArgumentIsType<T>(object value, string name)
        {
            if (!(value is T))
                throw new ArgumentException(String.Format("Argument is not type of {0}.", typeof(T).Name), name);
        }
        public static void ArgumentIsEnumType(object value, string name)
        {
            if (value == null)
                throw new ArgumentNullException(name);
            if (!value.GetType().GetTypeInfo().IsEnum)
                throw new ArgumentException(String.Format("Argument {0} is not enum type.", value.GetType().Name));
        }
        public static void ArgumentIsEnumType(Type value, string name)
        {
            if (value == null)
                throw new ArgumentNullException(name);
            if (!value.GetTypeInfo().IsEnum)
                throw new ArgumentException(String.Format("Argument {0} is not enum type.", value.Name));
        }

        public static void ArgumentNonNegative(int value, string name)
        {
            if (value < 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNonNegative(short value, string name)
        {
            if (value < 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNonNegative(long value, string name)
        {
            if (value < 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNonNegative(float value, string name)
        {
            if (value < 0f)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNonNegative(double value, string name)
        {
            if (value < 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNonNegative(decimal value, string name)
        {
            if (value < 0m)
                ThrowArgumentException(name, value);
        }

        public static void ArgumentPositive(float value, string name)
        {
            if (value <= 0f)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentPositive(double value, string name)
        {
            if (value <= 0.0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentPositive(decimal value, string name)
        {
            if (value <= 0.0m)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentPositive(int value, string name)
        {
            if (value <= 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentPositive(long value, string name)
        {
            if (value <= 0)
                ThrowArgumentException(name, value);
        }

        public static void ArgumentInRange(int value, string name, int from, int to)
        {
            if (value < from || value > to)
                throw new ArgumentOutOfRangeException(name, String.Format("Argument {0} is out of range [{1} ... {2}].", value, from, to));
        }
        public static void ArgumentInRange(long value, string name, long from, long to)
        {
            if (value < from || value > to)
                throw new ArgumentOutOfRangeException(name, String.Format("Argument {0} is out of range [{1} ... {2}].", value, from, to));
        }
        public static void ArgumentInRange(float value, string name, float from, float to)
        {
            if (value < from || value > to)
                throw new ArgumentOutOfRangeException(name, String.Format("Argument {0} is out of range [{1} ... {2}].", value, from, to));
        }
        public static void ArgumentInRange(double value, string name, double from, double to)
        {
            if (value < from || value > to)
                throw new ArgumentOutOfRangeException(name, String.Format("Argument {0} is out of range [{1} ... {2}].", value, from, to));
        }
        public static void ArgumentInRange(decimal value, string name, decimal from, decimal to)
        {
            if (value < from || value > to)
                throw new ArgumentOutOfRangeException(name, String.Format("Argument {0} is out of range [{1} ... {2}].", value, from, to));
        }

        public static void ArgumentIndexInRange(int value, string name, int rangeLength, int startIndex = 0)
        {
            ArgumentNonNegative(rangeLength, "rangeLength");

            if (value < startIndex || value >= (startIndex + rangeLength))
                if (rangeLength == 0)
                    throw new ArgumentOutOfRangeException(name, "Range length is 0.");
                else
                    throw new ArgumentOutOfRangeException(name, String.Format("Index={0} is out of range [{1} ... {2}].", value, startIndex, startIndex + rangeLength));
        }
        public static void ArgumentIndexInRange(long value, string name, long rangeLength, long startIndex = 0)
        {
            ArgumentNonNegative(rangeLength, "rangeLength");

            if (value < startIndex || value >= (startIndex + rangeLength))
                if (rangeLength == 0)
                    throw new ArgumentOutOfRangeException(name, "Range length is 0.");
                else
                    throw new ArgumentOutOfRangeException(name, String.Format("Index={0} is out of range [{1} ... {2}].", value, startIndex, startIndex + rangeLength));
        }

        public static void ArgumentNotNull<T>(T value, string name)
        {
            if (Helpers<T>.IsNull(value))
                ThrowArgumentNullException(name);
        }
        public static void ArgumentNotNull(object value, string name)
        {
            if (ReferenceEquals(value, null))
                ThrowArgumentNullException(name);
        }

        public static void ArgumentNotNullOrEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNotNullOrWhiteSpace(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNotNullOrEmpty(ICollection value, string name)
        {
            ArgumentNotNull(value, name);

            if (value.Count == 0)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNotNullOrEmpty(IEnumerable value, string name)
        {
            ArgumentNotNull(value, name);

            if (!value.GetEnumerator().MoveNext())
                ThrowArgumentException(name, value);
        }

        public static void ArgumentNotNullOrEmptyWithNotNullItems<T>(IEnumerable<T> value, string name)
            where T : class
        {
            ArgumentNotNull(value, name);

            var any = false;

            foreach (var item in value)
                if (item == null)
                    ThrowArgumentException(name, value);
                else
                    any = true;

            if (!any)
                ThrowArgumentException(name, value);
        }
        public static void ArgumentNotNullOrEmptyWithNotNullOrEmptyItems(IEnumerable<string> value, string name)
        {
            ArgumentNotNull(value, name);

            var any = false;

            foreach (var item in value)
                if (String.IsNullOrEmpty(item))
                    ThrowArgumentException(name, value);
                else
                    any = true;

            if (!any)
                ThrowArgumentException(name, value);
        }

        public static void ArgumentNotNullWithNotNullItems<T>(IEnumerable<T> value, string name)
            where T : class
        {
            ArgumentNotNull(value, name);

            foreach (var item in value)
                if (item == null)
                    ThrowArgumentException(name, value);
        }
        public static void ArgumentNotNullWithNotNullOrEmptyItems(IEnumerable<string> value, string name)
        {
            ArgumentNotNull(value, name);

            foreach (var item in value)
                if (String.IsNullOrEmpty(item))
                    ThrowArgumentException(name, value);
        }

        private static void ThrowArgumentException(string propName, object val)
        {
            var arg = ReferenceEquals(val, string.Empty)
                ? "String.Empty"
                : (val == null ? "null" : val.ToString());
            var message = string.Format("'{0}' is not a valid value for '{1}'", arg, propName);
            throw new ArgumentException(message);
        }
        private static void ThrowArgumentNullException(string propName)
        {
            throw new ArgumentNullException(propName);
        }

        public static void ThrowNotSupportedArgumentValue(object value, string name)
        {
            throw new ArgumentException(String.Format("'{0}' is not supported value for argument '{1}'.", value ?? String.Empty, name));
        }

        private static class Helpers<T>
        {
            public static readonly Func<T, bool> IsNull = typeof(T).GetTypeInfo().IsValueType
                ? (Func<T, bool>)(v => false)
                : v => ReferenceEquals(v, null);
        }
    }
}
