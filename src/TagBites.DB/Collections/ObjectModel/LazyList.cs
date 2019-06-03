using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBS.Utils;

namespace TBS.Collections.ObjectModel
{
    public interface ILazyList : IList
    {
        void LoadAll(bool loadInParts);
    }

    public abstract class LazyList<T> : IList<T>, ILazyList
    {
        private T[] m_list;
        private int m_loadedCount;
        private int m_windowSize = 1;

        public int Count
        {
            get
            {
                Prepare();
                return m_list.Length;
            }
        }
        public int LoadWindowSize
        {
            get => m_windowSize;
            set
            {
                Guard.ArgumentPositive(value, nameof(value));
                m_windowSize = value;
            }
        }
        public bool IsLoaded => m_list != null && m_loadedCount == m_list.Length;
        public bool IsPrepared => m_list != null;

        public T this[int index]
        {
            get
            {
                if (index < 0)
                    throw new IndexOutOfRangeException();

                if (m_list == null)
                    LoadRange(Math.Max(0, index - m_windowSize / 2), m_windowSize, false);

                if (index >= Count)
                    throw new IndexOutOfRangeException();

                if (m_list[index] == null)
                    LoadRange(Math.Max(0, index - m_windowSize / 2), Math.Min(m_windowSize, m_list.Length - Math.Max(0, index - m_windowSize / 2)));

                return m_list[index];
            }
        }

        protected LazyList()
        { }
        protected LazyList(int count)
        {
            m_list = new T[count];
        }


        public void Prepare()
        {
            if (m_list == null)
            {
                LoadCore(ref m_list);

                if (m_list == null)
                    throw new Exception("LoadCore return with null collection.");
            }
        }
        public void LoadAll(bool loadInParts)
        {
            if (IsLoaded)
                return;

            if (!loadInParts)
                LoadRange(0, IsPrepared ? Count : Int32.MaxValue, true);
            else
            {
                if (!IsPrepared)
                    LoadRange(0, m_windowSize, false);

                var count = Count;
                for (var i = m_windowSize; i < count; i += m_windowSize)
                    LoadRange(i, Math.Min(m_windowSize, count - i));
            }
        }
        public void LoadRange(int index, int count) => LoadRange(index, count, false);
        public void LoadRange(int index, int count, bool ignoreIndexOutOfRange)
        {
            if (!IsPrepared)
            {
                Guard.ArgumentNonNegative(index, nameof(index));
                Guard.ArgumentPositive(count, nameof(count));

                m_loadedCount = LoadCore(ref m_list, index, count);

                if (m_list == null)
                    throw new Exception("LoadCore return with null collection.");

                if (m_loadedCount == m_list.Length)
                    OnLoaded();

                if (index + count > m_list.Length && !ignoreIndexOutOfRange)
                    throw new IndexOutOfRangeException();
            }
            else
            {
                var realCount = Count;
                if (ignoreIndexOutOfRange)
                {
                    Guard.ArgumentNonNegative(index, nameof(index));
                    Guard.ArgumentPositive(count, nameof(count));

                    if (index >= realCount)
                        return;

                    if (index + count > Count)
                        count = Count - index;
                }
                else
                {
                    Guard.ArgumentIndexInRange(index, nameof(index), realCount);
                    Guard.ArgumentIndexInRange(index + count - 1, nameof(count), realCount);
                }

                if (IsLoaded)
                    return;

                // Find range
                var from = index;
                var to = index + count - 1;

                while (from <= to)
                    if (m_list[from] == null)
                        break;
                    else
                        ++from;

                while (to > from)
                    if (m_list[to] == null)
                        break;
                    else
                        --to;

                if (from > to)
                    return;

                // Load range
                var old = m_list;
                var loaded = LoadCore(ref m_list, from, to - from + 1);

                if (old != m_list)
                    m_loadedCount = loaded;
                else
                    m_loadedCount += loaded;

                if (m_loadedCount == m_list.Length)
                    OnLoaded();
            }
        }

        protected virtual int LoadCore(ref T[] collection) => LoadCore(ref collection, 0, m_windowSize);
        protected abstract int LoadCore(ref T[] collection, int index, int count);
        protected virtual void OnLoaded() { }
        protected void Reset()
        {
            m_list = null;
            m_loadedCount = 0;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }
        public int IndexOf(T item)
        {
            var count = Count;
            for (int i = 0; i < count; i++)
                if (Equals(item, this[i]))
                    return i;

            return -1;
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            var count = Count;
            for (int i = 0; i < count; i++)
                array.SetValue(this[i], arrayIndex + i);
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerable<T> GetEnumerable()
            {
                var count = Count;
                for (int i = 0; i < count; i++)
                    yield return this[i];
            }
            return GetEnumerable().GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerable GetEnumerable()
            {
                var count = Count;
                for (int i = 0; i < count; i++)
                    yield return this[i];
            }
            return GetEnumerable().GetEnumerator();
        }

        #region Interfaces

        bool ICollection<T>.IsReadOnly => true;
        bool IList.IsReadOnly => true;
        bool IList.IsFixedSize => true;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;

        object IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }
        T IList<T>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }


        bool IList.Contains(object value) => value is T && Contains((T)value);
        int IList.IndexOf(object value) => value is T ? IndexOf((T)value) : -1;

        void ICollection.CopyTo(Array array, int index)
        {
            for (int i = 0; i < Count; i++)
                array.SetValue(this[i], index + i);
        }

        public void Add(T item) => throw new NotSupportedException();
        int IList.Add(object value) => throw new NotSupportedException();
        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();
        void IList.Insert(int index, object value) => throw new NotSupportedException();
        void IList.Remove(object value) => throw new NotSupportedException();
        void IList.RemoveAt(int index) => throw new NotSupportedException();
        void IList<T>.RemoveAt(int index) => throw new NotSupportedException();
        bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
        void IList.Clear() => throw new NotSupportedException();
        void ICollection<T>.Clear() => throw new NotSupportedException();

        #endregion
    }
}
