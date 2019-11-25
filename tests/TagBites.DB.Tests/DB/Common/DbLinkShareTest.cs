using TagBites.DB.Tests.DB.Core;
using Xunit;

namespace TagBites.DB.Tests.DB.Common
{
    public class DbLinkShareTest : DbTestBase
    {
        [Fact]
        public void ShareLinkOverNestedCalls()
        {
            using (var link = DefaultProvider.CreateLink())
            {
                using (var link2 = DefaultProvider.CreateLink())
                {
                    Assert.Equal(link.ConnectionContext, link2.ConnectionContext);
                }
            }
        }

        //[Fact]
        //public void ShareLinkOverTaskCalls()
        //{
        //    using (var link = DefaultProvider.CreateLink())
        //    {
        //        link.ConnectionContext.Bag[nameof(ShareLinkOverTaskCalls)] = 1;

        //        var tasks = Enumerable.Range(0, 100)
        //            .Select(x => Task.Factory.StartNew(ShareLinkOverTaskCalls_Task1))
        //            .ToArray();
        //        Task.WaitAll(tasks);

        //        using (var transaction = link.Begin())
        //        {
        //            tasks = Enumerable.Range(0, 50)
        //                .Select(x => Task.Factory.StartNew(ShareLinkOverTaskCalls_Task2, x))
        //                .ToArray();
        //            Task.WaitAll(tasks);
        //        }
        //    }
        //}
        //private void ShareLinkOverTaskCalls_Task1()
        //{
        //    using (var link = DefaultProvider.CreateLink())
        //    {
        //        Thread.Sleep(10);
        //        Assert.Equal(1, link.ConnectionContext.Bag[nameof(ShareLinkOverTaskCalls)]);
        //    }
        //}
        //private void ShareLinkOverTaskCalls_Task2(object index)
        //{
        //    Thread.Sleep(10);

        //    using (var link = DefaultProvider.CreateLink())
        //    using (var transaction = link.Begin())
        //    {
        //        Assert.Equal(index, link.ExecuteScalar<int>("SELECT {0}", index));
        //        transaction.Commit();
        //        Thread.Sleep(1);
        //    }
        //}
    }
}
