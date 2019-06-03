using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBS.Data.DB;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class DbLinkBufferedQueriesTest : DbTestBase
    {
        [TestMethod]
        public void CancelTest()
        {
            var q1 = new Query("Select 1");
            var q2 = new Query("Select 2");

            // Execute before close connection
            using (var link = DefaultProvider.CreateLink())
            {
                var r1 = link.DelayedBatchExecute(q1);
                var r2 = link.DelayedBatchExecute(q2);

                r1.Cancel();
                link.Force();

                Assert.IsFalse(r1.HasResult);
                Assert.IsTrue(r2.HasResult);
            }
        }

        [TestMethod]
        public void BatchTest()
        {
            var q1 = new Query("Select 1");
            var q2 = new Query("Select 21; Select 2");
            var q3 = new Query("Select 3");

            // Execute before close connection
            using (var link = DefaultProvider.CreateLink())
            {
                var r1 = link.DelayedBatchExecute(q1);
                var r2 = link.DelayedBatchExecute(q2);
                var r3 = link.DelayedBatchExecute(q3);

                Assert.AreEqual(1, r1.Result.ToScalar<int>());
                Assert.AreEqual(2, r2.Result.ToScalar<int>());
                Assert.AreEqual(3, r3.Result.ToScalar<int>());
            }
        }

        [TestMethod]
        public void ShareLinkOverNestedCalls()
        {
            var q1 = new Query("Select 1");
            DelayedBatchQueryResult result = null;

            // Execute before close connection
            using (var link = DefaultProvider.CreateLink())
            {
                result = link.DelayedBatchExecute(q1);
                Assert.IsFalse(result.HasResult);
            }

            Assert.IsTrue(result.HasResult);

            // Execute before open transaction
            using (var link = DefaultProvider.CreateLink())
            {
                result = link.DelayedBatchExecute(q1);
                Assert.IsFalse(result.HasResult);

                using (var transaction = link.Begin())
                {
                    Assert.IsTrue(result.HasResult);
                    transaction.Commit();
                }
            }

            // Execute before commit
            using (var link = DefaultProvider.CreateLink())
            {
                using (var transaction = link.Begin())
                {
                    result = link.DelayedBatchExecute(q1);
                    Assert.IsFalse(result.HasResult);
                    transaction.Commit();
                }

                Assert.IsTrue(result.HasResult);
            }

            // Cancel when rollback
            using (var link = DefaultProvider.CreateLink())
            {
                using (var transaction = link.Begin())
                {
                    result = link.DelayedBatchExecute(q1);
                    Assert.IsFalse(result.HasResult);
                    transaction.Rollback();
                }

                Assert.IsFalse(result.HasResult);
                Assert.IsTrue(result.IsCompleted);
                Assert.IsTrue(result.IsCanceled);
            }

            // Execute before other query
            using (var link = DefaultProvider.CreateLink())
            {
                result = link.DelayedBatchExecute(q1);
                Assert.IsFalse(result.HasResult);
                link.ExecuteScalar<int>("Select 'a'");
                Assert.IsTrue(result.HasResult);
            }
        }
    }
}
