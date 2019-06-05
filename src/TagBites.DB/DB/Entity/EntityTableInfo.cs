using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using TBS.Utils;

namespace TBS.Data.DB.Entity
{
    public class EntityTableInfo
    {
        private readonly Dictionary<string, EntityColumnInfo> s_propertyNamesToInfo;
        private readonly Dictionary<string, EntityColumnInfo> s_columnNamesToInfo;
        private readonly Dictionary<string, EntityForeignKeyInfo> s_foreingPropertyNamesInfo;

        public Type Type { get; }
        public string Schema { get; }
        public string TableName { get; }
        public string TableFullName { get; }
        public IList<EntityColumnInfo> PrimaryKey { get; }
        public bool HasPrimaryKey => PrimaryKey.Count > 0;
        public IList<EntityColumnInfo> Columns { get; }
        public IList<EntityForeignKeyInfo> ForeignKeys { get; }

        public EntityTableInfo(Type type, TableAttribute table)
        {
            var columns = new List<EntityColumnInfo>();
            var foreignKeys = new List<EntityForeignKeyInfo>();

            foreach (var property in TypeUtils.GetProperties(type))
            {
                var columnAttribute = MemberUtils.TryGetFirstAttribute<ColumnAttribute>(property, true);
                if (columnAttribute != null)
                {
                    var info = new EntityColumnInfo(
                        property,
                        columnAttribute,
                         MemberUtils.TryGetFirstAttribute<DatabaseGeneratedAttribute>(property, true),
                         MemberUtils.TryGetFirstAttribute<KeyAttribute>(property, true));
                    columns.Add(info);
                }
            }
            s_propertyNamesToInfo = columns.ToDictionary(x => x.PropertyName);

            foreach (var property in TypeUtils.GetProperties(type))
            {
                var foreignKeyAttribute = MemberUtils.TryGetFirstAttribute<ForeignKeyAttribute>(property, true);
                if (foreignKeyAttribute != null)
                {
                    var foreignColumn = s_propertyNamesToInfo.TryGetValue(foreignKeyAttribute.Name, out var v) ? v : null;

                    var info = new EntityForeignKeyInfo(property, foreignColumn);
                    foreignKeys.Add(info);
                }
                else
                {
                    var inversePropertyAttribute = MemberUtils.TryGetFirstAttribute<InversePropertyAttribute>(property, true);
                    if (inversePropertyAttribute != null)
                    {

                    }
                }
            }

            Type = type;
            Schema = table.Schema;
            TableName = table.Name;
            TableFullName = !string.IsNullOrEmpty(Schema) ? $"{Schema}.{TableName}" : TableName;
            Columns = columns.OrderByDescending(x => x.IsKeyPart).ThenBy(x => x.Order).ToList().AsReadOnly();
            PrimaryKey = columns.Where(x => x.IsKeyPart).OrderBy(x => x.Order).ToList().AsReadOnly();
            ForeignKeys = foreignKeys.ToList().AsReadOnly();

            s_columnNamesToInfo = columns.ToDictionary(x => x.Name);
            s_foreingPropertyNamesInfo = foreignKeys.ToDictionary(x => x.PropertyName);
        }


        public EntityColumnInfo GetColumnByName(string columnName)
        {
            return s_columnNamesToInfo.TryGetValue(columnName, out var v) ? v : null;
        }
        public EntityColumnInfo GetColumnByPropertyName(string propertyName)
        {
            return s_propertyNamesToInfo.TryGetValue(propertyName, out var v) ? v : null;
        }

        public EntityForeignKeyInfo GetForeignKeyPropertyName(string propertyName)
        {
            return s_foreingPropertyNamesInfo.TryGetValue(propertyName, out var v) ? v : null;
        }

        public static EntityTableInfo GetTableByType(Type type)
        {
            return (EntityTableInfo)typeof(EntityTableInfo<>).MakeGenericType(type)
                .GetRuntimeProperty("Instance")
                .GetValue(null, null);
        }

        public override string ToString() => $"{Type.Name} ({TableFullName})";
    }

    public static class EntityTableInfo<T>
    {
        public static EntityTableInfo Instance { get; }

        static EntityTableInfo()
        {
            var type = typeof(T);
            var table = type.GetTypeInfo().GetCustomAttribute<TableAttribute>(true);
            if (table == null)
                return;

            Instance = new EntityTableInfo(type, table);
        }
    }
}
