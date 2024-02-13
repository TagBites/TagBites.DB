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
            public object ToDbType(object value)
            {
                return value;
            }
            public object FromDbType(object value)
            {
                return value is DBNull ? null : value;
            }

            public T ChangeType<T>(object value)
            {
                if (DataHelper.IsNull(value))
                    return default;

                return (T)ChangeType(value, typeof(T));
            }
            public object ChangeType(object value, Type conversionType)
            {
                if (DataHelper.IsNull(value))
                    return conversionType.IsValueType ? Activator.CreateInstance(conversionType) : null;

                if (value.GetType() == conversionType)
                    return value;

                // Enum
                if (conversionType.IsEnum && TypeUtils.IsIntegerType(value.GetType()))
                    return Enum.ToObject(conversionType, value);

                // Array
                if (conversionType.IsArray && value.GetType().IsArray)
                {
                    var elementType = conversionType.GetElementType()!;

                    var array = (Array)value;
                    var newArray = Array.CreateInstance(elementType, array.Length);

                    for (var i = 0; i < array.Length; i++)
                    {
                        var item = ChangeType(array.GetValue(i), elementType);
                        newArray.SetValue(item, i);
                    }

                    return newArray;
                }

                conversionType = Nullable.GetUnderlyingType(conversionType) ?? conversionType;
                return Convert.ChangeType(value, conversionType);
            }
        }
    }
}
