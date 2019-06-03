#if !NET_45

using System;
using TBS.Utils;

namespace TBS.Data.DB.Entity.Schema
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class KeyAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        private string _typeName;
        private int _order = -1;
        public string Name { get; }

        public int Order
        {
            get => _order;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _order = value;
            }
        }
        public string TypeName
        {
            get => _typeName;
            set
            {
                Guard.ArgumentNotNullOrEmpty(value, "value");
                _typeName = value;
            }
        }
        public ColumnAttribute()
        {
        }
        public ColumnAttribute(string name)
        {
            Guard.ArgumentNotNullOrEmpty(name, "name");
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ComplexTypeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DatabaseGeneratedAttribute : Attribute
    {
        public DatabaseGeneratedOption DatabaseGeneratedOption
        {
            get;
            private set;
        }
        public DatabaseGeneratedAttribute(DatabaseGeneratedOption databaseGeneratedOption)
        {
            if (!Enum.IsDefined(typeof(DatabaseGeneratedOption), databaseGeneratedOption))
            {
                throw new ArgumentOutOfRangeException(nameof(databaseGeneratedOption));
            }
            DatabaseGeneratedOption = databaseGeneratedOption;
        }
    }

    public enum DatabaseGeneratedOption
    {
        None,
        Identity,
        Computed
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ForeignKeyAttribute : Attribute
    {
        public string Name { get; }

        public ForeignKeyAttribute(string name)
        {
            Guard.ArgumentNotNullOrEmpty(name, "name");
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        private string _name;
        private int _order = -1;
        private bool? _isClustered;
        private bool? _isUnique;
        public virtual string Name
        {
            get => _name;
            internal set => _name = value;
        }
        public virtual int Order
        {
            get => _order;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _order = value;
            }
        }
        public virtual bool IsClustered
        {
            get => _isClustered.HasValue && _isClustered.Value;
            set => _isClustered = new bool?(value);
        }
        public virtual bool IsClusteredConfigured => _isClustered.HasValue;
        public virtual bool IsUnique
        {
            get => _isUnique.HasValue && _isUnique.Value;
            set => _isUnique = new bool?(value);
        }
        public virtual bool IsUniqueConfigured => _isUnique.HasValue;
        //public override object TypeId
        //{
        //    get
        //    {
        //        return base.GetHashCode();
        //    }
        //}
        public IndexAttribute()
        {
        }
        public IndexAttribute(string name)
        {
            Guard.ArgumentNotNullOrEmpty(name, "name");
            _name = name;
        }
        public IndexAttribute(string name, int order)
        {
            Guard.ArgumentNotNullOrEmpty(name, "name");
            if (order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(order));
            }
            _name = name;
            _order = order;
        }
        private IndexAttribute(string name, int order, bool? isClustered, bool? isUnique)
        {
            _name = name;
            _order = order;
            _isClustered = isClustered;
            _isUnique = isUnique;
        }
        protected virtual bool Equals(IndexAttribute other)
        {
            return _name == other._name && _order == other._order && _isClustered.Equals(other._isClustered) && _isUnique.Equals(other._isUnique);
        }
        public override string ToString()
        {
            return base.ToString();
        }
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || (!(obj.GetType() != GetType()) && Equals((IndexAttribute)obj)));
        }
        public override int GetHashCode()
        {
            int num = base.GetHashCode();
            num = (num * 397 ^ ((_name != null) ? _name.GetHashCode() : 0));
            num = (num * 397 ^ _order);
            num = (num * 397 ^ _isClustered.GetHashCode());
            return num * 397 ^ _isUnique.GetHashCode();
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class InversePropertyAttribute : Attribute
    {
        public string Property { get; }

        public InversePropertyAttribute(string property)
        {
            Guard.ArgumentNotNullOrEmpty(property, "property");
            Property = property;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class NotMappedAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        private string _schema;
        public string Name { get; }

        public string Schema
        {
            get => _schema;
            set
            {
                Guard.ArgumentNotNullOrEmpty(value, "value");
                _schema = value;
            }
        }
        public TableAttribute(string name)
        {
            Guard.ArgumentNotNullOrEmpty(name, "name");
            Name = name;
        }
    }
}

#endif
