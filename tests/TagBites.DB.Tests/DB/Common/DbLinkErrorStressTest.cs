using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBS.Data.DB;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbLinkErrorStressTest : DbTestBase
    {
        [TestMethod]
        public void ConnectionStaysOpenAfterQueryExceptionTest()
        {
            using (var link = CreateLink())
            {
                try
                {
                    link.ExecuteNonQuery("SELECT a");
                    Assert.Fail("Connection was break.");
                }
                catch { }

                Assert.AreEqual(link.ExecuteScalar<int>("SELECT 1"), 1);
            }
        }

        [TestMethod]
        public void TransactionFailsAfterQueryExceptionTest()
        {
            using (var link = CreateLink())
            using (var transaction = link.Begin())
            {
                try
                {
                    link.ExecuteNonQuery("SELECT a");
                    Assert.Fail();
                }
                catch { }

                Assert.AreEqual(link.TransactionStatus, DbLinkTransactionStatus.RollingBack);

                try
                {
                    link.ExecuteNonQuery("SELECT 1");
                    Assert.Fail();
                }
                catch { }

                try
                {
                    transaction.Commit();
                    Assert.Fail();
                }
                catch { }
            }
        }

        [TestMethod]
        public void TransactionFailsWhenConnectionBreaksTest()
        {
            using (var link = CreateLink())
            using (var transaction = link.Begin())
            {
                Assert.AreEqual(link.ExecuteScalar<int>("SELECT 1"), 1);

                ((DbLinkContext)link.ConnectionContext).GetOpenConnection().Close();

                try
                {
                    link.ExecuteNonQuery("SELECT 1");
                    Assert.Fail("Connection was break.");
                }
                catch { }

                try
                {
                    transaction.Commit();
                    Assert.Fail("Connection was break.");
                }
                catch { }
            }
        }

        [TestMethod]
        public void ErrorInQueryNotBreaksTheConnection()
        {
            using (var link = CreateLink())
            {
                try
                {
                    link.ExecuteNonQuery("SELECT a");
                    Assert.Fail();
                }
                catch { }

                link.ExecuteNonQuery("SELECT 1");
                ((DbLinkContext)link.ConnectionContext).GetOpenConnection().Close();
                link.ExecuteNonQuery("SELECT 1");
            }
        }

        [TestMethod]
        public void AutoReopenConnection()
        {
            using (var link = CreateLink())
            {
                link.ExecuteNonQuery("SELECT 1");
                ((DbLinkContext)link.ConnectionContext).GetOpenConnection().Close();

                using (var link2 = NpgsqlProvider.CreateLink())
                    link2.ExecuteNonQuery("SELECT 1");
            }
        }
    }
}
