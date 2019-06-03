using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Data.DB.Schema;

namespace TBS.Data.DB
{
    internal class StandardSqlSchema
    {
        public static DbSchema GetSchema(IDbLink link)
        {
            var schema = new DbSchema();
            schema.Tables = GetTables(link);

            var tables = new Dictionary<string, DbSchemaTable>();
            foreach (var item in schema.Tables)
            {
                tables[GetTableKey(item.Catalog, item.Schema, item.Name)] = item;

                item.Columns = new List<DbSchemaColumn>();
                item.ForeignKeys = new List<DbSchemaForeignKey>();
            }

            FillColumns(link, tables);
            FillPrimaryKeys(link, tables);
            FillForeignKeys(link, tables);

            return schema;
        }

        private static IList<DbSchemaTable> GetTables(IDbLink link)
        {
            var q = "SELECT table_catalog AS Catalog, table_schema AS Schema, table_name AS Name, (CASE WHEN table_type='BASE TABLE' THEN 0 ELSE 1 END) AS Type FROM information_schema.tables ORDER BY table_catalog, table_schema, table_name";
            var result = link.Execute(new Query(q));
            var reader = new QueryObjectResult<DbSchemaTable>(result);

            return reader.ToList();
        }
        private static void FillColumns(IDbLink link, Dictionary<string, DbSchemaTable> tables)
        {
            var q = "SELECT table_catalog, table_schema, table_name, column_name, ordinal_position, column_default, is_nullable='YES', udt_name FROM information_schema.columns ORDER BY ordinal_position";
            var data = link.Execute(new Query(q));

            for (int i = 0; i < data.RowCount; i++)
            {
                var tableKey = GetTableKey(data[i, 0], data[i, 1], data[i, 2]);
                var table = tables.TryGetValue(tableKey, out var v) ? v : null;
                if (table == null)
                    continue;

                var tc = GetTypeConverter();

                var column = new DbSchemaColumn()
                {
                    Name = data.GetValue<string>(i, 3),
                    Ordinal = data.GetValue<int>(i, 4),
                    DefaultValue = data.GetValue<string>(i, 5),
                    IsNullable = data.GetValue<bool>(i, 6),
                    DbTypeName = data.GetValue<string>(i, 7),
                    Type = tc.TryGetValue(data.GetValue<string>(i, 7), out var v2) ? v2 : typeof(string)
                };
                table.Columns.Add(column);
            }
        }
        private static void FillPrimaryKeys(IDbLink link, Dictionary<string, DbSchemaTable> tables)
        {
            var q = @"SELECT tc.table_catalog, tc.table_schema, tc.table_name, tc.constraint_name, kc.column_name, ordinal_position
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kc ON (kc.table_name = tc.table_name AND kc.table_schema = tc.table_schema AND kc.constraint_name = tc.constraint_name)
WHERE tc.constraint_type = 'PRIMARY KEY'";
            var data = link.Execute(new Query(q));

            var items = data.Select(x => Tuple.Create(GetTableKey(x[0], x[1], x[2]), (string)x[3], (string)x[4], (int)x[5])).GroupBy(x => x.Item1);
            foreach (var item in items)
            {
                var table = tables.TryGetValue(item.Key, out var v) ? v : null;
                if (table == null)
                    continue;

                foreach (var columnInfo in item.OrderBy(x => x.Item4))
                {
                    var column = table.Columns.FirstOrDefault(x => x.Name == columnInfo.Item3);
                    if (column == null)
                        continue;

                    if (table.PrimaryKey == null)
                        table.PrimaryKey = new DbSchemaPrimaryKey() { Columns = new List<DbSchemaColumn>() };

                    table.PrimaryKey.ConstraintName = columnInfo.Item2;
                    table.PrimaryKey.Columns.Add(column);
                }
            }
        }
        private static void FillForeignKeys(IDbLink link, Dictionary<string, DbSchemaTable> tables)
        {
            var q = @"SELECT tc.constraint_name, 
tc.table_catalog, tc.table_schema, tc.table_name, kcu.column_name,
ccu.table_catalog, ccu.table_schema, ccu.table_name, ccu.column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name
WHERE constraint_type = 'FOREIGN KEY'
ORDER BY kcu.ordinal_position";
            var data = link.Execute(new Query(q));

            for (int i = 0; i < data.RowCount; i++)
            {
                string constraintName = (string)data[i, 0];
                var tableName = GetTableKey(data[i, 1], data[i, 2], data[i, 3]);
                var tableColumnName = (string)data[i, 4];
                var foreignTableName = GetTableKey(data[i, 5], data[i, 6], data[i, 7]);
                var foreignColumnName = (string)data[i, 8];

                var table = tables.TryGetValue(tableName, out var v) ? v : null;
                var foreignTable = tables.TryGetValue(foreignTableName, out var v2) ? v2 : null;
                if (table == null || foreignTable == null)
                    continue;

                var tableColumn = table.Columns.FirstOrDefault(x => x.Name == tableColumnName);
                var foreignColumn = foreignTable.Columns.FirstOrDefault(x => x.Name == foreignColumnName);
                if (tableColumn == null || foreignColumn == null)
                    continue;

                var fk = table.ForeignKeys.FirstOrDefault(x => x.ConstraintName == constraintName);
                if (fk == null)
                {
                    fk = new DbSchemaForeignKey()
                    {
                        ConstraintName = constraintName,
                        ForeignTable = foreignTable,
                        Columns = new List<DbSchemaColumn>(),
                        ForeignColumns = new List<DbSchemaColumn>()
                    };

                    table.ForeignKeys.Add(fk);
                }

                fk.Columns.Add(tableColumn);
                fk.ForeignColumns.Add(foreignColumn);
            }
        }
        private static string GetTableKey(object catalog, object schema, object table)
        {
            return String.Format("{0}.{1}.{2}", catalog, schema, table);
        }

        private static Dictionary<string, Type> GetTypeConverter()
        {
            return new Dictionary<string, Type>()
            {
                { "int8", typeof(Int64) },
                { "bool", typeof(Boolean) },
                { "bytea", typeof(Byte[]) },
                { "date", typeof(DateTime) },
                { "float8", typeof(Double) },
                { "int4", typeof(Int32) },
                { "money", typeof(Decimal) },
                { "numeric", typeof(Decimal) },
                { "float4", typeof(Single) },
                { "int2", typeof(Int16) },
                { "text", typeof(String) },
                { "time", typeof(DateTime) },
                { "timetz", typeof(DateTime) },
                { "timestamp", typeof(DateTime) },
                { "timestamptz", typeof(DateTime) },
                { "interval", typeof(TimeSpan) },
                { "varchar", typeof(String) },
                { "inet", typeof(System.Net.IPAddress) },
                { "bit", typeof(Boolean) },
                { "uuid", typeof(Guid) },
                { "array", typeof(Array) },

                // TODO Fraction implementation
                //{ "mpq", typeof(Fraction) }
            };
        }
    }
}
