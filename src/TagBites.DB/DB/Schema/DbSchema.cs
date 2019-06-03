using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Data.DB.Schema
{
    public class DbSchema
    {
        public IList<DbSchemaTable> Tables { get; set; }
    }
}
