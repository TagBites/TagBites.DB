using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TagBites.Sql;
using TagBites.Utils;

namespace TagBites.DB.Entity
{
    public class EntityTable
    {
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
                throw new NotSupportedException(string.Format("{0} is not entity type.", typeof(T).Name));

            return info;
        }
    }
}
