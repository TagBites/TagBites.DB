using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TagBites.Utils;

namespace TagBites.DB
{
    public enum DbLinkContextSwitchMode
    {
        Activate,
        Suppress
    }

    public class DbLinkContextSwitch : IDisposable
    {
        private readonly object m_synchRoot;
        private readonly DbLinkContextSwitchMode m_mode;
        private DbLinkContext m_context;
        private DbLinkContextKey m_oldContextKey;

        internal System.Transactions.TransactionScope TransactionScope { get; set; }

        public DbLinkContextSwitch(IDbLink link)
            : this((DbLinkContext)link.ConnectionContext, DbLinkContextSwitchMode.Activate)
        { }
        public DbLinkContextSwitch(IDbLink link, DbLinkContextSwitchMode mode)
            : this((DbLinkContext)link.ConnectionContext, mode)
        { }
        internal DbLinkContextSwitch(DbLinkContext context, DbLinkContextSwitchMode mode)
        {
            Guard.ArgumentNotNull(context, nameof(context));

            m_synchRoot = context.SynchRoot;
            m_mode = mode;

            lock (m_synchRoot)
            {
                var oldContextKey = context.Provider.CurrentContextKey;

                if (mode == DbLinkContextSwitchMode.Activate)
                {
                    if (oldContextKey != context.Key)
                    {
                        m_context = context;
                        m_oldContextKey = oldContextKey;

                        context.AttachInternal();
                        context.Provider.CurrentContextKey = context.Key;
                    }
                }
                else
                {
                    if (oldContextKey != null)
                    {
                        m_context = context;
                        m_oldContextKey = oldContextKey;

                        context.Provider.CurrentContextKey = null;
                    }
                }
            }

            return;
            //lock (m_synchRoot)
            //{
            //    var oldKey = GetContextKey(context.Provider, out m_oldAsyncEnabled);

            //    if (mode == DbLinkContextSwitchMode.Activate)
            //    {
            //        if (oldKey != context.Key || m_oldAsyncEnabled != (flowOption == DbLinkAsyncFlowOption.Enabled))
            //        {
            //            m_context = context;
            //            m_oldContext = oldKey;

            //            context.AttachInternal();
            //            SetContextKey(context, flowOption == DbLinkAsyncFlowOption.Enabled);
            //        }
            //    }
            //    else
            //    {
            //        if (oldKey != null)
            //        {
            //            m_context = context;
            //            m_oldContext = oldKey;

            //            ClearContextKey(m_context.Provider);
            //        }
            //    }
            //}
        }
        ~DbLinkContextSwitch()
        {
            Debug.WriteLine(ErrorMessages.UnexpectedFinalizerCalled(nameof(DbLinkContextSwitch)));
            Dispose();
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);

            Exception ex = null;

            lock (m_synchRoot)
            {
                if (TransactionScope != null)
                    try
                    {
                        TransactionScope.Complete();
                        TransactionScope.Dispose();
                    }
                    catch (Exception e) { ex = e; }
                    finally
                    {
                        TransactionScope = null;
                    }

                if (m_context != null)
                {
                    try
                    {
                        m_context.Provider.CurrentContextKey = m_oldContextKey;

                        if (m_mode == DbLinkContextSwitchMode.Activate)
                            m_context.Release();
                    }
                    catch (Exception e)
                    {
                        ex = ex == null
                            ? e
                            : new AggregateException(ex, e);
                    }
                    finally
                    {
                        m_context = null;
                        m_oldContextKey = null;
                    }
                }
            }


            if (ex != null)
                throw new Exception("Exception occurred while disposing.", ex);
        }
    }
}
