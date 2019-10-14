using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
                var pms = new QueryParameter[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    pms[i] = new QueryParameter(ParameterPrefix + (i + 1).ToString(), args[i]);
                    names[i] = pms[i].Name;
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
            CommandResolve(stringBuilder, Command, name =>
            {
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
                    CommandResolve(sb, query.Command, name =>
                    {
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

        private static void CommandResolve(StringBuilder sb, string command, Func<string, string> parameterResolver)
        {
            var escape = false;
            var escapeChar = '\'';
            var start = 0;

            while (start < command.Length && (char.IsWhiteSpace(command[start]) || command[start] == ';'))
                ++start;

            for (var i = start; i < command.Length; i++)
            {
                var c = command[i];
                if (escape)
                {
                    if (c == escapeChar && command[i - 1] != '\\')
                    {
                        if (i + 1 < command.Length && command[i + 1] == escapeChar)
                        {
                            i++;
                            continue;
                        }

                        escape = false;
                    }
                }
                else
                {
                    if (c == '\'' || c == '"')
                    {
                        escape = true;
                        escapeChar = c;
                    }
                    else if (c == ParameterPrefix[0])
                    {
                        if (ParameterPrefix.Length > 1)
                        {
                            var isParameter = true;
                            for (var j = 1; j < ParameterPrefix.Length; j++)
                                if (command[i + j] != ParameterPrefix[j])
                                {
                                    isParameter = false;
                                    break;
                                }

                            if (!isParameter)
                                continue;
                        }

                        var end = i + 1;
                        while (end < command.Length && (char.IsLetterOrDigit(command[end]) || command[end] == '_'))
                            ++end;

                        if (i + ParameterPrefix.Length >= end)
                            continue;

                        sb.Append(command, start, i - start);
                        sb.Append(parameterResolver(command.Substring(i, end - i)));

                        start = end;
                        i = end - 1;
                    }
                }
            }

            var last = command.Length - 1;
            while (last > start && (char.IsWhiteSpace(command[last]) || command[last] == ';'))
                --last;

            sb.Append(command, start, last - start + 1);
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
