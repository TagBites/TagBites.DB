using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TagBites.DB.Tests.DB.Core;
using Xunit;

namespace TagBites.DB.Tests.DB
{
    public class EntityTableTest : DbTestBase
    {
        [Fact]
        public void EntityTableMethodsTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.ExecuteNonQuery("CREATE TABLE tmp ( id SERIAL PRIMARY KEY, value TEXT )");

                var entity = new Entity();
                entity.Value = "V";

                entity = link.UpsertReturning(entity);
                Assert.Equal("V", entity.Value);
                Assert.Equal(1, entity.Id);

                var entity2 = link.GetByKey<Entity>(1);
                Assert.Equal(entity.Value, entity2.Value);
                Assert.Equal(entity.Id, entity2.Id);

                entity.Value = "V2";
                entity = link.UpsertReturning(entity);
                Assert.Equal("V2", entity.Value);
                Assert.Equal(1, entity2.Id);

                var entity3 = link.EntityQuery<Entity>().FirstOrDefault(x => x.Id == 1);
                Assert.Equal(entity.Value, entity3?.Value);

                link.DeleteByKey<Entity>(1);
                var entity4 = link.GetByKey<Entity>(1);
                Assert.Null(entity4);

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
