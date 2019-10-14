using System.Collections.Generic;
using TagBites.Utils;

namespace TagBites.Sql
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

        private readonly List<T> _list = new List<T>();

        public int Count => _list.Count;

        public T this[int index]
        {
            get => _list[index];
            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                _list[index] = value;
            }
        }


        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
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
            _list.Add(item);
            return item;
        }
        public void Insert(int index, T item)
        {
            Guard.ArgumentNotNull(item, nameof(item));
            _list.Insert(index, item);
        }
        public bool Remove(T item)
        {
            return _list.Remove(item);
        }
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }
        public void Clear()
        {
            _list.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item)
        {
            Guard.ArgumentNotNull(item, nameof(item));
            _list.Add(item);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
