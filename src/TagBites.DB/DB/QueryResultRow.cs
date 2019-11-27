using System;
using System.Collections;
using System.Collections.Generic;
using TagBites.Utils;

namespace TagBites.DB
{
    public sealed class QueryResultRow : IList<object>
    {
        private readonly QueryResult _source;

        bool ICollection<object>.IsReadOnly => true;
        int ICollection<object>.Count => _source.ColumnCount;

        public int ColumnCount => _source.ColumnCount;
        public int RowIndex { get; internal set; }

        object IList<object>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }
        public object this[int columnIndex] => _source[RowIndex, columnIndex];
        public object this[string columnName] => _source[RowIndex, columnName];

        internal QueryResultRow(QueryResult source, int row)
        {
            Guard.ArgumentNotNull(source, nameof(source));
            Guard.ArgumentIndexInRange(row, nameof(row), source.RowCount);

            _source = source;
            RowIndex = row;
        }


        public bool ContainsColumn(string columnName) => _source.ContainsColumn(columnName);
        public int GetColumnIndex(string columnName) => _source.GetColumnIndex(columnName);
        public string GetColumnName(int column) => _source.GetColumnName(column);

        public object GetValue(string columnName) => _source.GetValue(RowIndex, columnName);
        public T GetValue<T>(string columnName) => _source.GetValue<T>(RowIndex, columnName);

        public object GetValue(int columnIndex) => _source.GetValue(RowIndex, columnIndex);
        public T GetValue<T>(int columnIndex) => _source.GetValue<T>(RowIndex, columnIndex);

        bool ICollection<object>.Contains(object item) => ((IList<object>)this).IndexOf(item) != -1;
        int IList<object>.IndexOf(object item)
        {
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

        void ICollection<object>.Add(object item) => throw new NotSupportedException();
        void IList<object>.Insert(int index, object item) => throw new NotSupportedException();
        bool ICollection<object>.Remove(object item) => throw new NotSupportedException();
        void IList<object>.RemoveAt(int index) => throw new NotSupportedException();
        void ICollection<object>.Clear() => throw new NotSupportedException();

        public IEnumerator<object> GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();

            IEnumerable<object> GetEnumerable()
            {
                for (var i = 0; i < _source.ColumnCount; i++)
                    yield return _source[RowIndex, i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
