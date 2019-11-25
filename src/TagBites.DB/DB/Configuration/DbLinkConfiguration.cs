namespace TagBites.DB.Configuration
{
    public class DbLinkConfiguration
    {
        public static DbLinkConfiguration Default { get; set; } = new DbLinkConfiguration();

        public bool PostponeTransactionBeginOnConnectionOpenEvent { get; set; } = true;
        public bool ForceOnLinkCreate { get; set; }
        public bool ForceOnTransactionBegin { get; set; }
        public bool ImplicitCreateTransactionScopeIfNotExists { get; set; }
        public bool UseSystemTransactions { get; set; }
        public DbLinkCreateOnDifferentSystemTransaction LinkCreateOnDifferentSystemTransaction { get; set; }
        public int DefaultWindowSize { get; set; } = 70;

        public bool MergeNextQueryWithDelayedBatchQuery { get; set; } = false;

        public DbLinkConfiguration Clone()
        {
            return (DbLinkConfiguration)MemberwiseClone();
        }
    }
}
