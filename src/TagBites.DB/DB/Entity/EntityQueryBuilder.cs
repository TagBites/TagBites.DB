using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TBS.Data.DB;
using TBS.Data.DB.Entity;
using TBS.Sql;
using TBS.Utils;

using static TBS.Sql.SqlExpression;

namespace TBS.DB.Entity
{
    public static class EntityQueryBuilder
    {
        private static readonly HashSet<string> s_autoConvertTypes = new HashSet<string>
        {
            "bool",
            "int2", "int4", "int8",
            "numeric", "float4", "float8",
            "text", "varchar",
            "timestamp", "timestamptz",
            // "interval", "time", "date", "xid", "tsvector", "mpq", "uuid", 
        };


        public static SqlQuerySelect CreateSelectQuery<T>() where T : class
        {
            return CreateSelectQuery<T>(null, null);
        }
        public static SqlQuerySelect CreateSelectQuery<T>(IList<string> properties, bool excludeProperties = false) where T : class
        {
            return CreateSelectQuery<T>(null, properties, excludeProperties);
        }
        public static SqlQuerySelect CreateSelectQuery<T>(IList recordsKeys) where T : class
        {
            return CreateSelectQuery<T>(recordsKeys, null);
        }
        public static SqlQuerySelect CreateSelectQuery<T>(IList recordsKeys, IList<string> properties, bool excludeProperties = false) where T : class
        {
            var table = EntityTableInfo.GetTableByType(typeof(T));

            var q = new SqlQuerySelect();
            var t = q.From.Add(table.TableFullName);

            foreach (var column in GetColumns(table, properties, excludeProperties, false))
                q.Select.Add(t.Column(column.Name), column.PropertyName);

            if (recordsKeys != null && recordsKeys.Count > 0)
            {
                q.Where.Add(CreatePrimaryKeyFilter(t, table, recordsKeys));
                q.Limit = recordsKeys.Count;
            }

            return q;
        }

        public static SqlQueryValues CreateValuesQuery<T>(IList<T> entities) where T : class
        {
            return CreateValuesQuery(entities, null);
        }
        public static SqlQueryValues CreateValuesQuery<T>(IList<T> entities, IList<string> properties, bool excludeProperties = false) where T : class
        {
            Guard.ArgumentNotNullOrEmpty(entities, nameof(entities));

            var table = EntityTableInfo.GetTableByType(typeof(T));

            var vq = new SqlQueryValues();
            FillValues(vq.Values, GetColumns(table, properties, excludeProperties, false), entities, false);

            return vq;
        }

        public static SqlQueryInsertValues CreateInsertQuery<T>(IList<T> entities) where T : class
        {
            return CreateInsertQuery(entities, null);
        }
        public static SqlQueryInsertValues CreateInsertQuery<T>(IList<T> entities, IList<string> properties, bool excludeProperties = false, bool withReturning = false) where T : class
        {
            Guard.ArgumentNotNullOrEmpty(entities, nameof(entities));

            var table = EntityTableInfo.GetTableByType(typeof(T));
            var columns = GetColumns(table, properties, excludeProperties, true);

            var q = new SqlQueryInsertValues(table.TableFullName);
            q.Columns.AddRange(columns.Select(x => x.Name));
            FillValues(q.Values, columns, entities, false);

            if (withReturning)
            {
                foreach (var column in table.Columns)
                    q.Returning.Add(column.Name, column.PropertyName);
            }

            return q;
        }

        public static SqlQueryUpdate CreateUpdateQuery<T>(IList<T> entities) where T : class
        {
            return CreateUpdateQuery(entities, null);
        }
        public static SqlQueryUpdate CreateUpdateQuery<T>(IList<T> entities, IList<string> properties, bool excludeProperties = false, bool withReturning = false) where T : class
        {
            Guard.ArgumentNotNullOrEmpty(entities, nameof(entities));

            var table = EntityTableInfo.GetTableByType(typeof(T));
            var columns = GetColumns(table, properties, excludeProperties, true);
            var columnsWithId = table.PrimaryKey.Concat(columns).Distinct().ToList();

            var vq = new SqlQueryValues();
            FillValues(vq.Values, columnsWithId, entities, false);

            var q = new SqlQueryUpdate(table.TableFullName);
            var w = q.With.Add("records_values", columnsWithId.Select(x => x.Name).ToArray(), vq);

            var t = q.From.Add(w);
            q.Where.Add(CreateSelfPrimaryKeyJoinCondition(q.Table, t, table));
            q.Where.Add(Or(columns.Select(x => AreDistinct(q.Table.Column(x.Name), t.Column(x.Name)))));

            foreach (var column in columns)
                q.Set.Add(column.Name, t.Column(column.Name));

            if (withReturning)
            {
                foreach (var column in table.Columns)
                    q.Returning.Add(q.Table.Column(column.Name), column.PropertyName);
            }

            return q;
        }

        public static SqlQuerySelect CreateUpsertQuery<T>(IList<T> entities, DbUpsertMode mode) where T : class
        {
            return CreateUpsertQuery(entities, mode, null);
        }
        public static SqlQuerySelect CreateUpsertQuery<T>(IList<T> entities, DbUpsertMode mode, IList<string> properties, bool excludeProperties = false, bool withReturning = false) where T : class
        {
            Guard.ArgumentNotNullOrEmpty(entities, nameof(entities));

            var table = EntityTableInfo.GetTableByType(typeof(T));
            var columns = GetColumns(table, properties, excludeProperties, true);
            var columnsWithId = table.PrimaryKey.Concat(columns).Distinct().ToList();
            var columnsWithIdNames = columnsWithId.Select(x => x.Name).ToArray();
            var firstIdColumn = table.PrimaryKey[0].Name;

            var q = new SqlQuerySelect();
            SqlClauseWithEntry records = null;

            // Values
            {
                var vq = new SqlQueryValues();
                FillValues(vq.Values, columnsWithId, entities, mode == DbUpsertMode.BasedOnKey);
                records = q.With.Add("records_values", columnsWithIdNames, vq);
            }

            // Values with ids on database
            if (mode == DbUpsertMode.BasedOnExistence)
            {
                var iq = new SqlQuerySelect();
                var r = iq.From.Add(records);
                var t = new SqlTable(table.TableFullName, "tx");

                iq.Join.AddOnExpression(SqlClauseJoinEntryType.LeftJoin, t, CreateSelfPrimaryKeyJoinCondition(r, t, table));

                foreach (var column in columnsWithId)
                    iq.Select.Add(r.Column(column.Name));

                iq.Select.Add(IsNotNull(t.Column(firstIdColumn)));

                records = q.With.Add("records_ids", columnsWithIdNames.Concat(new[] { "_record_exists" }).ToArray(), iq);
            }

            // Update
            {
                var uq = new SqlQueryUpdate(table.TableFullName);

                var t = uq.From.Add(records);
                uq.Where.Add(CreateSelfPrimaryKeyJoinCondition(uq.Table, t, table));

                if (mode == DbUpsertMode.BasedOnExistence)
                    uq.Where.Add(t.Column("_record_exists").ToCondition());

                uq.Where.Add(Or(columns.Select(x => AreDistinct(uq.Table.Column(x.Name), t.Column(x.Name)))));

                foreach (var column in columns)
                    uq.Set.Add(column.Name, t.Column(column.Name));

                if (!withReturning)
                    uq.Returning.Add(uq.Table.Column(firstIdColumn));
                else
                {
                    foreach (var column in table.Columns)
                        uq.Returning.Add(uq.Table.Column(column.Name), column.PropertyName);
                }

                q.With.Add("updated_ids", uq);
            }

            // Insert
            {
                var iq = new SqlQueryInsertSelect(table.TableFullName);
                iq.Columns.AddRange((mode == DbUpsertMode.BasedOnExistence ? columnsWithId : columns).Select(x => x.Name));

                var iqs = iq.Select;
                var ri = iqs.From.Add(records);

                if (mode == DbUpsertMode.BasedOnExistence)
                {
                    foreach (var column in columnsWithId)
                        iqs.Select.Add(ri.Column(column.Name));

                    iqs.Where.Add(Not(ri.Column("_record_exists").ToCondition()));
                }
                else
                {
                    foreach (var column in columns)
                        iqs.Select.Add(ri.Column(column.Name));

                    iqs.Where.Add(IsNull(ri.Column(firstIdColumn)));
                }

                iqs.Where.Add(IsNull(ri.Column(firstIdColumn)));

                if (!withReturning)
                    iq.Returning.Add(Literal(firstIdColumn));
                else
                {
                    foreach (var column in table.Columns)
                        iq.Returning.Add(column.Name, column.PropertyName);
                }

                q.With.Add("inserted_ids", iq);
            }

            if (!withReturning)
                q.Select.Add(Literal("((SELECT count(*) FROM updated_ids) + (SELECT count(*) FROM inserted_ids))"));
            else
            {
                q.Select.AddAll();
                q.From.Add(Literal("((SELECT * FROM updated_ids) UNION ALL (SELECT * FROM inserted_ids))"), "a");
            }

            return q;
        }

        public static SqlQueryDelete CreateDeleteQuery<T>(IList<T> models) where T : class
        {
            Guard.ArgumentNotNullOrEmpty(models, nameof(models));

            var table = EntityTableInfo.GetTableByType(typeof(T));

            var q = new SqlQueryDelete(table.TableFullName);
            q.Where.Add(CreatePrimaryKeyFilter(q.From, table, GetRecordsKeys(table, models)));

            return q;
        }
        public static SqlQueryDelete CreateDeleteByKeyQuery<T>(IList recordsKeys) where T : class
        {
            return CreateDeleteByKeyQuery(typeof(T), recordsKeys);
        }
        internal static SqlQueryDelete CreateDeleteByKeyQuery(Type modelType, IList recordsKeys)
        {
            Guard.ArgumentNotNullOrEmpty(recordsKeys, nameof(recordsKeys));

            var table = EntityTableInfo.GetTableByType(modelType);

            var q = new SqlQueryDelete(table.TableFullName);
            q.Where.Add(CreatePrimaryKeyFilter(q.From, table, recordsKeys));

            return q;
        }

        private static IList GetRecordsKeys<T>(EntityTableInfo table, IList<T> models) where T : class
        {
            var recordsKeys = new object[models.Count];

            if (table.PrimaryKey.Count == 1)
            {
                var getter = table.PrimaryKey[0].Getter;

                for (var mi = 0; mi < models.Count; mi++)
                    recordsKeys[mi] = getter(models[mi]);
            }
            else if (table.PrimaryKey.Count > 1)
            {
                for (var mi = 0; mi < models.Count; mi++)
                {
                    var recordKeys = new object[table.PrimaryKey.Count];

                    for (var i = 0; i < table.PrimaryKey.Count; i++)
                        recordKeys[i] = table.PrimaryKey[i].Getter(models[mi]);

                    recordsKeys[mi] = recordKeys;
                }
            }
            else
                throw new ArgumentException($"Table {table.TableFullName} does not have primary key.", nameof(table));

            return recordsKeys;
        }
        private static IList<EntityColumnInfo> GetColumns(EntityTableInfo table, IList<string> properties, bool excludeProperties, bool withoutGenerated)
        {
            if (properties == null || properties.Count == 0)
                return withoutGenerated ? table.Columns.Where(x => x.DatabaseGeneratedOption == DatabaseGeneratedOption.None).ToList() : table.Columns;

            if (excludeProperties)
                return table.Columns.Where(x => !properties.Contains(x.PropertyName) && (!withoutGenerated || x.DatabaseGeneratedOption == DatabaseGeneratedOption.None)).ToList();

            return properties.Select(x => table.GetColumnByPropertyName(x) ?? throw new ArgumentException($"Property {x} is not mapped.")).ToList();
        }
        private static SqlCondition CreatePrimaryKeyFilter(SqlTable t, EntityTableInfo table, IList recordsKeys)
        {
            SqlCondition condition = null;

            if (table.PrimaryKey.Count == 1)
            {
                var idColumn = t.Column(table.PrimaryKey[0].Name);

                condition = recordsKeys.Count == 1
                    ? AreEquals(idColumn, Argument(recordsKeys[0]))
                    : In(idColumn, recordsKeys.Cast<object>().Select(Argument));
            }
            else if (table.PrimaryKey.Count > 1)
            {
                foreach (var recordKeys in recordsKeys)
                {
                    if (!(recordKeys is IList list) || list.Count != table.PrimaryKey.Count)
                        throw new ArgumentException("Invalid key.", nameof(recordsKeys));

                    SqlCondition recordCondition = null;
                    for (var i = 0; i < list.Count; i++)
                        recordCondition = And(recordCondition, AreEquals(t.Column(table.PrimaryKey[i].Name), Argument(list[i])));

                    condition = Or(condition, recordCondition);
                }
            }
            else
                throw new ArgumentException($"Table {table.TableFullName} does not have primary key.", nameof(table));

            return condition;
        }
        private static SqlCondition CreateSelfPrimaryKeyJoinCondition(SqlTable t1, SqlTable t2, EntityTableInfo table)
        {
            SqlCondition condition = null;

            if (table.PrimaryKey.Count == 1)
                condition = AreEquals(t1.Column(table.PrimaryKey[0].Name), t2.Column(table.PrimaryKey[0].Name));
            else if (table.PrimaryKey.Count > 1)
            {
                foreach (var column in table.PrimaryKey)
                    condition = And(condition, AreEquals(t1.Column(column.Name), t2.Column(column.Name)));
            }
            else
                throw new ArgumentException($"Table {table.TableFullName} does not have primary key.", nameof(table));

            return condition;
        }
        private static void FillValues<T>(SqlClauseValues vq, IList<EntityColumnInfo> columns, IEnumerable<T> entities, bool setNullForEmptyId)
        {
            //var idChecker = TableInfo.GetTableInfo(table.TableName) as INonStandardID;

            foreach (var entity in entities)
            {
                var rec = new object[columns.Count];

                for (var i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    var value = column.Getter(entity);

                    if (setNullForEmptyId
                        && column.IsKeyPart
                        && (column.PropertyType == typeof(int) && (int)value == 0
                            || column.PropertyType.IsValueType && Equals(value, Activator.CreateInstance(column.PropertyType))))
                    //&& (idChecker == null || !idChecker.IsOkID(0)))
                    {
                        value = null;
                    }

                    if (!string.IsNullOrEmpty(column.TypeName) && !s_autoConvertTypes.Contains(column.TypeName.ToLower()))
                        rec[i] = value == null ? Cast(Null, column.TypeName) : Cast(Argument(value), column.TypeName);
                    else
                        rec[i] = value == null ? Cast(Null, column.PropertyType) : value;
                }

                vq.Add(rec);
            }
        }
    }

    //class SqlEntityBuilder<TEntity, TTable> : IQueryable<>
    //    where TEntity : IEntitySourdce<TTable> where TTable : SqlTable
    //{
    //    public static SqlEntityBuilder<TEntity, TTable> Create()
    //    {
    //        Queryable.Where()
    //    }

    //    private static void Test()
    //    {

    //    }
    //}

    //interface IEntitySourdce<T> where T : SqlTable
    //{

    //}
}
