using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TBS.Data.DB.Utils;
using TBS.Sql;
using TBS.Utils;

#if NET_45
using System.ComponentModel.DataAnnotations.Schema;
#else
using TBS.Data.DB.Entity.Schema;
#endif

namespace TBS.Data.DB.Entity
{
    public class EntityTable
    {
        public static T GetByKey<T>(IDbLink link, params object[] key)
            where T : class
        {
            var info = EntityTableInfo<T>.Instance;

            if (!info.HasPrimaryKey)
                throw new Exception(String.Format("{0} does not have a primary key.", typeof(T).Name));

            if (info.PrimaryKey.Count != key.Length)
                throw new ArgumentException("Invalid key arguments count.", nameof(key));

            var q = CreateSelect(info);

            for (int i = 0; i < info.PrimaryKey.Count; i++)
                q.Where.AddEquals(q.From[0].Table.Column(info.PrimaryKey[i].Name), SqlExpression.Argument(key[i]));

            return link.Execute<T>(q).FirstOrDefault();
        }
        public static IList<T> Where<T>(IDbLink link, Expression<Func<T, object>> property, object value)
            where T : class
        {
            var info = EntityTableInfo<T>.Instance;
            var q = CreateSelect(info);

            var column = info.GetColumnByPropertyName(PropertyUtils.GetMemberExpressionName(property));
            if (column == null)
                throw new ArgumentException("It is not column property.", nameof(property));

            q.Where.AddEquals(q.From[0].Table.Column(column.Name), SqlExpression.Argument(value));

            return link.Execute<T>(q);
        }

        public static int Delete<T>(IDbLink link, T entity)
            where T : class
        {
            Guard.ArgumentNotNull(entity, "entity");

            var info = GetTableInfo<T>();

            var q = new SqlQueryDelete(info.TableFullName);
            for (int i = 0; i < info.PrimaryKey.Count; i++)
                q.Where.AddEquals(q.From.Column(info.PrimaryKey[i].Name), SqlExpression.Argument(info.PrimaryKey[i].Getter(entity)));

            return link.ExecuteNonQuery(q);
        }
        public static int Delete<T>(IDbLink link, params object[] key)
            where T : class
        {
            var info = GetTableInfo<T>();

            if (info.PrimaryKey.Count != key.Length)
                throw new ArgumentException("Invalid key arguments count.", nameof(key));

            var q = new SqlQueryDelete(info.TableFullName);
            for (int i = 0; i < info.PrimaryKey.Count; i++)
                q.Where.AddEquals(q.From.Column(info.PrimaryKey[i].Name), SqlExpression.Argument(key[i]));

            return link.ExecuteNonQuery(q);
        }

        public static int Update<T>(IDbLink link, T entity, DbTableChangerExecuteMode mode = DbTableChangerExecuteMode.InsertOrUpdateBasedOnId)
            where T : class
        {
            Guard.ArgumentNotNull(entity, "entity");
            return Update<T>(link, new[] { entity }, mode);
        }
        public static int Update<T>(IDbLink link, IEnumerable<T> entities, DbTableChangerExecuteMode mode = DbTableChangerExecuteMode.InsertOrUpdateBasedOnId)
            where T : class
        {
            Guard.ArgumentNotNull(entities, "entities");

            var items = entities.ToArray();
            if (items.Length == 0)
                return 0;

            var info = EntityTableInfo<T>.Instance;
            if (info == null)
                throw new NotSupportedException(String.Format("{0} is not entity type.", typeof(T).Name));

            var tableChanger = new DbTableChanger(info.TableFullName, info.PrimaryKey.Select(x => x.Name).ToArray(), info.PrimaryKey[0].DatabaseGeneratedOption == DatabaseGeneratedOption.Identity);

            foreach (var entity in entities)
            {
                var record = tableChanger.Records.Add();

                foreach (var column in info.Columns)
                {
                    var value = column.Getter(entity);

                    if (column.DatabaseGeneratedOption == DatabaseGeneratedOption.None || !Equals(value, Activator.CreateInstance(column.PropertyType)))
                        record[column.Name] = value;
                }
            }

            foreach (var column in info.Columns)
                if (column.DatabaseGeneratedOption != DatabaseGeneratedOption.None && !column.IsKeyPart) //TODO
                    if (!tableChanger.Parameters.Contains(column.Name))
                        tableChanger.Parameters.Add(new DbTableChangerParameter(column.Name, DbParameterDirection.Output));
                    else
                        tableChanger.Parameters[column.Name].Direction = DbParameterDirection.InputOutput;

            var changed = tableChanger.Execute(link, mode);

            foreach (var column in info.Columns)
                if (column.IsKeyPart)
                    if (!tableChanger.Parameters.Contains(column.Name))
                    {
                        for (int i = 0; i < items.Length; i++)
                            column.Setter(items[i], tableChanger.Records[i][column.Name]);
                    }

            foreach (var parameter in tableChanger.Parameters.Where(x => (x.Direction & DbParameterDirection.Output) == DbParameterDirection.Output))
            {
                var columnInfo = info.GetColumnByName(parameter.Name);

                for (int i = 0; i < items.Length; i++)
                    columnInfo.Setter(items[i], tableChanger.Records[i][parameter.Name]);
            }

            if (tableChanger.KeyColumnsAreAutoIncremented)
            {
                var columnInfo = info.PrimaryKey[0];
                for (int i = 0; i < items.Length; i++)
                    columnInfo.Setter(items[i], tableChanger.Records[i][columnInfo.Name]);
            }

            return changed;
        }

        private static SqlQuerySelect CreateSelect(EntityTableInfo info)
        {
            var q = new SqlQuerySelect();
            var t = q.From.Add(info.TableFullName);

            foreach (var item in info.Columns)
                q.Select.Add(t, item.Name, item.PropertyName);

            return q;
        }
        private static EntityTableInfo GetTableInfo<T>()
            where T : class
        {
            var info = EntityTableInfo<T>.Instance;
            if (info == null)
                throw new NotSupportedException(String.Format("{0} is not entity type.", typeof(T).Name));

            return info;
        }
    }
}
