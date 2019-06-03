using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBS.Data.DB;
using TBS.Data.DB.PostgreSql;
using TBS.Data.DB.Utils;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbUtilsTest : DbTestBase
    {
        [TestMethod]
        public void DbTableChangerTest()
        {
            // Simple primary key
            using (var link = DefaultProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                if (link.ConnectionContext.Provider is PgSqlLinkProvider)
                    link.ExecuteNonQuery("CREATE TABLE tmp_t1 (id SERIAL PRIMARY KEY, v1 TEXT, v2 INT)");
                else
                    link.ExecuteNonQuery("CREATE TABLE tmp_t1 (id INTEGER PRIMARY KEY, v1 TEXT, v2 INT)");

                var changer = new DbTableChanger("tmp_t1", "id", true);

                var r1 = changer.Records.Add();
                r1["v1"] = "1";
                r1["v2"] = 1;
                var affectedRows = changer.Execute(link);

                Assert.AreEqual(1, affectedRows);
                Assert.AreEqual(DbTableChangerRecordStatus.Inserted, r1.Status);
                Assert.AreEqual(1, DataHelper.TryChangeTypeDefault<int>(r1.Key));

                changer.Parameters.Add(new DbTableChangerParameter("v2", DbParameterDirection.Output));
                r1["v1"] = "11";
                r1["v2"] = 11;
                var r2 = changer.Records.Add();
                r2["v1"] = "2";
                r2["v2"] = 2;
                affectedRows = changer.Execute(link);

                Assert.AreEqual(2, affectedRows);
                Assert.AreEqual(DbTableChangerRecordStatus.Inserted, r2.Status);
                Assert.AreEqual(DbTableChangerRecordStatus.Updated, r1.Status);
                Assert.AreEqual(1, DataHelper.TryChangeTypeDefault<int>(r1.Key));
                Assert.AreEqual(2, DataHelper.TryChangeTypeDefault<int>(r2.Key));
                Assert.AreEqual(1, DataHelper.TryChangeTypeDefault<int>(r1["v2"])); // TODO dla postgresa przy update returning dodać dodatkowy from !
                Assert.AreEqual(null, r2["v2"]);

                transaction.Rollback();
            }

            // Composite primary key
            using (var link = NpgsqlProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.ExecuteNonQuery("CREATE TABLE tmp_t1 (id1 INTEGER NOT NULL, id2 INTEGER NOT NULL, v1 TEXT, v2 INT, PRIMARY KEY (id1, id2))");

                var changer = new DbTableChanger("tmp_t1", new string[] { "id1", "id2" }, false);

                var r1 = changer.Records.Add();
                r1["id1"] = 1;
                r1["id2"] = 2;
                r1["v1"] = "1";
                r1["v2"] = 1;
                var affectedRows = changer.Execute(link, DbTableChangerExecuteMode.InsertOrUpdateBasedOnExistence);

                Assert.AreEqual(1, affectedRows);
                Assert.AreEqual(DbTableChangerRecordStatus.Inserted, r1.Status);
                //Assert.AreEqual(r1.Key, 1);

                changer.Parameters.Add(new DbTableChangerParameter("v2", DbParameterDirection.Output));
                r1["id1"] = 1;
                r1["id2"] = 2;
                r1["v1"] = "11";
                r1["v2"] = 11;
                var r2 = changer.Records.Add();
                r2["id1"] = 2;
                r2["id2"] = 2;
                r2["v1"] = "2";
                r2["v2"] = 2;
                affectedRows = changer.Execute(link, DbTableChangerExecuteMode.InsertOrUpdateBasedOnExistence);

                Assert.AreEqual(2, affectedRows);
                Assert.AreEqual(DbTableChangerRecordStatus.Updated, r1.Status);
                Assert.AreEqual(DbTableChangerRecordStatus.Inserted, r2.Status);
                Assert.AreEqual(1, DataHelper.TryChangeTypeDefault<int>(r1.Keys[0]));
                Assert.AreEqual(2, DataHelper.TryChangeTypeDefault<int>(r2.Keys[0]));
                Assert.AreEqual(1, DataHelper.TryChangeTypeDefault<int>(r1["v2"]));
                Assert.AreEqual(null, r2["v2"]);

                transaction.Rollback();
            }
        }
    }
}
