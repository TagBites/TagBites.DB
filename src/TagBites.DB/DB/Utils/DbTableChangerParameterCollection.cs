using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.DB.Utils
{
    public class DbTableChangerParameterCollection : ICollection<DbTableChangerParameter>
    {
        private readonly DbTableChanger m_owner;
        private readonly Dictionary<string, DbTableChangerParameter> m_parameters = new Dictionary<string, DbTableChangerParameter>();

        public int Count => m_parameters.Count;
        public bool IsReadOnly => false;

        public DbTableChangerParameter this[string columnName]
        {
            get
            {
                Guard.ArgumentNotNullOrWhiteSpace(columnName, "columnName");
                return m_parameters[columnName];
            }
            set
            {
                Guard.ArgumentNotNullOrWhiteSpace(columnName, "columnName");

                if (value == null)
                    m_parameters.Remove(columnName);
                else
                {
                    if (columnName != value.Name)
                        throw new ArgumentException();

                    Add(value);
                }
            }
        }

        internal DbTableChangerParameterCollection(DbTableChanger owner)
        {
            m_owner = owner;
        }

        public bool Contains(string columnName)
        {
            return m_parameters.ContainsKey(columnName);
        }
        public void Add(DbTableChangerParameter parameter)
        {
            if (m_owner.KeyColumnNamesCore.Contains(parameter.Name))
                throw new ArgumentException("Can not set id column as parameter.");

            m_parameters[parameter.Name] = parameter;
        }
        internal void AddInputIfNotExists(string columnName)
        {
            if (!m_owner.KeyColumnNamesCore.Contains(columnName) && !m_parameters.ContainsKey(columnName))
                m_parameters[columnName] = new DbTableChangerParameter(columnName, columnName, DbParameterDirection.Input);
        }

        void ICollection<DbTableChangerParameter>.Clear()
        {
            throw new NotImplementedException();
        }
        bool ICollection<DbTableChangerParameter>.Contains(DbTableChangerParameter item)
        {
            throw new NotImplementedException();
        }
        void ICollection<DbTableChangerParameter>.CopyTo(DbTableChangerParameter[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        bool ICollection<DbTableChangerParameter>.Remove(DbTableChangerParameter item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<DbTableChangerParameter> GetEnumerator()
        {
            return m_parameters.Values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
