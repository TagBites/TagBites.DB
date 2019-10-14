using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB
{
    public enum DbSchemaTableType
    {
        Table = 0,
        View = 1
    }

    public class DbSchemaTable
    {
        public string Catalog { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public DbSchemaTableType Type { get; set; }
        public DbSchemaPrimaryKey PrimaryKey { get; set; }
        public IList<DbSchemaColumn> Columns { get; set; }
        public IList<DbSchemaForeignKey> ForeignKeys { get; set; }
    }
}
