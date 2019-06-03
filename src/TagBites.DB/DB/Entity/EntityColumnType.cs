using System;
using System.Collections.Generic;
using System.Text;

namespace TBS.Data.DB.Entity
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityColumnType : Attribute
    {
        public string TypeName { get; }

        public EntityColumnType(string typeName)
        {
            TypeName = typeName;
        }
    }
}
