using System;
using TagBites.Utils;

namespace TagBites.DB.Configuration
{
    public static class DbLinkDataConverter
    {
        private static IDbLinkDataConverter s_default = new DefaultInstance();

        public static IDbLinkDataConverter Default
        {
            get => s_default;
            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                s_default = value;
            }
        }


        private class DefaultInstance : IDbLinkDataConverter
        {
            public object FromDbType(object value)
            {
                return value is DBNull ? null : value;
            }

            public T ChangeType<T>(object value)
            {
                if (DataHelper.IsNull(value))
                    return default;

                return (T)Convert.ChangeType(value, typeof(T));
            }
            public object ChangeType(object value, Type conversionType)
            {
                if (DataHelper.IsNull(value))
                    return conversionType.IsValueType ? Activator.CreateInstance(conversionType) : null;

                return Convert.ChangeType(value, conversionType);
            }
        }
    }
}
