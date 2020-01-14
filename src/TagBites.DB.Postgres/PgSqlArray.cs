using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TagBites.DB.Postgres
{
    public interface IPgSqlArray : IEnumerable
    {
        Type ElementType { get; }
    }

    public class PgSqlArray : IPgSqlArray, IEnumerable<string>
    {
        private int _startIndex;
        private readonly List<string> _array;

        public int StartIndex
        {
            get => _startIndex;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _startIndex = value;
            }
        }
        public int Length => _array.Count;
        public Type ElementType => typeof(string);

        public virtual string this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));

                index -= _startIndex;
                return (index < 0 || index >= _array.Count)
                    ? null
                    : _array[index];
            }
            set
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));

                while (index < _startIndex)
                {
                    _array.Insert(0, null);
                    --_startIndex;
                }

                index -= _startIndex;
                while (index >= _array.Count)
                    _array.Add(null);

                _array[index] = value;
            }
        }

        public PgSqlArray()
        {
            _array = new List<string>();
            _startIndex = 1;
        }
        public PgSqlArray(params string[] array)
            : this(array == null ? new string[] { null } : array, 1)
        { }
        public PgSqlArray(IEnumerable<string> array)
            : this(array, 1)
        { }
        public PgSqlArray(IEnumerable<string> array, int startIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            _array = array.ToList();
            _startIndex = startIndex;
        }


        public void Add(string value)
        {
            _array.Add(value);
        }
        public bool Remove(string value)
        {
            return _array.Remove(value);
        }
        public void RemoveAt(int index)
        {
            _array.RemoveAt(index);
        }
        public PgSqlArray<T> ToTypedArray<T>() where T : struct
        {
            var array = new PgSqlArray<T>() { StartIndex = StartIndex };

            foreach (var item in _array)
                array.Add(PgSqlArray<T>.TryFromString(item, out var v) ? v : default);

            return array;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<string> GetEnumerator()
        {
            return _array.GetEnumerator();
        }

        protected bool Equals(PgSqlArray other)
        {
            return _array.SequenceEqual(other._array);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((PgSqlArray)obj);
        }
        public override int GetHashCode()
        {
            return _array != null ? _array.GetHashCode() : 0;
        }
        public override string ToString()
        {
            if (Length == 0)
                return "{}";

            var sb = new StringBuilder();
            if (_startIndex != 1)
            {
                sb.Append('[');
                sb.Append(_startIndex);
                sb.Append(':');
                sb.Append(_startIndex + _array.Count - 1);
                sb.Append(']');
                sb.Append('=');
            }

            sb.Append('{');
            for (int i = 0; i < _array.Count; i++)
            {
                var item = _array[i];
                if (i > 0)
                    sb.Append(',');

                if (item == null)
                    sb.Append("NULL");
                else if (item.Length == 0 || item.Contains('\\') || item.Contains(' ') || item.Contains('"') || string.Equals(item, "null", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append('"');
                    sb.Append(item.Replace("\\", "\\\\").Replace("\"", "\\\""));
                    sb.Append('"');
                }
                else
                    sb.Append(item);
            }
            sb.Append('}');

            return sb.ToString();
        }

        public static IPgSqlArray TryCast(Array array)
        {
            if (array == null)
                return null;

            var elementType = array.GetType().GetElementType();
            if (elementType == null)
                return null;

            elementType = Nullable.GetUnderlyingType(elementType) ?? elementType;

            if (elementType == typeof(string))
                return new PgSqlArray((string[])array);

            if (elementType.IsValueType)
                return (IPgSqlArray)Activator.CreateInstance(typeof(PgSqlArray<>).MakeGenericType(elementType), array);

            return null;
        }
        public static PgSqlArray TryParseDefault(string arrayString)
        {
            return TryParse(arrayString, out var array) ? array : null;
        }
        public static bool TryParse(string arrayString, out PgSqlArray array)
        {
            if (!string.IsNullOrEmpty(arrayString) && TryParse(arrayString, out var items, out var startIndex))
            {
                array = new PgSqlArray(items, startIndex);
                return true;
            }

            array = null;
            return false;
        }
        private static bool TryParse(string arrayString, out List<string> array, out int startIndex)
        {
            array = null;
            startIndex = 1;
            int count = -1;

            try
            {
                arrayString = arrayString.Trim();

                if (arrayString.Length > 0 && arrayString[0] == '[')
                {
                    int last = arrayString.IndexOf(']', 1);
                    if (last == -1)
                        return false;

                    var parts = arrayString.Substring(1, last - 1).Split(':');
                    if (parts.Length != 2 || !int.TryParse(parts[0].Trim(), out startIndex) || !int.TryParse(parts[1].Trim(), out count) || startIndex < 0 || count < 0)
                        return false;

                    arrayString = arrayString.Substring(last + 1).Trim();
                    if (arrayString.Length == 0 || arrayString[0] != '=')
                        return false;

                    arrayString = arrayString.Substring(1).Trim();
                }

                if (arrayString.Length == 0 || arrayString[0] != '{' || arrayString[arrayString.Length - 1] != '}')
                    return false;

                arrayString = arrayString.Substring(1, arrayString.Length - 2).Trim();
                array = string.IsNullOrEmpty(arrayString)
                    ? new List<string>()
                    : TokenEnumeration(arrayString).ToList();

                if (count != -1 && (count - startIndex + 1) != array.Count)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
        private static IEnumerable<string> TokenEnumeration(string source)
        {
            bool wasQuoted = false;
            bool inQuoted = false;
            StringBuilder sb = new StringBuilder(source.Length);

            for (int idx = 0; idx < source.Length; ++idx)
            {
                char c = source[idx];
                switch (c)
                {
                    case '"': //entering of leaving a quoted string
                        inQuoted = !inQuoted;
                        wasQuoted = true;
                        break;
                    case ',': //ending this item, unless we're in a quoted string.
                        if (inQuoted)
                        {
                            sb.Append(',');
                        }
                        else
                        {
                            yield return Token(sb.ToString(), wasQuoted);
                            sb = new StringBuilder(source.Length - idx);
                            wasQuoted = false;
                        }
                        break;
                    case '\\': //next char is an escaped character, grab it, ignore the \ we are on now.
                        sb.Append(source[++idx]);
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            yield return Token(sb.ToString(), wasQuoted);
        }
        private static string Token(string source, bool wasQuoted)
        {
            if (wasQuoted)
                return source;

            source = source.Trim();

            return string.Equals(source, "NULL", StringComparison.OrdinalIgnoreCase)
                ? null
                : source;
        }
    }

    public class PgSqlArray<T> : IEnumerable<T?>
        where T : struct
    {
        private readonly PgSqlArray _array;

        public int StartIndex
        {
            get => _array.StartIndex;
            set => _array.StartIndex = value;
        }
        public int Length => _array.Length;
        public Type ElementType => typeof(T);

        public virtual T? this[int index]
        {
            get => FromString(_array[index]);
            set => _array[index] = value.HasValue ? Convert.ToString(value.Value, CultureInfo.InvariantCulture) : null;
        }

        public PgSqlArray()
        {
            _array = new PgSqlArray();
        }
        public PgSqlArray(params T?[] items)
        {
            _array = new PgSqlArray();

            if (items == null)
                Add(null);
            else
                foreach (var item in items)
                    Add(item);
        }
        public PgSqlArray(params T[] items)
        {
            _array = new PgSqlArray();

            if (items == null)
                Add(null);
            else
                foreach (var item in items)
                    Add(item);
        }
        public PgSqlArray(IEnumerable<T?> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _array = new PgSqlArray();

            foreach (var item in items)
                Add(item);
        }
        public PgSqlArray(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            _array = new PgSqlArray();

            foreach (var item in items)
                Add(item);
        }
        private PgSqlArray(PgSqlArray array)
        {
            _array = array ?? throw new ArgumentNullException(nameof(array));
        }


        public void Add(T? value)
        {
            _array.Add(value.HasValue ? Convert.ToString(value.Value, CultureInfo.InvariantCulture) : null);
        }
        public bool Remove(T? value)
        {
            return _array.Remove(value.HasValue ? Convert.ToString(value.Value, CultureInfo.InvariantCulture) : null);
        }
        public void RemoveAt(int index)
        {
            _array.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<T?> GetEnumerator()
        {
            return _array.Select(FromString).GetEnumerator();
        }

        protected bool Equals(PgSqlArray<T> other)
        {
            return Equals(_array, other._array);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((PgSqlArray<T>)obj);
        }
        public override int GetHashCode()
        {
            return (_array != null ? _array.GetHashCode() : 0);
        }
        public override string ToString()
        {
            return _array.ToString();
        }

        internal static T? FromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (typeof(T) == typeof(bool) && value.Length == 1)
            {
                var c = char.ToLower(value[0]);
                if (c == 't')
                    return (T)(object)true;

                if (c == 'f')
                    return (T)(object)false;
            }

            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
        internal static bool TryFromString(string value, out T? result)
        {
            try
            {
                if (typeof(T) == typeof(bool) && value?.Length == 1)
                {
                    var c = char.ToLower(value[0]);
                    if (c == 't')
                    {
                        result = (T)(object)true;
                        return true;
                    }

                    if (c == 'f')
                    {
                        result = (T)(object)false;
                        return true;
                    }
                }

                result = FromString(value);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static PgSqlArray<T> TryParseDefault(string arrayString)
        {
            return TryParse(arrayString, out var array) ? array : null;
        }
        public static bool TryParse(string arrayString, out PgSqlArray<T> array)
        {
            array = null;

            if (PgSqlArray.TryParse(arrayString, out var internalArray))
            {
                foreach (var item in internalArray)
                    if (!TryFromString(item, out _))
                        return false;

                array = new PgSqlArray<T>(internalArray);
                for (var i = 0; i < internalArray.Length; i++)
                    array[internalArray.StartIndex + i] = array[internalArray.StartIndex + i];

                return true;
            }

            return false;
        }
    }
}
