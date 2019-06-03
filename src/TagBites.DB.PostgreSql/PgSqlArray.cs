using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TBS.Data.DB.PostgreSql
{
    public class PgSqlArray : IEnumerable<string>
    {
        private int m_startIndex = 1;
        private readonly List<string> m_array;

        public int StartIndex
        {
            get => m_startIndex;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                m_startIndex = value;
            }
        }
        public int Length => m_array.Count;

        public virtual string this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));

                index -= m_startIndex;
                return (index < 0 || index >= m_array.Count)
                    ? null
                    : m_array[index];
            }
            set
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index));

                while (index < m_startIndex)
                {
                    m_array.Insert(0, null);
                    --m_startIndex;
                }

                index -= m_startIndex;
                while (index >= m_array.Count)
                    m_array.Add(null);

                m_array[index] = value;
            }
        }

        public PgSqlArray()
        {
            m_array = new List<string>();
            m_startIndex = 1;
        }
        public PgSqlArray(params string[] array)
            : this(array == null ? new string[] { null } : (IEnumerable<string>)array, 1)
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

            m_array = array.ToList();
            m_startIndex = startIndex;
        }


        public void Add(string value)
        {
            m_array.Add(value);
        }
        public bool Remove(string value)
        {
            return m_array.Remove(value);
        }
        public void RemoveAt(int index)
        {
            m_array.RemoveAt(index);
        }
        public PgSqlArray<T> ToTypedArray<T>() where T : struct
        {
            var array = new PgSqlArray<T>() { StartIndex = StartIndex };

            foreach (var item in m_array)
                array.Add(PgSqlArray<T>.TryFromString(item, out var v) ? v : default);

            return array;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<string> GetEnumerator()
        {
            return m_array.GetEnumerator();
        }

        protected bool Equals(PgSqlArray other)
        {
            return m_array.SequenceEqual(other.m_array);
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
            return m_array != null ? m_array.GetHashCode() : 0;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (m_startIndex != 1)
            {
                sb.Append('[');
                sb.Append(m_startIndex);
                sb.Append(':');
                sb.Append(m_startIndex + m_array.Count - 1);
                sb.Append(']');
                sb.Append('=');
            }

            sb.Append('{');
            for (int i = 0; i < m_array.Count; i++)
            {
                var item = m_array[i];
                if (i > 0)
                    sb.Append(',');

                if (item == null)
                    sb.Append("NULL");
                else if (item.Contains('\\') || item.Contains(' ') || item.Contains('"') || String.Equals(item, "null", StringComparison.OrdinalIgnoreCase))
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

        public static PgSqlArray TryParseDefault(string arrayString)
        {
            PgSqlArray array;
            return TryParse(arrayString, out array) ? array : null;
        }
        public static bool TryParse(string arrayString, out PgSqlArray array)
        {
            List<string> items;
            int startIndex;

            if (!string.IsNullOrEmpty(arrayString) && TryParse(arrayString, out items, out startIndex))
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

            return String.Equals(source, "NULL", StringComparison.OrdinalIgnoreCase)
                ? null
                : source;
        }
    }

    public class PgSqlArray<T> : IEnumerable<T?>
        where T : struct
    {
        private readonly PgSqlArray m_array;

        public int StartIndex
        {
            get => m_array.StartIndex;
            set => m_array.StartIndex = value;
        }
        public int Length => m_array.Length;

        public virtual T? this[int index]
        {
            get => FromString(m_array[index]);
            set => m_array[index] = value.HasValue ? Convert.ToString(value.Value, CultureInfo.InvariantCulture) : null;
        }

        public PgSqlArray()
        {
            m_array = new PgSqlArray();
        }
        public PgSqlArray(params T?[] items)
        {
            m_array = new PgSqlArray();

            if (items == null)
                Add((T?)null);
            else
                foreach (var item in items)
                    Add(item);
        }
        public PgSqlArray(IEnumerable<T?> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            m_array = new PgSqlArray();

            foreach (var item in items)
                Add(item);
        }
        private PgSqlArray(PgSqlArray array)
        {
            m_array = array ?? throw new ArgumentNullException(nameof(array));
        }


        public void Add(T? value)
        {
            m_array.Add(value.HasValue ? Convert.ToString(value.Value, CultureInfo.InvariantCulture) : null);
        }
        public bool Remove(T? value)
        {
            return m_array.Remove(value.HasValue ? Convert.ToString(value.Value, CultureInfo.InvariantCulture) : null);
        }
        public void RemoveAt(int index)
        {
            m_array.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<T?> GetEnumerator()
        {
            return m_array.Select(FromString).GetEnumerator();
        }

        protected bool Equals(PgSqlArray<T> other)
        {
            return Equals(m_array, other.m_array);
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
            return (m_array != null ? m_array.GetHashCode() : 0);
        }
        public override string ToString()
        {
            return m_array.ToString();
        }

        internal static T? FromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return (T?)null;

            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
        internal static bool TryFromString(string value, out T? result)
        {
            try
            {
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
            PgSqlArray<T> array;
            return TryParse(arrayString, out array) ? array : null;
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
