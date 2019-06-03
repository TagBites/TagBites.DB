using Npgsql;
using NpgsqlTypes;
using System;
using System.Data.Common;
using TBS.Sql;
using TBS.Sql.PostgreSql;

namespace TBS.Data.DB.PostgreSql
{
    public class NpgsqlLinkAdapter : DbLinkAdapter
    {
        public override SqlQueryResolver QueryResolver { get; } = new PqSqlQueryResolver();


        protected override DbLink CreateDbLink(DbLinkContext context)
        {
            return new PgSqlLink((PgSqlLinkContext)context);
        }
        protected override DbLinkContext CreateDbLinkContext(DbLinkProvider provider, Action<DbConnectionStringBuilder> connectionStringAdapter)
        {
            return new NpgsqlLinkContext(provider, connectionStringAdapter);
        }

        protected override DbConnection CreateConnection(string connectionString)
        {
            var connection = new NpgsqlConnection(connectionString);
            return connection;
        }
        protected override DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
        {
            return new NpgsqlConnectionStringBuilder(connectionString);
        }

        protected override DbCommand CreateCommandInner(Query query)
        {
            var cmd = new NpgsqlCommand(query.Command);
            cmd.AllResultTypesAreUnknown = true;

            foreach (var item in query.Parameters)
            {
                var value = item.Value;
                //if (value != null)
                //{
                //    Type valueType = value.GetType();
                //    if (valueType.IsClass && valueType != typeof(string) && !valueType.IsArray)
                //    {
                //        var args = GetGenericArguments(valueType, typeof(IEnumerable<>));
                //        if (args != null && args.Length == 1)
                //        {
                //            var array = Array.CreateInstance(valueType, 1);
                //            array.SetValue(value, 0);
                //            value = array;
                //        }
                //    }
                //}

                cmd.Parameters.Add(new NpgsqlParameter(item.Name, value ?? DBNull.Value)
                {
                    IsNullable = true,
                    NpgsqlDbType = NpgsqlDbType.Unknown
                });
            }

            //cmd.Prepare();
            return cmd;
        }
        protected override DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            return new NpgsqlDataAdapter((NpgsqlCommand)command);
        }

        //private static Type[] GetGenericArguments(Type type, Type genericTypeDefinition)
        //{
        //    var ti = type.GetTypeInfo();

        //    if (ti.IsGenericTypeDefinition && type == genericTypeDefinition)
        //        return ti.GenericTypeArguments;

        //    if (ti.IsInterface)
        //    {
        //        if (ti.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
        //            return ti.GenericTypeArguments;
        //    }
        //    else
        //    {
        //        for (var it = ti; it != null; it = it.BaseType?.GetTypeInfo())
        //            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericTypeDefinition)
        //                return it.GenericTypeArguments;
        //    }

        //    foreach (var item in ti.ImplementedInterfaces)
        //    {
        //        var iti = item.GetTypeInfo();
        //        if (iti.IsGenericType && item.GetGenericTypeDefinition() == genericTypeDefinition)
        //            return iti.GenericTypeArguments;
        //    }

        //    return new Type[0];
        //}
    }
}
