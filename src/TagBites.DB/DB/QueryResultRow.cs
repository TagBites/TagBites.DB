using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.DB
{
    public sealed class QueryResultRow : IList<object>
    {
        private readonly QueryResult m_queryDataProvider;
        private int m_row;

        bool ICollection<object>.IsReadOnly => true;
        int ICollection<object>.Count => m_queryDataProvider.ColumnCount;
        public int ColumnCount => m_queryDataProvider.ColumnCount;
        public int RowIndex
        {
            get => m_row;
            internal set => m_row = value;
        }

        object IList<object>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }
        public object this[int columnIndex] => m_queryDataProvider[m_row, columnIndex];
        public object this[string columnName] => m_queryDataProvider[m_row, columnName];

        internal QueryResultRow(QueryResult queryDataProvider, int row)
        {
            Guard.ArgumentNotNull(queryDataProvider, "queryDataProvider");
            Guard.ArgumentIndexInRange(row, "row", queryDataProvider.RowCount);

            m_queryDataProvider = queryDataProvider;
            m_row = row;
        }

        public bool ContainsColumn(string columnName) => m_queryDataProvider.ContainsColumn(columnName);
        public int GetColumnIndex(string columnName) => m_queryDataProvider.GetColumnIndex(columnName);
        public string GetColumnName(int column) => m_queryDataProvider.GetColumnName(column);

        public object GetValue(string columnName)
        {
            return m_queryDataProvider.GetValue(m_row, columnName);
        }
        public T GetValue<T>(string columnName)
        {
            return m_queryDataProvider.GetValue<T>(m_row, columnName);
        }
        public T GetValue<T>(string columnName, T defaultValue)
        {
            return m_queryDataProvider.GetValue<T>(m_row, columnName, defaultValue);
        }

        public object GetValue(int columnIndex)
        {
            return m_queryDataProvider.GetValue(m_row, columnIndex);
        }
        public T GetValue<T>(int columnIndex)
        {
            return m_queryDataProvider.GetValue<T>(m_row, columnIndex);
        }
        public T GetValue<T>(int columnIndex, T defaultValue)
        {
            return m_queryDataProvider.GetValue<T>(m_row, columnIndex, defaultValue);
        }

        bool ICollection<object>.Contains(object item)
        {
            return ((IList<object>)this).IndexOf(item) != -1;
        }
        int IList<object>.IndexOf(object item)
        {
            if (DataHelper.IsNull(item))
                return -1;

            var count = ColumnCount;
            for (var i = 0; i < count; i++)
                if (Equals(item, this[i]))
                    return i;

            return -1;
        }
        void ICollection<object>.CopyTo(object[] array, int arrayIndex)
        {
            var count = ColumnCount;
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

        private IEnumerable<object> GetEnumerable()
        {
            for (int i = 0; i < m_queryDataProvider.ColumnCount; i++)
                yield return m_queryDataProvider[m_row, i];
        }
        public IEnumerator<object> GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
