using System;
using System.Collections.Generic;
using System.Text;

namespace TBS.Collections
{
    internal class SpanCollection<T> : ReadOnlyCollectionBase<T>
    {
        private readonly IList<T> m_collection;
        private readonly int m_startIndex;
        private readonly bool m_reverse;

        public SpanCollection(IList<T> collection, int startIndex, int countCore, bool reverse = false)
        {
            m_collection = collection;
            m_startIndex = startIndex;
            m_reverse = reverse;
            CountCore = countCore;
        }


        protected override T GetItemCore(int index) => m_reverse
            ? m_collection[m_startIndex + CountCore - index - 1]
            : m_collection[m_startIndex + index];
    }
}
