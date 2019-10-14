using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.DB.Utils
{
    public class DbRecordValues : Dictionary<string, object>
    {
        internal object Id { get; }

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
