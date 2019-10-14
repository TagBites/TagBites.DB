using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.DB.Utils
{
    public class DbTableChangerRecord
    {
        private readonly IDictionary<string, object> m_values;
        private MultiColumnKey m_multiColumnKey;

        internal DbTableChanger Owner { get; }

        public DbTableChangerRecordStatus Status { get; internal set; } = DbTableChangerRecordStatus.Pending;

        [Obsolete("Please use Key or Keys instead.", false)]
        public object Id
        {
            get => Key;
            set => Key = value;
        }

        public object Key
        {
            get
            {
                if (Owner.IsMultiColumnKey)
                    throw new InvalidOperationException("For multi-column key please use 'Keys' property.");

                return this[Owner.KeyColumnNamesCore[0]];
            }
            set
            {
                if (Owner.IsMultiColumnKey)
                    throw new InvalidOperationException("For multi-column key please use 'Keys' property.");

                this[Owner.KeyColumnNamesCore[0]] = value;
            }
        }
        public IList<object> Keys
        {
            get
            {
                if (m_multiColumnKey == null)
                    m_multiColumnKey = new MultiColumnKey(this);

                return m_multiColumnKey;
            }
        }
        public bool IsKeyEmpty
        {
            get
            {
                if (m_multiColumnKey != null)
                {
                    for (int i = 0; i < Owner.KeyColumnNamesCore.Length; i++)
                        if (this[Owner.KeyColumnNamesCore[i]] != null)
                            return false;

                    return true;
                }

                return this[Owner.KeyColumnNamesCore[0]] == null;
            }
        }

        public IEnumerable<KeyValuePair<string, object>> Values => m_values;

        public object this[string columnName]
        {
            get => m_values.TryGetValue(columnName, out var v) ? v : null;
            set
            {
                m_values[columnName] = value;
                Owner.Parameters.AddInputIfNotExists(columnName);
            }
        }

        internal DbTableChangerRecord(DbTableChanger owner)
        {
            Owner = owner;
            m_values = new Dictionary<string, object>();
        }
        internal DbTableChangerRecord(DbTableChanger owner, IDictionary<string, object> values)
        {
            Owner = owner;
            m_values = values;

            foreach (var item in values)
                Owner.Parameters.AddInputIfNotExists(item.Key);
        }


        internal void InternalSetValue(string columnName, string value)
        {
            m_values[columnName] = value;
        }

        private class MultiColumnKey : IList<object>
        {
            private readonly DbTableChangerRecord m_owner;

            bool ICollection<object>.IsReadOnly => false;
            public int Count => m_owner.Owner.KeyColumnNamesCore.Length;

            public object this[int index]
            {
                get => m_owner[m_owner.Owner.KeyColumnNamesCore[index]];
                set => m_owner[m_owner.Owner.KeyColumnNamesCore[index]] = value;
            }

            public MultiColumnKey(DbTableChangerRecord owner)
            {
                m_owner = owner;
            }


            public bool Contains(object item)
            {
                return IndexOf(item) != -1;
            }
            public int IndexOf(object item)
            {
                if (DataHelper.IsNull(item))
                    return -1;

                var count = Count;
                for (var i = 0; i < count; i++)
                    if (Equals(item, this[i]))
                        return i;

                return -1;
            }
            void ICollection<object>.CopyTo(object[] array, int arrayIndex)
            {
                var count = Count;
                for (var i = 0; i < count; i++)
                    array[arrayIndex + i] = this[i];
            }

            void ICollection<object>.Add(object item)
            {
                throw new NotSupportedException();
            }
            void IList<object>.Insert(int index, object item)
            {
                throw new NotSupportedException();
            }
            bool ICollection<object>.Remove(object item)
            {
                throw new NotSupportedException();
            }
            void IList<object>.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }
            void ICollection<object>.Clear()
            {
                throw new NotSupportedException();
            }

            public IEnumerator<object> GetEnumerator()
            {
                return m_owner.Owner.KeyColumnNamesCore.Select(x => m_owner[x]).GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
