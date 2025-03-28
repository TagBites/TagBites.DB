﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB.Entity
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
