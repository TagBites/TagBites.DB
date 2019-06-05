using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using TBS.Utils;

namespace TBS.Data.DB.Entity
{
    public class EntityColumnInfo
    {
        public Func<object, object> Getter { get; }
        public Action<object, object> Setter { get; }
        public PropertyInfo Property { get; }
        public string PropertyName => Property.Name;
        public Type PropertyType => Property.PropertyType;
        public string TypeName { get; }
        public string Name { get; }
        public int Order { get; }
        public DatabaseGeneratedOption DatabaseGeneratedOption { get; }
        public bool IsKeyPart { get; }

        public EntityColumnInfo(PropertyInfo property, ColumnAttribute column, DatabaseGeneratedAttribute databaseGenerated, KeyAttribute key)
        {
            Property = property;
            Getter = PropertyUtils.BuildGetAccessor(property);
            Setter = (c, v) => Property.SetValue(c, v, null);

            Name = column.Name;
            TypeName = DataHelper.FirstNotNullOrEmpty(column.TypeName, property.GetCustomAttribute<EntityColumnType>()?.TypeName);
            Order = column.Order;
            IsKeyPart = key != null;
            DatabaseGeneratedOption = databaseGenerated != null
                ? databaseGenerated.DatabaseGeneratedOption
                : (key != null ? DatabaseGeneratedOption.Identity : DatabaseGeneratedOption.None);
        }


        public override string ToString() => $"{PropertyName} ({Name})";
    }
}
