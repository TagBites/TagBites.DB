using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace TagBites.Utils
{
    internal static class DataHelper
    {
        internal static TypeCode GetTypeCode(Type type)
        {
            if (type.GetTypeInfo().IsEnum)
                type = Enum.GetUnderlyingType(type);

            if (type == typeof(String)) return TypeCode.String;
            if (type == typeof(Int32)) return TypeCode.Int32;
            if (type == typeof(Decimal)) return TypeCode.Decimal;
            if (type == typeof(DateTime)) return TypeCode.DateTime;
            if (type == typeof(bool)) return TypeCode.Boolean;
            if (type == typeof(Double)) return TypeCode.Double;
            if (type == typeof(Single)) return TypeCode.Single;
            if (type == typeof(Byte)) return TypeCode.Byte;
            if (type == typeof(Int64)) return TypeCode.Int64;
            if (type == typeof(SByte)) return TypeCode.SByte;
            if (type == typeof(Int16)) return TypeCode.Int16;
            if (type == typeof(UInt16)) return TypeCode.UInt16;
            if (type == typeof(UInt32)) return TypeCode.UInt32;
            if (type == typeof(UInt64)) return TypeCode.UInt64;
            if (type == typeof(Char)) return TypeCode.Char;

            if (type.Name == "DBNull")
                return TypeCode.DBNull;

            return TypeCode.Object;
        }


        #region Converter methods

        public static T ChangeType<T>(object value)
        {
            object result;
            if (!TryChangeTypeInner(value, typeof(T), out result))
                throw new InvalidCastException("");

            return (T)result;
        }
        public static object ChangeType(object value, Type destinationType)
        {
            Guard.ArgumentNotNull(destinationType, "destinationType");

            object result;
            if (!TryChangeTypeInner(value, destinationType, out result))
                throw new InvalidCastException("");

            return result;
        }

        public static T TryChangeTypeDefault<T>(object value, T defaultValue = default(T))
        {
            if (typeof(T) == typeof(Type))
                throw new ArgumentException("Can not convert to Type.", "T");

            object result;
            return TryChangeTypeInner(value, typeof(T), out result)
                ? (T)result
                : defaultValue;
        }
        public static object TryChangeTypeDefault(object value, Type destinationType, object defaultValue)
        {
            Guard.ArgumentNotNull(destinationType, "destinationType");

            object result;
            return TryChangeTypeInner(value, destinationType, out result)
                ? result
                : defaultValue;
        }

        public static bool TryChangeType<T>(object value, out T result)
        {
            object r;
            if (TryChangeTypeInner(value, typeof(T), out r))
            {
                result = (T)r;
                return true;
            }

            result = default(T);
            return false;
        }
        public static bool TryChangeType(object value, Type destinationType, out object result)
        {
            Guard.ArgumentNotNull(destinationType, "destinationType");

            return TryChangeTypeInner(value, destinationType, out result);
        }

        private static bool TryChangeTypeInner(object value, Type destinationType, out object result)
        {
            Type valueType;
            TypeCode valueTypeCode;
            var di = destinationType.GetTypeInfo();

            // Null Value
            if (value == null || (valueTypeCode = GetTypeCode(valueType = value.GetType())) == TypeCode.DBNull)
            {
                if (GetTypeCode(destinationType) == TypeCode.DBNull)
                {
                    result = destinationType.GetTypeInfo().GetDeclaredField("Value")?.GetValue(null);
                    return true;
                }

                result = null;
                return !di.IsValueType || Nullable.GetUnderlyingType(destinationType) != null;
            }

            // Simple Cast
            if (di.IsAssignableFrom(valueType.GetTypeInfo()))
            {
                result = value;
                return true;
            }

            // Nullable
            if (TypeUtils.IsNullableType(destinationType))
                destinationType = Nullable.GetUnderlyingType(destinationType);

            // Convert
            if (TryChangeTypeFromCode(value, valueType, valueTypeCode, destinationType, GetTypeCode(destinationType), out result))
            {
                if (di.IsEnum && !result.GetType().GetTypeInfo().IsEnum)
                    result = Enum.ToObject(destinationType, result);

                return true;
            }

            // Standard
            if (valueTypeCode == TypeCode.Object)
            {
                try
                {
                    if (destinationType == typeof(string))
                    {
                        result = value.ToString();
                        return true;
                    }

                    //var converter = TypeDescriptor.GetConverter(value.GetType());
                    //if (converter != null && converter.CanConvertTo(destinationType))
                    //{
                    //    result = converter.ConvertTo(value, destinationType);
                    //    return true;
                    //}

                    result = Convert.ChangeType(value, destinationType, CultureInfo.CurrentCulture);
                    if (result != null && destinationType.GetTypeInfo().IsAssignableFrom(result.GetType().GetTypeInfo()))
                        return true;
                }
                catch { /* Ignored */ }
            }

            result = null;
            return false;
        }
        private static bool TryChangeTypeFromCode(object value, Type valueType, TypeCode valueCode, Type destinationType, TypeCode destinationCode, out object result)
        {
            try
            {
                // Codes
                if (valueCode > TypeCode.Object && destinationCode > TypeCode.Object)
                {
                    if (valueType.GetTypeInfo().IsEnum)
                    {
                        if (valueCode == destinationCode)
                        {
                            result = Convert.ChangeType(value, destinationType, CultureInfo.CurrentCulture);
                            return true;
                        }

                        value = Convert.ChangeType(value, destinationType, CultureInfo.CurrentCulture);
                    }

                    if (destinationType.GetTypeInfo().IsEnum)
                    {
                        if (valueCode == TypeCode.String)
                        {
                            result = Enum.Parse(destinationType, (string)value);
                            return true;
                        }
                    }

                    switch (destinationCode)
                    {
                        case TypeCode.Boolean:
                            {
                                switch (valueCode)
                                {
                                    case TypeCode.SByte: { result = (SByte)value != 0; return true; }
                                    case TypeCode.Byte: { result = (Byte)value != 0; return true; }
                                    case TypeCode.Int16: { result = (Int16)value != 0; return true; }
                                    case TypeCode.UInt16: { result = (UInt16)value != 0; return true; }
                                    case TypeCode.Int32: { result = (Int32)value != 0; return true; }
                                    case TypeCode.UInt32: { result = (UInt32)value != 0; return true; }
                                    case TypeCode.Int64: { result = (Int64)value != 0; return true; }
                                    case TypeCode.UInt64: { result = (UInt64)value != 0; return true; }

                                    case TypeCode.Single: { result = (Single)value != 0; return true; }
                                    case TypeCode.Double: { result = (Double)value != 0; return true; }
                                    case TypeCode.Decimal: { result = (Decimal)value != 0; return true; }

                                    case TypeCode.String:
                                    case TypeCode.Char:
                                        {
                                            char c;
                                            if (valueCode == TypeCode.Char)
                                                c = (char)value;
                                            else
                                            {
                                                if (((string)value).Length == 1)
                                                    c = ((string)value)[0];
                                                else
                                                {
                                                    bool outr;
                                                    if (bool.TryParse((string)value, out outr))
                                                    {
                                                        result = outr;
                                                        return true;
                                                    }

                                                    break;
                                                }
                                            }

                                            if (c == '1' || c == 't' || c == 'T') { result = true; return true; }
                                            if (c == '0' || c == 'f' || c == 'F') { result = false; return true; }
                                        }
                                        break;
                                }
                            }
                            break;

                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            {
                                if (valueCode == TypeCode.Boolean)
                                {
                                    var v = (bool)value ? 1 : 0;

                                    switch (destinationCode)
                                    {
                                        case TypeCode.SByte:
                                            { result = (SByte)v; return true; }
                                        case TypeCode.Byte:
                                            { result = (Byte)v; return true; }
                                        case TypeCode.Int16:
                                            { result = (Int16)v; return true; }
                                        case TypeCode.UInt16:
                                            { result = (UInt16)v; return true; }
                                        case TypeCode.Int32:
                                            { result = (Int32)v; return true; }
                                        case TypeCode.UInt32:
                                            { result = (UInt32)v; return true; }
                                        case TypeCode.Int64:
                                            { result = (Int64)v; return true; }
                                        case TypeCode.UInt64:
                                            { result = (UInt64)v; return true; }
                                    }
                                }

                                var text = value.ToString().Trim();
                                var idx = -1;
                                var ok = true;
                                var comma = false;

                                for (var i = 0; i < text.Length; i++)
                                    if (!char.IsDigit(text[i]))
                                        if (i == 0 && text[0] == '-')
                                        {
                                            if (comma)
                                            {
                                                ok = false;
                                                break;
                                            }

                                            comma = true;
                                        }
                                        else if (text[i] == '.' || text[i] == ',')
                                        {
                                            if (idx == -1)
                                                idx = i;
                                            else
                                            {
                                                ok = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            ok = false;
                                            break;
                                        }

                                if (ok)
                                {
                                    if (idx != -1)
                                    {
                                        if (idx > 0)
                                        {
                                            text = idx == 1 && comma
                                                ? "0"
                                                : text.Substring(0, idx);
                                        }
                                        else if (text.Length > 1)
                                        {
                                            switch (destinationCode)
                                            {
                                                case TypeCode.SByte:
                                                    { result = (SByte)0; return true; }
                                                case TypeCode.Byte:
                                                    { result = (Byte)0; return true; }
                                                case TypeCode.Int16:
                                                    { result = (Int16)0; return true; }
                                                case TypeCode.UInt16:
                                                    { result = (UInt16)0; return true; }
                                                case TypeCode.Int32:
                                                    { result = (Int32)0; return true; }
                                                case TypeCode.UInt32:
                                                    { result = (UInt32)0; return true; }
                                                case TypeCode.Int64:
                                                    { result = (Int64)0; return true; }
                                                case TypeCode.UInt64:
                                                    { result = (UInt64)0; return true; }
                                            }
                                        }
                                        else
                                        {
                                            result = null;
                                            return false;
                                        }
                                    }

                                    switch (destinationCode)
                                    {
                                        case TypeCode.SByte: { SByte r; if (sbyte.TryParse(text, out r)) { result = r; return true; } } break;
                                        case TypeCode.Byte: { Byte r; if (byte.TryParse(text, out r)) { result = r; return true; } } break;
                                        case TypeCode.Int16: { Int16 r; if (short.TryParse(text, out r)) { result = r; return true; } } break;
                                        case TypeCode.UInt16: { UInt16 r; if (ushort.TryParse(text, out r)) { result = r; return true; } } break;
                                        case TypeCode.Int32: { Int32 r; if (int.TryParse(text, out r)) { result = r; return true; } } break;
                                        case TypeCode.UInt32: { UInt32 r; if (uint.TryParse(text, out r)) { result = r; return true; } } break;
                                        case TypeCode.Int64: { Int64 r; if (long.TryParse(text, out r)) { result = r; return true; } } break;
                                        case TypeCode.UInt64: { UInt64 r; if (ulong.TryParse(text, out r)) { result = r; return true; } } break;
                                    }
                                }
                            }
                            break;

                        case TypeCode.DateTime:
                            {
                                if (valueCode == TypeCode.Double)
                                {
                                    DateTime r;
                                    if (TryFromOADate((Double)value, out r))
                                    {
                                        result = r;
                                        return true;
                                    }
                                    else
                                    {
                                        result = null;
                                        return false;
                                    }
                                }
                                else
                                {
                                    var text = value.ToString();
                                    DateTime r;
                                    if (DateTime.TryParse(text, out r))
                                    {
                                        result = r;
                                        return true;
                                    }
                                }
                            }
                            break;

                        case TypeCode.Single:
                            {
                                if (valueCode == TypeCode.Boolean)
                                {
                                    result = (Single)((bool)value ? 1 : 0);
                                    return true;
                                }

                                var text = CorrectNumber(value.ToString());
                                Single r;
                                if (float.TryParse(text, out r))
                                {
                                    result = r;
                                    return true;
                                }
                            }
                            break;
                        case TypeCode.Double:
                            {
                                if (valueCode == TypeCode.Boolean)
                                {
                                    result = (Double)((bool)value ? 1 : 0);
                                    return true;
                                }

                                var text = CorrectNumber(value.ToString());
                                Double r;
                                if (double.TryParse(text, out r))
                                {
                                    result = r;
                                    return true;
                                }
                            }
                            break;
                        case TypeCode.Decimal:
                            {
                                if (valueCode == TypeCode.Boolean)
                                {
                                    result = (Decimal)((bool)value ? 1 : 0);
                                    return true;
                                }

                                var text = CorrectNumber(value.ToString());
                                Decimal r;
                                if (decimal.TryParse(text, out r))
                                {
                                    result = r;
                                    return true;
                                }
                            }
                            break;

                        case TypeCode.Char:
                            {
                                var text = value.ToString();
                                if (text.Length == 1)
                                {
                                    result = text[0];
                                    return true;
                                }
                            }
                            break;
                        case TypeCode.String:
                            {
                                result = value.ToString();
                                return true;
                            }
                    }

                    result = null;
                    return false;
                }

                // TimeSpan
                if (destinationType == typeof(TimeSpan))
                {
                    if (valueCode == TypeCode.String)
                    {
                        TimeSpan r;
                        if (TimeSpan.TryParse((string)value, out r))
                        {
                            result = r;
                            return true;
                        }

                        result = null;
                        return false;
                    }

                    if (valueCode == TypeCode.DateTime)
                    {
                        result = new TimeSpan(((DateTime)value).Ticks);
                        return true;
                    }
                }

                // Custom .NET types parse
                var converter = TypeDescriptor.GetConverter(destinationType);
                if (converter.CanConvertFrom(valueType))
                {
                    result = converter.ConvertFrom(value);
                    return true;
                }

                converter = TypeDescriptor.GetConverter(valueType);
                if (converter.CanConvertTo(valueType))
                {
                    result = converter.ConvertTo(value, destinationType);
                    return true;
                }
            }
            catch { }

            result = null;
            return false;
        }

        private static string CorrectNumber(string value)
        {
            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (decimalSeparator == ",")
                return value.Replace(".", ",");
            else if (decimalSeparator == ".")
                return value.Replace(",", ".");

            return value;
        }

        #endregion

        #region String converters (proxy calls)

        public static string ToString(object value)
        {
            Guard.ArgumentNotNull(value, "value");
            return ToStringInner(value);
        }
        public static string ToStringDefault(object value, string defaultValue = null)
        {
            if (value == null)
                return defaultValue;

            return ToStringInner(value);
        }
        private static string ToStringInner(object value)
        {
            if (value is Array)
            {
                var sb = new StringBuilder();
                sb.Append("[");

                foreach (var item in (IEnumerable)value)
                {
                    if (sb.Length > 1)
                        sb.Append(", ");
                    sb.Append(ToStringDefault(item, "null"));
                }

                sb.Append("]");
                return sb.ToString();
            }

            return value.ToString();
        }

        public static T Parse<T>(string value)
        {
            return ChangeType<T>(value);
        }
        public static object Parse(string value, Type destinationType)
        {
            return ChangeType(value, destinationType);
        }

        public static T TryParseDefault<T>(string value, T defaultValue = default(T))
        {
            return TryChangeTypeDefault<T>(value, defaultValue);
        }
        public static object TryParseDefault(string value, Type destinationType, object defaultValue)
        {
            return TryChangeTypeDefault(value, destinationType, defaultValue);
        }

        public static bool TryParse<T>(string value, out T result)
        {
            return TryChangeType<T>(value, out result);
        }
        public static bool TryParse(string value, Type destinationType, out object result)
        {
            return TryChangeType(value, destinationType, out result);
        }

        #endregion

        #region String helpers

        public static string JoinWithoutNullOrEmpty(string separator, params string[] values)
        {
            return JoinWithoutNullOrEmpty(separator, (IEnumerable<string>)values);
        }
        public static string JoinWithoutNullOrEmpty(string separator, IEnumerable<string> values)
        {
            var sb = new StringBuilder();

            foreach (var v in values)
                if (!string.IsNullOrEmpty(v))
                {
                    if (sb.Length > 0)
                        sb.Append(separator);
                    sb.Append(v);
                }

            return sb.ToString();
        }

        #endregion

        #region Null/Empty Helpers

        public static bool IsNull(object value)
        {
            return value == null || GetTypeCode(value.GetType()) == TypeCode.DBNull;
        }
        public static bool IsDefault<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default(T));
        }
        public static bool IsDefaultOrEmpty<T>(T value)
        {
            if (IsDefault(value))
                return true;

            var s = value as string;
            if (s != null)
                return s.Length == 0;

            if (value is ICollection)
                return ((ICollection)value).Count == 0;

            if (value is IEnumerable)
            {
                var enumerator = ((IEnumerable)value).GetEnumerator();

                try
                {
                    return !enumerator.MoveNext();
                }
                finally
                {
                    if (enumerator is IDisposable)
                        ((IDisposable)enumerator).Dispose();
                }
            }

            return false;
        }

        public static T? FirstNotNull<T>(params T?[] values) where T : struct
        {
            foreach (var item in values)
                if (item.HasValue)
                    return item;

            return null;
        }
        public static T FirstNotNull<T>(params T[] values) where T : class
        {
            return FirstNotNull((IEnumerable<T>)values);
        }
        public static T FirstNotNull<T>(IEnumerable<T> values) where T : class
        {
            Guard.ArgumentNotNull(values, "values");

            foreach (var item in values)
                if (item != null)
                    return item;
            return null;
        }

        public static string FirstNotNullOrEmpty(params string[] values)
        {
            return FirstNotNullOrEmpty((IEnumerable<string>)values);
        }
        public static string FirstNotNullOrEmpty(IEnumerable<string> values)
        {
            Guard.ArgumentNotNull(values, "values");

            foreach (var item in values)
                if (!string.IsNullOrEmpty(item))
                    return item;

            return null;

        }
        public static string FirstNotNullOrEmptyWhiteSpace(params string[] values)
        {
            return FirstNotNullOrEmptyWhiteSpace((IEnumerable<string>)values);
        }
        public static string FirstNotNullOrEmptyWhiteSpace(IEnumerable<string> values)
        {
            Guard.ArgumentNotNull(values, "values");

            foreach (var item in values)
                if (!string.IsNullOrWhiteSpace(item))
                    return item;

            return null;
        }

        //public static U PropagateNulls<T, U>(T obj, Expression<Func<T, U>> expr)
        //{
        //    if (obj == null)
        //        return default(U);

        //    var members = new Stack<MemberInfo>();
        //    var searchingForMembers = true;
        //    var currentExpression = expr.Body;

        //    while (searchingForMembers)
        //        switch (currentExpression.NodeType)
        //        {
        //            case ExpressionType.Parameter:
        //                searchingForMembers = false;
        //                break;

        //            case ExpressionType.MemberAccess:
        //                {
        //                    var ma = (MemberExpression)currentExpression;
        //                    members.Push(ma.Member);
        //                    currentExpression = ma.Expression;
        //                }
        //                break;

        //            case ExpressionType.Call:
        //                {
        //                    var mc = (MethodCallExpression)currentExpression;
        //                    members.Push(mc.Method);

        //                    //only supports 1-arg static methods and 0-arg instance methods
        //                    if ((mc.Method.IsStatic && mc.Arguments.Count == 1) || (mc.Arguments.Count == 0))
        //                    {
        //                        currentExpression = mc.Method.IsStatic ? mc.Arguments[0] : mc.Object;
        //                        break;
        //                    }

        //                    throw new NotSupportedException(mc.Method + " is not supported");
        //                }

        //            default:
        //                throw new NotSupportedException(currentExpression.GetType() + " not supported");
        //        }

        //    object currValue = obj;
        //    while (members.Count > 0)
        //    {
        //        var m = members.Pop();

        //        switch (m.MemberType)
        //        {
        //            case MemberTypes.Field:
        //                currValue = ((FieldInfo)m).GetValue(currValue);
        //                break;

        //            case MemberTypes.Method:
        //                {
        //                    var method = (MethodBase)m;
        //                    currValue = method.IsStatic
        //                                       ? method.Invoke(null, new[] { currValue })
        //                                       : method.Invoke(currValue, null);
        //                }
        //                break;

        //            case MemberTypes.Property:
        //                {
        //                    var method = ((PropertyInfo)m).GetMethod;
        //                    currValue = method.Invoke(currValue, null);
        //                }
        //                break;

        //        }

        //        if (currValue == null)
        //            return default(U);
        //    }

        //    return (U)currValue;
        //}

        #endregion

        #region Numbers

        public static T Max<T>(params T[] values)
            where T : IComparable<T>
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var max = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i].CompareTo(max) == 1)
                    max = values[i];

            return max;
        }
        public static T Max<T>(IEnumerable<T> values)
            where T : IComparable<T>
        {
            Guard.ArgumentNotNull(values, "values");

            var iterator = values.GetEnumerator();
            if (!iterator.MoveNext())
                throw new ArgumentException();

            var max = iterator.Current;
            while (iterator.MoveNext())
                if (iterator.Current.CompareTo(max) == 1)
                    max = iterator.Current;

            return max;
        }
        public static byte Max(params byte[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var max = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];

            return max;
        }
        public static short Max(params short[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var max = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];

            return max;
        }
        public static int Max(params int[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var max = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];

            return max;
        }
        public static long Max(params long[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var max = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];

            return max;
        }
        public static float Max(params float[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var max = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];

            return max;
        }
        public static double Max(params double[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var max = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];

            return max;
        }

        public static T Min<T>(params T[] values)
            where T : IComparable<T>
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var min = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i].CompareTo(min) == -1)
                    min = values[i];

            return min;
        }
        public static T Min<T>(IEnumerable<T> values)
            where T : IComparable<T>
        {
            Guard.ArgumentNotNull(values, "values");

            var iterator = values.GetEnumerator();
            if (!iterator.MoveNext())
                throw new ArgumentException();

            var max = iterator.Current;
            while (iterator.MoveNext())
                if (iterator.Current.CompareTo(max) == -1)
                    max = iterator.Current;

            return max;
        }
        public static byte Min(params byte[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var min = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];

            return min;
        }
        public static short Min(params short[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var min = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];

            return min;
        }
        public static int Min(params int[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var min = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];

            return min;
        }
        public static long Min(params long[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var min = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];

            return min;
        }
        public static float Min(params float[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var min = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];

            return min;
        }
        public static double Min(params double[] values)
        {
            Guard.ArgumentNotNullOrEmpty(values, "values");

            var min = values[0];
            for (var i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];

            return min;
        }

        public static T ValueToRange<T>(T value, T rangeFrom, T rangeTo)
            where T : IComparable<T>
        {
            if (value.CompareTo(rangeFrom) == -1)
                return rangeFrom;
            if (value.CompareTo(rangeTo) == 1)
                return rangeTo;
            return value;
        }
        public static byte ValueToRange(byte value, byte rangeFrom, byte rangeTo)
        {
            if (value < rangeFrom)
                return rangeFrom;
            if (value > rangeTo)
                return rangeTo;
            return value;
        }
        public static short ValueToRange(short value, short rangeFrom, short rangeTo)
        {
            if (value < rangeFrom)
                return rangeFrom;
            if (value > rangeTo)
                return rangeTo;
            return value;
        }
        public static int ValueToRange(int value, int rangeFrom, int rangeTo)
        {
            if (value < rangeFrom)
                return rangeFrom;
            if (value > rangeTo)
                return rangeTo;
            return value;
        }
        public static long ValueToRange(long value, long rangeFrom, long rangeTo)
        {
            if (value < rangeFrom)
                return rangeFrom;
            if (value > rangeTo)
                return rangeTo;
            return value;
        }
        public static float ValueToRange(float value, float rangeFrom, float rangeTo)
        {
            if (value < rangeFrom)
                return rangeFrom;
            if (value > rangeTo)
                return rangeTo;
            return value;
        }
        public static double ValueToRange(double value, double rangeFrom, double rangeTo)
        {
            if (value < rangeFrom)
                return rangeFrom;
            if (value > rangeTo)
                return rangeTo;
            return value;
        }
        public static DateTime ValueToRange(DateTime value, DateTime rangeFrom, DateTime rangeTo)
        {
            if (value < rangeFrom)
                return rangeFrom;
            if (value > rangeTo)
                return rangeTo;
            return value;
        }

        public static int ModuloFloor(int value, int modulo)
        {
            return value - value % modulo;
        }
        public static long ModuloFloor(long value, long modulo)
        {
            return value - value % modulo;
        }
        public static int ModuloCeiling(int value, int modulo)
        {
            var rest = value % modulo;
            return rest == 0
                ? value
                : (value + (modulo - rest));
        }
        public static long ModuloCeiling(long value, long modulo)
        {
            var rest = value % modulo;
            return rest == 0
                ? value
                : (value + (modulo - rest));
        }

        #endregion

        #region Operations

        public static void Swap<T>(ref T a, ref T b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

        #endregion

        #region Exceptions

        private static void ThrowInvalidCastException(Type sourceType, Type destinationType, Exception innerException)
        {
            throw new InvalidCastException(string.Format("Invalid cast from {0} to {1}.", sourceType, destinationType), innerException);
        }

        #endregion

        private static bool TryFromOADate(double value, out DateTime dateTime)
        {
            if (!(value >= 2958466.0) && !(value <= -657435.0))
            {
                long num1 = (long)(value * 86400000.0 + (value >= 0.0 ? 0.5 : -0.5));
                if (num1 < 0L)
                {
                    long num2 = num1;
                    long num3 = 86400000;
                    long num4 = num2 % num3 * 2L;
                    num1 = num2 - num4;
                }

                long num5 = num1 + 59926435200000L;
                if (num5 >= 0L && num5 < 315537897600000L)
                {
                    dateTime = new DateTime(num5 * 10000L, DateTimeKind.Unspecified);
                    return true;
                }
            }

            dateTime = DateTime.MinValue;
            return false;
        }
    }
}
