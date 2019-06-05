using System;
using System.Collections.Generic;
using System.Data.Common;

namespace TBS.Data.DB
{
    public class DbConnectionArguments
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ICollection<string> Keys => _values.Keys;

        public string Host { get => this[nameof(Host)]; set => this[nameof(Host)] = value; }
        public int Port { get => int.TryParse(this[nameof(Port)], out var v) ? v : 0; set => this[nameof(Port)] = value.ToString(); }
        public string Database { get => this[nameof(Database)]; set => this[nameof(Database)] = value; }
        public string Username { get => this[nameof(Username)]; set => this[nameof(Username)] = value; }
        public string Password { get => this[nameof(Password)]; set => this[nameof(Password)] = value; }

        public bool UsePooling { get => bool.TryParse(this[nameof(UsePooling)], out var usePooling) && usePooling; set => this[nameof(UsePooling)] = value.ToString(); }
        public int MinPoolSize { get => int.TryParse(this[nameof(MinPoolSize)], out var v) ? v : 0; set => this[nameof(MinPoolSize)] = value.ToString(); }
        public int MaxPoolSize { get => int.TryParse(this[nameof(MaxPoolSize)], out var v) ? v : 0; set => this[nameof(MaxPoolSize)] = value.ToString(); }

        public string this[string name]
        {
            get => _values.TryGetValue(name, out var value) ? value : null;
            set
            {
                if (value == null)
                    _values.Remove(name);
                else
                    _values[name] = value;
            }
        }

        public DbConnectionArguments()
        {
            UsePooling = true;
            MinPoolSize = 1;
            MaxPoolSize = 100;
        }
        public DbConnectionArguments(string connectionString)
            : this()
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Value cannot be null or empty.", nameof(connectionString));

            var sb = new DbConnectionStringBuilder { ConnectionString = connectionString };

            foreach (string key in sb.Keys)
                this[key] = sb[key]?.ToString();

            if (sb.ContainsKey("database"))
                Database = (string)sb["database"];
            else if (sb.ContainsKey("data source"))
                Database = (string)sb["data source"];
            else if (sb.ContainsKey("uri"))
                Database = (string)sb["uri"];
            else if (sb.ContainsKey("fulluri"))
                Database = (string)sb["fulluri"];

            if (sb.ContainsKey("host"))
                Host = (string)sb["host"];
            else if (sb.ContainsKey("server"))
                Host = (string)sb["server"];

            if (sb.ContainsKey("port") && int.TryParse(sb["port"] as string, out var port))
                Port = port;

            if (sb.ContainsKey("username"))
                Username = (string)sb["username"];
            else if (sb.ContainsKey("user name"))
                Username = (string)sb["user name"];

            if (sb.ContainsKey("password"))
                Password = (string)sb["password"];

            UsePooling = !sb.ContainsKey("POOLING") || DataHelper.TryChangeTypeDefault(sb["POOLING"], true);
            MinPoolSize = sb.ContainsKey("MINPOOLSIZE") ? DataHelper.TryChangeTypeDefault(sb["MINPOOLSIZE"], 1) : 1;
            MaxPoolSize = sb.ContainsKey("MAXPOOLSIZE") ? DataHelper.TryChangeTypeDefault(sb["MAXPOOLSIZE"], 100) : 100;
        }
        public DbConnectionArguments(string host, int port, string database, string username, string password)
            : this()
        {
            Host = host;
            Port = port;
            Database = database;
            Username = username;
            Password = password;
        }
    }
}
