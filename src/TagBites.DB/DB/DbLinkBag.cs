using System.Collections.Generic;
using TBS.Utils;

namespace TBS.Data.DB
{
    public class DbLinkBag
    {
        private readonly object _locker;
        private IDictionary<string, object> _cache;

        public object this[string name]
        {
            get
            {
                Guard.ArgumentNotNull(name, "name");

                lock (_locker)
                {
                    if (_cache == null)
                        return null;

                    return _cache.TryGetValue(name, out var v) ? v : null;
                }
            }
            set
            {
                Guard.ArgumentNotNull(name, "name");

                lock (_locker)
                {
                    if (_cache == null)
                        _cache = new Dictionary<string, object>();

                    if (value == null)
                        _cache.Remove(name);
                    else
                        _cache[name] = value;
                }
            }
        }

        internal DbLinkBag(object locker)
        {
            _locker = locker;
        }
    }
}
