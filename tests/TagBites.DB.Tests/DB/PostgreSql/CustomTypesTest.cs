using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBS.Data.DB;
using TBS.Data.DB.PostgreSql;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class CustomTypesTest : DbTestBase
    {
        [TestMethod]
        public void SupportsArrayTypeTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                // {ala,NULL," ma ","NULL","null"}
                var textArrayText = new PgSqlArray("ala", null, " ma ", "NULL", "null");
                // {1,NULL,3} 
                var intArrayText = new PgSqlArray<int>(1, null, 3);
                // [3:5]={True,NULL,False}
                var boolArrayText = new PgSqlArray<bool>(true, null, false) { StartIndex = 3 };

                link.ExecuteNonQuery("CREATE TABLE tmp_array_test ( a text[], b int[], c bool[] )");
                link.ExecuteNonQuery("INSERT INTO tmp_array_test VALUES ({0}, {1}, {2})", textArrayText.ToString(), intArrayText.ToString(), boolArrayText.ToString());

                var arrays = link.Execute("SELECT * FROM tmp_array_test");
                var textArrayTextOut = PgSqlArray.TryParseDefault(arrays.GetValue<string>(0, 0));
                var intArrayTextOut = PgSqlArray<int>.TryParseDefault(arrays.GetValue<string>(0, 1));
                var boolArrayTextOut = PgSqlArray<bool>.TryParseDefault(arrays.GetValue<string>(0, 2));

                Assert.AreEqual(textArrayText, textArrayTextOut);
                Assert.AreEqual(intArrayText, intArrayTextOut);
                Assert.AreEqual(boolArrayText, boolArrayTextOut);

                transaction.Rollback();
            }
        }

        [TestMethod]
        public void SupportsUnknownTypeTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            {
                var mpq = link.ExecuteScalar<string>("SELECT '1/2'::mpq");
                Assert.AreEqual("1/2", mpq);
            }
        }
    }
}
