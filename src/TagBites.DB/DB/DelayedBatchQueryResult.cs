using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.DB
{
    public class DelayedBatchQueryResult
    {
        private IList<QueryResult> m_result;
        private Exception m_exception;
        private bool m_canceled;

        internal DelayedBatchQueryQueue Queue { get; }
        public Query Query { get; }

        public bool HasResult => m_result != null;
        public bool IsCompleted => m_result != null || m_exception != null || m_canceled;
        public bool IsFaulted => m_exception != null;
        public bool IsCanceled => m_canceled;

        public QueryResult Result
        {
            get
            {
                CheckResult();

                if (m_result == null)
                    return null;

                if (m_result.Count == 0)
                    return QueryResult.Empty;

                return m_result[m_result.Count - 1];
            }
        }
        public IList<QueryResult> Results
        {
            get
            {
                CheckResult();
                return m_result;
            }
        }
        public Exception Exception => m_exception;

        internal DelayedBatchQueryResult(DelayedBatchQueryQueue queue, Query query)
        {
            Guard.ArgumentNotNull(queue, nameof(queue));
            Guard.ArgumentNotNull(query, nameof(query));

            Queue = queue;
            Query = query;
        }


        public void Cancel()
        {
            if (!IsCompleted)
                lock (Queue.SynchRoot)
                    if (Queue.Cancel(this))
                        m_canceled = true;
        }

        internal void SetResult(IList<QueryResult> result, Exception exception, bool canceled)
        {
            m_result = result;
            m_exception = exception;
            m_canceled = canceled;
        }

        private void CheckResult()
        {
            if (m_result == null && m_exception == null && !m_canceled)
                lock (Queue.SynchRoot)
                    if (m_result == null && m_exception == null && !m_canceled)
                        Queue.Flush();

            if (m_exception != null)
                throw new Exception("An error occurred while executing batch query.", m_exception);
        }
    }
}
