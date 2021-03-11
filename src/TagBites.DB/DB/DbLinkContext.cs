using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Transactions;
using TagBites.Utils;

namespace TagBites.DB
{
    public class DbLinkContext : IDbLinkContext
    {
        internal readonly DbLinkContextKey Key = new DbLinkContextKey();
        public object SynchRoot { get; } = new object();
        private DbLinkProvider _provider;
        private DbConnection _connection;
        private DbLinkTransactionContext _transactionContext;
        private int _connectionReferenceCount;
        private bool _suppressTransactionBegin;
        private readonly DelayedBatchQueryQueue _batchQueue;
        private DbLinkBag _bag;

        private EventHandler _connectionOpened;
        private EventHandler _connectionClosed;
        private EventHandler<DbLinkConnectionLostEventArgs> _connectionLost;
        private EventHandler _transactionContextBegan;
        private EventHandler _transactionBeginning;
        private EventHandler _transactionBegan;
        private EventHandler _transactionCommiting;
        private EventHandler<DbLinkTransactionCloseEventArgs> _transactionClosed;
        private EventHandler<DbLinkTransactionContextCloseEventArgs> _transactionContextClosed;
        private EventHandler<DbLinkInfoMessageEventArgs> _infoMessageReceived;
        private EventHandler<DbExceptionFormatEventArgs> _exceptionFormatting;
        private EventHandler<DbLinkQueryExecutingEventArgs> _queryExecuting;
        private EventHandler<DbLinkQueryExecutedEventArgs> _queryExecuted;

        public event EventHandler ConnectionOpened
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _connectionOpened = (EventHandler)Delegate.Combine(_connectionOpened, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _connectionOpened = (EventHandler)Delegate.Remove(_connectionOpened, value);
                }
            }
        }
        public event EventHandler ConnectionClosed
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _connectionClosed = (EventHandler)Delegate.Combine(value, _connectionClosed);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _connectionClosed = (EventHandler)Delegate.Remove(_connectionClosed, value);
                }
            }
        }
        public event EventHandler<DbLinkConnectionLostEventArgs> ConnectionLost
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _connectionLost = (EventHandler<DbLinkConnectionLostEventArgs>)Delegate.Combine(value, _connectionLost);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _connectionLost = (EventHandler<DbLinkConnectionLostEventArgs>)Delegate.Remove(_connectionLost, value);
                }
            }
        }
        public event EventHandler TransactionContextBegan
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionContextBegan = (EventHandler)Delegate.Combine(_transactionContextBegan, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionContextBegan = (EventHandler)Delegate.Remove(_transactionContextBegan, value);
                }
            }
        }
        public event EventHandler TransactionBeginning
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionBeginning = (EventHandler)Delegate.Combine(_transactionBeginning, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionBeginning = (EventHandler)Delegate.Remove(_transactionBeginning, value);
                }
            }
        }
        public event EventHandler TransactionBegan
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionBegan = (EventHandler)Delegate.Combine(_transactionBegan, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionBegan = (EventHandler)Delegate.Remove(_transactionBegan, value);
                }
            }
        }
        public event EventHandler TransactionCommiting
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionCommiting = (EventHandler)Delegate.Combine(_transactionCommiting, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionCommiting = (EventHandler)Delegate.Remove(_transactionCommiting, value);
                }
            }
        }
        public event EventHandler<DbLinkTransactionCloseEventArgs> TransactionClosed
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionClosed = (EventHandler<DbLinkTransactionCloseEventArgs>)Delegate.Combine(value, _transactionClosed);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionClosed = (EventHandler<DbLinkTransactionCloseEventArgs>)Delegate.Remove(_transactionClosed, value);
                }
            }
        }
        public event EventHandler<DbLinkTransactionContextCloseEventArgs> TransactionContextClosed
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionContextClosed = (EventHandler<DbLinkTransactionContextCloseEventArgs>)Delegate.Combine(value, _transactionContextClosed);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _transactionContextClosed = (EventHandler<DbLinkTransactionContextCloseEventArgs>)Delegate.Remove(_transactionContextClosed, value);
                }
            }
        }
        public event EventHandler<DbLinkInfoMessageEventArgs> InfoMessageReceived
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _infoMessageReceived = (EventHandler<DbLinkInfoMessageEventArgs>)Delegate.Combine(_infoMessageReceived, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _infoMessageReceived = (EventHandler<DbLinkInfoMessageEventArgs>)Delegate.Remove(_infoMessageReceived, value);
                }
            }
        }
        public event EventHandler<DbExceptionFormatEventArgs> ExceptionFormatting
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _exceptionFormatting = (EventHandler<DbExceptionFormatEventArgs>)Delegate.Combine(_exceptionFormatting, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _exceptionFormatting = (EventHandler<DbExceptionFormatEventArgs>)Delegate.Remove(_exceptionFormatting, value);
                }
            }
        }
        public event EventHandler<DbLinkQueryExecutingEventArgs> QueryExecuting
        {
            add
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _queryExecuting = (EventHandler<DbLinkQueryExecutingEventArgs>)Delegate.Combine(_queryExecuting, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _queryExecuting = (EventHandler<DbLinkQueryExecutingEventArgs>)Delegate.Remove(_queryExecuting, value);
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
                    _queryExecuted = (EventHandler<DbLinkQueryExecutedEventArgs>)Delegate.Combine(_queryExecuted, value);
                }
            }
            remove
            {
                lock (SynchRoot)
                {
                    CheckDispose();
                    _queryExecuted = (EventHandler<DbLinkQueryExecutedEventArgs>)Delegate.Remove(_queryExecuted, value);
                }
            }
        }

        public DbLinkProvider Provider
        {
            get => _provider;
            internal set
            {
                if (_provider != null)
                    throw new InvalidOperationException();

                _provider = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        IDbLinkProvider IDbLinkContext.Provider => _provider;

        internal DbLinkTransactionContext TransactionContextInternal => _transactionContext;
        private DbTransaction TransactionInternal => _transactionContext?.DbTransactionInternal;
        private DbLinkTransactionStatus TransactionStatusInternal
        {
            get
            {
                if (_transactionContext == null)
                    return DbLinkTransactionStatus.None;

                return _transactionContext.Status;
            }
        }

        internal Action<DbConnectionArguments> ConnectionStringAdapter { get; set; }

        public bool IsDisposed => _provider == null;
        public bool IsActive => _connection != null;
        public bool IsExecuting { get; private set; }
        public DateTime LastExecuted { get; private set; }

        public string Database
        {
            get
            {
                lock (SynchRoot)
                {
                    CheckDispose();

                    var c = _connection;
                    return c != null ? c.Database : _provider.Database;
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
                        _connection.ChangeDatabase(value);
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
                    return _transactionContext;
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
                    if (_bag == null)
                        _bag = new DbLinkBag(SynchRoot);

                    return _bag;
                }
            }
        }

        protected internal DbLinkContext()
        {
            _batchQueue = new DelayedBatchQueryQueue(this);
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
                return _connection;
            }
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public DbConnection GetOpenConnection()
        {
            lock (SynchRoot)
            {
                CheckDispose();
                ExecuteInner(() => 0);
                return _connection;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IDbLink CreateLink()
        {
            lock (SynchRoot)
            {
                CheckDispose();
                return _provider.CreateLinkForContextInternal(this);
            }
        }

        public int ExecuteNonQuery(IQuerySource query)
        {
            Guard.ArgumentNotNull(query, "query");

            return ExecuteInner(query, q =>
            {
                using (var command = _provider.LinkAdapter.CreateCommand(_connection, TransactionInternal, q))
                    return command.ExecuteNonQuery();
            });
        }

        public QueryResult Execute(IQuerySource query)
        {
            lock (SynchRoot)
            {
                if (!_batchQueue.IsEmpty && Provider.Configuration.MergeNextQueryWithDelayedBatchQuery)
                {
                    var result = DelayedBatchExecute(query);
                    return result.Result;
                }

                return ExecuteInner(query, (IQuerySource q, out int rowCount) =>
                {
                    using (var command = _provider.LinkAdapter.CreateCommand(_connection, TransactionInternal, q))
                    using (var adapter = _provider.LinkAdapter.CreateDataAdapter(command))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        rowCount = dt.Rows.Count;

                        return QueryResult.Create(dt);
                    }
                });
            }
        }
        public object ExecuteScalar(IQuerySource query)
        {
            Guard.ArgumentNotNull(query, "query");

            lock (SynchRoot)
            {
                if (!_batchQueue.IsEmpty && Provider.Configuration.MergeNextQueryWithDelayedBatchQuery)
                {
                    var result = DelayedBatchExecute(query);
                    return result.Result.ToScalar();
                }

                return ExecuteInner(query, q =>
                {
                    using (var command = _provider.LinkAdapter.CreateCommand(_connection, TransactionInternal, q))
                        return command.ExecuteScalar();
                });
            }
        }

        public QueryResult[] BatchExecute(IQuerySource query)
        {
            lock (SynchRoot)
            {
                if (!_batchQueue.IsEmpty && Provider.Configuration.MergeNextQueryWithDelayedBatchQuery)
                {
                    var result = DelayedBatchExecute(query);
                    return result.Results.ToArray();
                }

                return BatchExecuteInternal(query);
            }
        }
        internal QueryResult[] BatchExecuteInternal(IQuerySource query)
        {
            return ExecuteInner(query, (IQuerySource q, out int rowCount) =>
            {
                using (var command = _provider.LinkAdapter.CreateCommand(_connection, TransactionInternal, q))
                using (var adapter = _provider.LinkAdapter.CreateDataAdapter(command))
                {
                    using (var set = new DataSet())
                    {
                        adapter.Fill(set);

                        var results = new QueryResult[set.Tables.Count];
                        rowCount = 0;

                        for (int i = 0; i < set.Tables.Count; i++)
                        {
                            results[i] = QueryResult.Create(set.Tables[i]);
                            rowCount += results[i].RowCount;
                        }

                        return results;
                    }
                }
            });
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

                return _batchQueue.Add(query);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public T ExecuteOnAdapter<T>(IQuerySource query, Func<DbDataAdapter, T> executor)
        {
            Guard.ArgumentNotNull(query, "query");

            return ExecuteInner(query, q =>
            {
                using (var command = _provider.LinkAdapter.CreateCommand(_connection, TransactionInternal, q))
                using (var adapter = _provider.LinkAdapter.CreateDataAdapter(command))
                    return executor(adapter);
            });
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public T ExecuteOnReader<T>(IQuerySource query, Func<DbDataReader, T> executor)
        {
            Guard.ArgumentNotNull(query, "query");

            return ExecuteInner(query, q =>
            {
                using (var command = _provider.LinkAdapter.CreateCommand(_connection, TransactionInternal, q))
                using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                    return executor(reader);
            });
        }

        internal void OnQuery(string query)
        {
            if (_queryExecuting != null && !string.IsNullOrWhiteSpace(query))
            {
                var e = new DbLinkQueryExecutingEventArgs(new Query(query));
                _queryExecuting(this, e);
            }
        }

        internal void AttachInternal()
        {
            ++_connectionReferenceCount;
        }
        internal void Release()
        {
            Action connectionCloseEvent = null;
            Exception ex = null;

            lock (SynchRoot)
            {
                CheckDispose();

                if (--_connectionReferenceCount != 0)
                    return;
                _connectionReferenceCount = int.MinValue;

                // Batch execution
                if (_transactionContext == null)
                {
                    try
                    {
                        _batchQueue.Flush();
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                }
                _connectionReferenceCount = 0;

                // Dispose transaction
                if (_transactionContext != null)
                {
                    if (_transactionContext.DbTransactionInternal != null)
                    {
                        try { _transactionContext.DbTransactionInternal.Dispose(); }
                        catch {/* Ignored*/}
                        finally { _transactionContext.DbTransactionInternal = null; }
                    }

                    _transactionContext.Status = DbLinkTransactionStatus.None;
                    _transactionContext.ForceRelease();
                    _transactionContext = null;

                    ex = new InvalidOperationException("Trying to release link before releasing transaction.");

                    _batchQueue.Cancel();
                }

                // Release Context
                if (_provider.TryReleaseContext(this))
                {
                    try
                    {
                        if (_connection != null)
                        {
                            DisposeAndSetNull(ref _connection);

                            var connectionClose = _connectionClosed;
                            if (connectionClose != null)
                                connectionCloseEvent = () => connectionClose(this, EventArgs.Empty);
                        }
                    }
                    finally
                    {
                        // Finall Dispose
                        _provider = null;
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
            if (_transactionContext == null)
            {
                _batchQueue.Flush();

                if (_provider.Configuration.UseSystemTransactions)
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
                        _transactionContext = new DbLinkTransactionContext(this, DbLinkTransactionStatus.Pending, false);
                        _transactionContextBegan?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    _transactionContext = new DbLinkTransactionContext(this, DbLinkTransactionStatus.Pending, false);
                    _transactionContextBegan?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (_transactionContext.Status == DbLinkTransactionStatus.RollingBack)
                ThrowRollingBack();
            else if (_transactionContext.Status == DbLinkTransactionStatus.Committing)
                ThrowCommitting();

            return new DbLinkTransaction(this);
        }
        internal void SystemTransactionEnlist(Transaction transaction, bool ignoreIfHasTransactionContext = false)
        {
            lock (SynchRoot)
            {
                CheckDispose();

                if (_transactionContext != null)
                    if (ignoreIfHasTransactionContext)
                        return;
                    else
                        throw new InvalidOperationException("Can not mix system transactions and db transactions.");

                if (transaction.TransactionInformation.Status == System.Transactions.TransactionStatus.Aborted)
                    throw new InvalidOperationException("Can not perform this operation because transaction is in aborted state.");

                if (!_provider.Configuration.UseSystemTransactions)
                    throw new Exception("Try to use system transaction while Configuration.UseSystemTransactions=false.");

                _batchQueue.Flush();

                AttachInternal(); // Transaction could be outside of DbLinkContext. Release in SystemTransaction_Completed.

                _transactionContext = new DbLinkTransactionContext(this, DbLinkTransactionStatus.Pending, true);
                _transactionContext.SystemTransactionInternal = transaction;
                _transactionContext.Attach();

                transaction.TransactionCompleted += SystemTransactionCompleted;
                transaction.EnlistVolatile(new Enlistment(this), EnlistmentOptions.EnlistDuringPrepareRequired);
                _transactionContextBegan?.Invoke(this, EventArgs.Empty);
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

                        if (rollback)
                            _transactionContext.Status = DbLinkTransactionStatus.RollingBack;

                        // Already called in Enlistment.Commit
                        // MarkTransaction(rollback, rollback);
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

                if (_transactionContext.Status == DbLinkTransactionStatus.RollingBack && !rollback)
                    throw new InvalidOperationException("Can not commit already rollback transaction.");

                // Commit
                if (!rollback)
                {
                    if (_transactionContext.TransactionReferenceCountInternal == 1)
                    {
                        if (_transactionContext.Started)
                            try
                            {
                                OnTransactionBeforeCommit();
                                _batchQueue.Flush();
                            }
                            catch
                            {
                                _transactionContext.Status = DbLinkTransactionStatus.RollingBack;
                                throw;
                            }

                        if (_transactionContext.DbTransactionInternal != null)
                            try
                            {
                                _transactionContext.DbTransactionInternal.Commit();
                            }
                            catch (Exception e)
                            {
                                _transactionContext.Status = DbLinkTransactionStatus.RollingBack;

                                var ex = OnFormatException(e);
                                if (e == ex)
                                    throw;
                                throw ex;
                            }

                        _transactionContext.Status = DbLinkTransactionStatus.Committing;
                    }
                }
                // Rollback
                else
                {
                    if (_transactionContext.Status != DbLinkTransactionStatus.RollingBack)
                    {
                        _transactionContext.Status = DbLinkTransactionStatus.RollingBack;

                        // System Transaction
                        if (_transactionContext.SystemTransactionInternal != null)
                        {
                            var state = _transactionContext.SystemTransactionInternal.TransactionInformation.Status;
                            if (!rollbackCalled || (state != System.Transactions.TransactionStatus.Aborted && state != System.Transactions.TransactionStatus.InDoubt))
                                _transactionContext.SystemTransactionInternal.Rollback();
                        }
                        // Db Transaction
                        else if (_transactionContext.DbTransactionInternal != null)
                        {
                            if (!rollbackCalled)
                                _transactionContext.DbTransactionInternal.Rollback();
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
                    var expectedLevel = TransactionStatusInternal == DbLinkTransactionStatus.RollingBack && _transactionContext.IsSystemTransaction
                        ? TransactionContextInternal.Level + 1
                        : TransactionContextInternal.Level;

                    if (expectedLevel != level)
                        ex = new InvalidOperationException("Transaction nested incorrectly.");
                }

                var transactionClose = _transactionClosed;
                var transactionContextClose = _transactionContextClosed;

                if (_transactionContext.BeginRelease())
                {
                    // System Transaction
                    if (_transactionContext.SystemTransactionInternal != null)
                    {
                        _transactionContext.SystemTransactionInternal = null;
                    }
                    // Db Transaction
                    else if (_transactionContext.DbTransactionInternal != null)
                    {
                        try { _transactionContext.DbTransactionInternal.Dispose(); }
                        catch { /*Ignored*/ }
                        finally { _transactionContext.DbTransactionInternal = null; }
                    }

                    // Close Event
                    var reason = ex != null
                        ? DbLinkTransactionCloseReason.Exception
                        : (_transactionContext.Status == DbLinkTransactionStatus.RollingBack ? DbLinkTransactionCloseReason.Rollback : DbLinkTransactionCloseReason.Commit);
                    var bag = _transactionContext.Bag;

                    if (_transactionContext.Started && transactionClose != null)
                    {
                        var cea = new DbLinkTransactionCloseEventArgs(reason, bag, ex);
                        closeTransactionEvent = () => transactionClose(this, cea);
                    }
                    if (transactionContextClose != null)
                    {
                        var cea = new DbLinkTransactionContextCloseEventArgs(reason, bag, ex, _transactionContext.Started);
                        closeTransactionContextEvent = () => transactionContextClose(this, cea);
                    }

                    // Cancel batch execution
                    if (_transactionContext.Status != DbLinkTransactionStatus.Committing)
                        _batchQueue.Cancel();

                    // Clear Transaction
                    _transactionContext.Status = DbLinkTransactionStatus.None;
                    _transactionContext.ForceRelease();
                    _transactionContext = null;

                    // Transaction Closed
                    try
                    {
                        closeTransactionEvent?.Invoke();
                    }
                    catch (Exception ex2)
                    {
                        ex = ToAggregateException("Exception occurred while executing TransactionClose event.", ex, ex2);
                    }

                    // Transaction Context Closed
                    try
                    {
                        closeTransactionContextEvent?.Invoke();
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
                if (_queryExecuting != null)
                {
                    var e = new DbLinkQueryExecutingEventArgs(source);
                    _queryExecuting(this, e);
                    source = e.Query;
                }

                if (_queryExecuted == null)
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
                    _queryExecuted(this, new DbLinkQueryExecutedEventArgs(source, DateTime.UtcNow - time, ex));
                }
            });
        }
        private T ExecuteInner<T>(IQuerySource source, ExecuteQueryWithRowCountDelegate<T> action)
        {
            return ExecuteInner(() =>
            {
                if (_queryExecuting != null)
                {
                    var e = new DbLinkQueryExecutingEventArgs(source);
                    _queryExecuting(this, e);
                    source = e.Query;
                }

                if (_queryExecuted == null)
                    return action(source, out _);

                var time = DateTime.UtcNow;
                Exception ex = null;
                var rowCount = 0;

                try
                {
                    return action(source, out rowCount);
                }
                catch (Exception e)
                {
                    ex = e;
                    throw;
                }
                finally
                {
                    _queryExecuted(this, new DbLinkQueryExecutedEventArgs(source, DateTime.UtcNow - time, ex, ex != null ? (int?)null : rowCount));
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

                IsExecuting = true;
                try
                {
                    var reconnectAttempts = 0;
                    do
                    {
                        try
                        {
                            // Check connection
                            if (_connection == null)
                            {
                                if (_transactionContext != null && _transactionContext.Started)
                                    throw new Exception("Connection was lost after starting a transaction.");

                                try
                                {
                                    // Connection String Adapter
                                    var cs = _provider.ConnectionString;
                                    var adapter = ConnectionStringAdapter;

                                    if (adapter != null)
                                    {
                                        var arguments = new DbConnectionArguments(cs);
                                        adapter(arguments);

                                        cs = _provider.LinkAdapter.CreateConnectionString(arguments);
                                    }

                                    // Create Connection
                                    _connection = _provider.LinkAdapter.CreateConnection(cs);
                                    OnConnectionCreated();

                                    if (_connection.State != ConnectionState.Closed)
                                        throw new Exception("LinkAdapter.CreateConnection(...) returns unclosed connection.");
                                }
                                catch
                                {
                                    DisposeAndSetNull(ref _connection);
                                    throw;
                                }
                            }

                            // Open Connection
                            if (_connection.State != ConnectionState.Open)
                            {
                                if (_transactionContext != null && _transactionContext.Started)
                                    throw new Exception("Connection was lost after starting a transaction.");

                                try
                                {
                                    _connection.OpenAsync().GetAwaiter().GetResult();
                                    OnConnectionOpen();

                                    if (_connectionOpened != null)
                                    {
                                        isUserException = true;
                                        _suppressTransactionBegin = Provider.Configuration.PostponeTransactionBeginOnConnectionOpenEvent;
                                        _connectionOpened(this, EventArgs.Empty);
                                        isUserException = false;
                                    }
                                }
                                catch
                                {
                                    DisposeAndSetNull(ref _connection);
                                    throw;
                                }
                                finally
                                {
                                    _suppressTransactionBegin = false;
                                }
                            }

                            // Transaction
                            if (!_suppressTransactionBegin)
                            {
                                // Enlist transaction
                                if (_transactionContext == null && _provider.Configuration.UseSystemTransactions)
                                {
                                    var transaction = System.Transactions.Transaction.Current;
                                    if (transaction != null)
                                        SystemTransactionEnlist(transaction);
                                }

                                // Start transaction
                                if (_transactionContext != null && _transactionContext.Status == DbLinkTransactionStatus.Pending)
                                {
                                    // Before Begin
                                    if (_transactionBeginning != null)
                                    {
                                        isUserException = true;
                                        _transactionBeginning(this, EventArgs.Empty);
                                        isUserException = false;
                                    }

                                    // Begin
                                    if (_transactionContext.SystemTransactionInternal != null)
                                        _connection.EnlistTransaction(_transactionContext.SystemTransactionInternal);
                                    else
                                        _transactionContext.DbTransactionInternal = _connection.BeginTransaction();

                                    _transactionContext.Started = true;
                                    _transactionContext.Status = DbLinkTransactionStatus.Open;

                                    // After Begin
                                    if (_transactionBegan != null)
                                    {
                                        isUserException = true;
                                        _transactionBegan(this, EventArgs.Empty);
                                        isUserException = false;
                                    }
                                }
                            }

                            // Execute delayed batch
                            _batchQueue.Flush();

                            // Execute action
                            return action();
                        }
                        catch (Exception e)
                        {
                            var connectionLost = !isUserException
                               && (_connection == null
                                   || _connection.State == ConnectionState.Closed
                                   || (_connection.State & ConnectionState.Broken) == ConnectionState.Broken
                                   || IsConnectionBrokenException(e));

                            // Format Exception
                            Exception ex = OnFormatException(e);

                            // Clear current transaction
                            if (TransactionStatusInternal != DbLinkTransactionStatus.None)
                            {
                                if (_transactionContext.Exception == null)
                                    _transactionContext.Exception = ex;

                                if (connectionLost)
                                    DisposeAndSetNull(ref _connection);

                                try
                                {
                                    MarkTransaction(true, true);
                                }
                                catch (Exception ex2)
                                {
                                    throw ToAggregateException("Exception occurred while executing transaction rollback after another exception.", ex, ex2);
                                }

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

                            DisposeAndSetNull(ref _connection);

                            // Connection Reconnect
                            if (_connectionLost == null)
                            {
                                if (e == ex)
                                    throw;
                                throw ex;
                            }

                            var ea = new DbLinkConnectionLostEventArgs(reconnectAttempts);
                            _connectionLost(this, ea);

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
                finally
                {
                    LastExecuted = DateTime.Now;
                    IsExecuting = false;
                }
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
            if (_infoMessageReceived != null)
                _infoMessageReceived(this, new DbLinkInfoMessageEventArgs(message, source));
        }
        protected virtual Exception OnFormatException(Exception e)
        {
            if (_exceptionFormatting != null && e is DbException)
            {
                var args = new DbExceptionFormatEventArgs(e);
                try
                {
                    _exceptionFormatting(this, args);
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
            _transactionCommiting?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnConnectionCreated() { }
        protected virtual void OnConnectionOpen() { }

        protected void CheckDispose()
        {
            if (_provider == null)
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

        private delegate T ExecuteQueryWithRowCountDelegate<out T>(IQuerySource query, out int rowCount);

        private class Enlistment : IEnlistmentNotification
        {
            private readonly DbLinkContext _context;

            public Enlistment(DbLinkContext context)
            {
                _context = context;
            }


            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.Prepared();
            }
            public void InDoubt(System.Transactions.Enlistment enlistment)
            {
                _context.MarkTransaction(true, true);
                enlistment.Done();
            }
            public void Commit(System.Transactions.Enlistment enlistment)
            {
                _context.MarkTransaction(false);
                enlistment.Done();
            }
            public void Rollback(System.Transactions.Enlistment enlistment)
            {
                _context.MarkTransaction(true, true);
                enlistment.Done();
            }
        }
    }
}
