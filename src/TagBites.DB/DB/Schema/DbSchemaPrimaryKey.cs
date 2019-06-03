using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Data.DB.Schema
{
    public class DbSchemaPrimaryKey
    {
        public string ConstraintName { get; set; }
        public IList<DbSchemaColumn> Columns { get; set; }
    }
}
