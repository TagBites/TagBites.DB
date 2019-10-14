using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB
{
    public class DbSchemaForeignKey
    {
        public string ConstraintName { get; set; }
        public IList<DbSchemaColumn> Columns { get; set; }
        public DbSchemaTable ForeignTable { get; set; }
        public IList<DbSchemaColumn> ForeignColumns { get; set; }
    }
}
