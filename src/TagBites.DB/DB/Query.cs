using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TagBites.DB.Configuration;
using TagBites.Sql;
using TagBites.Utils;

namespace TagBites.DB
{
    public class Query : IQuerySource, IEquatable<Query>
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public const string ParameterPrefix = "@";

        public string Command { get; }

        public IList<QueryParameter> Parameters { get; }

        public Query(string command)
        {
            Guard.ArgumentNotNullOrWhiteSpace(command, "command");

            Command = command;
            Parameters = Array.Empty<QueryParameter>();
        }
        public Query(string commandFormat, params object[] args)
        {
            Guard.ArgumentNotNullOrWhiteSpace(commandFormat, "commandFormat");

            if (args.Length == 0)
            {
                Command = commandFormat;
                Parameters = Array.Empty<QueryParameter>();
            }
            else
            {
                var names = new object[args.Length];
                var pms = new List<QueryParameter>(args.Length);

                for (int i = 0; i < args.Length; i++)
                {
                    var dbValue = DbLinkDataConverter.Default.ToDbType(args[i]);

                    if (dbValue is SqlExpression s)
                    {
                        var builder = SqlQueryBuilder.CreateToStringBuilder();
                        SqlQueryResolver.DefaultToStringResolver.Visit(s, builder);

                        names[i] = builder.Query;
                    }
                    else
                    {
                        var p = new QueryParameter(ParameterPrefix + (pms.Count + 1).ToString(), dbValue);
                        pms.Add(p);

                        names[i] = p.Name;
                    }
                }

                Command = string.Format(commandFormat, names);
                Parameters = new ReadOnlyCollection<QueryParameter>(pms);
            }
        }
        public Query(string command, IEnumerable<QueryParameter> parameters)
        {
            Guard.ArgumentNotNullOrWhiteSpace(command, "command");

            Command = command;

            Parameters = parameters.ToArray();
            if (Parameters.Count > 0)
                Parameters = new ReadOnlyCollection<QueryParameter>(Parameters);
        }


        public string GetUnsafeEscapeString(SqlQueryResolver resolver)
        {
            var stringBuilder = new StringBuilder(Command.Length + Parameters.Count * 32);
            ResolveSqlQueryParameters(stringBuilder, Command, ParameterPrefix[0], name =>
            {
                name = "@" + name;
                var arg = Parameters.FirstOrDefault(x => x.Name == name);
                if (arg == null)
                    throw new Exception(string.Format("Argument \"{0}\" not found!", name));

                return resolver.ToParameterString(arg.Value, true);
            });
            return stringBuilder.ToString();
        }

        public override int GetHashCode()
        {
            return Command.GetHashCode() ^ Parameters.Count;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Query))
                return false;

            return Equals((Query)obj);
        }
        public bool Equals(Query other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(other, null))
                return false;

            if (Parameters.Count != other.Parameters.Count || Command != other.Command)
                return false;

            for (int i = 0; i < Parameters.Count; i++)
                if (!Parameters[i].Equals(other.Parameters[i]))
                    return false;

            return true;
        }
        public override string ToString()
        {
            return Command;
        }

        Query IQuerySource.CreateQuery(SqlQueryResolver resolver, SqlQueryBuilder builder) => this;

        public static Query Format(string commandFormat, params object[] args)
        {
            return new Query(commandFormat, args);
        }
        public static Query FormatUsingModel(string commandFormat, object model)
        {
            Guard.ArgumentNotNullOrWhiteSpace(commandFormat, "command");
            Guard.ArgumentNotNull(model, "model");

            var type = model.GetType();
            var parameters = new List<QueryParameter>();
            var processed = new HashSet<string>();

            for (int i = commandFormat.IndexOf(ParameterPrefix); i != -1 && i + 1 < commandFormat.Length; i = commandFormat.IndexOf(ParameterPrefix, i + 1))
            {
                int end = i + 1;
                if (!(char.IsLetter(commandFormat[end]) || commandFormat[end] == '_'))
                    continue;

                while (end + 1 < commandFormat.Length && char.IsLetterOrDigit(commandFormat[end + 1]))
                    ++end;

                var propertyName = commandFormat.Substring(i + 1, end - i);
                if (processed.Add(propertyName))
                {
                    var property = type.GetProperty(propertyName);
                    if (property == null || property.Name != propertyName)
                        continue;

                    var parameterName = ParameterPrefix + propertyName;//ParameterPrefix + (parameters.Count + 1).ToString();
                    var parameterValue = property.GetValue(model, null);
                    parameters.Add(new QueryParameter(parameterName, parameterValue));

                    //commandFormat = commandFormat.Replace("@" + propertyName, parameterName);
                }
            }

            return new Query(commandFormat, parameters);
        }

        public static Query Concat(params Query[] queries) => Concat((IList<Query>)queries);
        public static Query Concat(IList<Query> queries)
        {
            Guard.ArgumentNotNull(queries, nameof(queries));

            if (queries.Count == 1)
                return queries[0];

            var qLength = 0;
            var qParametersCount = 0;

            foreach (var query in queries)
            {
                if (query == null)
                    throw new ArgumentException(nameof(query));

                qLength += query.Command.Length;
                qParametersCount += query.Parameters.Count;
            }

            var sb = new StringBuilder(qLength + queries.Count + qParametersCount * 2);
            var parameters = new List<QueryParameter>();
            var nextParameterIndex = 1;

            for (var i = 0; i < queries.Count; i++)
            {
                var query = queries[i];

                if (sb.Length > 0)
                    sb.Append("\n;");

                if (parameters.Count == 0 || query.Parameters.Count == 0)
                {
                    sb.Append(query.Command.Trim().Trim(';').Trim());
                    parameters.AddRange(query.Parameters);
                }
                else
                {
                    ResolveSqlQueryParameters(sb, query.Command, ParameterPrefix[0], name =>
                    {
                        name = "@" + name;
                        var qp = query.Parameters.FirstOrDefault(x => x.Name == name);
                        if (qp == null)
                            return name;

                        var ex = parameters.FirstOrDefault(x => Equals(x.Value, qp.Value));
                        if (ex != null)
                            return ex.Name;

                        nextParameterIndex = Math.Max(parameters.Count + 1, nextParameterIndex);
                        while (parameters.Any(x => x.Name == ParameterPrefix + nextParameterIndex))
                            ++nextParameterIndex;

                        var pn = ParameterPrefix + nextParameterIndex;
                        parameters.Add(new QueryParameter(pn, qp.Value));
                        ++nextParameterIndex;

                        return pn;
                    });
                }
            }

            return new Query(sb.ToString(), parameters);
        }

        internal static void ResolveSqlQueryParameters(StringBuilder sb, string query, char parameterPrefix, Func<string, string> parameterResolver)
        {
            for (var idx = 0; idx < query.Length; idx++)
            {
                var c = query[idx];

                // Line comment
                if (c == '-' && idx + 1 < query.Length && query[idx + 1] == '-')
                {
                    sb.Append(c);
                    sb.Append(query[++idx]);

                    while (++idx < query.Length)
                    {
                        sb.Append(query[idx]);

                        if (query[idx] == '\n')
                            break;
                    }

                    continue;
                }

                // Multiline comment
                if (c == '/' && idx + 2 < query.Length && query[idx + 1] == '*')
                {
                    sb.Append(c);
                    sb.Append(query[++idx]);
                    sb.Append(query[++idx]);

                    while (++idx < query.Length)
                    {
                        sb.Append(query[idx]);

                        if (query[idx] == '/' && query[idx - 1] == '*')
                            break;
                    }

                    continue;
                }

                // String $$Text with ' sign.$$ or $any$Text with ' sign.$any$
                if (c == '$')
                {
                    var next = query.IndexOf('$', idx + 1);
                    if (next < 0)
                    {
                        sb.Append(query, idx, query.Length - idx);
                        break;
                    }

                    var escapePhrase = query.Substring(idx, next - idx + 1);
                    next = query.IndexOf(escapePhrase, idx + escapePhrase.Length);
                    if (next < 0)
                    {
                        sb.Append(query, idx, query.Length - idx);
                        break;
                    }

                    next += escapePhrase.Length;
                    sb.Append(query, idx, next - idx);

                    idx = next - 1;
                    continue;
                }

                // String E'Text with \' sign.'
                if (c == 'E' && idx + 1 < query.Length && query[idx + 1] == '\'' && !(idx > 0 && char.IsLetterOrDigit(query[idx - 1])))
                {
                    sb.Append(c);
                    sb.Append(query[++idx]);

                    while (++idx < query.Length)
                    {
                        sb.Append(query[idx]);

                        if (query[idx] == '\'' && query[idx - 1] != '\\')
                            break;
                    }

                    continue;
                }

                // String 'Text with '' sign.' or "Text with "" sign."
                if (c == '\'' || c == '"')
                {
                    sb.Append(c);

                    while (++idx < query.Length)
                    {
                        sb.Append(query[idx]);

                        if (query[idx] == c)
                            if (idx + 1 < query.Length && query[idx + 1] == c)
                                sb.Append(query[++idx]);
                            else
                                break;
                    }

                    continue;
                }

                // Argument @name
                if (c == parameterPrefix
                    && idx + 1 < query.Length && (char.IsLetterOrDigit(query[idx + 1]) || query[idx + 1] == '_')
                    && (idx == 0 || !(char.IsLetterOrDigit(query[idx - 1]) || query[idx - 1] == '_')))
                {
                    var end = idx + 2;
                    while (end < query.Length && (char.IsLetterOrDigit(query[end]) || query[end] == '_'))
                        ++end;

                    var arg = query.Substring(idx + 1, end - idx - 1);
                    arg = parameterResolver(arg);
                    sb.Append(arg);

                    idx = end - 1;
                    continue;
                }

                // Other
                sb.Append(c);
            }
        }

        public static bool operator ==(Query left, Query right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (ReferenceEquals(left, null))
                return false;

            return left.Equals(right);
        }
        public static bool operator !=(Query left, Query right)
        {
            return !(left == right);
        }
    }

    public class QueryParameter : IEquatable<QueryParameter>
    {
        public string Name { get; }
        public object Value { get; }

        public QueryParameter(string name, object value)
        {
            Guard.ArgumentNotNullOrWhiteSpace(name, "name");

            if (!name.StartsWith(Query.ParameterPrefix))
                throw new ArgumentException($"Argument name is missing prefix '{Query.ParameterPrefix}'.", nameof(name));

            Name = name;

            if (value != null && value.GetType().IsEnum)
            {
                var type = Enum.GetUnderlyingType(value.GetType());

                if (type == typeof(int))
                    Value = Convert.ToInt32(value);
                else if (type == typeof(uint))
                    Value = Convert.ToUInt32(value);

                else if (type == typeof(short))
                    Value = Convert.ToInt16(value);
                else if (type == typeof(ushort))
                    Value = Convert.ToUInt16(value);

                else if (type == typeof(sbyte))
                    Value = Convert.ToSByte(value);
                else if (type == typeof(byte))
                    Value = Convert.ToByte(value);

                else if (type == typeof(long))
                    Value = Convert.ToInt64(value);
                else if (type == typeof(ulong))
                    Value = Convert.ToUInt64(value);
            }
            else
                Value = value;
        }


        public override int GetHashCode()
        {
            if (Value == null)
                return Name.GetHashCode();

            return Name.GetHashCode() ^ Value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var query = obj as QueryParameter;
            return query != null && Equals(query);
        }
        public bool Equals(QueryParameter other)
        {
            return Name == other.Name
                && Equals(Value, other.Value);
        }
        public override string ToString()
        {
            return string.Format("{0} = {1}",
                Name,
                Value != null ? Value : "null");
        }
    }
}
