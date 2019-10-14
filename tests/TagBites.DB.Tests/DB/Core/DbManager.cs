﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.DB.Npgsql;
using TagBites.DB.Postgres;
using TagBites.DB.SqLite;

namespace TagBites.DB.Tests.DB.Core
{
    public static class DbManager
    {
        public static PgSqlLinkProvider CreateNpgsqlProvider(bool pooling = true, int minPoolSize = 1, int maxPoolSize = 3)
        {
            var arguments = new DbConnectionArguments()
            {
                Host = "192.168.50.1",
                Port = 5432,
                Database = "newrr",
                Username = "tomasz",
                Password = "tomasz",

                UsePooling = pooling,
                MinPoolSize = minPoolSize,
                MaxPoolSize = maxPoolSize
            };

            return new NpgsqlLinkProvider(arguments)
            {
                Configuration = { UseSystemTransactions = true, ImplicitCreateTransactionScopeIfNotExists = true }
            };
        }

        // ReSharper disable once InconsistentNaming
        public static SqliteLinkProvider CreateSQLiteProvider()
        {
            var arguments = new DbConnectionArguments()
            {
                Database = "D:/db.sqlite",

                UsePooling = true,
                MinPoolSize = 1,
                MaxPoolSize = 3
            };

            return new SqliteLinkProvider(arguments);
        }
    }
}
