using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Data.DB
{
    public class DbLinkBag
    {
        private readonly object m_locker;
        private IDictionary<string, object> m_cache;

        public object this[string name]
        {
            get
            {
                Guard.ArgumentNotNull(name, "name");

                lock (m_locker)
                {
                    if (m_cache == null)
                        return null;

                    return m_cache.TryGetValue(name, out var v) ? v : null;
                }
            }
            set
            {
                Guard.ArgumentNotNull(name, "name");

                lock (m_locker)
                {
                    if (m_cache == null)
                        m_cache = new Dictionary<string, object>();

                    if (value == null)
                        m_cache.Remove(name);
                    else
                        m_cache[name] = value;
                }
            }
        }

        internal DbLinkBag(object locker)
        {
            m_locker = locker;
        }
    }
}
