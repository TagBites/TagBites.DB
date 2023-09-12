using System;
using System.Collections;

namespace TagBites.DB.Postgres;

public interface IPgSqlArray : IEnumerable
{
    Type ElementType { get; }
}
