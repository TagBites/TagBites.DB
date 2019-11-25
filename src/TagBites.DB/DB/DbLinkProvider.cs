using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using TagBites.DB.Configuration;
using TagBites.Sql;

namespace TagBites.DB
{
    /// <summary>
    /// Thread safe.
    /// </summary>
    public abstract class DbLinkProvider : IDbLinkProvider
    {
        #region Execution context

        private readonly AsyncLocal<DbLinkContextKey> _currentContextKey = new AsyncLocal<DbLinkContextKey>();

        internal DbLinkContextKey CurrentContextKey
        {
            get => _currentContextKey.Value;
            set => _currentContextKey.Value = value;
        }

        #endregion

        #region Events

        public event DbLinkContextEventHandler ContextCreated;

        #endregion

        #region Members

        internal readonly string Id = Guid.NewGuid().ToString("N");
        private readonly object SynchRootForContextLookup = new object();
        private readonly object SynchRootForContextCreation = new object();
        private readonly object SynchRootForContextCollections = new object();

        #endregion

        #region Pooling Properties

        private readonly Semaphore m_createContextSemaphore;
        private readonly Queue<DbLinkContext> m_poolContexts;
        private readonly List<DbLinkContext> m_activeConnections = new List<DbLinkContext>();

        public bool UsePooling { get; }
        public int MinPoolSize { get; }
        public int MaxPoolSize { get; }

        /// <summary>
        /// Number of all connections.
        /// </summary>
        public int ConnectionsCount
        {
            get
            {
                lock (SynchRootForContextCollections)
                    return m_poolContexts.Count + m_activeConnections.Count;
            }
        }
        /// <summary>
        /// Number of all connections in the pool.
        /// </summary>
        public int PoolConnectionsCount
        {
            get
            {
                lock (SynchRootForContextCollections)
                    return m_poolContexts.Count;
            }
        }
        /// <summary>
        /// Number of using connections. 
        /// </summary>
        public int UsingConnectionsCount
        {
            get
            {
                lock (SynchRootForContextCollections)
                    return m_activeConnections.Count;
            }
        }
        /// <summary>
        /// Number of active connections (including connections from pool).
        /// </summary>
        public int ActiveConnectionsCount
        {
            get
            {
                lock (SynchRootForContextCollections)
                    return m_poolContexts.Count(x => x.IsActive) + m_activeConnections.Count(x => x.IsActive);
            }
        }

        #endregion

        #region Connection Properties, Info

        public DbLinkConfiguration Configuration { get; } = DbLinkConfiguration.Default.Clone();
        public SqlQueryResolver QueryResolver => LinkAdapter.QueryResolver;
        public string Database { get; }
        public string Server { get; }
        public int Port { get; }
        public virtual bool IsCursorSupported => true;
        internal DbLinkAdapter LinkAdapter { get; }
        internal string ConnectionString { get; }

        public IDbLinkContext CurrentConnectionContext => GetCurrentContext();
        public IDbLinkTransactionContext CurrentTransactionContext => CurrentConnectionContext?.TransactionContext;

        #endregion

        #region Constructor

        protected DbLinkProvider(DbLinkAdapter linkAdapter, DbConnectionArguments arguments)
        {
            if (linkAdapter == null)
                throw new ArgumentNullException(nameof(linkAdapter));
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            Database = arguments.Database;
            Server = arguments.Host;
            Port = arguments.Port == 0 ? linkAdapter.DefaultPort : arguments.Port;
            UsePooling = arguments.UsePooling;

            if (UsePooling)
            {
                MinPoolSize = arguments.MinPoolSize;
                MaxPoolSize = arguments.MaxPoolSize;

                m_createContextSemaphore = new Semaphore(MaxPoolSize, MaxPoolSize);
                m_poolContexts = new Queue<DbLinkContext>();
            }

            LinkAdapter = linkAdapter;
            ConnectionString = linkAdapter.CreateConnectionString(arguments);

            StartEvaluation();
        }

        #endregion


        #region Create cursor manager

        protected virtual IDbCursorManager CreateCursorManagerInner() => new DbQueryCursorManager(this);

        IDbCursorManager IDbLinkProvider.CreateCursorManager() { return CreateCursorManagerInner(); }

        #endregion

        #region Create link

        public DbLink CreateLink()
        {
            return CreateLink(DbLinkCreateOption.Required);
        }
        public DbLink CreateLink(DbLinkCreateOption createOption)
        {
            DbLink link = null;
            DbLinkContextSwitch switcher = null;
            DbLinkContext context = null;

            var currentContextKey = CurrentContextKey;

            // Get current context
            if (createOption == DbLinkCreateOption.Required)
            {
                lock (SynchRootForContextLookup)
                {
                    var useSystemTransactions = Configuration.UseSystemTransactions;
                    var transaction = useSystemTransactions ? System.Transactions.Transaction.Current : null;

                    if (transaction != null && Configuration.LinkCreateOnDifferentSystemTransaction == DbLinkCreateOnDifferentSystemTransaction.CreateLinkWithNewContextOrAssigedToCurrentTransaction)
                    {
                        lock (SynchRootForContextCollections)
                            context = m_activeConnections.FirstOrDefault(x => x.TransactionContextInternal?.SystemTransactionInternal == transaction);

                        if (context != null && context.Key != currentContextKey)
                            if (currentContextKey == null)
                                CurrentContextKey = context.Key;
                            else
                                switcher = new DbLinkContextSwitch(context, DbLinkContextSwitchMode.Activate);
                    }
                    else
                    {
                        if (currentContextKey != null)
                            lock (SynchRootForContextCollections)
                                context = m_activeConnections.FirstOrDefault(x => x.Key == currentContextKey);

                        // Use existing context
                        if (useSystemTransactions && context != null)
                        {
                            var linkTransaction = context.TransactionContextInternal?.SystemTransactionInternal;
                            System.Transactions.TransactionScope transactionScope = null;

                            if (linkTransaction != null && transaction != linkTransaction)
                            {
                                if (Configuration.LinkCreateOnDifferentSystemTransaction == DbLinkCreateOnDifferentSystemTransaction.CreateLinkWithNewContextOrAssigedToCurrentTransaction)
                                    context = null;
                                else if (Configuration.LinkCreateOnDifferentSystemTransaction == DbLinkCreateOnDifferentSystemTransaction.TryToMoveTransactionOrThrowException && transaction == null)
                                    transactionScope = new System.Transactions.TransactionScope(linkTransaction.DependentClone(System.Transactions.DependentCloneOption.BlockCommitUntilComplete));
                                else
                                    throw new Exception("This link is already assosiated with different transaction.");
                            }

                            if (context != null && transactionScope != null)
                                switcher = new DbLinkContextSwitch(context, DbLinkContextSwitchMode.Activate) { TransactionScope = transactionScope };
                        }
                    }

                    if (context != null)
                    {
                        link = CreateLinkCore(context);
                        link.Switcher = switcher;
                    }
                }
            }

            // Create new context
            if (context == null)
            {
                Evaluate();

                var isNewContext = true;
                var newContextEvent = ContextCreated;

                try
                {
                    lock (SynchRootForContextCreation)
                    {
                        // Context from pool
                        if (!UsePooling)
                            context = CreateLinkContext();
                        else
                        {
                            m_createContextSemaphore.WaitOne();

                            lock (SynchRootForContextCollections)
                            {
                                if (m_poolContexts.Count > 0)
                                {
                                    context = m_poolContexts.Dequeue();
                                    isNewContext = false;
                                }
                            }

                            if (context == null)
                                context = CreateLinkContext();
                        }

                        // Attach to call context
                        if (currentContextKey != null)
                            switcher = new DbLinkContextSwitch(context, DbLinkContextSwitchMode.Activate);
                        else
                            CurrentContextKey = context.Key;

                        lock (SynchRootForContextCollections)
                            m_activeConnections.Add(context);

                        // Event
                        if (isNewContext && newContextEvent != null)
                        {
                            context.AttachInternal(); // In case of releasing new link in ContextCreated
                            newContextEvent(this, new DbLinkContextEventArgs(context));
                        }

                        link = CreateLinkCore(context);
                        link.Switcher = switcher;
                    }
                }
                finally
                {
                    if (isNewContext && newContextEvent != null)
                        context?.Release();
                }
            }

            PrepareLink(link, true);
            return link;
        }

        public DbLink CreateExclusiveLink()
        {
            return CreateExclusiveLink(null);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DbLink CreateExclusiveLink(Action<DbConnectionArguments> connectionStringAdapter)
        {
            DbLink link;

            lock (SynchRootForContextCreation)
            {
                if (UsePooling)
                    m_createContextSemaphore.WaitOne();

                var context = CreateLinkContext();
                context.ConnectionStringAdapter = connectionStringAdapter;

                lock (SynchRootForContextCollections)
                    m_activeConnections.Add(context);
                ContextCreated?.Invoke(this, new DbLinkContextEventArgs(context));

                link = CreateLinkCore(context);
            }

            PrepareLink(link, true);
            return link;
        }

        internal DbLink CreateLinkForContextInternal(DbLinkContext context)
        {
            var link = CreateLinkCore(context);
            PrepareLink(link, false);
            return link;
        }

        private DbLink CreateLinkCore(DbLinkContext context)
        {
            var link = LinkAdapter.CreateDbLink();
            link.ConnectionContext = context;
            context.AttachInternal();
            return link;
        }
        private void PrepareLink(DbLink link, bool allowTransactionBegin)
        {
            var context = link.ConnectionContext;

            try
            {
                // Force open
                if (Configuration.ForceOnLinkCreate && !context.IsActive)
                    context.Force();

                // Transaction
                if (allowTransactionBegin && Configuration.UseSystemTransactions)
                {
                    var transaction = System.Transactions.Transaction.Current;
                    if (transaction != null)
                        context.SystemTransactionEnlist(transaction, true);

                }
            }
            catch
            {
                link.Dispose();
                throw;
            }
        }

        private DbLinkContext GetCurrentContext()
        {
            var currentContextKey = CurrentContextKey;
            var useSystemTransactions = Configuration.UseSystemTransactions;
            var transaction = useSystemTransactions ? System.Transactions.Transaction.Current : null;

            if (transaction != null && Configuration.LinkCreateOnDifferentSystemTransaction == DbLinkCreateOnDifferentSystemTransaction.CreateLinkWithNewContextOrAssigedToCurrentTransaction)
            {
                lock (SynchRootForContextCollections)
                    return m_activeConnections.FirstOrDefault(x => x.TransactionContextInternal?.SystemTransactionInternal == transaction);
            }

            if (currentContextKey != null)
                lock (SynchRootForContextCollections)
                    return m_activeConnections.FirstOrDefault(x => x.Key == currentContextKey);

            return null;
        }

        IDbLink IDbLinkProvider.CreateLink() { return CreateLink(); }
        IDbLink IDbLinkProvider.CreateLink(DbLinkCreateOption createOption) { return CreateLink(createOption); }
        IDbLink IDbLinkProvider.CreateExclusiveLink() { return CreateExclusiveLink(); }

        #endregion

        #region Create link context

        private DbLinkContext CreateLinkContext()
        {
            var context = LinkAdapter.CreateDbLinkContext();
            context.Provider = this;
            return context;
        }

        #endregion

        #region Release Context Method

        internal bool TryReleaseContext(DbLinkContext context)
        {
            var released = true;
            var pooling = UsePooling;
            var synchRootLocked = false;

            // Deadlock:
            // - 2 wątki mają współdzielić kontekst (pierwszy tworzy, drugi korzysta).
            // - pierwszy blokuje SynchRoot i czeka na m_createContextSemaphore, poneiwaz pula jest wykorzystana
            // - drugi czeka na SynchRoot
            // - trzeci chce zwolnić kontekst, ale nie może bo czeka na SynchRoot
            // Rozwiązanie:
            // - nie blokować SynchRoot jeżeli wątek czeka na m_createContextSemaphore
            // - dodatkowy lock SynchRootForContextLookup, aby co najwyżej jeden wątek wykonywał aktywne czekanie (wpp błąd w m_createContextSemaphore.WaitOne(0))
            lock (SynchRootForContextLookup)
            {
                if (!pooling)
                    Monitor.Enter(SynchRootForContextCreation, ref synchRootLocked);
                else
                {
                    while (true)
                    {
                        synchRootLocked = Monitor.TryEnter(SynchRootForContextCreation);
                        if (synchRootLocked)
                            break;

                        var poolingLocked = m_createContextSemaphore.WaitOne(0);
                        if (poolingLocked)
                            m_createContextSemaphore.Release(1);
                        else
                            break;

                        Thread.Sleep(1);
                    }
                }

                try
                {
                    lock (SynchRootForContextCollections)
                        if (!m_activeConnections.Remove(context))
                            throw new Exception("Unknown context!");

                    // Clear CallContext
                    if (CurrentContextKey == context.Key)
                        CurrentContextKey = null;

                    // Clear Pooling
                    if (pooling)
                    {
                        lock (SynchRootForContextCollections)
                        {
                            if (m_poolContexts.Count < MinPoolSize)
                            {
                                m_poolContexts.Enqueue(context);
                                released = false;
                            }
                        }

                        m_createContextSemaphore.Release(1);
                    }
                }
                finally
                {
                    if (synchRootLocked)
                        Monitor.Exit(SynchRootForContextCreation);
                }
            }

            return released;
        }

        #endregion

        #region License evaluation

        private static long s_nextEvaluationTime = 0;


        private void StartEvaluation()
        {
            if (TagBites.DB.Licensing.LicenseManager.HasLicense)
                return;

            if (s_nextEvaluationTime == 0)
                s_nextEvaluationTime = DateTime.UtcNow.Ticks + TimeSpan.TicksPerMinute * 5;
        }
        private void Evaluate()
        {
            if (s_nextEvaluationTime > 0)
            {
                var now = DateTime.UtcNow.Ticks;
                if (s_nextEvaluationTime < now)
                {
                    s_nextEvaluationTime = now + TimeSpan.TicksPerMinute * 2;
                    throw new Exception("This is evaluation version. Please purchase a TagBites.DB license.");
                }
            }
        }

        #endregion
    }
}
