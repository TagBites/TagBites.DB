using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBS.Data.DB;
using TBS.Data.DB.Entity;
using TBS.DB.Entity;

#if !NET_45
using TBS.Data.DB.Entity.Schema;
#else
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#endif

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class EntityTableTest : DbTestBase
    {
        [TestMethod]
        public void EntityTableMethodsTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.ExecuteNonQuery("CREATE TABLE tmp ( id SERIAL PRIMARY KEY, value TEXT )");

                var entity = new Entity();
                entity.Value = "V";

                EntityTable.Update(link, entity);
                var entity2 = TBS.Data.DB.Entity.EntityTable.GetByKey<Entity>(link, 1);
                Assert.AreEqual(entity.Value, entity2.Value);
                Assert.AreEqual(entity.Id, entity2.Id);

                EntityTable.Update(link, entity);
                Assert.AreEqual(1, entity2.Id);

                var entity3 = EntityTable.Where<Entity>(link, x => x.Id, 1).FirstOrDefault();
                Assert.AreEqual(entity.Value, entity3.Value);

                EntityTable.Delete<Entity>(link, 1);
                var entity4 = TBS.Data.DB.Entity.EntityTable.GetByKey<Entity>(link, 1);
                Assert.IsNull(entity4);

                transaction.Rollback();
            }
        }

        [TestMethod]
        public void EntityTableMethodsTest2()
        {
            using (var link = NpgsqlProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.ExecuteNonQuery("CREATE TABLE tmp ( id SERIAL PRIMARY KEY, value TEXT )");

                var entity = new Entity();
                entity.Value = "V";
                EntityTable.Update(link, entity);
                Assert.AreEqual(entity.Id, 1);

                var entity2 = link.EntityQuery<Entity>().First(x => x.Value == "V");
                Assert.AreEqual(entity.Value, entity2.Value);

                var entityQuery = link.EntityQuery<Entity>().Where(x => x.Value == "V").Select(x => new { x.Id, x.Value });
                var entityQuerySelect = link.ParseEntityQuery(entityQuery);
                var entity3 = entityQuery.First();
                Assert.AreEqual(entity.Value, entity3.Value);

                transaction.Rollback();
            }
        }

        [Table("tmp")]
        public class Entity
        {
            [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            [Column("value")]
            public string Value { get; set; }
        }
    }
}
