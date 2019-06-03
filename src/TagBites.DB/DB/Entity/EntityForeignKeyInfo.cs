using System;
using System.Reflection;
using TBS.Utils;

#if NET_45
using System.ComponentModel.DataAnnotations.Schema;
#else
using TBS.Data.DB.Entity.Schema;
#endif

namespace TBS.Data.DB.Entity
{
    public class EntityForeignKeyInfo
    {
        private EntityTableInfo _tableInfo;

        public PropertyInfo Property { get; }
        public string PropertyName => Property.Name;
        public Type PropertyType => Property.PropertyType;

        public EntityTableInfo TableInfo => _tableInfo ?? (_tableInfo = EntityTableInfo.GetTableByType(PropertyType));
        public EntityColumnInfo Column { get; }

        public EntityForeignKeyInfo(PropertyInfo property, EntityColumnInfo column)
        {
            Property = property;
            Column = column;

            var table = PropertyType.GetTypeInfo().GetCustomAttribute<TableAttribute>(true);
            if (table == null)
                throw new Exception();
        }

        public override string ToString() => $"{PropertyName} ({Column.Name})";
    }
}
