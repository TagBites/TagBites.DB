using System;
using System.Collections;
using System.Collections.Generic;
using TBS.Utils;

namespace TBS.Collections.ObjectModel
{
    internal abstract class LazyList<T> : IList<T>, IList
    {
        private T[] _list;
        private int _loadedCount;
        private int _windowSize = 1;

        public int Count
        {
            get
            {
                Prepare();
                return _list.Length;
            }
        }
        public int LoadWindowSize
        {
            get => _windowSize;
            set
            {
                Guard.ArgumentPositive(value, nameof(value));
                _windowSize = value;
            }
        }
        public bool IsLoaded => _list != null && _loadedCount == _list.Length;
        public bool IsPrepared => _list != null;

        public T this[int index]
        {
            get
            {
                if (index < 0)
                    throw new IndexOutOfRangeException();

                if (_list == null)
                    LoadRange(Math.Max(0, index - _windowSize / 2), _windowSize, false);

                if (index >= Count)
                    throw new IndexOutOfRangeException();

                if (_list[index] == null)
                    LoadRange(Math.Max(0, index - _windowSize / 2), Math.Min(_windowSize, _list.Length - Math.Max(0, index - _windowSize / 2)));

                return _list[index];
            }
        }

        protected LazyList()
        { }
        protected LazyList(int count)
        {
            _list = new T[count];
        }


        public void Prepare()
        {
            if (_list == null)
            {
                LoadCore(ref _list);

                if (_list == null)
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
                    LoadRange(0, _windowSize, false);

                var count = Count;
                for (var i = _windowSize; i < count; i += _windowSize)
                    LoadRange(i, Math.Min(_windowSize, count - i));
            }
        }
        public void LoadRange(int index, int count) => LoadRange(index, count, false);
        public void LoadRange(int index, int count, bool ignoreIndexOutOfRange)
        {
            if (!IsPrepared)
            {
                Guard.ArgumentNonNegative(index, nameof(index));
                Guard.ArgumentPositive(count, nameof(count));

                _loadedCount = LoadCore(ref _list, index, count);

                if (_list == null)
                    throw new Exception("LoadCore return with null collection.");

                if (_loadedCount == _list.Length)
                    OnLoaded();

                if (index + count > _list.Length && !ignoreIndexOutOfRange)
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
                    if (_list[from] == null)
                        break;
                    else
                        ++from;

                while (to > from)
                    if (_list[to] == null)
                        break;
                    else
                        --to;

                if (from > to)
                    return;

                // Load range
                var old = _list;
                var loaded = LoadCore(ref _list, from, to - from + 1);

                if (old != _list)
                    _loadedCount = loaded;
                else
                    _loadedCount += loaded;

                if (_loadedCount == _list.Length)
                    OnLoaded();
            }
        }

        protected virtual int LoadCore(ref T[] collection) => LoadCore(ref collection, 0, _windowSize);
        protected abstract int LoadCore(ref T[] collection, int index, int count);
        protected virtual void OnLoaded() { }
        protected void Reset()
        {
            _list = null;
            _loadedCount = 0;
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
