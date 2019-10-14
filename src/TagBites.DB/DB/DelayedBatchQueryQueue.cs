using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TagBites.Collections;

namespace TagBites.DB
{
    internal class DelayedBatchQueryQueue
    {
        private readonly IDbLinkContext m_context;
        private List<DelayedBatchQueryResult> m_queries = new List<DelayedBatchQueryResult>();

        public bool IsEmpty => m_queries.Count == 0;
        public object SynchRoot => m_context.SynchRoot;

        public DelayedBatchQueryQueue(IDbLinkContext context)
        {
            m_context = context;
        }


        public DelayedBatchQueryResult Add(IQuerySource source)
        {
            var item = new DelayedBatchQueryResult(this, m_context.Provider.QueryResolver.GetQuery(source));
            m_queries.Add(item);
            return item;
        }
        public bool Cancel(DelayedBatchQueryResult item)
        {
            return m_queries.Remove(item);
        }
        public void Flush()
        {
            if (m_queries.Count == 0)
                return;

            var items = m_queries;
            m_queries = new List<DelayedBatchQueryResult>();

            // Prepare query
            var queries = new List<Query>(items.Count * 2 + 1);
            var ids = new List<string>();

            if (items.Count > 1 && m_context.TransactionStatus == DbLinkTransactionStatus.None)
                queries.Add(new Query("begin; -- ..."));

            for (var i = 0; i < items.Count; i++)
            {
                var query = items[i];
                if (i > 0)
                {
                    var id = Guid.NewGuid().ToString("N");
                    ids.Add(id);
                    queries.Add(new Query($"SELECT '{id}'"));
                }

                queries.Add(query.Query);
            }

            if (items.Count > 1 && m_context.TransactionStatus == DbLinkTransactionStatus.None)
                queries.Add(new Query("commit;"));

            var batchQuery = Query.Concat(queries);

            // Execute
            QueryResult[] results;
            try
            {
                using (var set = new DataSet())
                {
                    m_context.ExecuteOnAdapter(batchQuery, adapter => adapter.Fill(set));

                    results = new QueryResult[set.Tables.Count];

                    for (int i = 0; i < set.Tables.Count; i++)
                        results[i] = QueryResult.Create(set.Tables[i]);
                }
            }
            catch (Exception e)
            {
                // Set exceptions
                foreach (var item in items)
                    item.SetResult(null, e, false);

                throw;
            }

            // Set results
            var currentItemIndex = 0;
            var currentResultIndex = 0;
            var currentResultCount = 0;

            for (var i = 0; i < results.Length; i++)
            {
                QueryResult result = results[i];
                if (result.RowCount == 1 && result.ColumnCount == 1 && currentItemIndex < ids.Count && Equals(result.GetValue(0, 0), ids[currentItemIndex]))
                {
                    items[currentItemIndex].SetResult(new SpanCollection<QueryResult>(results, currentResultIndex, currentResultCount), null, false);

                    currentResultIndex = i + 1;
                    currentResultCount = 0;
                    ++currentItemIndex;

                    continue;
                }

                ++currentResultCount;
            }

            items[currentItemIndex].SetResult(new SpanCollection<QueryResult>(results, currentResultIndex, currentResultCount), null, false);
        }
        public void Cancel()
        {
            if (m_queries.Count == 0)
                return;

            var items = m_queries;
            m_queries = new List<DelayedBatchQueryResult>();

            foreach (var item in items)
                item.SetResult(null, null, true);
        }
    }
}
