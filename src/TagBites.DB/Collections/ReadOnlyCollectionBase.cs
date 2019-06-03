using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBS.Collections
{
    internal abstract class ReadOnlyCollectionBase<T> : IList<T>
    {
        bool ICollection<T>.IsReadOnly => true;

        protected int CountCore = 0;
        public int Count => CountCore;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= CountCore)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return GetItemCore(index);
            }
        }
        T IList<T>.this[int index] { get => this[index]; set => throw new NotImplementedException(); }


        protected abstract T GetItemCore(int index);

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (var i = 0; i < CountCore; i++)
                array[arrayIndex + i] = GetItemCore(i);
        }

        public int IndexOf(T item)
        {
            for (var i = 0; i < CountCore; i++)
                if (Equals(item, GetItemCore(i)))
                    return i;

            return -1;
        }
        bool ICollection<T>.Contains(T item) => IndexOf(item) != -1;

        public IEnumerator<T> GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();

            IEnumerable<T> GetEnumerable()
            {
                for (var i = 0; i < CountCore; i++)
                    yield return GetItemCore(i);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void IList<T>.Insert(int index, T item) => throw new NotImplementedException();
        void IList<T>.RemoveAt(int index) => throw new NotImplementedException();
        void ICollection<T>.Add(T item) => throw new NotImplementedException();
        void ICollection<T>.Clear() => throw new NotImplementedException();
        bool ICollection<T>.Remove(T item) => throw new NotImplementedException();
    }
}
