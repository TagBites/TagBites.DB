using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Sql
{
    internal interface ISqlElement
    {
        void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder);
    }
}