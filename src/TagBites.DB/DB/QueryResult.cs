using System;
using System.Collections.Generic;
using System.Data;
using TagBites.DB.Configuration;

namespace TagBites.DB
{
    public abstract class QueryResult : IEnumerable<QueryResultRow>
    {
        public static readonly QueryResult Empty = new EmptyProvider();

        public abstract int RowCount { get; }
        public abstract int ColumnCount { get; }

        public object this[int row, string columnName] => GetValue(row, columnName);
        public object this[int row, int column] => GetValue(row, column);


        public object GetValue(int rowIndex, string columnName)
        {
            var columnIndex = GetColumnIndex(columnName);
            return GetValue(rowIndex, columnIndex);
        }
        public T GetValue<T>(int rowIndex, string columnName)
        {
            var columnIndex = GetColumnIndex(columnName);
            return GetValue<T>(rowIndex, columnIndex);
        }

        public object GetValue(int rowIndex, int columnIndex)
        {
            var value = GetValueCore(rowIndex, columnIndex);
            return DbLinkDataConverter.Default.FromDbType(value);
        }
        public T GetValue<T>(int rowIndex, int columnIndex)
        {
            var value = GetValueCore(rowIndex, columnIndex);
            return DbLinkDataConverter.Default.ChangeType<T>(value);
        }

        public bool ContainsColumn(string columnName) => GetColumnIndex(columnName) >= 0;
        public abstract int GetColumnIndex(string columnName);
        public abstract string GetColumnName(int column);
        protected abstract object GetValueCore(int row, int column);

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
        public IList<T> ToColumnScalars<T>()
        {
            var list = new T[RowCount];

            for (var i = 0; i < RowCount; i++)
                list[i] = GetValue<T>(i, 0);

            return list;
        }
        public IList<T> ToRowScalars<T>()
        {
            var list = new T[ColumnCount];

            for (int i = 0; i < ColumnCount; i++)
                list[i] = GetValue<T>(0, i);

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


            public override int GetColumnIndex(string columnName) => -1;
            public override string GetColumnName(int column) => null;
            protected override object GetValueCore(int row, int column) => null;
        }
        private class DataTableProvider : QueryResult
        {
            private readonly DataTable _data;

            public override int RowCount => _data.Rows.Count;
            public override int ColumnCount => _data.Columns.Count;

            public DataTableProvider(DataTable table)
            {
                _data = table ?? throw new ArgumentNullException(nameof(table));
            }


            public override int GetColumnIndex(string columnName) => _data.Columns.IndexOf(columnName);
            public override string GetColumnName(int column) => _data.Columns[column].ColumnName;
            protected override object GetValueCore(int row, int column) => _data.Rows[row][column];
        }
    }
}
