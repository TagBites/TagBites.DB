using System;

namespace TagBites.DB.Configuration
{
    public interface IDbLinkDataConverter
    {
        object ToDbType(object value);
        object FromDbType(object value);

        T ChangeType<T>(object value);
        object ChangeType(object value, Type conversionType);
    }
}
