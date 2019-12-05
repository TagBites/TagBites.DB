using System;
using System.Collections.Generic;
using System.Text;
using TagBites.DB;

namespace TagBites.Sql
{
    public class SqlQueryBuilder
    {
        private StringBuilder m_buffor = new StringBuilder();
        private Stack<StringBuilder> m_buffers;
        private readonly List<QueryParameter> m_parameters = new List<QueryParameter>();

        public bool IsForToString { get; private set; }
        public bool ValidationEnabled { get; set; } = true;
        public bool SupportParameters { get; set; } = true;

        public bool IsEmpty => m_buffor.Length == 0;
        public string Query => m_buffor.ToString();
        public IList<QueryParameter> Parameters => m_parameters;


        public void Append(char value)
        {
            m_buffor.Append(value);
        }
        public void Append(string value)
        {
            m_buffor.Append(value);
        }
        public void AppendKeyword(string value)
        {
            if (m_buffor.Length > 0 && m_buffor[m_buffor.Length - 1] != ' ')
                m_buffor.Append(' ');

            if (value.Length > 0)
            {
                m_buffor.Append(value);

                if (m_buffor[m_buffor.Length - 1] != ' ')
                    m_buffor.Append(' ');
            }
        }
        public void AppendParameter(object value)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < m_parameters.Count; i++)
                if (ReferenceEquals(m_parameters[i].Value, value))
                {
                    m_buffor.Append(m_parameters[i].Name);
                    return;
                }

            var name = DB.Query.ParameterPrefix + (m_parameters.Count + 1);
            m_parameters.Add(new QueryParameter(name, value));
            m_buffor.Append(name);
        }

        public void Push()
        {
            if (m_buffers == null)
                m_buffers = new Stack<StringBuilder>();

            m_buffers.Push(m_buffor);
            m_buffor = new StringBuilder();
        }
        public string Pop()
        {
            var value = m_buffor.ToString();
            m_buffor = m_buffers.Pop();
            return value;
        }

        internal void AppendOrThrowNotSupportedByResolver(object sqlElement, SqlQueryResolver sqlResolver)
        {
            var text = $"Element '{sqlElement.GetType().Name} is not supported by {sqlResolver.GetType().Name}.";
            if (IsForToString)
                m_buffor.Append(text);
            else
                throw new NotSupportedException(text);
        }

        internal static SqlQueryBuilder CreateToStringBuilder()
        {
            return new SqlQueryBuilder()
            {
                IsForToString = true,
                ValidationEnabled = false,
                SupportParameters = false
            };
        }
    }
}
