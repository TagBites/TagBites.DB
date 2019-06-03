using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Transactions;
using TBS.Utils;

namespace TBS.Data.DB
{
    public class DbLinkContext : IDbLinkContext
    {
        private EventHandler m_connectionOpen;
        private EventHandler m_connectionClose;
        private DbLinkConnectionLostEventHandler m_connectionLost;
        private EventHandler m_transactionContextBegin;
        private DbLinkTransactionContextCloseEventHandler m_transactionContextClose;
        private EventHandler m_transactionBeforeBegin;
        private EventHandler m_transactionBegin;
        private EventHandler m_transactionBeforeCommit;
        private DbLinkTransactionCloseEventHandler m_transactionClose;
        private DbLinkInfoMessageEventHandler m_infoMessage;
        private DbExceptionFormatEventHandler m_exceptionFormat;
        private DbLinkQueryEventHandler m_query;
        private EventHandler<DbLinkQueryExecutedEventArgs> m_queryExecuted;

        internal readonly DbLinkContextKey Key = new DbLinkContextKey();
        public object SynchRoot { get; } = new object();
        private DbLinkProvider m_provider;
        private readonly Action<DbConnectionStringBuilder> m_connectionStringAdapter;
        private DbConnection m_connection;
        private DbLinkTransactionContext m_transactionContext;
        private int m_connectionRefferenceCount;
        private bool m_suppressTransactionBegin;
        private bool m_suppressTransactionBeforeCommit;
        private readonly DelayedBatchQueryQueue m_batchQueue;
        private DbLinkBag m_bag;

        public event EventHandler ConnectionOpen
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_connectionOpen = (EventHandler)Delegate.Combine(m_connectionOpen, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_connectionOpen = (EventHandler)Delegate.Remove(m_connectionOpen, value);
                }
            }
        }
        public event EventHandler ConnectionClose
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_connectionClose = (EventHandler)Delegate.Combine(value, m_connectionClose);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_connectionClose = (EventHandler)Delegate.Remove(m_connectionClose, value);
                }
            }
        }
        public event DbLinkConnectionLostEventHandler ConnectionLost
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_connectionLost = (DbLinkConnectionLostEventHandler)Delegate.Combine(value, m_connectionLost);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_connectionLost = (DbLinkConnectionLostEventHandler)Delegate.Remove(m_connectionLost, value);
                }
            }
        }
        public event EventHandler TransactionContextBegin
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionContextBegin = (EventHandler)Delegate.Combine(m_transactionContextBegin, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionContextBegin = (EventHandler)Delegate.Remove(m_transactionContextBegin, value);
                }
            }
        }
        public event DbLinkTransactionContextCloseEventHandler TransactionContextClose
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionContextClose = (DbLinkTransactionContextCloseEventHandler)Delegate.Combine(value, m_transactionContextClose);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionContextClose = (DbLinkTransactionContextCloseEventHandler)Delegate.Remove(m_transactionContextClose, value);
                }
            }
        }
        public event EventHandler TransactionBeforeBegin
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionBeforeBegin = (EventHandler)Delegate.Combine(m_transactionBeforeBegin, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionBeforeBegin = (EventHandler)Delegate.Remove(m_transactionBeforeBegin, value);
                }
            }
        }
        public event EventHandler TransactionBegin
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionBegin = (EventHandler)Delegate.Combine(m_transactionBegin, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionBegin = (EventHandler)Delegate.Remove(m_transactionBegin, value);
                }
            }
        }
        public event EventHandler TransactionBeforeCommit
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionBeforeCommit = (EventHandler)Delegate.Combine(m_transactionBeforeCommit, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionBeforeCommit = (EventHandler)Delegate.Remove(m_transactionBeforeCommit, value);
                }
            }
        }
        public event DbLinkTransactionCloseEventHandler TransactionClose
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionClose = (DbLinkTransactionCloseEventHandler)Delegate.Combine(value, m_transactionClose);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_transactionClose = (DbLinkTransactionCloseEventHandler)Delegate.Remove(m_transactionClose, value);
                }
            }
        }
        public event DbLinkInfoMessageEventHandler InfoMessage
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_infoMessage = (DbLinkInfoMessageEventHandler)Delegate.Combine(m_infoMessage, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_infoMessage = (DbLinkInfoMessageEventHandler)Delegate.Remove(m_infoMessage, value);
                }
            }
        }
        public event DbExceptionFormatEventHandler ExceptionFormat
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_exceptionFormat = (DbExceptionFormatEventHandler)Delegate.Combine(m_exceptionFormat, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_exceptionFormat = (DbExceptionFormatEventHandler)Delegate.Remove(m_exceptionFormat, value);
                }
            }
        }
        public event DbLinkQueryEventHandler Query
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_query = (DbLinkQueryEventHandler)Delegate.Combine(m_query, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_query = (DbLinkQueryEventHandler)Delegate.Remove(m_query, value);
                }
            }
        }
        public event EventHandler<DbLinkQueryExecutedEventArgs> QueryExecuted
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_queryExecuted = (EventHandler<DbLinkQueryExecutedEventArgs>)Delegate.Combine(m_queryExecuted, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    m_queryExecuted = (EventHandler<DbLinkQueryExecutedEventArgs>)Delegate.Remove(m_queryExecuted, value);
                }
            }
        }

        public DbLinkProvider Provider => m_provider;
        IDbLinkProvider IDbLinkContext.Provider => m_provider;
        internal DbLinkTransactionContext TransactionContextInternal => m_transactionContext;
        private DbTransaction TransactionInternal => m_transactionContext?.DbTransactionInternal;
        private DbLinkTransactionStatus TransactionStatusInternal
        {
            get
            {
                if (m_transactionContext == null)
                    return DbLinkTransactionStatus.None;

                return m_transactionContext.TransactionStatusInternal;
            }
        }

        public bool IsDisposed => m_provider == null;
        public bool IsActive => m_connection != null;
        public string Database
        {
            get
            {
                lock (SynchRoot)
                {
                    CheckDispose();

                    var c = m_connection;
                    return c != null ? c.Database : m_provider.Database;
                }
            }
            set
            {
                Guard.ArgumentNotNullOrEmpty(value, nameof(value));

                lock (SynchRoot)
                {
                    CheckDispose();

                    if (Database == value)
                        return;

                    ExecuteInner(() =>
                    {
                        m_connection.ChangeDatabase(value);
                        return 0;
                    });
                }
            }
        }

        IDbLinkContext IDbLink.ConnectionContext => this;
        public IDbLinkTransactionContext TransactionContext
        {
            get
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    return m_transactionContext;
                }
            }
        }
        public DbLinkTransactionStatus TransactionStatus
        {
            get
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    return TransactionStatusInternal;
                }
            }
        }
        public DbLinkBag Bag
        {
            get
            {
                lock (SynchRoot)
                {
                    if (m_bag == null)
                        m_bag = new DbLinkBag(SynchRoot);

                    return m_bag;
                }
            }
        }

        protected internal DbLinkContext(DbLinkProvider provider, Action<DbConnectionStringBuilder> connectionStringAdapter)
        {
            Guard.ArgumentNotNull(provider, "provider");

            m_provider = provider;
            m_connectionStringAdapter = connectionStringAdapter;
            m_batchQueue = new DelayedBatchQueryQueue(this);
        }


        public void Force()
        {
            GetOpenConnection();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public DbConnection GetConnection()
        {
            lock (SynchRoot)
            {
                CheckDispose();
                return m_connection;
            }
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DbConnection GetOpenConnection()
        {
            lock (SynchRoot)
            {
                CheckDispose();
                ExecuteInner(() => 0);
                return m_connection;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IDbLink CreateLink()
        {
            lock (SynchRoot)
            {
                CheckDispose();
                return m_provider.CreateLinkForContextInternal(this);
            }
        }

        public int ExecuteNonQuery(IQuerySource query)
        {
            Guard.ArgumentNotNull(query, "query");

            return ExecuteInner(query, q =>
            {
                using (var command = m_provider.LinkAdapter.CreateCommand(m_connection, TransactionInternal, q))
                    return command.ExecuteNonQuery();
            });
        }

        public QueryResult Execute(IQuerySource query)
        {
            lock (SynchRoot)
            {
                if (!m_batchQueue.IsEmpty && Provider.Configuration.MergeNextQueryWithDelayedBatchQuery)
                {
                    var result = DelayedBatchExecute(query);
                    return result.Result;
                }

                var dt = new DataTable();
                ExecuteOnAdapter(query, adapter => adapter.Fill(dt));
                return QueryResult.Create(dt);
            }
        }
        public object ExecuteScalar(IQuerySource query)
        {
            Guard.ArgumentNotNull(query, "query");

            lock (SynchRoot)
            {
                if (!m_batchQueue.IsEmpty && Provider.Configuration.MergeNextQueryWithDelayedBatchQuery)
                {
                    var result = DelayedBatchExecute(query);
                    return result.Result.ToScalar();
                }

                return ExecuteInner(query, q =>
                {
                    using (var command = m_provider.LinkAdapter.CreateCommand(m_connection, TransactionInternal, q))
                        return command.ExecuteScalar();
                });
            }
        }

        public QueryResult[] BatchExecute(IQuerySource query)
        {
            lock (SynchRoot)
            {
                if (!m_batchQueue.IsEmpty && Provider.Configuration.MergeNextQueryWithDelayedBatchQuery)
                {
                    var result = DelayedBatchExecute(query);
                    return result.Results.ToArray();
                }

                return BatchExecuteInternal(query);
            }
        }
        internal QueryResult[] BatchExecuteInternal(IQuerySource query)
        {
            using (var set = new DataSet())
            {
                ExecuteOnAdapter(query, adapter => adapter.Fill(set));

                var results = new QueryResult[set.Tables.Count];

                for (int i = 0; i < set.Tables.Count; i++)
                    results[i] = QueryResult.Create(set.Tables[i]);

                return results;
            }
        }
        public DelayedBatchQueryResult DelayedBatchExecute(IQuerySource query)
        {
            lock (SynchRoot)
            {
                CheckDispose();

                var status = TransactionStatus;
                if (status == DbLinkTransactionStatus.Committing || status == DbLinkTransactionStatus.RollingBack)
                    throw new Exception("Can not execute query while committing or rolling back.");
                if (status == DbLinkTransactionStatus.Pending)
                    Force();

                return m_batchQueue.Add(query);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public T ExecuteOnAdapter<T>(IQuerySource query, Func<DbDataAdapter, T> executor)
        {
            Guard.ArgumentNotNull(query, "query");

            return ExecuteInner(query, q =>
            {
                using (var command = m_provider.LinkAdapter.CreateCommand(m_connection, TransactionInternal, q))
                using (var adapter = m_provider.LinkAdapter.CreateDataAdapter(command))
                    return executor(adapter);
            });
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public T ExecuteOnReader<T>(IQuerySource query, Func<DbDataReader, T> executor)
        {
            Guard.ArgumentNotNull(query, "query");

            return ExecuteInner(query, q =>
            {
                using (var command = m_provider.LinkAdapter.CreateCommand(m_connection, TransactionInternal, q))
                using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                    return executor(reader);
            });
        }

        internal void OnQuery(string query)
        {
            if (m_query != null && !string.IsNullOrWhiteSpace(query))
            {
                var e = new DbLinkQueryEventArgs(new Query(query));
                m_query(this, e);
            }
        }

        internal void AttachInternal()
        {
            ++m_connectionRefferenceCount;
        }
        internal void Release()
        {
            Action connectionCloseEvent = null;
            Exception ex = null;

            lock (SynchRoot)
            {
                CheckDispose();

                if (--m_connectionRefferenceCount != 0)
                    return;

                // Dispose transaction
                if (m_transactionContext != null)
                {
                    if (m_transactionContext.DbTransactionInternal != null)
                    {
                        try { m_transactionContext.DbTransactionInternal.Dispose(); }
                        catch {/* Ignored*/}
                        finally { m_transactionContext.DbTransactionInternal = null; }
                    }

                    m_transactionContext.TransactionStatusInternal = DbLinkTransactionStatus.None;
                    m_transactionContext.Release(true);
                    m_transactionContext = null;

                    ex = new InvalidOperationException("Trying to release link before releasing transaction.");

                    m_batchQueue.Cancel();
                }
                else
                {
                    try
                    {
                        m_batchQueue.Flush();
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                }

                // Release Context
                if (m_provider.TryReleaseContext(this))
                {
                    try
                    {
                        if (m_connection != null)
                        {
                            DisposeAndSetNull(ref m_connection);

                            var connectionClose = m_connectionClose;
                            if (connectionClose != null)
                                connectionCloseEvent = () => connectionClose(this, EventArgs.Empty);
                        }
                    }
                    finally
                    {
                        // Finall Dispose
                        m_provider = null;
                    }
                }
            }

            // Connection Close
            if (connectionCloseEvent != null)
            {
                if (ex == null)
                {
                    connectionCloseEvent();
                    return;
                }

                try
                {
                    connectionCloseEvent();
                }
                catch (Exception ex2)
                {
                    throw ToAggregateException("Exception occurred while executing ConnectionClose event.", ex, ex2);
                }
            }

            // Throw last exception
            if (ex != null)
                throw ex;
        }

        public IDbLinkTransaction Begin()
        {
            lock (SynchRoot)
            {
                CheckDispose();
                return BeginCore(Provider.Configuration.ForceOnTransactionBegin, Provider.Configuration.ImplicitCreateTransactionScopeIfNotExists);
            }
        }
        protected internal IDbLinkTransaction Begin(bool force)
        {
            lock (SynchRoot)
            {
                CheckDispose();
                return BeginCore(force, Provider.Configuration.ImplicitCreateTransactionScopeIfNotExists);
            }
        }
        protected internal IDbLinkTransaction Begin(bool force, bool createTransactionScopeIfNotExists)
        {
            lock (SynchRoot)
            {
                CheckDispose();
                return BeginCore(force, createTransactionScopeIfNotExists);
            }
        }
        private IDbLinkTransaction BeginCore(bool force, bool createTransactionScopeIfNotExists)
        {
            var t = CreateTransaction(createTransactionScopeIfNotExists);

            if (force)
            {
                try
                {
                    Force();
                }
                catch
                {
                    try
                    {
                        t.Dispose();
                    }
                    catch { /* ignored */ }

                    throw;
                }
            }

            return t;
        }
        private IDbLinkTransaction CreateTransaction(bool createTransactionScopeIfNotExists)
        {
            if (m_transactionContext == null)
            {
                m_batchQueue.Flush();

                if (m_provider.Configuration.UseSystemTransactions)
                {
                    var transaction = Transaction.Current;

                    if (transaction != null)
                        SystemTransactionEnlist(transaction);
                    else if (createTransactionScopeIfNotExists)
                    {
                        var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted });
                        transaction = Transaction.Current;
                        SystemTransactionEnlist(transaction);

                        return new DbLinkTransactionWithScope(this, transactionScope, transaction);
                    }
                    else
                    {
                        m_transactionContext = new DbLinkTransactionContext(this, DbLinkTransactionStatus.Pending, false);
                        m_transactionContextBegin?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    m_transactionContext = new DbLinkTransactionContext(this, DbLinkTransactionStatus.Pending, false);
                    m_transactionContextBegin?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (m_transactionContext.TransactionStatusInternal == DbLinkTransactionStatus.RollingBack)
                ThrowRollingBack();
            else if (m_transactionContext.TransactionStatusInternal == DbLinkTransactionStatus.Committing)
                ThrowCommitting();

            return new DbLinkTransaction(this);
        }
        internal void SystemTransactionEnlist(Transaction transaction, bool ignoreIfHasTransactionContext = false)
        {
            lock (SynchRoot)
            {
                CheckDispose();

                if (m_transactionContext != null)
                    if (ignoreIfHasTransactionContext)
                        return;
                    else
                        throw new InvalidOperationException("Can not mix system transactions and db transactions.");

                if (transaction.TransactionInformation.Status == System.Transactions.TransactionStatus.Aborted)
                    throw new InvalidOperationException("Can not perform this operation because transaction is in aborted state.");

                if (!m_provider.Configuration.UseSystemTransactions)
                    throw new Exception("Try to use system transaction while Configuration.UseSystemTransactions=false.");

                m_batchQueue.Flush();

                AttachInternal(); // Transaction could be outside of DbLinkContext. Release in SystemTransaction_Completed.

                m_transactionContext = new DbLinkTransactionContext(this, DbLinkTransactionStatus.Pending, true);
                m_transactionContext.SystemTransactionInternal = transaction;
                m_transactionContext.Attach();

                transaction.TransactionCompleted += SystemTransactionCompleted;
                m_transactionContextBegin?.Invoke(this, EventArgs.Empty);
            }
        }
        private void SystemTransactionCompleted(object sender, TransactionEventArgs e)
        {
            lock (SynchRoot)
            {
                try
                {
                    try
                    {
                        var rollback = e.Transaction.TransactionInformation.Status != System.Transactions.TransactionStatus.Committed;
                        MarkTransaction(rollback, rollback);
                    }
                    finally
                    {
                        CloseTransaction(0);
                    }
                }
                finally
                {
                    Release(); // Attach in EnlistTransaction.
                }
            }
        }
        internal void MarkTransaction(bool rollback, bool rollbackCalled = false)
        {
            lock (SynchRoot)
            {
                CheckDispose();

                if (TransactionStatusInternal == DbLinkTransactionStatus.None)
                    throw new InvalidOperationException("There is no transaction!");

                if (m_transactionContext.TransactionStatusInternal == DbLinkTransactionStatus.RollingBack && !rollback)
                    throw new InvalidOperationException("Can not commit already rollback transaction.");

                // Commit
                if (!rollback)
                {
                    if (m_transactionContext.TransactionRefferenceCountInternal == 1)
                    {
                        m_batchQueue.Flush();
                        m_transactionContext.TransactionStatusInternal = DbLinkTransactionStatus.Committing;
                    }
                }
                // Rollback
                else
                {
                    if (m_transactionContext.TransactionStatusInternal != DbLinkTransactionStatus.RollingBack)
                    {
                        m_transactionContext.TransactionStatusInternal = DbLinkTransactionStatus.RollingBack;

                        // System Transaction
                        if (m_transactionContext.SystemTransactionInternal != null)
                        {
                            var state = m_transactionContext.SystemTransactionInternal.TransactionInformation.Status;
                            if (!rollbackCalled || (state != System.Transactions.TransactionStatus.Aborted && state != System.Transactions.TransactionStatus.InDoubt))
                                m_transactionContext.SystemTransactionInternal.Rollback();
                        }
                        // Db Transaction
                        else if (m_transactionContext.DbTransactionInternal != null)
                        {
                            if (!rollbackCalled)
                                m_transactionContext.DbTransactionInternal.Rollback();
                        }
                    }
                }
            }
        }
        internal void CloseTransaction(int level)
        {
            Exception ex = null;
            Action closeTransactionEvent = null;
            Action closeTransactionContextEvent = null;

            lock (SynchRoot)
            {
                if (TransactionStatusInternal == DbLinkTransactionStatus.None)
                    ex = new InvalidOperationException("There is no transaction.");
                else if (level != 0)
                {
                    var expectedLevel = TransactionStatusInternal == DbLinkTransactionStatus.RollingBack && m_transactionContext.SystemTransaction
                        ? TransactionContextInternal.NestingLevel + 1
                        : TransactionContextInternal.NestingLevel;

                    if (expectedLevel != level)
                        ex = new InvalidOperationException("Transaction nested incorrectly.");
                }

                var transactionClose = m_transactionClose;
                if (m_transactionContext.Release())
                {
                    // System Transaction
                    if (m_transactionContext.SystemTransactionInternal != null)
                    {
                        m_transactionContext.SystemTransactionInternal = null;
                    }
                    // Db Transaction
                    else if (m_transactionContext.DbTransactionInternal != null)
                    {
                        // Commit
                        try
                        {
                            if (m_transactionContext.TransactionStatusInternal == DbLinkTransactionStatus.Committing)
                            {
                                OnTransactionBeforeCommit();

                                m_suppressTransactionBeforeCommit = true;
                                try
                                {
                                    m_transactionContext.DbTransactionInternal.Commit();
                                }
                                finally { m_suppressTransactionBeforeCommit = false; }
                            }
                        }
                        catch (Exception ex2)
                        {
                            // Format Exception
                            ex2 = OnFormatException(ex2);

                            ex = ToAggregateException("Exception occurred while commiting transaction.", ex, ex2);
                        }

                        // Dispose
                        try { m_transactionContext.DbTransactionInternal.Dispose(); }
                        catch { /*Ignored*/ }
                        finally { m_transactionContext.DbTransactionInternal = null; }
                    }

                    // Close Event
                    var reason = ex != null
                        ? DbLinkTransactionCloseReason.Exception
                        : (m_transactionContext.TransactionStatusInternal == DbLinkTransactionStatus.RollingBack ? DbLinkTransactionCloseReason.Rollback : DbLinkTransactionCloseReason.Commit);
                    var bag = m_transactionContext.Bag;

                    if (m_transactionContext.Started && transactionClose != null)
                    {
                        var cea = new DbLinkTransactionCloseEventArgs(reason, bag, ex);
                        closeTransactionEvent = () => transactionClose(this, cea);
                    }
                    if (m_transactionContextClose != null)
                    {
                        var cea = new DbLinkTransactionContextCloseEventArgs(reason, bag, ex, m_transactionContext.Started);
                        closeTransactionContextEvent = () => m_transactionContextClose(this, cea);
                    }

                    // Cancel batch execution
                    if (m_transactionContext.TransactionStatusInternal != DbLinkTransactionStatus.Committing)
                        m_batchQueue.Cancel();

                    // Clear Transaction
                    m_transactionContext.TransactionStatusInternal = DbLinkTransactionStatus.None;
                    m_transactionContext.Release(true);
                    m_transactionContext = null;

                    // Transaction Closed
                    try
                    {
                        if (closeTransactionEvent != null)
                            closeTransactionEvent();
                    }
                    catch (Exception ex2)
                    {
                        ex = ToAggregateException("Exception occurred while executing TransactionClose event.", ex, ex2);
                    }

                    // Transaction Context Closed
                    try
                    {
                        if (closeTransactionContextEvent != null)
                            closeTransactionContextEvent();
                    }
                    catch (Exception ex2)
                    {
                        ex = ToAggregateException("Exception occurred while executing TransactionContextClose event.", ex, ex2);
                    }
                }

                // Throw exception
                if (ex != null)
                    throw ex;
            }
        }

        protected T ExecuteInner<T>(IQuerySource source, Func<IQuerySource, T> action)
        {
            return ExecuteInner(() =>
            {
                if (m_query != null)
                {
                    var e = new DbLinkQueryEventArgs(source);
                    m_query(this, e);
                    source = e.Query;
                }

                if (m_queryExecuted == null)
                    return action(source);

                var time = DateTime.UtcNow;
                Exception ex = null;

                try
                {
                    return action(source);
                }
                catch (Exception e)
                {
                    ex = e;
                    throw;
                }
                finally
                {
                    m_queryExecuted(this, new DbLinkQueryExecutedEventArgs(source, DateTime.UtcNow - time, ex));
                }
            });
        }
        protected T ExecuteInner<T>(Func<T> action)
        {
            var isUserException = false;

            lock (SynchRoot)
            {
                CheckDispose();

                if (TransactionStatusInternal == DbLinkTransactionStatus.Committing)
                    ThrowCommitting();
                else if (TransactionStatusInternal == DbLinkTransactionStatus.RollingBack)
                    ThrowRollingBack();

                var reconnectAttempts = 0;
                do
                {
                    try
                    {
                        // Check connection
                        if (m_connection == null)
                        {
                            if (m_transactionContext != null && m_transactionContext.Started)
                                throw new Exception("Connection was lost after starting a transaction.");

                            try
                            {
                                // Connection String Adapter
                                var cs = m_provider.ConnectionString;

                                if (m_connectionStringAdapter != null)
                                {
                                    var csb = m_provider.LinkAdapter.CreateConnectionStringBuilder(cs);
                                    m_connectionStringAdapter(csb);
                                    cs = csb.ToString();
                                }

                                // Open Connection
                                m_connection = m_provider.LinkAdapter.CreateConnection(cs);
                            }
                            catch
                            {
                                DisposeAndSetNull(ref m_connection);
                                throw;
                            }
                        }

                        // Open Connection
                        if (m_connection.State != ConnectionState.Open)
                        {
                            if (m_transactionContext != null && m_transactionContext.Started)
                                throw new Exception("Connection was lost after starting a transaction.");

                            try
                            {
                                m_connection.Open();

                                if (m_connectionOpen != null)
                                {
                                    isUserException = true;
                                    m_suppressTransactionBegin = Provider.Configuration.PostponeTransactionBeginOnConnectionOpenEvent;
                                    m_connectionOpen(this, EventArgs.Empty);
                                    isUserException = false;
                                }
                            }
                            catch
                            {
                                DisposeAndSetNull(ref m_connection);
                                throw;
                            }
                            finally
                            {
                                m_suppressTransactionBegin = false;
                            }
                        }

                        // Transaction
                        if (!m_suppressTransactionBegin)
                        {
                            // Enlist transaction
                            if (m_transactionContext == null && m_provider.Configuration.UseSystemTransactions)
                            {
                                var transaction = System.Transactions.Transaction.Current;
                                if (transaction != null)
                                    SystemTransactionEnlist(transaction);
                            }

                            // Start transaction
                            if (m_transactionContext != null && m_transactionContext.TransactionStatusInternal == DbLinkTransactionStatus.Pending)
                            {
                                // Before Begin
                                if (m_transactionBeforeBegin != null)
                                {
                                    isUserException = true;
                                    m_transactionBeforeBegin(this, EventArgs.Empty);
                                    isUserException = false;
                                }

                                // Begin
                                if (m_transactionContext.SystemTransactionInternal != null)
                                    m_connection.EnlistTransaction(m_transactionContext.SystemTransactionInternal);
                                else
                                    m_transactionContext.DbTransactionInternal = m_connection.BeginTransaction();

                                m_transactionContext.Started = true;
                                m_transactionContext.TransactionStatusInternal = DbLinkTransactionStatus.Open;

                                // After Begin
                                if (m_transactionBegin != null)
                                {
                                    isUserException = true;
                                    m_transactionBegin(this, EventArgs.Empty);
                                    isUserException = false;
                                }
                            }
                        }

                        // Execute delayed batch
                        m_batchQueue.Flush();

                        // Execute action
                        return action();
                    }
                    catch (Exception e)
                    {
                        var connectionLost = !isUserException
                           && (m_connection == null
                               || m_connection.State == ConnectionState.Closed
                               || IsConnectionBrokenException(e));

                        // Format Exception
                        Exception ex = OnFormatException(e);

                        // Clear current transaction
                        if (TransactionStatusInternal != DbLinkTransactionStatus.None)
                        {
                            if (m_transactionContext.Exception == null)
                                m_transactionContext.Exception = ex;

                            try
                            {
                                MarkTransaction(true, true);
                            }
                            catch (Exception ex2)
                            {
                                throw ToAggregateException("Exception occurred while executing transaction rollback after another exception.", ex, ex2);
                            }

                            if (connectionLost)
                                DisposeAndSetNull(ref m_connection);

                            if (e == ex)
                                throw;
                            throw ex;
                        }

                        // Is Connection Lost
                        if (!connectionLost)
                        {
                            if (e == ex)
                                throw;
                            throw ex;
                        }

                        DisposeAndSetNull(ref m_connection);

                        // Connection Reconnect
                        if (m_connectionLost == null)
                        {
                            if (e == ex)
                                throw;
                            throw ex;
                        }

                        var ea = new DbLinkConnectionLostEventArgs(reconnectAttempts);
                        m_connectionLost(this, ea);

                        if (!ea.Reconnect)
                        {
                            if (e == ex)
                                throw;
                            throw ex;
                        }

                        ++reconnectAttempts;
                    }
                }
                while (true);
            }
        }
        protected virtual bool IsConnectionBrokenException(Exception e)
        {
            for (var ex = e; ex != null; ex = ex.InnerException)
            {
                var exception = ex as SocketException;
                if (exception != null && exception.ErrorCode == 10054)
                    return true;
            }

            return e is IOException;
        }

        protected void OnInfoMessage(string message, string source)
        {
            if (m_infoMessage != null)
                m_infoMessage(this, new DbLinkInfoMessageEventArgs(message, source));
        }
        protected virtual Exception OnFormatException(Exception e)
        {
            if (m_exceptionFormat != null && e is DbException)
            {
                var args = new DbExceptionFormatEventArgs(e);
                try
                {
                    m_exceptionFormat(this, args);
                    e = args.Exception;
                }
                catch (Exception e2)
                {
                    e = new AggregateException(e, e2);
                }
            }

            return e;
        }
        protected virtual void OnTransactionBeforeCommit()
        {
            if (!m_suppressTransactionBeforeCommit)
            {
                m_transactionBeforeCommit?.Invoke(this, EventArgs.Empty);
                m_batchQueue.Flush();
            }
        }

        protected void CheckDispose()
        {
            if (m_provider == null)
                throw new ObjectDisposedException("DbLinkContext");
        }
        private static void ThrowRollingBack()
        {
            throw new InvalidOperationException("Can not execute command because transaction is in progress of rollback.");
        }
        private static void ThrowCommitting()
        {
            throw new InvalidOperationException("Can not execute command because transaction is in progress of commit.");
        }
        private static void DisposeAndSetNull<T>(ref T disposable) where T : class, IDisposable
        {
            if (disposable != null)
                try { disposable.Dispose(); }
                catch { /* Ignored */ }

            disposable = null;
        }

        private static Exception ToAggregateException(string message, Exception oldException, Exception newException)
        {
            return oldException != null
                ? new AggregateException(message, oldException, newException)
                : new Exception(message, newException);
        }

        void IDisposable.Dispose() => throw new NotSupportedException("Can not call dispose!");
    }
}
