using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using TagBites.Utils;

namespace TagBites.DB.Entity
{
    public class EntityInversePropertyInfo
    {
        private EntityTableInfo _tableInfo;

        public PropertyInfo Property { get; }
        public string PropertyName => Property.Name;
        public string InversePropertyName { get; }
        public Type InversePropertyType { get; }

        public EntityTableInfo TableInfo => _tableInfo ?? (_tableInfo = EntityTableInfo.GetTableByType(InversePropertyType));

        public EntityInversePropertyInfo(PropertyInfo property, string inversePropertyName, Type inversePropertyType)
        {
            Property = property;
            InversePropertyName = inversePropertyName;
            InversePropertyType = inversePropertyType;

            var table = TypeUtils.TryGetFirstAttributeDefault<TableAttribute>(inversePropertyType, true);
            if (table == null)
                throw new Exception();
        }


        public override string ToString() => $"{PropertyName}";
    }
}
