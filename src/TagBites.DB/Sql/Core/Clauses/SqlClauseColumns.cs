using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TagBites.Sql
{
    public class SqlClauseColumns : SqlClauseCollectionBase<string>
    {
        /// <summary>
        /// Adds an array element, an array slice or a field of a column, e.g. <c>column[1]</c>.
        /// </summary>
        /// <remarks>
        /// The entry keeps the name of the column, so the indexer returns <c>column</c> for <c>column[1]</c>.
        /// </remarks>
        public void Add(SqlExpression target)
        {
            var column = SqlColumn.GetTargetColumn(target);

            if (_list is not ColumnList columns)
                _list = columns = new ColumnList(_list);

            columns.Add(column.ColumnName, target);
        }
        /// <summary>
        /// Gets the column name and the target added by <see cref="Add(SqlExpression)"/>.
        /// </summary>
        /// <remarks>
        /// The target is <c>null</c> for a column added by name.
        /// </remarks>
        public (string Name, SqlExpression Target) GetEntry(int index)
        {
            return _list is ColumnList columns
                ? columns.GetEntry(index)
                : (_list[index], null);
        }

        protected override void Accept(SqlQueryResolver resolver, SqlQueryBuilder builder)
        {
            resolver.VisitClause(this, builder);
        }

        private sealed class ColumnList : IList<string>
        {
            private readonly List<(string Name, SqlExpression Target)> _entries;

            public int Count => _entries.Count;
            public bool IsReadOnly => false;

            public string this[int index]
            {
                get => _entries[index].Name;
                set => _entries[index] = (value, null);
            }

            public ColumnList(IEnumerable<string> names) => _entries = names.Select(x => (x, (SqlExpression)null)).ToList();


            public (string Name, SqlExpression Target) GetEntry(int index) => _entries[index];
            public void Add(string name, SqlExpression target) => _entries.Add((name, target));

            public void Add(string item) => _entries.Add((item, null));
            public void Insert(int index, string item) => _entries.Insert(index, (item, null));
            public bool Remove(string item)
            {
                var index = IndexOf(item);
                if (index < 0)
                    return false;

                _entries.RemoveAt(index);
                return true;
            }
            public void RemoveAt(int index) => _entries.RemoveAt(index);
            public void Clear() => _entries.Clear();

            public int IndexOf(string item) => _entries.FindIndex(x => x.Name == item);
            public bool Contains(string item) => IndexOf(item) >= 0;
            public void CopyTo(string[] array, int arrayIndex)
            {
                foreach (var entry in _entries)
                    array[arrayIndex++] = entry.Name;
            }

            public IEnumerator<string> GetEnumerator() => _entries.Select(x => x.Name).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
