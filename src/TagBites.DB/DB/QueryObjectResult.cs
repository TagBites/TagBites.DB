using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TagBites.DB.Configuration;
using TagBites.Utils;

namespace TagBites.DB
{
    public delegate void QueryObjectResultItemFiller<T>(T item, QueryResultRow rowData);

    public delegate object QueryObjectResultPropertyResolver(PropertyInfo property, QueryResultRow rowData);

    public class QueryObjectResult<T> : IList<T>, IList
    {
        private readonly QueryObjectBinder _binder;
        private readonly int[] _columnIndexes;
        private readonly QueryObjectResultPropertyResolver _customPropertyResolver;
        private readonly QueryObjectResultItemFiller<T> _filler;
        private readonly List<T> _items;
        private QueryResult _dataProvider;

        public int Count { get; }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                if (index >= _items.Count)
                {
                    var rowDataProvider = new QueryResultRow(_dataProvider, 0);

                    for (var rowIndex = _items.Count; rowIndex <= index; ++rowIndex)
                    {
                        rowDataProvider.RowIndex = rowIndex;
                        var row = CreateItem(rowIndex, rowDataProvider);

                        _filler?.Invoke(row, rowDataProvider);

                        _items.Add(row);
                    }
                }

                if (index + 1 == Count)
                    _dataProvider = null;

                return _items[index];
            }
        }

        public QueryObjectResult(QueryResult dataProvider)
            : this(dataProvider, null, null)
        { }
        public QueryObjectResult(QueryResult dataProvider, QueryObjectResultPropertyResolver customPropertyResolver)
            : this(dataProvider, customPropertyResolver, null)
        { }
        public QueryObjectResult(QueryResult dataProvider, QueryObjectResultPropertyResolver customPropertyResolver, QueryObjectResultItemFiller<T> filler)
        {
            Guard.ArgumentNotNull(dataProvider, nameof(dataProvider));

            _dataProvider = dataProvider;
            _customPropertyResolver = customPropertyResolver;
            _filler = filler;
            _binder = QueryObjectBinder.Get(typeof(T));
            _items = new List<T>(dataProvider.RowCount);

            var properties = _binder.Properties;
            _columnIndexes = new int[properties.Length];

            for (var i = 0; i < properties.Length; i++)
                _columnIndexes[i] = dataProvider.GetColumnIndex(properties[i].PropertyInfo.Name);

            Count = dataProvider.RowCount;
        }


        private T CreateItem(int rowIndex, QueryResultRow rowDataProvider)
        {
            var item = (T)_binder.Factory();
            var properties = _binder.Properties;

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var columnIndex = _columnIndexes[i];

                if (columnIndex != -1)
                {
                    var value = DbLinkDataConverter.Default.ChangeType(_dataProvider[rowIndex, columnIndex], property.PropertyType);
                    property.Setter(item, value);
                }
                else if (_customPropertyResolver != null)
                {
                    var value = _customPropertyResolver(property.PropertyInfo, rowDataProvider);
                    if (value != null)
                        property.Setter(item, value);
                }
            }

            return item;
        }

        #region IList<T>

        bool ICollection<T>.IsReadOnly => true;

        T IList<T>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }


        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
                if (Equals(item, this[i]))
                    return i;

            return -1;
        }
        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
                array[i + arrayIndex] = this[i];
        }

        void ICollection<T>.Add(T item) { throw new NotSupportedException(); }
        void IList<T>.Insert(int index, T item) { throw new NotSupportedException(); }

        void IList<T>.RemoveAt(int index) { throw new NotSupportedException(); }
        bool ICollection<T>.Remove(T item) { throw new NotSupportedException(); }
        void ICollection<T>.Clear() { throw new NotSupportedException(); }

        public IEnumerator<T> GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<T> GetEnumerable()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        #endregion

        #region IList

        bool IList.IsFixedSize => true;
        bool IList.IsReadOnly => true;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => null;
        object IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        int IList.Add(object value) { throw new NotSupportedException(); }
        void IList.Insert(int index, object value) { throw new NotSupportedException(); }
        void IList.Remove(object value) { throw new NotSupportedException(); }
        void IList.RemoveAt(int index) { throw new NotSupportedException(); }
        void IList.Clear() { throw new NotSupportedException(); }

        bool IList.Contains(object value)
        {
            if (value is T)
                return Contains((T)value);
            return false;
        }
        int IList.IndexOf(object value)
        {
            if (value is T)
                return IndexOf((T)value);
            return -1;
        }
        void ICollection.CopyTo(Array array, int index)
        {
            for (int i = 0; i < Count; i++)
                array.SetValue(this[i], i + index);
        }

        #endregion
    }
}
