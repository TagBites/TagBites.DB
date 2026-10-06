using System.Linq;
using System.Reflection;
using Xunit;

namespace TagBites.DB
{
    public class QueryObjectResultTests : DbTests
    {
        [Fact]
        public void QueryObjectResultPropertyResolverTest()
        {
            using (var link = NpgsqlProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                var item = new Item() { Item1 = 1 };

                link.ExecuteNonQuery("CREATE TABLE tmp_item_test ( item1 int, item2 numeric, item3 bool, item4 text)");
                link.ExecuteNonQuery("INSERT INTO tmp_item_test VALUES ({0}, {1}, {2}, {3})", item.Item1, item.Item2, item.Item3, item.Item4);

                var itemResultList = link.Execute<Item>(new Query("SELECT item1 AS Item1, item2 AS Item2, item3 AS Item3, item4 AS Item4 FROM tmp_item_test"), new QueryObjectResultPropertyResolver(ResolverMethod));
                var itemResult = itemResultList.FirstOrDefault();

                Assert.Equal(item.Item1, itemResult.Item1);
                Assert.Equal(item.Item2, itemResult.Item2);
                Assert.Equal(item.Item3, itemResult.Item3);
                Assert.Equal(item.Item4, itemResult.Item4);
                Assert.NotNull(itemResult.ItemInner);
                Assert.Equal(1, itemResult.Item1);

                transaction.Rollback();
            }
        }

        [Fact]
        public void MapsNullIntoNullableAndValueTypeTest()
        {
            using var link = CreateLink();

            var result = link.Execute("SELECT 5 AS Number, NULL AS NullableNumber, NULL AS MissingNumber");
            var item = result.ToObjects<NumberItem>()[0];

            Assert.Equal(5, item.Number);
            Assert.Null(item.NullableNumber);
            Assert.Equal(0, item.MissingNumber);
        }

        [Fact]
        public void MatchesColumnNameIgnoringCaseTest()
        {
            using var link = CreateLink();

            var result = link.Execute("SELECT 7 AS NUMBER");

            Assert.Equal(7, result.ToObjects<NumberItem>()[0].Number);
            Assert.Equal(7, result.GetValue<int>(0, "number"));
        }

        [Fact]
        public void SkipsPropertyWithoutSetterTest()
        {
            using var link = CreateLink();

            var result = link.Execute("SELECT 3 AS Number, 4 AS Computed");
            var item = result.ToObjects<NumberItem>()[0];

            Assert.Equal(3, item.Number);
            Assert.Equal(0, item.Computed);
        }

        [Fact]
        public void RejectsTypeWithoutPublicParameterlessConstructorTest()
        {
            using var link = CreateLink();

            var result = link.Execute("SELECT 1 AS Number");

            Assert.ThrowsAny<Exception>(() => result.ToObjects<HiddenConstructorItem>()[0]);
        }

        private static object ResolverMethod(PropertyInfo property, QueryResultRow resultRow)
        {
            if (property.Name == "ItemInner")
            {
                var value = resultRow.GetValue<int>(0);
                return new ItemInner() { Item1 = value, Item2 = value };
            }
            else
                return null;
        }

        private class Item
        {
            public int Item1 { get; set; }
            public double Item2 { get; set; }
            public bool Item3 { get; set; }
            public string Item4 { get; set; }
            public ItemInner ItemInner { get; set; }
        }
        private class NumberItem
        {
            public int Number { get; set; }
            public int? NullableNumber { get; set; }
            public int MissingNumber { get; set; }
            public int Computed { get; }
        }
        private class HiddenConstructorItem
        {
            public int Number { get; set; }

            private HiddenConstructorItem() { }
        }
        private class ItemInner
        {
            public int Item1 { get; set; }
            public int Item2 { get; set; }
        }
    }
}
