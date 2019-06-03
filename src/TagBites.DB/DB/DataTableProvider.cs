using System;
using System.Data;
using TBS.Utils;

namespace TBS.Data.DB
{
    internal class DataTableProvider : QueryResult
    {
        private readonly DataTable m_data;

        public override int RowCount => m_data.Rows.Count;
        public override int ColumnCount => m_data.Columns.Count;

        internal override object this[int row, string columnName]
        {
            get
            {
                var v = m_data.Rows[row][columnName];
                return v is DBNull ? null : v;
            }
        }
        internal override object this[int row, int column]
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
