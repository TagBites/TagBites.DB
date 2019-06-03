using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TBS.Data.DB.Schema;
using TBS.DB.Entity;
using TBS.Sql;
using TBS.Utils;

namespace TBS.Data.DB
{
    public static class DbLinkExtensions
    {
        public static DbSchema GetSchemaModel(this IDbLink link)
        {
            return StandardSqlSchema.GetSchema(link);
        }

        public static int Fill(this IDbLink link, IQuerySource query, DataTable dataTable)
        {
            Guard.ArgumentNotNull(query, "query");
            Guard.ArgumentNotNull(dataTable, "dataTable");

            return link.ExecuteOnAdapter(query, adapter => adapter.Fill(dataTable));
        }
        public static int Fill(this IDbLink link, IQuerySource query, DataSet dataSet)
        {
            Guard.ArgumentNotNull(query, "query");
            Guard.ArgumentNotNull(dataSet, "dataSet");

            return link.ExecuteOnAdapter(query, adapter => adapter.Fill(dataSet));
        }
        public static int Fill(this IDbLink link, IQuerySource query, DataSet dataSet, string sourceTableName)
        {
            Guard.ArgumentNotNull(query, "query");
            Guard.ArgumentNotNull(dataSet, "dataSet");

            return link.ExecuteOnAdapter(query, adapter => adapter.Fill(dataSet, sourceTableName));
        }

        public static int ExecuteNonQuery(this IDbLink link, string queryFormat, params object[] agrs)
        {
            return link.ExecuteNonQuery(new Query(queryFormat, agrs));
        }

        public static QueryResult Execute(this IDbLink link, string queryFormat, params object[] agrs)
        {
            return link.Execute(new Query(queryFormat, agrs));
        }

        public static QueryResult[] BatchExecute(this IDbLink link, string queryFormat, params object[] agrs)
        {
            return link.BatchExecute(new Query(queryFormat, agrs));
        }
        public static DelayedBatchQueryResult DelayedBatchExecute(this IDbLink link, string queryFormat, params object[] agrs)
        {
            return link.DelayedBatchExecute(new Query(queryFormat, agrs));
        }

        public static IList<object> ExecuteRowScalars(this IDbLink link, string queryFormat, params object[] agrs)
        {
            return ExecuteRowScalars(link, new Query(queryFormat, agrs));
        }
        public static IList<T> ExecuteRowScalars<T>(this IDbLink link, string queryFormat, params object[] agrs)
        {
            return ExecuteRowScalars<T>(link, new Query(queryFormat, agrs));
        }
        public static IList<T> ExecuteRowScalars<T>(this IDbLink link, IQuerySource query, T defaultValue = default(T))
        {
            Guard.ArgumentNotNull(query, "query");

            return link.ExecuteOnReader(query, reader =>
            {
                var items = new List<T>();

                if (reader.Read())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        if (value is DBNull)
                            value = null;

                        items.Add(DataHelper.TryChangeTypeDefault<T>(value, defaultValue));
                    }
                }

                return items;
            });
        }
        public static IList<object> ExecuteRowScalars(this IDbLink link, IQuerySource query)
        {
            Guard.ArgumentNotNull(query, "query");

            return link.ExecuteOnReader(query, reader =>
            {
                var items = new List<object>();

                if (reader.Read())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        if (value is DBNull)
                            value = null;

                        items.Add(value);
                    }
                }

                return items;
            });
        }

        public static IList<object> ExecuteColumnScalars(this IDbLink link, string queryFormat, params object[] agrs)
        {
            return ExecuteColumnScalars(link, new Query(queryFormat, agrs));
        }
        public static IList<T> ExecuteColumnScalars<T>(this IDbLink link, string queryFormat, params object[] agrs)
        {
            return ExecuteColumnScalars<T>(link, new Query(queryFormat, agrs));
        }
        public static IList<T> ExecuteColumnScalars<T>(this IDbLink link, IQuerySource query, T defaultValue = default(T))
        {
            Guard.ArgumentNotNull(query, "query");

            return link.ExecuteOnReader(query, reader =>
            {
                var items = new List<T>();

                while (reader.Read() && reader.FieldCount > 0)
                {
                    var value = reader.GetValue(0);
                    if (value is DBNull)
                        value = null;

                    items.Add(DataHelper.TryChangeTypeDefault<T>(value, defaultValue));
                }

                return items;
            });
        }
        public static IList<object> ExecuteColumnScalars(this IDbLink link, IQuerySource query)
        {
            Guard.ArgumentNotNull(query, "query");

            return link.ExecuteOnReader(query, reader =>
            {
                var items = new List<object>();

                while (reader.Read() && reader.FieldCount > 0)
                {
                    var value = reader.GetValue(0);
                    if (value is DBNull)
                        value = null;

                    items.Add(value);
                }

                return items;
            });
        }

        public static object ExecuteScalar(this IDbLink link, string queryFormat, params object[] agrs)
        {
            return link.ExecuteScalar(new Query(queryFormat, agrs));
        }
        public static T ExecuteScalar<T>(this IDbLink link, string queryFormat, params object[] agrs)
        {
            var value = link.ExecuteScalar(new Query(queryFormat, agrs));
            return DataHelper.TryChangeTypeDefault<T>(value);
        }
        public static T ExecuteScalar<T>(this IDbLink link, IQuerySource query, T defaultValue = default(T))
        {
            var value = link.ExecuteScalar(query);
            return DataHelper.TryChangeTypeDefault<T>(value, defaultValue);
        }

        public static QueryObjectResult<T> Execute<T>(this IDbLink link, string queryFormat, params object[] agrs)
        {
            Guard.ArgumentNotNullOrEmpty(queryFormat, "queryFormat");
            return Execute<T>(link, new Query(queryFormat, agrs));
        }
        public static QueryObjectResult<T> Execute<T>(this IDbLink link, IQuerySource query)
        {
            return link.Execute(query).ToObjects<T>();
        }
        public static QueryObjectResult<T> Execute<T>(this IDbLink link, IQuerySource query, QueryObjectResultPropertyResolver customPropertyResolver)
        {
            return link.Execute(query).ToObjects<T>(customPropertyResolver);
        }
        public static QueryObjectResult<T> Execute<T>(this IDbLink link, IQuerySource query, QueryObjectResultPropertyResolver customPropertyResolver, QueryObjectResultItemFiller<T> additionalFiller)
        {
            return link.Execute(query).ToObjects<T>(customPropertyResolver, additionalFiller);
        }

        public static IOrderedQueryable<TSource> EntityQuery<TSource>(this IDbLink link)
        {
            return new EntityQuery<TSource>(new DbLinkQueryProvider(link));
        }
        public static SqlQuerySelect ParseEntityQuery<TSource>(this IDbLink link, IQueryable<TSource> queryable)
        {
            var provider = queryable.Provider as IDbLinkEntityQueryProvider
                           ?? new DbLinkQueryProvider(link);
            return provider.GetQuery(queryable.Expression);
        }

        public static T GetByKey<T>(this IDbLink link, object key) where T : class, new()
        {
            var q = EntityQueryBuilder.CreateSelectQuery<T>(new[] { key });
            return link.Execute<T>(q).FirstOrDefault();
        }
        public static T GetByKey<T>(this IDbLink link, object[] key) where T : class, new()
        {
            var q = EntityQueryBuilder.CreateSelectQuery<T>(new[] { key });
            return link.Execute<T>(q).FirstOrDefault();
        }
        public static IList<T> GetManyByKey<T>(this IDbLink link, IEnumerable<object> recordsKeys) where T : class, new()
        {
            var items = recordsKeys as IList ?? recordsKeys.ToArray();
            if (items.Count == 0)
                return new T[0];

            var q = EntityQueryBuilder.CreateSelectQuery<T>(items);
            return link.Execute<T>(q);
        }
        public static IList<T> GetManyByKey<T>(this IDbLink link, IEnumerable<object[]> recordsKeys) where T : class, new()
        {
            var items = recordsKeys as IList ?? recordsKeys.ToArray();
            if (items.Count == 0)
                return new T[0];

            var q = EntityQueryBuilder.CreateSelectQuery<T>(items);
            return link.Execute<T>(q);
        }

        public static int Insert<T>(this IDbLink link, T entity) where T : class
        {
            return Insert(link, (IEnumerable<T>)new[] { entity }, false, null);
        }
        public static int Insert<T>(this IDbLink link, T entity, bool allowUnexpectedResult) where T : class
        {
            return Insert(link, (IEnumerable<T>)new[] { entity }, allowUnexpectedResult, null);
        }
        public static int Insert<T>(this IDbLink link, T entity, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            return Insert(link, (IEnumerable<T>)new[] { entity }, allowUnexpectedResult, properties, excludeProperties);
        }
        public static int Insert<T>(this IDbLink link, IEnumerable<T> entities) where T : class
        {
            return Insert(link, entities, false, null);
        }
        public static int Insert<T>(this IDbLink link, IEnumerable<T> entities, bool allowUnexpectedResult) where T : class
        {
            return Insert(link, entities, allowUnexpectedResult, null);
        }
        public static int Insert<T>(this IDbLink link, IEnumerable<T> entities, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            var items = GetItems(entities);
            if (items.Count == 0)
                return 0;

            var q = EntityQueryBuilder.CreateInsertQuery(items, properties, excludeProperties);
            var result = link.ExecuteNonQuery(q);

            CheckAffected(allowUnexpectedResult, items.Count, result);
            return result;
        }

        public static T InsertReturning<T>(this IDbLink link, T entity) where T : class
        {
            return InsertReturning(link, (IEnumerable<T>)new[] { entity }, false, null).FirstOrDefault();
        }
        public static T InsertReturning<T>(this IDbLink link, T entity, bool allowUnexpectedResult) where T : class
        {
            return InsertReturning(link, (IEnumerable<T>)new[] { entity }, allowUnexpectedResult, null).FirstOrDefault();
        }
        public static T InsertReturning<T>(this IDbLink link, T entity, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            return InsertReturning(link, (IEnumerable<T>)new[] { entity }, allowUnexpectedResult, properties, excludeProperties).FirstOrDefault();
        }
        public static IList<T> InsertReturning<T>(this IDbLink link, IEnumerable<T> entities) where T : class
        {
            return InsertReturning(link, entities, false, null);
        }
        public static IList<T> InsertReturning<T>(this IDbLink link, IEnumerable<T> entities, bool allowUnexpectedResult) where T : class
        {
            return InsertReturning(link, entities, allowUnexpectedResult, null);
        }
        public static IList<T> InsertReturning<T>(this IDbLink link, IEnumerable<T> entities, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            var items = GetItems(entities);
            if (items.Count == 0)
                return new T[0];

            var q = EntityQueryBuilder.CreateInsertQuery(items, properties, excludeProperties, true);
            var result = link.Execute<T>(q);

            CheckAffected(allowUnexpectedResult, items.Count, result.Count);
            return result;
        }

        public static int Update<T>(this IDbLink link, T entity) where T : class
        {
            return Update(link, (IEnumerable<T>)new[] { entity }, false, null);
        }
        public static int Update<T>(this IDbLink link, T entity, bool allowUnexpectedResult) where T : class
        {
            return Update(link, (IEnumerable<T>)new[] { entity }, allowUnexpectedResult, null);
        }
        public static int Update<T>(this IDbLink link, T entity, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            return Update(link, (IEnumerable<T>)new[] { entity }, allowUnexpectedResult, properties, excludeProperties);
        }
        public static int Update<T>(this IDbLink link, IEnumerable<T> entities) where T : class
        {
            return Update(link, entities, false, null);
        }
        public static int Update<T>(this IDbLink link, IEnumerable<T> entities, bool allowUnexpectedResult) where T : class
        {
            return Update(link, entities, allowUnexpectedResult, null);
        }
        public static int Update<T>(this IDbLink link, IEnumerable<T> entities, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            var items = GetItems(entities);
            if (items.Count == 0)
                return 0;

            var q = EntityQueryBuilder.CreateUpdateQuery(items, properties, excludeProperties);
            var result = link.ExecuteNonQuery(q);

            CheckAffected(allowUnexpectedResult, items.Count, result);
            return result;
        }

        public static T UpdateReturning<T>(this IDbLink link, T entity) where T : class
        {
            return UpdateReturning(link, (IEnumerable<T>)new[] { entity }, false, null).FirstOrDefault();
        }
        public static T UpdateReturning<T>(this IDbLink link, T entity, bool allowUnexpectedResult) where T : class
        {
            return UpdateReturning(link, (IEnumerable<T>)new[] { entity }, allowUnexpectedResult, null).FirstOrDefault();
        }
        public static T UpdateReturning<T>(this IDbLink link, T entity, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            return UpdateReturning(link, (IEnumerable<T>)new[] { entity }, allowUnexpectedResult, properties, excludeProperties).FirstOrDefault();
        }
        public static IList<T> UpdateReturning<T>(this IDbLink link, IEnumerable<T> entities) where T : class
        {
            return UpdateReturning(link, entities, false, null);
        }
        public static IList<T> UpdateReturning<T>(this IDbLink link, IEnumerable<T> entities, bool allowUnexpectedResult) where T : class
        {
            return UpdateReturning(link, entities, allowUnexpectedResult, null);
        }
        public static IList<T> UpdateReturning<T>(this IDbLink link, IEnumerable<T> entities, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            var items = GetItems(entities);
            if (items.Count == 0)
                return new T[0];

            var q = EntityQueryBuilder.CreateUpdateQuery(items, properties, excludeProperties, true);
            var result = link.Execute<T>(q);

            CheckAffected(allowUnexpectedResult, items.Count, result.Count);
            return result;
        }

        public static int Upsert<T>(this IDbLink link, T entity, DbUpsertMode mode = DbUpsertMode.BasedOnKey) where T : class
        {
            return Upsert(link, (IEnumerable<T>)new[] { entity }, mode, false, null);
        }
        public static int Upsert<T>(this IDbLink link, T entity, DbUpsertMode mode, bool allowUnexpectedResult) where T : class
        {
            return Upsert(link, (IEnumerable<T>)new[] { entity }, mode, allowUnexpectedResult, null);
        }
        public static int Upsert<T>(this IDbLink link, T entity, DbUpsertMode mode, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            return Upsert(link, (IEnumerable<T>)new[] { entity }, mode, allowUnexpectedResult, properties, excludeProperties);
        }
        public static int Upsert<T>(this IDbLink link, IEnumerable<T> entities, DbUpsertMode mode = DbUpsertMode.BasedOnKey) where T : class
        {
            return Upsert(link, entities, mode, false, null);
        }
        public static int Upsert<T>(this IDbLink link, IEnumerable<T> entities, DbUpsertMode mode, bool allowUnexpectedResult) where T : class
        {
            return Upsert(link, entities, mode, allowUnexpectedResult, null);
        }
        public static int Upsert<T>(this IDbLink link, IEnumerable<T> entities, DbUpsertMode mode, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            var items = GetItems(entities);
            if (items.Count == 0)
                return 0;

            var q = EntityQueryBuilder.CreateUpsertQuery(items, mode, properties, excludeProperties);
            var result = link.ExecuteNonQuery(q);

            CheckAffected(allowUnexpectedResult, items.Count, result);
            return result;
        }

        public static T UpsertReturning<T>(this IDbLink link, T entity, DbUpsertMode mode = DbUpsertMode.BasedOnKey) where T : class
        {
            return UpsertReturning(link, (IEnumerable<T>)new[] { entity }, mode, false, null).FirstOrDefault();
        }
        public static T UpsertReturning<T>(this IDbLink link, T entity, DbUpsertMode mode, bool allowUnexpectedResult) where T : class
        {
            return UpsertReturning(link, (IEnumerable<T>)new[] { entity }, mode, allowUnexpectedResult, null).FirstOrDefault();
        }
        public static T UpsertReturning<T>(this IDbLink link, T entity, DbUpsertMode mode, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            return UpsertReturning(link, (IEnumerable<T>)new[] { entity }, mode, allowUnexpectedResult, properties, excludeProperties).FirstOrDefault();
        }
        public static IList<T> UpsertReturning<T>(this IDbLink link, IEnumerable<T> entities, DbUpsertMode mode = DbUpsertMode.BasedOnKey) where T : class
        {
            return UpsertReturning(link, entities, mode, false, null);
        }
        public static IList<T> UpsertReturning<T>(this IDbLink link, IEnumerable<T> entities, DbUpsertMode mode, bool allowUnexpectedResult) where T : class
        {
            return UpsertReturning(link, entities, mode, allowUnexpectedResult, null);
        }
        public static IList<T> UpsertReturning<T>(this IDbLink link, IEnumerable<T> entities, DbUpsertMode mode, bool allowUnexpectedResult, IList<string> properties, bool excludeProperties = false) where T : class
        {
            var items = GetItems(entities);
            if (items.Count == 0)
                return new T[0];

            var q = EntityQueryBuilder.CreateUpsertQuery(items, mode, properties, excludeProperties, true);
            var result = link.Execute<T>(q);

            CheckAffected(allowUnexpectedResult, items.Count, result.Count);
            return result;
        }

        public static int Delete<T>(this IDbLink link, T entity) where T : class
        {
            return Delete(link, (IEnumerable<T>)new[] { entity }, link.ConnectionContext.Provider.Configuration.DefaultAllowUnexpectedRowCountOnDelete);
        }
        public static int Delete<T>(this IDbLink link, T entity, bool allowUnexpectedResult) where T : class
        {
            return Delete(link, (IEnumerable<T>)new[] { entity }, allowUnexpectedResult);
        }
        public static int Delete<T>(this IDbLink link, IEnumerable<T> entities) where T : class
        {
            return Delete(link, entities, link.ConnectionContext.Provider.Configuration.DefaultAllowUnexpectedRowCountOnDelete);
        }
        public static int Delete<T>(this IDbLink link, IEnumerable<T> entities, bool allowUnexpectedResult) where T : class
        {
            var items = GetItems(entities);
            if (items.Count == 0)
                return 0;

            var q = EntityQueryBuilder.CreateDeleteQuery(items);
            var result = link.ExecuteNonQuery(q);

            CheckAffected(allowUnexpectedResult, items.Count, result);
            return result;
        }

        public static int DeleteByKey<T>(this IDbLink link, object key) where T : class
        {
            return DeleteByKeyCore<T>(link, (IList)new[] { key }, null);
        }
        public static int DeleteByKey<T>(this IDbLink link, object key, bool allowUnexpectedResult) where T : class
        {
            return DeleteByKeyCore<T>(link, (IList)new[] { key }, allowUnexpectedResult);
        }
        public static int DeleteByKey<T>(this IDbLink link, object[] key) where T : class
        {
            return DeleteByKeyCore<T>(link, (IList)new[] { key }, null);
        }
        public static int DeleteByKey<T>(this IDbLink link, object[] key, bool allowUnexpectedResult) where T : class
        {
            return DeleteByKeyCore<T>(link, (IList)new[] { key }, allowUnexpectedResult);
        }
        public static int DeleteManyByKey<T>(this IDbLink link, IEnumerable<object> recordsKeys) where T : class
        {
            return DeleteByKeyCore<T>(link, recordsKeys as IList ?? recordsKeys.ToList(), null);
        }
        public static int DeleteManyByKey<T>(this IDbLink link, IEnumerable<object> recordsKeys, bool allowUnexpectedResult) where T : class
        {
            return DeleteByKeyCore<T>(link, recordsKeys as IList ?? recordsKeys.ToList(), allowUnexpectedResult);
        }
        public static int DeleteManyByKey<T>(this IDbLink link, IEnumerable<object[]> recordsKeys) where T : class
        {
            return DeleteByKeyCore<T>(link, recordsKeys as IList ?? recordsKeys.ToList(), null);
        }
        public static int DeleteManyByKey<T>(this IDbLink link, IEnumerable<object[]> recordsKeys, bool allowUnexpectedResult) where T : class
        {
            return DeleteByKeyCore<T>(link, recordsKeys as IList ?? recordsKeys.ToList(), allowUnexpectedResult);
        }
        private static int DeleteByKeyCore<T>(this IDbLink link, IList recordsKeys, bool? allowUnexpectedResult) where T : class
        {
            if (recordsKeys.Count == 0)
                return 0;

            var q = EntityQueryBuilder.CreateDeleteByKeyQuery<T>(recordsKeys);
            var result = link.ExecuteNonQuery(q);

            CheckAffected(allowUnexpectedResult ?? link.ConnectionContext.Provider.Configuration.DefaultAllowUnexpectedRowCountOnDelete, recordsKeys.Count, result);
            return result;
        }

        private static IList<T> GetItems<T>(IEnumerable<T> entities) => entities as IList<T> ?? entities.ToArray();
        private static void CheckAffected(bool allowUnexpectedResult, int itemsCount, int affectedCount)
        {
            if (!allowUnexpectedResult && itemsCount != affectedCount)
                throw new Exception($"Store update, insert, or delete statement affected an unexpected number of rows (affected: {affectedCount}, expected: {itemsCount}).");
        }
    }
}
