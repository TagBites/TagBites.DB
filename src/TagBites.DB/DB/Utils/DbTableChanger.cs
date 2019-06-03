using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Sql;
using TBS.Utils;

namespace TBS.Data.DB.Utils
{
    public class DbTableChanger
    {
        internal readonly string[] KeyColumnNamesCore;

        [Obsolete("Please use IdColumnNames instead.")]
        public string IdColumnName
        {
            get
            {
                if (IsMultiColumnKey)
                    throw new InvalidOperationException("For multi-column key please use 'IdColumnNames' property.");

                return KeyColumnNamesCore[0];
            }
        }

        public string TableName { get; }
        public bool IsMultiColumnKey => KeyColumnNamesCore.Length > 1;

        public bool KeyColumnsAreAutoIncremented { get; }

        public DbTableChangerParameterCollection Parameters { get; }

        public DbTableChangerRecordCollection Records { get; }

        public DbTableChanger(string tableName, string idColumnName, bool idColumnIsAutoIncremented = true)
            : this(tableName, new string[] { idColumnName }, idColumnIsAutoIncremented)
        { }
        public DbTableChanger(string tableName, string[] idColumnNames, bool idColumnIsAutoIncremented = true)
        {
            Guard.ArgumentNotNullOrWhiteSpace(tableName, nameof(tableName));
            Guard.ArgumentNotNullOrEmptyWithNotNullOrEmptyItems(idColumnNames, nameof(idColumnNames));

            TableName = tableName;
            KeyColumnNamesCore = idColumnNames;
            KeyColumnsAreAutoIncremented = idColumnIsAutoIncremented;
            Parameters = new DbTableChangerParameterCollection(this);
            Records = new DbTableChangerRecordCollection(this);
        }


        public int Execute(IDbLink link, DbTableChangerExecuteMode mode = DbTableChangerExecuteMode.InsertOrUpdateBasedOnId)
        {
            int affectedRows = 0;

            if (Records.Count == 0)
                return affectedRows;

            var needUpdate = mode != DbTableChangerExecuteMode.Insert;
            var needInsert = mode != DbTableChangerExecuteMode.Update;

            if (mode == DbTableChangerExecuteMode.InsertOrUpdateBasedOnId)
            {
                if (needUpdate)
                    needUpdate = Records.Any(x => !x.IsKeyEmpty);

                if (needInsert)
                    needInsert = Records.Any(x => x.IsKeyEmpty);
            }

            if (!needInsert && !needUpdate)
                return affectedRows;

            // Parameters
            var idParameters = KeyColumnNamesCore.Select(x => new DbTableChangerParameter(x, DbParameterDirection.InputOutput)).ToList();

            // Settings
            var withReturning = link.ConnectionContext.Provider.QueryResolver.SupportReturningClause;

            // Transactions
            using (var transaction = link.Begin())
            {
                //
                // Update
                if (needUpdate)
                {
                    Func<DbTableChangerParameter, SqlQuerySelect> select =
                        x =>
                        {
                            var qInner = new SqlQuerySelect() { Limit = 0 };
                            var from = qInner.From.Add(TableName);
                            qInner.Select.Add(from.Column(x.Name));
                            return qInner;
                        };
                    var records = mode == DbTableChangerExecuteMode.InsertOrUpdateBasedOnId
                        ? Records.Where(x => !x.IsKeyEmpty)
                        : Records;

                    var input = Parameters.Where(x => (x.Direction & DbParameterDirection.Input) == DbParameterDirection.Input).ToList();
                    var output = Parameters.Where(x => (x.Direction & DbParameterDirection.Output) == DbParameterDirection.Output).ToList();

                    if (withReturning)
                    {
                        for (int i = 0; i < idParameters.Count; i++)
                        {
                            input.Insert(i, idParameters[i]);
                            output.Insert(i, idParameters[i]);
                        }

                        var qValues = new SqlQueryValues();
                        qValues.Values.Add(input.Select(select).Cast<object>().ToArray());
                        foreach (var record in records)
                            qValues.Values.Add(input.Select(x => record[x.Name]).ToArray());

                        var qSelect = new SqlQuerySelect() { Offset = 1 };
                        qSelect.From.Add(qValues);
                        qSelect.Select.AddAll();

                        var qUpdate = new SqlQueryUpdate(TableName, "t");
                        var qUpdateFrom = qUpdate.From.Add(qSelect, "r", input.Select(x => x.Name).ToArray());
                        for (int i = KeyColumnNamesCore.Length; i < input.Count; i++)
                            qUpdate.Set.Add(input[i].Name, qUpdateFrom.Column(input[i].Name));

                        foreach (var name in KeyColumnNamesCore)
                            qUpdate.Where.AddEquals(qUpdate.Table.Column(name), qUpdateFrom.Column(name));
                        foreach (var item in output)
                            qUpdate.Returning.Add(qUpdate.Table, qUpdate.Table.Column(item.Name).ColumnName, item.Name);

                        // Execute
                        var result = link.Execute(qUpdate);

                        // Resultus
                        for (int i = 0; i < Records.Count; i++)
                            Records[i].Status = DbTableChangerRecordStatus.NotFound;

                        for (int i = 0; i < result.RowCount; i++)
                        {
                            var record = IsMultiColumnKey
                                    ? Records.GetByKeyInternal(result.GetRow(i))
                                    : Records.GetByKey(result[i, 0]);
                            if (record != null)
                            {
                                record.Status = DbTableChangerRecordStatus.Updated;

                                for (int c = KeyColumnNamesCore.Length; c < output.Count; c++)
                                {
                                    var value = result[i, output[c].Name];
                                    if (DataHelper.IsNull(value))
                                        value = null;

                                    record[output[c].Name] = value;
                                }
                            }
                        }

                        affectedRows += result.RowCount;
                    }
                    else
                    {
                        foreach (var record in records)
                        {
                            var qUpdate = new SqlQueryUpdate(TableName, "t");

                            for (int i = 0; i < input.Count; i++)
                                qUpdate.Set.Add(input[i].Name, SqlExpression.Argument(record[input[i].Name]));

                            for (int i = 0; i < idParameters.Count; i++)
                                qUpdate.Where.AddEquals(SqlExpression.Literal(idParameters[i].Name), SqlExpression.Argument(record[idParameters[i].Name]));

                            var rowCount = link.ExecuteNonQuery(qUpdate);
                            record.Status = rowCount == 1 ? DbTableChangerRecordStatus.Updated : DbTableChangerRecordStatus.NotFound;

                            if (rowCount > 0 && output.Count > 0)
                            {
                                var qSelect = new SqlQuerySelect();
                                qSelect.From.Add(TableName);

                                for (int i = 0; i < output.Count; i++)
                                    qSelect.Select.Add(SqlExpression.Literal(output[i].Expression), output[i].Name);

                                for (int i = 0; i < idParameters.Count; i++)
                                    qSelect.Where.AddEquals(SqlExpression.Literal(idParameters[i].Name), SqlExpression.Argument(record[idParameters[i].Name]));

                                var result = link.Execute(qSelect);
                                if (result.RowCount == 1)
                                {
                                    for (int c = 1; c < output.Count; c++)
                                    {
                                        var value = result[0, output[c].Name];
                                        if (DataHelper.IsNull(value))
                                            value = null;

                                        record[output[c].Name] = value;
                                    }
                                }
                            }

                            affectedRows += rowCount;
                        }
                    }
                }

                //
                // Insert
                if (needInsert)
                {
                    var input = Parameters.Where(x => (x.Direction & DbParameterDirection.Input) == DbParameterDirection.Input).ToList();
                    var output = Parameters.Where(x => (x.Direction & DbParameterDirection.Output) == DbParameterDirection.Output).ToList();

                    for (int i = 0; i < KeyColumnNamesCore.Length; i++)
                    {
                        if (KeyColumnsAreAutoIncremented)
                            output.Insert(i, idParameters[i]);
                        else
                            input.Insert(i, idParameters[i]);
                    }

                    var records = mode == DbTableChangerExecuteMode.InsertOrUpdateBasedOnId
                       ? Records.Where(x => x.IsKeyEmpty).ToList()
                       : needUpdate
                         ? (IList<DbTableChangerRecord>)Records.Where(x => x.Status == DbTableChangerRecordStatus.NotFound).ToList()
                         : Records;

                    var qInsert = new SqlQueryInsertValues(TableName);
                    qInsert.Columns.AddRange(input.Select(x => x.Name));

                    if (withReturning)
                    {
                        foreach (var record in records)
                        {
                            qInsert.Values.Add(input.Select(x => record[x.Name]).ToArray());
                            record.Status = DbTableChangerRecordStatus.Inserted;
                        }

                        if (output.Count == 0)
                        {
                            link.ExecuteNonQuery(qInsert);
                            affectedRows += records.Count;
                        }
                        else
                        {
                            qInsert.Returning.AddRange(output.Select(x => new SqlExpressionWithAlias(new SqlLiteral(x.Expression), x.Name)));

                            var result = link.Execute(qInsert);
                            for (int i = 0; i < result.RowCount; i++)
                            {
                                var record = records[i];
                                for (int c = 0; c < output.Count; c++)
                                {
                                    var value = result[i, output[c].Name];
                                    if (DataHelper.IsNull(value))
                                        value = null;

                                    record[output[c].Name] = value;
                                }
                            }

                            affectedRows += result.RowCount;
                        }
                    }
                    else
                    {
                        foreach (var record in records)
                        {
                            qInsert.Values.Clear();
                            qInsert.Values.Add(input.Select(x => record[x.Name]).ToArray());

                            link.ExecuteNonQuery(qInsert);
                            affectedRows += 1;
                            record.Status = DbTableChangerRecordStatus.Inserted;

                            if (output.Count > 0)
                            {
                                var qSelect = new SqlQuerySelect();
                                qSelect.From.Add(TableName);
                                qSelect.Select.Add(SqlExpression.Literal("rowid"));

                                for (int i = 0; i < output.Count; i++)
                                    qSelect.Select.Add(SqlExpression.Literal(output[i].Expression), output[i].Name);

                                // TODO TEMPORARY
                                qSelect.Where.AddEquals(SqlExpression.Literal("rowid"), SqlFunction.Custom("last_insert_rowid"));

                                var result = link.Execute(qSelect);
                                if (result.RowCount == 1)
                                {
                                    for (int c = 0; c < output.Count; c++)
                                    {
                                        var value = result[0, output[c].Name];
                                        if (DataHelper.IsNull(value))
                                            value = null;

                                        record[output[c].Name] = value;
                                    }
                                }
                            }
                        }
                    }
                }

                transaction.Commit();
            }

            return affectedRows;
        }
    }
}
