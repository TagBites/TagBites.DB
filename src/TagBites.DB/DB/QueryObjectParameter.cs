using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TagBites.Utils;

namespace TagBites.DB
{
    internal class QueryObjectParameter
    {
        private readonly int m_columnIndex;
        private readonly object m_localValue;
        private readonly ParameterInfo m_parameterInfo;
        private readonly QueryObjectInitializer m_initializer;

        public int ColumnIndex => m_columnIndex;
        public object LocalValue => m_localValue;
        public ParameterInfo ParameterInfo => m_parameterInfo;
        public QueryObjectInitializer Initializer => m_initializer;

        public QueryObjectParameter(int columnIndex, ParameterInfo parameterInfo, QueryObjectInitializer initializer)
        {
            Guard.ArgumentNotNull(parameterInfo, nameof(parameterInfo));

            m_columnIndex = columnIndex;
            m_parameterInfo = parameterInfo;
            m_initializer = initializer;
        }
        public QueryObjectParameter(object localValue, ParameterInfo parameterInfo, QueryObjectInitializer initializer)
        {
            Guard.ArgumentNotNull(parameterInfo, nameof(parameterInfo));

            m_columnIndex = -1;
            m_localValue = localValue;
            m_parameterInfo = parameterInfo;
            m_initializer = initializer;
        }
    }
}
