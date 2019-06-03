using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TBS.Utils;

namespace TBS.Collections.ObjectModel
{
    public class ListCollection<T> : IList<T>, IList
    {
        private readonly IList<T> m_list;

        public virtual bool IsReadOnly => m_list.IsReadOnly;
        public int Count => m_list.Count;

        public T this[int index]
        {
            get => m_list[index];
            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                Guard.ArgumentIndexInRange(index, nameof(index), Count);
                ThrowIfReadOnly();

                var old = m_list[index];
                if (!EqualityComparer<T>.Default.Equals(old, value))
                    SetCore(index, old, value);
            }
        }

        public ListCollection()
        {
            m_list = new List<T>();
        }
        public ListCollection(IList<T> container)
        {
            Guard.ArgumentNotNull(container, "container");
            m_list = container;
        }


        public int IndexOf(T item)
        {
            return m_list.IndexOf(item);
        }
        public bool Contains(T item)
        {
            return m_list.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            m_list.CopyTo(array, arrayIndex);
        }

        public void Move(int oldIndex, int newIndex)
        {
            Guard.ArgumentIndexInRange(oldIndex, nameof(oldIndex), Count);
            Guard.ArgumentIndexInRange(newIndex, nameof(newIndex), Count);

            MoveCore(oldIndex, newIndex, m_list[oldIndex]);
        }
        public void Add(T item)
        {
            Guard.ArgumentNotNull(item, nameof(item));
            ThrowIfReadOnly();

            InsertCore(Count, item);
        }
        public void AddRange(IEnumerable<T> items)
        {
            Guard.ArgumentNotNull(items, nameof(items));
            ThrowIfReadOnly();

            AddRangeCore(items);
        }
        public void Insert(int index, T item)
        {
            Guard.ArgumentNotNull(item, nameof(item));
            Guard.ArgumentIndexInRange(index, nameof(index), Count + 1);
            ThrowIfReadOnly();

            InsertCore(index, item);
        }
        public bool Remove(T item)
        {
            ThrowIfReadOnly();

            var index = IndexOf(item);
            if (index >= 0)
            {
                RemoveCore(index, item);
                return true;
            }

            return false;
        }
        public void RemoveAt(int index)
        {
            Guard.ArgumentIndexInRange(index, nameof(index), Count);
            ThrowIfReadOnly();

            RemoveCore(index, m_list[index]);
        }
        public void Clear()
        {
            ThrowIfReadOnly();

            if (m_list.Count > 0)
                ClearCore();
        }

        protected virtual void MoveCore(int oldIndex, int newIndex, T item)
        {
            m_list.RemoveAt(oldIndex);
            m_list.Insert(newIndex, item);

            OnCollectionChanged();
        }
        protected virtual void AddRangeCore(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Guard.ArgumentNotNull(item, "items[]");
                var index = Count;

                OnItemInserting(index, item);
                m_list.Insert(index, item);
                OnItemInserted(index, item);
            }

            OnCollectionChanged();
        }
        protected virtual void InsertCore(int index, T item)
        {
            OnItemInserting(index, item);

            m_list.Insert(index, item);

            OnItemInserted(index, item);
            OnCollectionChanged();
        }
        protected virtual void RemoveCore(int index, T item)
        {
            OnItemRemoving(index, item);

            m_list.RemoveAt(index);

            OnItemRemoved(index, item);
            OnCollectionChanged();
        }
        protected virtual void SetCore(int index, T old, T value)
        {
            OnItemRemoving(index, old);
            OnItemInserting(index, value);

            m_list[index] = value;

            OnItemRemoved(index, old);
            OnItemInserted(index, value);
            OnCollectionChanged();
        }
        protected virtual void ClearCore()
        {
            if (Count > 0)
            {
                do
                {
                    var index = Count - 1;
                    var item = m_list[index];

                    OnItemRemoving(index, item);
                    m_list.RemoveAt(index);
                    OnItemRemoved(index, item);
                }
                while (Count > 0);

                OnCollectionChanged();
            }
        }

        protected virtual void OnItemInserting(int index, T item) { }
        protected virtual void OnItemInserted(int index, T item) { }
        protected virtual void OnItemRemoving(int index, T item) { }
        protected virtual void OnItemRemoved(int index, T item) { }
        protected virtual void OnCollectionChanged() { }

        public IEnumerator<T> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ThrowIfReadOnly()
        {
            if (IsReadOnly)
                throw new NotSupportedException();
        }

        #region IList

        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => ((ICollection)m_list).SyncRoot;
        bool IList.IsFixedSize => IsReadOnly;

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }


        bool IList.Contains(object value)
        {
            return ((IList)this).IndexOf(value) != -1;
        }
        int IList.IndexOf(object value)
        {
            if (value is T)
                return IndexOf((T)value);

            return -1;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }
        void IList<T>.Insert(int index, T item)
        {
            Insert(index, item);
        }

        int IList.Add(object value)
        {
            var index = Count;
            Add((T)value);
            return index;
        }
        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            if (value is T)
                Remove((T)value);
        }
        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }
        void IList.Clear()
        {
            Clear();
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            ((IList)m_list).CopyTo(array, arrayIndex);
        }

        #endregion
    }
}
