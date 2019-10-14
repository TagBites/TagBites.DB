using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.DB
{
    public class InsertQueryBuilder
    {
        private readonly List<Record> m_records = new List<Record>();
        private readonly List<string> m_returningColumns = new List<string>();

        public string TableName { get; }

        public IEnumerable<string> Columns { get { return m_records.SelectMany(x => x.Values).Select(x => x.Key).Distinct(); } }
        public IEnumerable<string> ReturningColumns => m_returningColumns.AsEnumerable();

        public InsertQueryBuilder(string tableName)
        {
            Guard.ArgumentNotNullOrWhiteSpace(tableName, "tableName");
            TableName = tableName;
        }


        public Record AddRecord()
        {
            var record = new Record();
            m_records.Add(record);
            return record;
        }
        public void AddRecord(Record record)
        {
            Guard.ArgumentNotNull(record, "record");
            m_records.Add(record);
        }
        public void AddReturning(string columnName)
        {
            Guard.ArgumentNotNullOrEmpty(columnName, "columnName");
            m_returningColumns.Add(columnName);
        }

        public Query GetQuery()
        {
            if (m_records.Count == 0)
                return null;

            var columns = Columns.ToList();
            var parameters = new List<QueryParameter>();

            var sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0}(", TableName);

            for (int ci = 0; ci < columns.Count; ci++)
            {
                if (ci > 0)
                    sb.Append(", ");
                sb.Append(columns[ci]);
            }

            sb.Append(") VALUES ");

            for (int ri = 0; ri < m_records.Count; ri++)
            {
                var record = m_records[ri];

                if (ri > 0)
                    sb.Append(", ");

                sb.Append('(');

                for (int ci = 0; ci < columns.Count; ci++)
                {
                    var param = new QueryParameter(Query.ParameterPrefix + (parameters.Count + 1).ToString(), record[columns[ci]]);
                    parameters.Add(param);

                    if (ci > 0)
                        sb.Append(", ");
                    sb.Append(param.Name);
                }

                sb.Append(')');
            }

            if (m_returningColumns.Count > 0)
            {
                sb.Append(" RETURNING ");
                for (int ci = 0; ci < m_returningColumns.Count; ci++)
                {
                    if (ci > 0)
                        sb.Append(", ");
                    sb.Append(m_returningColumns[ci]);
                }
            }

            return new Query(sb.ToString(), parameters);
        }

        public class Record
        {
            private readonly Dictionary<string, object> m_values = new Dictionary<string, object>();

            public IEnumerable<KeyValuePair<string, object>> Values => m_values;

            public object this[string columnName]
            {
                get => m_values.TryGetValue(columnName, out var v) ? v : null;
                set => m_values[columnName] = value;
            }
        }
    }
}
