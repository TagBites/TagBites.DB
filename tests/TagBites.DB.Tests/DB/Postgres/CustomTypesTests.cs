using System;
using System.Linq;
using TagBites.Sql;
using Xunit;

namespace TagBites.DB.Postgres
{
    public class CustomTypesTests : DbTests
    {
        [Fact]
        public void LargeDecimalTest()
        {
            using var link = NpgsqlProvider.CreateLink();

            var v = link.ExecuteScalar<decimal>("SELECT 7354153637825415363782515415363782515415363782515100568052832642.5000");
            Assert.Equal(decimal.MaxValue, v);

            v = link.ExecuteScalar<decimal>("SELECT 0.7354153637825415363782515415363782515415363782515100568052832642");
            Assert.True(v > 0.7354153m);

            for (var i = 0; i < 40; i++)
            {
                v = link.ExecuteScalar<decimal>($"SELECT 0.{new string('0', i)}1");
                Assert.True(v <= 0.1m);
            }
        }

        [Fact]
        public void AutoDetectParameterTypeTest()
        {
            using var link = NpgsqlProvider.CreateLink();
            using var transaction = link.Begin();

            var now = DateTime.Now;

            link.ExecuteNonQuery("CREATE TABLE t11 ( a int, b bigint, c numeric, d date, e timestamp, f timestamp with time zone );");

            var q = new SqlQueryInsertValues("t11");
            q.Columns.AddRange("a", "b", "c", "d", "e", "f");
            q.Values.Add(1, 1, '1', now, now, null);

            link.ExecuteNonQuery(q);

            transaction.Rollback();
        }

        [Fact]
        public void ArrayBoundsAlwaysFromZeroTest()
        {
            using var link = NpgsqlProvider.CreateLink();
            var result = link.ExecuteScalar<int[]>("SELECT '[4:6]={5,null,6}'::int[]");

            Assert.Equal(7, result.Length);
            Assert.Equal(new[] { 0, 0, 0, 0, 5, 0, 6 }, result);
        }

        [Fact]
        public void SupportsArrayTypeTest()
        {
            using var link = NpgsqlProvider.CreateLink();

            // {ala,NULL," ma ","NULL","null","{","{},"}"}
            Compare(new PgSqlArray("ala", null, " ma ", "NULL", "null", "{", "{}", "}"));

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

            var mpq = link.ExecuteScalar("SELECT '1/2'::mpq");
            Assert.Equal("1/2", mpq);
        }

        [Fact]
        public void NullableTypeTest()
        {
            var q = "SELECT 1 AS NullableInt";
            using var link = NpgsqlProvider.CreateLink();
            var result = link.Execute<Model>(q);

            Assert.Single(result);

            Assert.Equal(1, result[0].NullableInt);
        }

        [Fact]
        public void ArrayTest()
        {
            var q = "SELECT Array[1,2]::int[] AS IntArray, Array['x','y']::text[] AS TextArray";
            using var link = NpgsqlProvider.CreateLink();
            var result = link.Execute<Model>(q);

            Assert.Single(result);

            // A
            Assert.Equal(3, result[0].IntArray.Length);
            Assert.Equal(0, result[0].IntArray[0]);
            Assert.Equal(1, result[0].IntArray[1]);
            Assert.Equal(2, result[0].IntArray[2]);

            // B
            Assert.Equal(3, result[0].TextArray.Length);
            Assert.Null(result[0].TextArray[0]);
            Assert.Equal("x", result[0].TextArray[1]);
            Assert.Equal("y", result[0].TextArray[2]);
        }
    }
}
