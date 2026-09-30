namespace TagBites.DB.Postgres;

/// <summary>
/// Provides the keys of the values that TagBites stores in <see cref="DbLinkContext.Bag"/>.
/// </summary>
public static class PgSqlBagKeys
{
    /// <summary>
    /// The key of the <c>true</c> value that marks the connection of <see cref="PgSqlNotifyListener"/>.
    /// </summary>
    public const string IsNotifyContext = "TagBites.DB.Postgres." + nameof(IsNotifyContext);
}
