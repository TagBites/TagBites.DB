using System;

namespace TagBites.DB.Configuration
{
    public interface IDbLinkDataConverter
    {
        T ChangeType<T>(object value);
        object ChangeType(object value, Type conversionType);
    }
}
