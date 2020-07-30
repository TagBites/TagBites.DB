using System;
using System.Diagnostics;
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
        private DbLinkProvider m_provider;
        private DbLinkContext m_context;
        private DbLinkContextKey m_oldContextKey;
        private bool m_attached;

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

                        m_attached = true;
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
        internal DbLinkContextSwitch(DbLinkProvider provider)
        {
            Guard.ArgumentNotNull(provider, nameof(provider));

            m_synchRoot = new object();
            m_mode = DbLinkContextSwitchMode.Suppress;

            lock (m_synchRoot)
            {
                var oldContextKey = provider.CurrentContextKey;

                if (oldContextKey != null)
                {
                    m_provider = provider;
                    m_oldContextKey = oldContextKey;

                    provider.CurrentContextKey = null;
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

                        if (m_attached)
                        {
                            m_context.Release();
                            m_attached = false;
                        }
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

                if (m_provider != null)
                {
                    try
                    {
                        m_provider.CurrentContextKey = m_oldContextKey;
                    }
                    catch (Exception e)
                    {
                        ex = ex == null
                            ? e
                            : new AggregateException(ex, e);
                    }
                    finally
                    {
                        m_provider = null;
                        m_oldContextKey = null;
                    }
                }
            }

            if (ex != null)
                throw new Exception("Exception occurred while disposing.", ex);
        }

        public static DbLinkContextSwitch SuppressCurrentConnection(DbLinkProvider provider) => new DbLinkContextSwitch(provider);
    }
}
