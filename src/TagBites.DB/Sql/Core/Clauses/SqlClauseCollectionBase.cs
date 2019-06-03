using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TBS.Collections;
using TBS.Utils;

namespace TBS.Sql
{
    public abstract class SqlClauseCollectionBase<T> : IList<T>, ISqlElement
        where T : class
    {
        void ISqlElement.Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            Accept(resolver, builder);
        }
        protected abstract void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder);

        public override string ToString()
        {
            var builder = SqlQueryBuilder.CreateToStringBuilder();
            Accept(SqlQueryResolver.DefaultToStringResolver, builder);
            return builder.Query;
        }

        #region IList<T>

        private readonly List<T> m_list = new List<T>();

        public int Count => m_list.Count;

        public T this[int index]
        {
            get => m_list[index];
            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                m_list[index] = value;
            }
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

        public void AddRange(params T[] range)
        {
            for (int i = 0; i < range.Length; i++)
                Add(range[i]);
        }
        public void AddRange(IEnumerable<T> range)
        {
            Guard.ArgumentNotNull(range, "range");

            foreach (var item in range)
                Add(item);
        }
        public T Add(T item)
        {
            Guard.ArgumentNotNull(item, nameof(item));
            m_list.Add(item);
            return item;
        }
        public void Insert(int index, T item)
        {
            Guard.ArgumentNotNull(item, nameof(item));
            m_list.Insert(index, item);
        }
        public bool Remove(T item)
        {
            return m_list.Remove(item);
        }
        public void RemoveAt(int index)
        {
            m_list.RemoveAt(index);
        }
        public void Clear()
        {
            m_list.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item)
        {
            Guard.ArgumentNotNull(item, nameof(item));
            m_list.Add(item);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
