namespace TagBites.DB;

/// <summary>
/// Provides the keys of the <see cref="DbLinkContext.Bag"/> values that TagBites uses.
/// </summary>
public static class DbLinkBagKeys
{
    /// <summary>
    /// The key of the <c>true</c> value that keeps the pooled connection open after <see cref="DbConnectionArguments.ConnectionIdleLifetime"/> passes.
    /// </summary>
    public const string KeepOpenWhenIdle = "TagBites.DB." + nameof(KeepOpenWhenIdle);
}
