using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TBS.Utils;

namespace TBS.Data.DB.Utils
{
    public class DbTableChangerRecordCollection : Collection<DbTableChangerRecord>
    {
        private readonly DbTableChanger _owner;

        internal DbTableChangerRecordCollection(DbTableChanger owner)
        {
            _owner = owner;
        }


        public DbTableChangerRecord Add()
        {
            var record = new DbTableChangerRecord(_owner);
            base.Add(record);
            return record;
        }
        public DbTableChangerRecord Add(IDictionary<string, object> recordValues)
        {
            Guard.ArgumentNotNull(recordValues, "recordValues");
            var record = new DbTableChangerRecord(_owner, recordValues);
            base.Add(record);
            return record;
        }
        public DbTableChangerRecord Add(DbRecordValues recordValues)
        {
            var record = Add((IDictionary<string, object>)recordValues);

            if (recordValues.Id != null)
            {
                if (recordValues.Id is Array != _owner.IsMultiColumnKey)
                    throw new ArgumentException("");

                if (_owner.IsMultiColumnKey)
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
            if (_owner.IsMultiColumnKey)
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
            if (_owner.KeyColumnNamesCore.Length != key.Length)
                throw new ArgumentException();

            return GetByKeyInternal(key);
        }
        internal DbTableChangerRecord GetByKeyInternal(IList<object> key)
        {
            var count = _owner.KeyColumnNamesCore.Length;
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

        protected override void SetItem(int index, DbTableChangerRecord item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            base.SetItem(index, item);
        }
        protected override void InsertItem(int index, DbTableChangerRecord item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            base.InsertItem(index, item);
        }
    }
}
