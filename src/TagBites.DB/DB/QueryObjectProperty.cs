using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TagBites.Utils;

namespace TagBites.DB
{
    internal class QueryObjectProperty
    {
        private readonly int m_columnIndex;
        private readonly object m_localValue;
        private readonly PropertyInfo m_propertyInfo;
        private readonly QueryObjectInitializer m_initializer;

        public int ColumnIndex => m_columnIndex;
        public object LocalValue => m_localValue;
        public PropertyInfo PropertyInfo => m_propertyInfo;
        public QueryObjectInitializer Initializer => m_initializer;

        public QueryObjectProperty(int columnIndex, PropertyInfo propertyInfo, QueryObjectInitializer initializer)
        {
            Guard.ArgumentNotNull(propertyInfo, nameof(propertyInfo));

            m_columnIndex = columnIndex;
            m_propertyInfo = propertyInfo;
            m_initializer = initializer;
        }
        public QueryObjectProperty(object localValue, PropertyInfo propertyInfo, QueryObjectInitializer initializer)
        {
            Guard.ArgumentNotNull(propertyInfo, nameof(propertyInfo));

            m_columnIndex = -1;
            m_localValue = localValue;
            m_propertyInfo = propertyInfo;
            m_initializer = initializer;
        }
    }
}
