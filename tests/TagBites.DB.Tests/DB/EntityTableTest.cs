using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
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
                DbLinkExtensions.ExecuteNonQuery(link, "CREATE TABLE tmp ( id SERIAL PRIMARY KEY, value TEXT )");

                var entity = new Entity();
                entity.Value = "V";

                entity = DbLinkExtensions.UpsertReturning(link, entity);
                Assert.Equal("V", entity.Value);
                Assert.Equal(1, entity.Id);

                var entity2 = DbLinkExtensions.GetByKey<Entity>((IDbLink)link, 1);
                Assert.Equal(entity.Value, entity2.Value);
                Assert.Equal(entity.Id, entity2.Id);

                entity.Value = "V2";
                entity = DbLinkExtensions.UpsertReturning(link, entity);
                Assert.Equal("V2", entity.Value);
                Assert.Equal(1, entity2.Id);

                var entity3 = DbLinkExtensions.EntityQuery<Entity>(link).FirstOrDefault(x => x.Id == 1);
                Assert.Equal(entity.Value, entity3?.Value);

                DbLinkExtensions.DeleteByKey<Entity>((IDbLink)link, 1);
                var entity4 = DbLinkExtensions.GetByKey<Entity>((IDbLink)link, 1);
                Assert.Null(entity4);

                transaction.Rollback();
            }
        }

        [Fact]
        public void EntityTableMethodsTest2()
        {
            using (var link = NpgsqlProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                DbLinkExtensions.ExecuteNonQuery(link, "CREATE TABLE tmp ( id SERIAL PRIMARY KEY, value TEXT )");

                var entity = new Entity();
                entity.Value = "V";

                entity = DbLinkExtensions.UpsertReturning(link, entity);
                Assert.Equal(1, entity.Id);

                var entity2 = DbLinkExtensions.EntityQuery<Entity>(link).First(x => x.Value == "V");
                Assert.Equal(entity.Value, entity2.Value);

                var entityQuery = DbLinkExtensions.EntityQuery<Entity>(link).Where(x => x.Value == "V").Select(x => new { x.Id, x.Value });
                var entityQuerySelect = DbLinkExtensions.ParseEntityQuery(link, entityQuery);
                var entity3 = entityQuery.First();
                Assert.Equal(entity.Value, entity3.Value);

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
