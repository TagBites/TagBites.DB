using Xunit;

namespace TagBites.DB
{
    public class DelayedBatchTests : DbTests
    {
        [Fact]
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

                Assert.False(r1.HasResult);
                Assert.True(r2.HasResult);
            }
        }

        [Fact]
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

                Assert.Equal(1, r1.Result.ToScalar<int>());
                Assert.Equal(2, r2.Result.ToScalar<int>());
                Assert.Equal(3, r3.Result.ToScalar<int>());
            }
        }

        [Fact]
        public void ShareLinkOverNestedCalls()
        {
            var q1 = new Query("Select 1");
            DelayedBatchQueryResult result = null;

            // Execute before close connection
            using (var link = DefaultProvider.CreateLink())
            {
                result = link.DelayedBatchExecute(q1);
                Assert.False(result.HasResult);
            }

            Assert.True(result.HasResult);

            // Execute before open transaction
            using (var link = DefaultProvider.CreateLink())
            {
                result = link.DelayedBatchExecute(q1);
                Assert.False(result.HasResult);

                using (var transaction = link.Begin())
                {
                    Assert.True(result.HasResult);
                    transaction.Commit();
                }
            }

            // Execute before commit
            using (var link = DefaultProvider.CreateLink())
            {
                using (var transaction = link.Begin())
                {
                    result = link.DelayedBatchExecute(q1);
                    Assert.False(result.HasResult);
                    transaction.Commit();
                }

                Assert.True(result.HasResult);
            }

            // Cancel when rollback
            using (var link = DefaultProvider.CreateLink())
            {
                using (var transaction = link.Begin())
                {
                    result = link.DelayedBatchExecute(q1);
                    Assert.False(result.HasResult);
                    transaction.Rollback();
                }

                Assert.False(result.HasResult);
                Assert.True(result.IsCompleted);
                Assert.True(result.IsCanceled);
            }

            // Execute before other query
            using (var link = DefaultProvider.CreateLink())
            {
                result = link.DelayedBatchExecute(q1);
                Assert.False(result.HasResult);
                link.ExecuteScalar("Select 'a'");
                Assert.True(result.HasResult);
            }
        }
    }
}
