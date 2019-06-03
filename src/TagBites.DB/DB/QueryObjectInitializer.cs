using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Data.DB
{
    internal class QueryObjectInitializer
    {
        private readonly Type m_type;
        private readonly QueryObjectParameter[] m_parameters;
        private readonly QueryObjectProperty[] m_properties;

        public Type Type => m_type;
        public QueryObjectParameter[] Parameters => m_parameters;
        public QueryObjectProperty[] Properties => m_properties;

        public QueryObjectInitializer(Type type, QueryObjectParameter[] parameters, QueryObjectProperty[] properties)
        {
            m_type = type;
            m_parameters = parameters;
            m_properties = properties;
        }
    }
}
