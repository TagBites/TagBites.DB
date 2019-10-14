using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.DB
{
    public abstract class QueryResult : IEnumerable<QueryResultRow>
    {
        public static readonly QueryResult Empty = new EmptyProvider();

        public abstract int RowCount { get; }
        public abstract int ColumnCount { get; }

        public abstract object this[int row, string columnName] { get; }
        public abstract object this[int row, int column] { get; }


        public abstract bool ContainsColumn(string columnName);
        public abstract int GetColumnIndex(string columnName);
        public abstract string GetColumnName(int column);

        public object GetValue(int rowIndex, string columnName)
        {
            return this[rowIndex, columnName];
        }
        public T GetValue<T>(int rowIndex, string columnName)
        {
            return DataHelper.TryChangeTypeDefault<T>(this[rowIndex, columnName]);
        }
        public T GetValue<T>(int rowIndex, string columnName, T defaultValue)
        {
            return DataHelper.TryChangeTypeDefault<T>(this[rowIndex, columnName], defaultValue);
        }

        public object GetValue(int rowIndex, int columnIndex)
        {
            return this[rowIndex, columnIndex];
        }
        public T GetValue<T>(int rowIndex, int columnIndex)
        {
            return DataHelper.TryChangeTypeDefault<T>(this[rowIndex, columnIndex]);
        }
        public T GetValue<T>(int rowIndex, int columnIndex, T defaultValue)
        {
            return DataHelper.TryChangeTypeDefault<T>(this[rowIndex, columnIndex], defaultValue);
        }

        public QueryResultRow GetRow(int index)
        {
            return new QueryResultRow(this, index);
        }

        private IEnumerable<QueryResultRow> GetEnumerable()
        {
            for (int i = 0; i < RowCount; i++)
                yield return new QueryResultRow(this, i);
        }
        public IEnumerator<QueryResultRow> GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public object ToScalar() => RowCount == 0 ? null : GetValue(0, 0);
        public T ToScalar<T>(T defaultValue = default(T)) => RowCount == 0 ? defaultValue : GetValue<T>(0, 0);
        public IList<object> ToScalars()
        {
            var list = new object[RowCount];

            for (int i = 0; i < RowCount; i++)
                list[i] = GetValue(i, 0);

            return list;
        }
        public IList<T> ToColumnScalars<T>(T defaultValue = default(T))
        {
            var list = new T[RowCount];

            for (int i = 0; i < RowCount; i++)
                list[i] = GetValue<T>(i, 0, defaultValue);

            return list;
        }
        public IList<T> ToRowScalars<T>(T defaultValue = default(T))
        {
            var list = new T[ColumnCount];

            for (int i = 0; i < ColumnCount; i++)
                list[i] = GetValue<T>(0, i, defaultValue);

            return list;
        }

        public QueryObjectResult<T> ToObjects<T>()
        {
            return new QueryObjectResult<T>(this);
        }
        public QueryObjectResult<T> ToObjects<T>(QueryObjectResultPropertyResolver customPropertyResolver)
        {
            return new QueryObjectResult<T>(this, customPropertyResolver);
        }
        public QueryObjectResult<T> ToObjects<T>(QueryObjectResultPropertyResolver customPropertyResolver, QueryObjectResultItemFiller<T> additionalFiller)
        {
            return new QueryObjectResult<T>(this, customPropertyResolver, additionalFiller);
        }

        internal static QueryResult Create(DataTable table)
        {
            return new DataTableProvider(table);
        }

        private class EmptyProvider : QueryResult
        {
            public override int RowCount => 0;
            public override int ColumnCount => 0;

            public override object this[int row, string columnName] => null;
            public override object this[int row, int column] => null;


            public override bool ContainsColumn(string columnName) { return false; }
            public override int GetColumnIndex(string columnName) { return -1; }
            public override string GetColumnName(int column) { return null; }
        }
        private class DataTableProvider : QueryResult
        {
            private readonly DataTable m_data;

            public override int RowCount => m_data.Rows.Count;
            public override int ColumnCount => m_data.Columns.Count;

            public override object this[int row, string columnName]
            {
                get
                {
                    var v = m_data.Rows[row][columnName];
                    return v is DBNull ? null : v;
                }
            }
            public override object this[int row, int column]
            {
                get
                {
                    var v = m_data.Rows[row][column];
                    return v is DBNull ? null : v;
                }
            }

            public DataTableProvider(DataTable table)
            {
                Guard.ArgumentNotNull(table, "table");
                m_data = table;
            }


            public override bool ContainsColumn(string columnName)
            {
                return m_data.Columns.Contains(columnName);
            }
            public override int GetColumnIndex(string columnName)
            {
                return m_data.Columns.IndexOf(columnName);
            }
            public override string GetColumnName(int column)
            {
                return m_data.Columns[column].ColumnName;
            }
        }
    }
}
