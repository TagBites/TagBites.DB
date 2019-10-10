using System;
using System.Linq;
using TBS.Data.DB;
using TBS.Data.DB.PostgreSql;
using Xunit;

namespace TBS.Data.UnitTests.DB
{
    public class CustomTypesTest : DbTestBase
    {
        [Fact]
        public void SupportsArrayTypeTest()
        {
            using var link = NpgsqlProvider.CreateLink();

            // {ala,NULL," ma ","NULL","null"}
            Compare(new PgSqlArray("ala", null, " ma ", "NULL", "null"));

            // {1,NULL,3}
            CompareT<int>(new PgSqlArray<int>(1, null, 3));

            // [3:5]={True,NULL,False}
            CompareT<bool>(new PgSqlArray<bool>(true, null, false) { StartIndex = 3 });


            void Compare(PgSqlArray array)
            {
                var value = link.ExecuteScalar("SELECT {0}::text[]", array.ToString());

                switch (value)
                {
                    case string s:
                        {
                            var valueArray = PgSqlArray.TryParseDefault(s);
                            Assert.Equal<string>(array, valueArray);
                            break;
                        }

                    case string[] strings:
                        Assert.Equal<string>(array, strings);
                        break;
                }
            }
            void CompareT<T>(PgSqlArray<T> array) where T : struct
            {
                var name = typeof(T) == typeof(int)
                    ? "int"
                    : typeof(T).Name.ToLower();

                var value = link.ExecuteScalar($"SELECT {{0}}::{name}[]", array.ToString());

                switch (value)
                {
                    case string s:
                        {
                            var valueArray = PgSqlArray<T>.TryParseDefault(s);
                            Assert.Equal<T?>(array, valueArray);
                            break;
                        }

                    case T?[] nullable:
                        Assert.Equal<T?>(array, nullable);
                        break;

                    case T[] nonNullable:
                        {
                            var items = array.ToArray().Select(x => x.GetValueOrDefault()).ToArray();
                            Assert.Equal<T>(items, nonNullable);
                            break;
                        }

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        [Fact]
        public void SupportsUnknownTypeTest()
        {
            using var link = NpgsqlProvider.CreateLink();

            var mpq = link.ExecuteScalar<string>("SELECT '1/2'::mpq");
            Assert.Equal("1/2", mpq);
        }
    }
}
