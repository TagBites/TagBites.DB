using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.DB
{
    public class DbLinkConnectionLostEventArgs : EventArgs
    {
        public bool Reconnect { get; set; }
        public int ReconnectAttempts { get; }

        internal DbLinkConnectionLostEventArgs(int reconnectAttempts)
        {
            Guard.ArgumentNonNegative(reconnectAttempts, "reconnectAttempts");
            ReconnectAttempts = reconnectAttempts;
        }
    }
    public delegate void DbLinkConnectionLostEventHandler(object sender, DbLinkConnectionLostEventArgs e);

    public class DbLinkTransactionContextCloseEventArgs : EventArgs
    {
        public DbLinkTransactionCloseReason CloseReason { get; }
        public DbLinkBag TransactionBag { get; }
        public Exception Exception { get; }
        public bool Started { get; }

        internal DbLinkTransactionContextCloseEventArgs(DbLinkTransactionCloseReason closeReason, DbLinkBag transactionBag, Exception exception, bool started)
        {
            Guard.ArgumentNotNull(transactionBag, nameof(transactionBag));

            CloseReason = closeReason;
            TransactionBag = transactionBag;
            Exception = exception;
            Started = started;
        }
    }
    public delegate void DbLinkTransactionContextCloseEventHandler(object sender, DbLinkTransactionContextCloseEventArgs e);

    public enum DbLinkTransactionCloseReason : byte
    {
        Commit,
        Rollback,
        Exception
    }
    public class DbLinkTransactionCloseEventArgs : EventArgs
    {
        public DbLinkTransactionCloseReason CloseReason { get; }
        public DbLinkBag TransactionBag { get; }
        public Exception Exception { get; }

        internal DbLinkTransactionCloseEventArgs(DbLinkTransactionCloseReason closeReason, DbLinkBag transactionBag, Exception exception)
        {
            Guard.ArgumentNotNull(transactionBag, nameof(transactionBag));

            CloseReason = closeReason;
            TransactionBag = transactionBag;
            Exception = exception;
        }
    }
    public delegate void DbLinkTransactionCloseEventHandler(object sender, DbLinkTransactionCloseEventArgs e);

    public class DbLinkContextEventArgs
    {
        public IDbLinkContext LinkContext { get; }

        internal DbLinkContextEventArgs(IDbLinkContext linkContext)
        {
            LinkContext = linkContext;
        }
    }
    public delegate void DbLinkContextEventHandler(object sender, DbLinkContextEventArgs e);

    public class DbLinkInfoMessageEventArgs : EventArgs
    {
        public string Message { get; }
        public string Source { get; }

        public DbLinkInfoMessageEventArgs(string message, string source)
        {
            Message = message;
            Source = source;
        }
    }
    public delegate void DbLinkInfoMessageEventHandler(object sender, DbLinkInfoMessageEventArgs e);

    public class DbExceptionFormatEventArgs
    {
        private Exception _exception;

        public Exception Exception
        {
            get => _exception;
            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                _exception = value;
            }
        }

        internal DbExceptionFormatEventArgs(Exception exception)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));
            _exception = exception;
        }
    }
    public delegate void DbExceptionFormatEventHandler(object sender, DbExceptionFormatEventArgs e);

    public class DbLinkQueryEventArgs
    {
        private IQuerySource _query;

        public IQuerySource Query
        {
            get => _query;
            set
            {
                Guard.ArgumentNotNull(value, nameof(value));
                _query = value;
            }
        }

        public DbLinkQueryEventArgs(IQuerySource query)
        {
            _query = query;
        }
    }
    public delegate void DbLinkQueryEventHandler(object sender, DbLinkQueryEventArgs e);

    public class DbLinkQueryExecutedEventArgs
    {
        public IQuerySource Query { get; }
        public TimeSpan Duration { get; }
        public Exception Exception { get; }

        public DbLinkQueryExecutedEventArgs(IQuerySource query, TimeSpan duration, Exception exception)
        {
            Query = query;
            Duration = duration;
            Exception = exception;
        }
    }
}
