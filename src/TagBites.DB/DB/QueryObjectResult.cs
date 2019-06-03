using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TBS.Utils;

namespace TBS.Data.DB
{
    public delegate void QueryObjectResultItemFiller<T>(T item, QueryResultRow rowData);

    public delegate object QueryObjectResultPropertyResolver(PropertyInfo property, QueryResultRow rowData);

    public class QueryObjectResult<T> : IList<T>, IList
    {
        private QueryResult m_dataProvider;
        private QueryObjectInitializer m_initializer;
        private readonly QueryObjectResultPropertyResolver m_customPropertyResolver;
        private readonly QueryObjectResultItemFiller<T> m_filler;
        private readonly List<T> m_items;

        public int Count { get; }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                if (index >= m_items.Count)
                {
                    var rowDataProvider = new QueryResultRow(m_dataProvider, 0);

                    for (int rowIndex = m_items.Count; rowIndex <= index; ++rowIndex)
                    {
                        rowDataProvider.RowIndex = rowIndex;
                        T row = (T)CreateItem(m_initializer, rowIndex, rowDataProvider);

                        if (m_filler != null)
                            m_filler(row, rowDataProvider);

                        m_items.Add(row);
                    }
                }

                if (index + 1 == Count)
                {
                    m_initializer = null;
                    m_dataProvider = null;
                }

                return m_items[index];
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
            Guard.ArgumentNotNull(dataProvider, "dataProvider");

            m_dataProvider = dataProvider;
            m_customPropertyResolver = customPropertyResolver;
            m_filler = filler;
            m_initializer = CreateInitializer(typeof(T));
            m_items = new List<T>(m_dataProvider.RowCount);

            Count = m_dataProvider.RowCount;
        }
        internal QueryObjectResult(QueryResult dataProvider, QueryObjectInitializer initializer)
        {
            Guard.ArgumentNotNull(dataProvider, "dataProvider");
            Guard.ArgumentNotNull(initializer, "initializer");

            m_dataProvider = dataProvider;
            m_initializer = initializer;
            m_items = new List<T>(m_dataProvider.RowCount);

            Count = m_dataProvider.RowCount;
        }


        private object CreateItem(QueryObjectInitializer initializer, int rowIndex, QueryResultRow rowDataProvider)
        {
            object[] parameters;

            if (initializer.Parameters == null || initializer.Parameters.Length <= 0)
                parameters = Array.Empty<object>();
            else
            {
                parameters = new object[initializer.Parameters.Length];

                for (int i = 0; i < initializer.Parameters.Length; i++)
                {
                    var queryObjectParameter = initializer.Parameters[i];
                    if (queryObjectParameter.Initializer != null)
                        parameters[i] = CreateItem(queryObjectParameter.Initializer, rowIndex, rowDataProvider);
                    else if (queryObjectParameter.ColumnIndex != -1)
                        parameters[i] = m_dataProvider[rowIndex, queryObjectParameter.ColumnIndex];
                    else
                        parameters[i] = queryObjectParameter.LocalValue;
                }
            }

            var result = Activator.CreateInstance(initializer.Type, parameters);
            for (int i = 0; i < initializer.Properties.Length; i++)
            {
                var queryObjectProperty = initializer.Properties[i];
                // complex value
                if (queryObjectProperty.Initializer != null)
                {
                    queryObjectProperty.PropertyInfo.SetValue(
                        result,
                        CreateItem(queryObjectProperty.Initializer, rowIndex, rowDataProvider),
                        null);
                }
                // simple remote value
                else if (queryObjectProperty.ColumnIndex != -1)
                {
                    queryObjectProperty.PropertyInfo.SetValue(
                        result,
                        DataHelper.TryChangeTypeDefault(
                            m_dataProvider[rowIndex, queryObjectProperty.ColumnIndex],
                            queryObjectProperty.PropertyInfo.PropertyType,
                            null),
                        null);
                }
                // 
                else if (m_customPropertyResolver != null)
                {
                    var value = m_customPropertyResolver(queryObjectProperty.PropertyInfo, rowDataProvider);
                    if (value != null)
                        queryObjectProperty.PropertyInfo.SetValue(result, value, null);
                }
                // simple local value
                else if (queryObjectProperty.PropertyInfo.CanWrite && queryObjectProperty.LocalValue != null)
                {
                    queryObjectProperty.PropertyInfo.SetValue(
                        result,
                        queryObjectProperty.LocalValue,
                        null);
                }
            }

            return result;
        }
        private QueryObjectInitializer CreateInitializer(Type type)
        {
            var constructor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.GetParameters().Length == 0);
            if (constructor == null)
                throw new Exception();

            var parameterInfos = constructor.GetParameters();
            if (parameterInfos.Length > 0)
                throw new Exception();

            var queryParameters = new QueryObjectParameter[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                //var initializer = propertiesInfos[i].PropertyType.BaseType == typeof(object) && propertiesInfos[i].PropertyType.BaseType != typeof(string)
                //    ? CreateInitializer(parameterInfos[i].ParameterType)
                //    : null;
                queryParameters[i] = new QueryObjectParameter(m_dataProvider.GetColumnIndex(parameterInfos[i].Name), parameterInfos[i], null);
            }

            var propertiesInfos = TypeUtils.GetProperties(type).ToList();
            var queryProperties = new QueryObjectProperty[propertiesInfos.Count];
            for (int i = 0; i < propertiesInfos.Count; i++)
            {
                //var initializer = propertiesInfos[i].PropertyType.BaseType == typeof(object) && propertiesInfos[i].PropertyType.BaseType != typeof(string)
                //    ? CreateInitializer(propertiesInfos[i].PropertyType)
                //    : null;
                queryProperties[i] = new QueryObjectProperty(m_dataProvider.GetColumnIndex(propertiesInfos[i].Name), propertiesInfos[i], null);
            }

            return new QueryObjectInitializer(type, queryParameters, queryProperties);
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
