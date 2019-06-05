using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using TBS.Data.DB;
using TBS.Data.UnitTests.DB.Core;
using TBS.DB.Entity;
using TBS.Sql;
using static TBS.Sql.SqlExpression;

namespace TBS.Data.UnitTests.DB
{
    [TestClass]
    public class EntityQueryTest : DbTestBase
    {
        #region Private members

        private const string DbStructureSql =
        @"CREATE TABLE tb_secondentity (id SERIAL PRIMARY KEY, column_string TEXT);
        CREATE TABLE tb_firstentity (id SERIAL PRIMARY KEY, column_time timestamp, secondid INTEGER NULL REFERENCES tb_secondentity(id) ON UPDATE CASCADE ON DELETE CASCADE);
        CREATE TABLE tb_entity (id SERIAL PRIMARY KEY, column_int INTEGER, column_string TEXT, column_datetime date, column_bool BOOLEAN, firstid INTEGER NULL REFERENCES tb_firstentity(id) ON UPDATE CASCADE ON DELETE CASCADE);";

        private MainEnitityEqualityComparer m_mainEntityEqualityComparer = new MainEnitityEqualityComparer();
        private IntegerComparer m_integerComparer = new IntegerComparer();
        private StringComparer m_stringComparer = new StringComparer();

        #endregion

        #region IQueryable<T> source methods

        [TestMethod]
        public void QueryableBaseTest()
        {
            CollectionTest(GetMainQuery().Item1, x => new EntityQuery<MainEnitity>(x));
        }

        [TestMethod]
        public void QueryableGenerationTest()
        {
            (var q, var t) = GetMainQuery();
            var entity = new MainEnitity() { Id = 2 };
            CollectionTest(q, x => new EntityQuery<MainEnitity>(x).DefaultIfEmpty(), x => Assert.AreEqual(x.Single(), null));
            CollectionTest(q, x => new EntityQuery<MainEnitity>(x).DefaultIfEmpty(entity), x => Assert.AreEqual(x.Single(), entity));
        }

        [TestMethod]
        public void QueryableFilteringTest()
        {
            // Where
            {
                var date = new DateTime(2012, 12, 12);
                (var q, var t) = GetMainQuery();
                q.Where.Add(Or(
                   AreEquals(t.ColumnInt, Argument(1)),
                   And(
                       Or(AreNotEquals(t.ColumnString, Argument("test")), ToCondition(t.ColumnsBool)),
                       IsLessOrEqual(t.ColumnDatetime, Argument(date)))));
                CollectionTest(q, x => new EntityQuery<MainEnitity>(x).Where(y => y.ColumnInt == 1 || ((y.ColumnString != "test" || y.ColumnsBool) && y.ColumnDatetime <= date)));
                CollectionTestException(x => new EntityQuery<MainEnitity>(x).Where((y, i) => y.ColumnString != "test"));
            }
            // TODO: BJ: OfType  
        }

        [TestMethod]
        public void QueryableMappingTest()
        {
            var select1Query = new SqlQuerySelect();
            select1Query.Select.Add(select1Query.From.Add("public.tb_entity"), "column_int");
            CollectionTest(select1Query, x => new EntityQuery<MainEnitity>(x).Select(y => y.ColumnInt));

            //var select2Query = new SqlQuerySelect();
            //var select2Table = select2Query.From.Add("public.tb_entity");
            //select2Query.Select.Add(SqlExpression.LiteralExpression("{0}+{1}", SqlExpression.Column(select2Table, "column_int"), SqlExpression.Column(select2Table, "column_int")));
            //CollectionTest(select2Query, x => new EntityQuery<MainEnitity>(x).Select(y => y.ColumnInt + y.ColumnInt));

            var selectAnonymousQuery = new SqlQuerySelect();
            var selectAnonymousTable = selectAnonymousQuery.From.Add<MainEnitityTable>();
            selectAnonymousQuery.Select.Add(selectAnonymousTable.FirstId);
            CollectionTest(selectAnonymousQuery, x => new EntityQuery<MainEnitity>(x)
              .Select(y => new { y.FirstId, y.ColumnDatetime, y.ColumnsBool })
              .Select(y => new { y.FirstId, y.ColumnsBool })
              .Select(y => new { y.FirstId }));

            selectAnonymousQuery.Select.Add(selectAnonymousTable.ColumnsBool);
            CollectionTest(selectAnonymousQuery, x => new EntityQuery<MainEnitity>(x)
              .Select(y => new { y.FirstId, y.ColumnDatetime, y.ColumnsBool })
              .Select(y => new { y.FirstId, y.ColumnsBool }));

            selectAnonymousQuery.Select.Add(selectAnonymousTable.ColumnDatetime);
            CollectionTest(selectAnonymousQuery, x => new EntityQuery<MainEnitity>(x)
               .Select(y => new { y.FirstId, y.ColumnDatetime, y.ColumnsBool }));

            // Select - index
            CollectionTestException(x => new EntityQuery<MainEnitity>(x).Select((y, i) => y.ColumnString != "test"));
        }

        [TestMethod]
        public void QueryableJoinTest()
        {
            // TODO: BJ: Join
            // TODO: BJ: GroupJoin
            // TODO: BJ: SelectMany
        }

        [TestMethod]
        public void QueryableConcatenationTest()
        {
            (var q2, var t2) = GetMainQuery();
            q2.Where.Add(IsGreater(t2.ColumnInt, Argument(10)));
            (var q1, var t1) = GetMainQuery();
            q1.Where.Add(IsLess(t1.ColumnInt, Argument(100)));
            q1.Union.Add(q2, SqlClauseUnionEntryType.All);
            var q = new SqlQuerySelect();
            q.From.Add(q1);
            q.Select.AddAll();

            CollectionTest(q, x =>
            {
                var first = new EntityQuery<MainEnitity>(x).Where(y => y.ColumnInt < 100);
                var second = new EntityQuery<MainEnitity>(x).Where(y => y.ColumnInt > 10);
                return first.Concat(second);
            });
        }

        [TestMethod]
        public void QueryableSetTest()
        {
            //// Distinct
            {
                //(var q, var t) = GetMainQuery();
                //q.Distinct.Enabled = true;
                //CollectionTest(q, x => new EntityQuery<MainEnitity>(x).Distinct());
                //CollectionTestException(x => new EntityQuery<MainEnitity>(x).Distinct(m_mainEntityEqualityComparer));
            }
            // GroupBy
            {
                (var q, var t) = GetMainQuery();
                q.OrderBy.Add(t.ColumnInt, SqlClauseOrderByEntryType.Ascending);
                CollectionTest(q, x => new EntityQuery<MainEnitity>(x).GroupBy(y => y.ColumnInt));
                //CollectionTest(q, x => new EntityQuery<MainEnitity>(x).GroupBy(y => y.ColumnInt, m_integerComparer));
            }
            // Uniom
            {
                //(var q, var t) = GetMainQuery();
                //var collection = new[] { new MainEnitity { Id = 1 } };
                //CollectionTest(q, x => new EntityQuery<MainEnitity>(x).Union(collection));

                // TODO
            }
            // Intersect
            {
                // TODO
            }
            // Except
            {
                // TODO
            }
        }

        [TestMethod]
        public void QueryableConvolutionTest()
        {
            var collection = new List<int>();
            CollectionTestException(x => new EntityQuery<MainEnitity>(x).Zip(collection, (y, z) => new MainEnitity()));
        }

        [TestMethod]
        public void QueryablePartitioningTest()
        {
            (var q, var t) = GetMainQuery();
            q.Limit = 10;
            q.Offset = 5;
            CollectionTest(q, x => new EntityQuery<MainEnitity>(x).Skip(5).Take(10));
        }

        [TestMethod]
        public void QueryableOrderingByTest()
        {
            // OrderBy
            {
                (var q, var t) = GetMainQuery();
                q.OrderBy.Add(t.ColumnInt, SqlClauseOrderByEntryType.Ascending);
                CollectionTest(q, x => new EntityQuery<MainEnitity>(x).OrderBy(y => y.ColumnInt));

                CollectionTestException(x => new EntityQuery<MainEnitity>(x).OrderBy(y => y.ColumnInt, m_integerComparer));
            }
            // ThenBy
            {
                (var q, var t) = GetMainQuery();
                q.OrderBy.Add(t.ColumnInt, SqlClauseOrderByEntryType.Ascending);
                q.OrderBy.Add(t.ColumnString, SqlClauseOrderByEntryType.Ascending);
                CollectionTest(q, x => new EntityQuery<MainEnitity>(x).OrderBy(y => y.ColumnInt).ThenBy(y => y.ColumnString));

                CollectionTestException(x => new EntityQuery<MainEnitity>(x).OrderBy(y => y.ColumnInt, m_integerComparer).ThenBy(y => y.ColumnString, m_stringComparer));
            }
            // OrderByDescending
            {
                (var q, var t) = GetMainQuery();
                q.OrderBy.Add(t.ColumnInt, SqlClauseOrderByEntryType.Descending);
                CollectionTest(q, x => new EntityQuery<MainEnitity>(x).OrderByDescending(y => y.ColumnInt));

                CollectionTestException(x => new EntityQuery<MainEnitity>(x).OrderByDescending(y => y.ColumnInt, m_integerComparer).ThenBy(y => y.ColumnString, m_stringComparer));
            }
            // ThenByDescending
            {
                (var q, var t) = GetMainQuery();
                q.OrderBy.Add(t.ColumnInt, SqlClauseOrderByEntryType.Descending);
                q.OrderBy.Add(t.ColumnString, SqlClauseOrderByEntryType.Descending);
                CollectionTest(q, x => new EntityQuery<MainEnitity>(x).OrderByDescending(y => y.ColumnInt).ThenByDescending(y => y.ColumnString));

                CollectionTestException(x => new EntityQuery<MainEnitity>(x).OrderByDescending(y => y.ColumnInt, m_integerComparer).ThenByDescending(y => y.ColumnString, m_stringComparer));
            }
            // Reverse
            {
                CollectionTestException(x => new EntityQuery<MainEnitity>(x).Reverse());
            }
        }

        [TestMethod]
        public void QueryableConversionTest()
        {
            // TODO: BJ: Cast
            // TODO: BJ: AsQueryable

            //var cast = "SELECT tf_1.id, tf_1.column_int, tf_1.column_string, tf_1.column_datetime, tf_1.column_bool, tf_1.firstid FROM public.tb_entity AS tf_1";
            //CollectionTest<MainEnitity, object>(cast, x => x.AsQueryable<object>(), null, x => Assert.AreEqual(typeof(IEnumerable<object>), x.GetType()));
        }

        #endregion

        #region Single value methods

        [TestMethod]
        public void QuearyableElementTest()
        {
            var entity = new MainEnitity() { ColumnInt = 12 };
            {
                // Queries
                (var firstQuery, var firstQueryTable) = GetMainQuery();
                firstQuery.Limit = 1;

                (var firstExpressionQuery, var firstQueryExpressionTable) = GetMainQuery();
                firstExpressionQuery.Where.Add(IsGreater(firstQueryExpressionTable.ColumnInt, Argument(10)));
                firstExpressionQuery.Limit = 1;

                // First
                {
                    SingleWithInitializationTest(firstQuery, x => new EntityQuery<MainEnitity>(x).First(), x => Assert.AreEqual(entity, x), new MainEnitity[] { entity });
                    SingleTestExceptionResult(firstQuery, x => new EntityQuery<MainEnitity>(x).First(), new MainEnitity[0]);
                    SingleWithInitializationTest(firstExpressionQuery, x => new EntityQuery<MainEnitity>(x).First(y => y.ColumnInt > 10), x => Assert.AreEqual(entity, x), new MainEnitity[] { entity });
                }
                // FirstOrDefault
                {
                    SingleTest(firstQuery, x => new EntityQuery<MainEnitity>(x).FirstOrDefault(), x => Assert.AreEqual(null, x));
                    SingleWithInitializationTest(firstQuery, x => new EntityQuery<MainEnitity>(x).FirstOrDefault(), x => Assert.AreEqual(entity, x), new MainEnitity[] { entity });
                    SingleTest(firstExpressionQuery, x => new EntityQuery<MainEnitity>(x).FirstOrDefault(y => y.ColumnInt > 10), x => Assert.AreEqual(null, x));
                    SingleWithInitializationTest(firstExpressionQuery, x => new EntityQuery<MainEnitity>(x).FirstOrDefault(y => y.ColumnInt > 10), x => Assert.AreEqual(entity, x), new MainEnitity[] { entity });
                }
            }
            {
                // Queries
                (var singleQuery, var singleQueryTable) = GetMainQuery();
                singleQuery.Limit = 2;

                (var singleExpressionQuery, var singleExpressionQueryTable) = GetMainQuery();
                singleExpressionQuery.Where.Add(IsGreater(singleExpressionQueryTable.ColumnInt, Argument(10)));
                singleExpressionQuery.Limit = 2;

                // Single
                {
                    SingleWithInitializationTest(singleQuery, x => new EntityQuery<MainEnitity>(x).Single(), x => Assert.AreEqual(entity, x), new MainEnitity[] { entity });
                    SingleTestExceptionResult(singleQuery, x => new EntityQuery<MainEnitity>(x).Single(), new MainEnitity[0]);
                    SingleTestExceptionResult(singleQuery, x => new EntityQuery<MainEnitity>(x).Single(), new MainEnitity[] { entity, entity });
                    SingleWithInitializationTest(singleExpressionQuery, x => new EntityQuery<MainEnitity>(x).Single(y => y.ColumnInt > 10), x => Assert.AreEqual(entity, x), new MainEnitity[] { entity });
                }
                // SingleOrDefault
                {
                    SingleTest(singleQuery, x => new EntityQuery<MainEnitity>(x).SingleOrDefault());
                    SingleTest(singleExpressionQuery, x => new EntityQuery<MainEnitity>(x).SingleOrDefault(y => y.ColumnInt > 10));
                }
            }
            {
                // Last
                SingleTestExceptionMethod(x => new EntityQuery<MainEnitity>(x).Last());
                SingleTestExceptionMethod(x => new EntityQuery<MainEnitity>(x).Last(y => y.ColumnInt > 10));
                // LastOrDefault
                SingleTestExceptionMethod(x => new EntityQuery<MainEnitity>(x).LastOrDefault());
                SingleTestExceptionMethod(x => new EntityQuery<MainEnitity>(x).LastOrDefault(y => y.ColumnInt > 10));
                // ElementAt
                SingleTestExceptionMethod(x => new EntityQuery<MainEnitity>(x).ElementAt(5));
                // ElementAtOrDefault
                SingleTestExceptionMethod(x => new EntityQuery<MainEnitity>(x).ElementAtOrDefault(5));
            }
        }

        [TestMethod]
        public void QueryableAggregationTest()
        {
            {
                var countQuery = new SqlQuerySelect();
                countQuery.Select.Add(SqlFunction.Count(Literal("*")));
                countQuery.From.Add(GetMainQuery().Item1);

                (var countExpressionBaseQuery, var countExpressionBaseQueryTable) = GetMainQuery();
                countExpressionBaseQuery.Where.Add(IsGreater(countExpressionBaseQueryTable.ColumnInt, Argument(5)));
                var countExpressionQuery = new SqlQuerySelect();
                countExpressionQuery.Select.Add(SqlFunction.Count(Literal("*")));
                countExpressionQuery.From.Add(countExpressionBaseQuery);

                // Count
                {
                    SingleTest(countQuery, x => new EntityQuery<MainEnitity>(x).Count());
                    SingleTest(countExpressionQuery, x => new EntityQuery<MainEnitity>(x).Count(y => y.ColumnInt > 5));
                }
                // LongCount
                {
                    SingleTest(countQuery, x => new EntityQuery<MainEnitity>(x).LongCount());
                    SingleTest(countExpressionQuery, x => new EntityQuery<MainEnitity>(x).LongCount(y => y.ColumnInt > 5));
                }
            }
            // Sum
            {
                var q = new SqlQuerySelect();
                var t = q.From.Add<MainEnitityTable>();
                q.Select.Add(SqlFunction.Sum(t.ColumnInt));
                SingleTest(q, x => new EntityQuery<MainEnitity>(x).Sum(y => y.ColumnInt));
            }
            // Min
            {
                var q = new SqlQuerySelect();
                var t = q.From.Add<MainEnitityTable>();
                q.Select.Add(SqlFunction.Min(t.ColumnInt));
                SingleTest(q, x => new EntityQuery<MainEnitity>(x).Min(y => y.ColumnInt));
            }
            // Max
            {
                var q = new SqlQuerySelect();
                var t = q.From.Add<MainEnitityTable>();
                q.Select.Add(SqlFunction.Max(t.ColumnInt));
                SingleTest(q, x => new EntityQuery<MainEnitity>(x).Max(y => y.ColumnInt));
            }
            // Average
            {
                var q = new SqlQuerySelect();
                var t = q.From.Add<MainEnitityTable>();
                q.Select.Add(SqlFunction.Avg(t.ColumnInt));
                SingleTest(q, x => new EntityQuery<MainEnitity>(x).Average(y => y.ColumnInt));
            }
        }

        [TestMethod]
        public void QueryableQualifierTest()
        {
            // Any
            {
                var subquery = new SqlQuerySelect();
                var qt = subquery.From.Add<MainEnitityTable>();
                subquery.Select.Add(Argument(1));
                var query = new SqlQuerySelect();
                query.Select.Add(Exists((SqlExpression)subquery));

                SingleWithInitializationTest(query, x => new EntityQuery<MainEnitity>(x).Any(), x => Assert.AreEqual(false, x), new MainEnitity[] { });

                subquery.Where.Add(IsGreater(qt.ColumnInt, Argument(2)));
                SingleWithInitializationTest(query, x => new EntityQuery<MainEnitity>(x).Any(y => y.ColumnInt > 2), x => Assert.AreEqual(false, x), new MainEnitity[] { });

            }
            //All
            {
                var subquery = new SqlQuerySelect();
                var qt = subquery.From.Add<MainEnitityTable>();
                subquery.Select.Add(Argument(1));
                subquery.Where.Add(Not(IsGreater(qt.ColumnInt, Argument(2))));
                var query = new SqlQuerySelect();
                query.Select.Add(NotExists((SqlExpression)subquery));

                SingleWithInitializationTest(query, x => new EntityQuery<MainEnitity>(x).All(y => y.ColumnInt > 2), x => Assert.AreEqual(true, x), new MainEnitity[] { });

            }
            // Contains
            {

                var entity = new MainEnitity();
                var subquery = new SqlQuerySelect();
                var qt = subquery.From.Add<MainEnitityTable>();
                subquery.Select.Add(Argument(1));
                subquery.Where.AddEquals(qt.ColumnInt, Argument(2));
                var query = new SqlQuerySelect();
                query.Select.Add(Exists((SqlExpression)subquery));

                SingleWithInitializationTest(query, x => new EntityQuery<MainEnitity>(x).Select(y => y.ColumnInt).Contains(2), x => Assert.AreEqual(false, x), new MainEnitity[] { });
                SingleWithInitializationTest(query, x => new EntityQuery<MainEnitity>(x).Select(y => y.ColumnInt).Contains(2), x => Assert.AreEqual(true, x), new MainEnitity[] { new MainEnitity() { ColumnInt = 2 } });
                SingleWithInitializationTest(GetMainQuery().Item1, x => new EntityQuery<MainEnitity>(x).Contains(entity), x => Assert.AreEqual(true, x), new MainEnitity[] { entity });
                SingleWithInitializationTest(GetMainQuery().Item1, x => new EntityQuery<MainEnitity>(x).Contains(entity), x => Assert.AreEqual(false, x), new MainEnitity[] { });
                SingleTestExceptionMethod(x => new EntityQuery<MainEnitity>(x).Contains(entity, m_mainEntityEqualityComparer));

            }
        }

        [TestMethod]
        public void QueryableEqualityTest()
        {
            var source = new List<MainEnitity>();
            SingleTestExceptionMethod(x => new EntityQuery<MainEnitity>(x).SequenceEqual(source));
            SingleTestExceptionMethod(x => new EntityQuery<MainEnitity>(x).SequenceEqual(source, m_mainEntityEqualityComparer));
        }

        #endregion

        #region Helper methods

        private (SqlQuerySelect, MainEnitityTable) GetMainQuery()
        {
            var q = new SqlQuerySelect();
            var qt = q.From.Add<MainEnitityTable>();
            q.Select.Add(qt.Id);
            q.Select.Add(qt.ColumnInt);
            q.Select.Add(qt.ColumnString);
            q.Select.Add(qt.ColumnDatetime);
            q.Select.Add(qt.ColumnsBool);
            q.Select.Add(qt.FirstId);

            return (q, qt);
        }

        private void CollectionTest<TResult>(SqlQuerySelect querySelect, Func<DbLinkQueryProvider, IQueryable<TResult>> entityQuery, Action<IEnumerable<TResult>> resultAsserts = null)
        {
            StandardTestCore<object>(querySelect, x =>
            {
                var result = entityQuery(x).ToList();
                resultAsserts?.Invoke(result);
            });
        }
        private void CollectionWithInitializationTest<TResult, TEntity>(SqlQuerySelect querySelect, Func<DbLinkQueryProvider, IQueryable<TResult>> entityQuery, Action<IEnumerable<TResult>> resultAsserts = null, IEnumerable<TEntity> values = null)
            where TEntity : class
        {
            StandardTestCore(querySelect, x =>
            {
                var result = entityQuery(x).ToList();
                resultAsserts?.Invoke(result);
            }, values);
        }
        private void SingleTest<TResult>(SqlQuerySelect querySelect, Func<DbLinkQueryProvider, TResult> entityQuery, Action<TResult> resultAsserts = null)
        {
            StandardTestCore<object>(querySelect, x =>
            {
                var result = entityQuery(x);
                resultAsserts?.Invoke(result);
            });
        }
        private void SingleWithInitializationTest<TResult, TEntity>(SqlQuerySelect querySelect, Func<DbLinkQueryProvider, TResult> entityQuery, Action<TResult> resultAsserts = null, IEnumerable<TEntity> values = null)
            where TEntity : class
        {
            StandardTestCore(querySelect, x =>
            {
                var result = entityQuery(x);
                resultAsserts?.Invoke(result);
            }, values);
        }

        private void CollectionTestException<TResult>(Func<DbLinkQueryProvider, IQueryable<TResult>> entityQuery)
        {
            TestExceptionCore<NotSupportedException, object>(null, x => entityQuery(x).GetEnumerator().MoveNext());
        }
        private void SingleTestExceptionMethod<TResult>(Func<DbLinkQueryProvider, TResult> entityQuery)
        {
            TestExceptionCore<NotSupportedException, object>(null, x => entityQuery(x));
        }
        private void SingleTestExceptionResult<TResult, TEntity>(SqlQuerySelect querySelect, Func<DbLinkQueryProvider, TResult> entityQuery, IEnumerable<TEntity> values = null)
            where TEntity : class
        {
            TestExceptionCore<TargetInvocationException, TEntity>(querySelect, x => entityQuery(x), values);
        }

        private void StandardTestCore<TEntity>(SqlQuerySelect querySelect, Action<DbLinkQueryProvider> action, IEnumerable<TEntity> values = null)
             where TEntity : class
        {
            TestCore(querySelect, action, values);
        }
        private void TestExceptionCore<TException, TEntity>(SqlQuerySelect querySelect, Action<DbLinkQueryProvider> action, IEnumerable<TEntity> values = null)
            where TException : Exception
            where TEntity : class
        {
            TestCore(querySelect, x => AssertException.Throws<TException>(() => action(x)), values);
        }
        private void TestCore<TEntity>(SqlQuerySelect querySelect, Action<DbLinkQueryProvider> action, IEnumerable<TEntity> values = null)
            where TEntity : class
        {
            var querySelectString = querySelect?.ToString();

            using (var link = NpgsqlProvider.CreateLink())
            using (var transaction = link.Begin())
            {
                link.ExecuteNonQuery(DbStructureSql);
                if (values != null)
                    foreach (var item in values)
                        link.Insert(item);

                var provider = new DbLinkQueryProvider(link);
                if (querySelect != null)
                    provider.SqlQueryGenerated = s => Assert.AreEqual(querySelectString, s.ToString());

                action(provider);
                transaction.Rollback();
            }
        }

        #endregion

        #region Private classes

        [Table("tb_entity")]
        private class MainEnitity
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Column("column_int")]
            public int ColumnInt { get; set; }

            [Column("column_string")]
            public string ColumnString { get; set; }

            [Column("column_datetime")]
            public DateTime ColumnDatetime { get; set; }

            [Column("column_bool")]
            public bool ColumnsBool { get; set; }

            [Column("firstid")]
            public int? FirstId { get; set; }

            [ForeignKey(nameof(FirstId))]
            public virtual FirstEntity First { get; set; }

            public override bool Equals(object obj)
            {
                var entity = obj as MainEnitity;
                if (entity == null)
                    return false;

                return Id == entity.Id
                    && ColumnInt == entity.ColumnInt
                    && ColumnString == entity.ColumnString
                    && ColumnDatetime == entity.ColumnDatetime
                    && ColumnsBool == entity.ColumnsBool
                    && FirstId == entity.FirstId;
            }
            public override int GetHashCode()
            {
                unchecked
                {
                    var result = 0;
                    result = (result * 397) ^ Id;
                    result = (result * 397) ^ ColumnInt;
                    result = (result * 397) ^ ColumnString.GetHashCode();
                    result = (result * 397) ^ ColumnDatetime.GetHashCode();
                    if (FirstId.HasValue)
                        result = (result * 397) ^ FirstId.Value;

                    return result;
                }
            }
        }
        [Table("tb_firstentity")]
        private class FirstEntity
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Column("column_time")]
            public TimeSpan Time { get; set; }

            [Column("secondid")]
            public int? SecondId { get; set; }

            [ForeignKey(nameof(SecondId))]
            public virtual SecondEntity Second { get; set; }

            [InverseProperty(nameof(MainEnitity))]
            public virtual ICollection<MainEnitity> Mains { get; set; }
        }
        [Table("tb_secondentity")]
        private class SecondEntity
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Column("column_string")]
            public string StringProperty { get; set; }

            [InverseProperty(nameof(FirstEntity))]
            public virtual ICollection<FirstEntity> FristModels { get; set; }
        }

        private class MainEnitityTable : SqlTable
        {
            public SqlColumn Id => Column("id");
            public SqlColumn ColumnInt => Column("column_int");
            public SqlColumn ColumnString => Column("column_string");
            public SqlColumn ColumnDatetime => Column("column_datetime");
            public SqlColumn ColumnsBool => Column("column_bool");
            public SqlColumn FirstId => Column("firstid");

            public MainEnitityTable() : base("tb_entity") { }
        }
        private class FirstEntityTable : SqlTable
        {
            public SqlColumn Id => Column("id");
            public SqlColumn Time => Column("column_time");
            public SqlColumn SecondId => Column("secondid");

            public FirstEntityTable() : base("tb_firstentity") { }
        }
        private class SecondEntityTable : SqlTable
        {
            public SqlColumn Id => Column("id");
            public SqlColumn StringProperty => Column("column_string");

            public SecondEntityTable() : base("tb_secondentity") { }
        }

        private class MainEnitityEqualityComparer : IEqualityComparer<MainEnitity>
        {
            public bool Equals(MainEnitity stringA, MainEnitity stringB) => true;
            public int GetHashCode(MainEnitity obj) => 1;
        }
        private class IntegerComparer : IComparer<int>
        {
            public int Compare(int stringA, int stringB) => 1;
        }
        private class StringComparer : IComparer<string>
        {
            public int Compare(string stringA, string stringB) => 1;
        }

        #endregion
    }
}
