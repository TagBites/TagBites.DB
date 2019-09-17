//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TBS.Data.DB;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Data.Entity;
//using System.Transactions;
//using TBS.Data.DB.PostgreSql.Entity;

//namespace TBS.Data.UnitTests.DB
//{
//    //[TestClass]
//    //public class DbEntityLinkTest : DbTestBase
//    //{
//    //    [TestMethod]
//    //    public void EntityConnectionTest()
//    //    {
//    //        using (var link = NpgsqlProvider.CreateLink())
//            {
//    //            using (var context = new EventContext(link))
//    //            {
//    //                var e = context.Events.FirstOrDefault();
//    //                Assert.NotNull(e);
//    //            }
//    //        }

//    //        using (var link = NpgsqlProvider.CreateLink())
//    //        {
//    //            using (var context = new EventContext(link))
//    //            {
//    //                context.Events.Count();
//    //            }
//    //        }
//    //    }

//    //    [TestMethod]
//    //    public void EntityTransactionTest()
//    //    {
//    //        NpgsqlProvider.Configuration.ImplicitCreateTransactionScopeIfNotExists = true;

//    //        using (var link = NpgsqlProvider.CreateLink())
//    //        using (var transaction = link.Begin())
//    //        {
//    //            Event e = null;

//    //            using(new TransactionScope(TransactionScopeOption.Suppress))
//    //            using (var context = new EventContext(link))
//    //            {
//    //                e = context.Events.FirstOrDefault(x => x.ID == 50);
//    //                Assert.NotNull(e);

//    //                e.Subject = (e.Subject ?? String.Empty) + " " + (e.Subject ?? String.Empty).Length.ToString();

//    //                var updated = context.SaveChanges();
//    //                Assert.Equal(1, updated);
//    //            }

//    //            var s = link.ExecuteScalar("SELECT zd_temat FROM tb_zdarzenia WHERE zd_idzdarzenia={0}", e.ID);
//    //            Assert.Equal(e.Subject, s);

//    //            transaction.Commit();
//    //        }

//    //        using (var link = NpgsqlProvider.CreateLink())
//    //        {
//    //            link.Force();
//    //        }
//    //    }

//    //    #region Context Example

//    //    [Table("tb_zdarzenia", Schema = "public")]
//    //    public class Event
//    //    {
//    //        [Column("zd_idzdarzenia")]
//    //        public int ID { get; set; }

//    //        [Column("zd_temat")]
//    //        public string Subject { get; set; }
//    //    }

//    //    public class EventContext : NpgsqlDbContext
//    //    {
//    //        public DbSet<Event> Events { get; set; }

//    //        static EventContext()
//    //        {
//    //            Database.SetInitializer<EventContext>(null);
//    //        }
//    //        public EventContext(DbLink link)
//    //            : base(link, false)
//    //        {
//    //            //Configuration.AutoDetectChangesEnabled = false;
//    //            Configuration.LazyLoadingEnabled = false;
//    //        }
//    //    }

//    //    #endregion
//    //}
//}
