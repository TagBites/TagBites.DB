using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Data.DB.Utils
{
    public class DbRecordValues : Dictionary<string, object>
    {
        internal object Id { get; private set; }

        public DbRecordValues()
        { }
        public DbRecordValues(object key)
        {
            Id = key;
        }
        public DbRecordValues(object[] keys)
        {
            Id = keys;
        }
    }
}
