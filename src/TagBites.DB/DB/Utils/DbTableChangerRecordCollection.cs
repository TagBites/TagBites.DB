using System;
using System.Collections.Generic;
using TBS.Utils;

namespace TBS.Data.DB.Utils
{
    public class DbTableChangerRecordCollection : Collections.ObjectModel.ListCollection<DbTableChangerRecord>
    {
        private readonly DbTableChanger m_owner;

        internal DbTableChangerRecordCollection(DbTableChanger owner)
        {
            m_owner = owner;
        }


        public DbTableChangerRecord Add()
        {
            var record = new DbTableChangerRecord(m_owner);
            base.Add(record);
            return record;
        }
        public DbTableChangerRecord Add(IDictionary<string, object> recordValues)
        {
            Guard.ArgumentNotNull(recordValues, "recordValues");
            var record = new DbTableChangerRecord(m_owner, recordValues);
            base.Add(record);
            return record;
        }
        public DbTableChangerRecord Add(DbRecordValues recordValues)
        {
            var record = Add((IDictionary<string, object>)recordValues);

            if (recordValues.Id != null)
            {
                if (recordValues.Id is Array != m_owner.IsMultiColumnKey)
                    throw new ArgumentException("");

                if (m_owner.IsMultiColumnKey)
                {
                    var array = (Array)recordValues.Id;
                    if (array.Length != record.Keys.Count)
                        throw new ArgumentException();

                    for (int i = 0; i < array.Length; i++)
                        record.Keys[i] = array.GetValue(i);
                }
                else
                    record.Key = recordValues.Id;
            }

            return record;
        }

        public DbTableChangerRecord GetByKey(object key)
        {
            if (m_owner.IsMultiColumnKey)
                throw new InvalidOperationException("For multi-column key please use 'GetByKey[object[])' method.");

            for (int i = 0; i < Count; i++)
            {
                if (Equals(this[i].Key, key))
                    return this[i];
            }

            return null;
        }
        public DbTableChangerRecord GetByKey(object[] key)
        {
            if (m_owner.KeyColumnNamesCore.Length != key.Length)
                throw new ArgumentException();

            return GetByKeyInternal(key);
        }
        internal DbTableChangerRecord GetByKeyInternal(IList<object> key)
        {
            var count = m_owner.KeyColumnNamesCore.Length;
            for (int i = 0; i < Count; i++)
            {
                var equals = true;
                for (int j = 0; j < count; j++)
                {
                    if (!Equals(this[i].Keys[j], key[j]))
                    {
                        equals = false;
                        break;
                    }
                }
                if (equals)
                    return this[i];
            }

            return null;
        }

        protected override void OnItemInserting(int index, DbTableChangerRecord item)
        {
            if (item.Owner != m_owner)
                throw new ArgumentException();
        }
    }
}
