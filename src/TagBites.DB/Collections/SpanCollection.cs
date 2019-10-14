using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TagBites.Collections
{
    internal class SpanCollection<T> : ReadOnlyCollectionBase<T>
    {
        private readonly IList<T> _collection;
        private readonly int _startIndex;
        private readonly bool _reverse;

        public SpanCollection(IList<T> collection, int startIndex, int countCore, bool reverse = false)
        {
            _collection = collection;
            _startIndex = startIndex;
            _reverse = reverse;
            CountCore = countCore;
        }


        protected override T GetItemCore(int index) => _reverse
            ? _collection[_startIndex + CountCore - index - 1]
            : _collection[_startIndex + index];
    }
}
