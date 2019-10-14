using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB
{
    public class DbSchema
    {
        public IList<DbSchemaTable> Tables { get; set; }
    }
}
