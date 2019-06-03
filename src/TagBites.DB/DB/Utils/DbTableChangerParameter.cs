using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Data.DB.Utils
{
    public class DbTableChangerParameter
    {
        public string Name { get; }

        public string Expression { get; }

        public DbParameterDirection Direction { get; internal set; }

        public DbTableChangerParameter(string name, DbParameterDirection direction)
            : this(name, name, direction)
        { }
        public DbTableChangerParameter(string name, string expression, DbParameterDirection direction)
        {
            Guard.ArgumentNotNullOrWhiteSpace(name, "name");
            Guard.ArgumentNotNullOrWhiteSpace(expression, "expression");

            Name = name;
            Expression = expression;
            Direction = direction;
        }
    }
}