using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TBS.Data.DB;

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

                entity = link.UpsertReturning(entity);
                Assert.AreEqual("V", entity.Value);
                Assert.AreEqual(1, entity.Id);

                var entity2 = link.GetByKey<Entity>(1);
                Assert.AreEqual(entity.Value, entity2.Value);
                Assert.AreEqual(entity.Id, entity2.Id);

                entity.Value = "V2";
                entity = link.UpsertReturning(entity);
                Assert.AreEqual("V2", entity.Value);
                Assert.AreEqual(1, entity2.Id);

                var entity3 = link.EntityQuery<Entity>().FirstOrDefault(x => x.Id == 1);
                Assert.AreEqual(entity.Value, entity3?.Value);

                link.DeleteByKey<Entity>(1);
                var entity4 = link.GetByKey<Entity>(1);
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

                entity = link.UpsertReturning(entity);
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
