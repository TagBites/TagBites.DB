using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using TagBites.Sql;
using Xunit;
using static TagBites.Sql.SqlExpression;

namespace TagBites.DB.Entity
{
    public class EntityQueryTests : DbTests
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


        #region Base

        [Fact]
        public void QueryableBaseTest()
        {
            CollectionTest(GetMainQuery().Item1, x => new EntityQueryable<MainEnitity>(x));
        }

        #endregion

        #region Generation

        [Fact]
        public void Generation_DefaultIfEmpty_CorrectResult()
        {
            var (q, t) = GetMainQuery();
            var entity = new MainEnitity() { Id = 2 };
            CollectionTest(q, x => new EntityQueryable<MainEnitity>(x).DefaultIfEmpty(), x => Assert.Null(x.Single()));
            CollectionTest(q, x => new EntityQueryable<MainEnitity>(x).DefaultIfEmpty(entity), x => Assert.Equal(x.Single(), entity));
        }

        #endregion

        #region Filtering

        [Fact]
        public void Filtering_Where_CorrectResult()
        {

            var date = new DateTime(2012, 12, 12);
            (var q, var t) = GetMainQuery();
            q.Where.Add(Or(
                AreEquals(t.ColumnInt, Argument(1)),
                And(
                    Or(AreNotEquals(t.ColumnString, Argument("test")), ToCondition(t.ColumnsBool)),
                    IsLessOrEqual(t.ColumnDatetime, Argument(date)))));
            CollectionTest(q, x => new EntityQueryable<MainEnitity>(x).Where(y => y.ColumnInt == 1 || ((y.ColumnString != "test" || y.ColumnsBool) && y.ColumnDatetime <= date)));
            CollectionTestException(x => new EntityQueryable<MainEnitity>(x).Where((y, i) => y.ColumnString != "test"));

        }

        [Fact]
        public void Filtering_OfType_CorrectResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Mapping

        [Fact]
        public void Mapping_Select_CorrectResult()
        {
            var select1Query = new SqlQuerySelect();
            select1Query.Select.Add(select1Query.From.Add("public.tb_entity"), "column_int");
            CollectionTest(select1Query, x => new EntityQueryable<MainEnitity>(x).Select(y => y.ColumnInt));

            //var select2Query = new SqlQuerySelect();
            //var select2Table = select2Query.From.Add("public.tb_entity");
            //select2Query.Select.Add(SqlExpression.LiteralExpression("{0}+{1}", SqlExpression.Column(select2Table, "column_int"), SqlExpression.Column(select2Table, "column_int")));
            //CollectionTest(select2Query, x => new EntityQuery<MainEnitity>(x).Select(y => y.ColumnInt + y.ColumnInt));

            var selectAnonymousQuery = new SqlQuerySelect();
            var selectAnonymousTable = selectAnonymousQuery.From.Add<MainEnitityTable>();
            selectAnonymousQuery.Select.Add(selectAnonymousTable.FirstId);
            CollectionTest(selectAnonymousQuery, x => new EntityQueryable<MainEnitity>(x)
                .Select(y => new { y.FirstId, y.ColumnDatetime, y.ColumnsBool })
                .Select(y => new { y.FirstId, y.ColumnsBool })
                .Select(y => new { y.FirstId }));

            selectAnonymousQuery.Select.Add(selectAnonymousTable.ColumnsBool);
            CollectionTest(selectAnonymousQuery, x => new EntityQueryable<MainEnitity>(x)
                .Select(y => new { y.FirstId, y.ColumnDatetime, y.ColumnsBool })
                .Select(y => new { y.FirstId, y.ColumnsBool }));

            selectAnonymousQuery.Select.Add(selectAnonymousTable.ColumnDatetime);
            CollectionTest(selectAnonymousQuery, x => new EntityQueryable<MainEnitity>(x)
                .Select(y => new { y.FirstId, y.ColumnDatetime, y.ColumnsBool }));

            // Select - index
            CollectionTestException(x => new EntityQueryable<MainEnitity>(x).Select((y, i) => y.ColumnString != "test"));
        }

        #endregion

        #region Join

        [Fact]
        public void Join_Join_CorrectResult()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Join_GroupJoin_CorrectResult()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Join_SelectMany_CorrectResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Concatenation

        [Fact]
        public void Concatenation_Concat_CorrectResult()
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
                var first = new EntityQueryable<MainEnitity>(x).Where(y => y.ColumnInt < 100);
                var second = new EntityQueryable<MainEnitity>(x).Where(y => y.ColumnInt > 10);
                return first.Concat(second);
            });
        }

        #endregion

        #region Set

        [Fact]
        public void Set_Distinct_CorrectResult()
        {
            throw new NotImplementedException();

            //(var q, var t) = GetMainQuery();
            //q.Distinct.Enabled = true;
            //CollectionTest(q, x => new EntityQuery<MainEnitity>(x).Distinct());
            //CollectionTestException(x => new EntityQuery<MainEnitity>(x).Distinct(m_mainEntityEqualityComparer));
        }

        [Fact]
        public void Set_GroupBy_CorrectResult()
        {
            var (q, t) = GetMainQuery();
            q.OrderBy.Add(t.ColumnInt, SqlClauseOrderByEntryType.Ascending);
            CollectionTest(q, x => new EntityQueryable<MainEnitity>(x).GroupBy(y => y.ColumnInt));
        }

        [Fact]
        public void Set_Union_CorrectResult()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Set_Intersect_CorrectResult()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Set__CorrectResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Convolution

        [Fact]
        public void Convolution_Zip_CorrectResult()
        {
            var collection = new List<int>();
            CollectionTestException(x => new EntityQueryable<MainEnitity>(x).Zip(collection, (y, z) => new MainEnitity()));
        }
        #endregion

        #region Partitioning

        [Fact]
        public void Partitioning_SkipTake_CorrectResult()
        {
            (var q, var t) = GetMainQuery();
            q.Limit = 10;
            q.Offset = 5;
            CollectionTest(q, x => new EntityQueryable<MainEnitity>(x).Skip(5).Take(10));
        }

        #endregion

        #region Ordering

        [Fact]
        public void Ordering_OrderBy_CorrectResult()
        {
            var (q, t) = GetMainQuery();
            q.OrderBy.Add(t.ColumnInt, SqlClauseOrderByEntryType.Ascending);
            CollectionTest(q, x => new EntityQueryable<MainEnitity>(x).OrderBy(y => y.ColumnInt));

            CollectionTestException(x => new EntityQueryable<MainEnitity>(x).OrderBy(y => y.ColumnInt, m_integerComparer));
        }

        [Fact]
        public void Ordering_ThenBy_CorrectResult()
        {
            var (q, t) = GetMainQuery();
            q.OrderBy.Add(t.ColumnInt, SqlClauseOrderByEntryType.Ascending);
            q.OrderBy.Add(t.ColumnString, SqlClauseOrderByEntryType.Ascending);
            CollectionTest(q, x => new EntityQueryable<MainEnitity>(x).OrderBy(y => y.ColumnInt).ThenBy(y => y.ColumnString));

            CollectionTestException(x => new EntityQueryable<MainEnitity>(x).OrderBy(y => y.ColumnInt, m_integerComparer).ThenBy(y => y.ColumnString, m_stringComparer));
        }

        [Fact]
        public void Ordering_OrderByDescending_CorrectResult()
        {
            var (q, t) = GetMainQuery();
            q.OrderBy.Add(t.ColumnInt, SqlClauseOrderByEntryType.Descending);
            CollectionTest(q, x => new EntityQueryable<MainEnitity>(x).OrderByDescending(y => y.ColumnInt));

            CollectionTestException(x => new EntityQueryable<MainEnitity>(x).OrderByDescending(y => y.ColumnInt, m_integerComparer).ThenBy(y => y.ColumnString, m_stringComparer));
        }

        [Fact]
        public void Ordering_ThenByDescending_CorrectResult()
        {
            var (q, t) = GetMainQuery();
            q.OrderBy.Add(t.ColumnInt, SqlClauseOrderByEntryType.Descending);
            q.OrderBy.Add(t.ColumnString, SqlClauseOrderByEntryType.Descending);
            CollectionTest(q, x => new EntityQueryable<MainEnitity>(x).OrderByDescending(y => y.ColumnInt).ThenByDescending(y => y.ColumnString));

            CollectionTestException(x => new EntityQueryable<MainEnitity>(x).OrderByDescending(y => y.ColumnInt, m_integerComparer).ThenByDescending(y => y.ColumnString, m_stringComparer));
        }

        [Fact]
        public void Ordering_Reverse_CorrectResult()
        {
            CollectionTestException(x => new EntityQueryable<MainEnitity>(x).Reverse());
        }

        #endregion

        #region Conversion

        [Fact]
        public void Conversion_Cast_CorrectResult()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void Conversion_AsQueryable_CorrectResult()
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Element

        [Fact]
        public void Element_First_CorrectResult()
        {
            var entity = new MainEnitity() { ColumnInt = 12 };

            // Queries
            (var firstQuery, var firstQueryTable) = GetMainQuery();
            firstQuery.Limit = 1;

            (var firstExpressionQuery, var firstQueryExpressionTable) = GetMainQuery();
            firstExpressionQuery.Where.Add(IsGreater(firstQueryExpressionTable.ColumnInt, Argument(10)));
            firstExpressionQuery.Limit = 1;

            SingleWithInitializationTest(firstQuery, x => new EntityQueryable<MainEnitity>(x).First(), x => Assert.Equal(entity, x), new MainEnitity[] { entity });
            SingleTestExceptionResult(firstQuery, x => new EntityQueryable<MainEnitity>(x).First(), new MainEnitity[0]);
            SingleWithInitializationTest(firstExpressionQuery, x => new EntityQueryable<MainEnitity>(x).First(y => y.ColumnInt > 10), x => Assert.Equal(entity, x), new MainEnitity[] { entity });
        }

        [Fact]
        public void Element_FirstOrDefault_CorrectResult()
        {
            var entity = new MainEnitity() { ColumnInt = 12 };

            // Queries
            var (firstQuery, firstQueryTable) = GetMainQuery();
            firstQuery.Limit = 1;

            var (firstExpressionQuery, firstQueryExpressionTable) = GetMainQuery();
            firstExpressionQuery.Where.Add(IsGreater(firstQueryExpressionTable.ColumnInt, Argument(10)));
            firstExpressionQuery.Limit = 1;

            SingleTest(firstQuery, x => new EntityQueryable<MainEnitity>(x).FirstOrDefault(), Assert.Null);
            SingleWithInitializationTest(firstQuery, x => new EntityQueryable<MainEnitity>(x).FirstOrDefault(), x => Assert.Equal(entity, x), new MainEnitity[] { entity });
            SingleTest(firstExpressionQuery, x => new EntityQueryable<MainEnitity>(x).FirstOrDefault(y => y.ColumnInt > 10), Assert.Null);
            SingleWithInitializationTest(firstExpressionQuery, x => new EntityQueryable<MainEnitity>(x).FirstOrDefault(y => y.ColumnInt > 10), x => Assert.Equal(entity, x), new MainEnitity[] { entity });
        }

        [Fact]
        public void Element_Single_CorrectResult()
        {
            var entity = new MainEnitity() { ColumnInt = 12 };

            // Queries
            (var singleQuery, var singleQueryTable) = GetMainQuery();
            singleQuery.Limit = 2;

            (var singleExpressionQuery, var singleExpressionQueryTable) = GetMainQuery();
            singleExpressionQuery.Where.Add(IsGreater(singleExpressionQueryTable.ColumnInt, Argument(10)));
            singleExpressionQuery.Limit = 2;

            SingleWithInitializationTest(singleQuery, x => new EntityQueryable<MainEnitity>(x).Single(), x => Assert.Equal(entity, x), new MainEnitity[] { entity });
            SingleTestExceptionResult(singleQuery, x => new EntityQueryable<MainEnitity>(x).Single(), new MainEnitity[0]);
            SingleTestExceptionResult(singleQuery, x => new EntityQueryable<MainEnitity>(x).Single(), new MainEnitity[] { entity, entity });
            SingleWithInitializationTest(singleExpressionQuery, x => new EntityQueryable<MainEnitity>(x).Single(y => y.ColumnInt > 10), x => Assert.Equal(entity, x), new MainEnitity[] { entity });

        }

        [Fact]
        public void Element_SingleOrDefault_CorrectResult()
        {
            var entity = new MainEnitity() { ColumnInt = 12 };

            // Queries
            (var singleQuery, var singleQueryTable) = GetMainQuery();
            singleQuery.Limit = 2;

            (var singleExpressionQuery, var singleExpressionQueryTable) = GetMainQuery();
            singleExpressionQuery.Where.Add(IsGreater(singleExpressionQueryTable.ColumnInt, Argument(10)));
            singleExpressionQuery.Limit = 2;

            SingleTest(singleQuery, x => new EntityQueryable<MainEnitity>(x).SingleOrDefault());
            SingleTest(singleExpressionQuery, x => new EntityQueryable<MainEnitity>(x).SingleOrDefault(y => y.ColumnInt > 10));
        }

        [Fact]
        public void Element_Last_CorrectResult()
        {
            SingleTestExceptionMethod(x => new EntityQueryable<MainEnitity>(x).Last());
            SingleTestExceptionMethod(x => new EntityQueryable<MainEnitity>(x).Last(y => y.ColumnInt > 10));
        }

        [Fact]
        public void Element_LastOrDefault_CorrectResult()
        {
            SingleTestExceptionMethod(x => new EntityQueryable<MainEnitity>(x).LastOrDefault());
            SingleTestExceptionMethod(x => new EntityQueryable<MainEnitity>(x).LastOrDefault(y => y.ColumnInt > 10));
        }

        [Fact]
        public void Element_ElementAtt_CorrectResult()
        {
            SingleTestExceptionMethod(x => new EntityQueryable<MainEnitity>(x).ElementAt(5));
        }

        [Fact]
        public void Element_ElementAtOrDefault_CorrectResult()
        {
            SingleTestExceptionMethod(x => new EntityQueryable<MainEnitity>(x).ElementAtOrDefault(5));
        }

        #endregion

        #region Aggregation

        [Fact]
        public void Aggregation_Sum_CorrectResult()
        {
            var q = new SqlQuerySelect();
            var t = q.From.Add<MainEnitityTable>();
            q.Select.Add(SqlFunction.Sum(t.ColumnInt));

            SingleTest(q, x => new EntityQueryable<MainEnitity>(x).Sum(y => y.ColumnInt));
        }

        [Fact]
        public void Aggregation_Min_CorrectResult()
        {
            var q = new SqlQuerySelect();
            var t = q.From.Add<MainEnitityTable>();
            q.Select.Add(SqlFunction.Min(t.ColumnInt));

            SingleTest(q, x => new EntityQueryable<MainEnitity>(x).Min(y => y.ColumnInt));
        }

        [Fact]
        public void Aggregation_Max_CorrectResult()
        {
            var q = new SqlQuerySelect();
            var t = q.From.Add<MainEnitityTable>();
            q.Select.Add(SqlFunction.Max(t.ColumnInt));

            SingleTest(q, x => new EntityQueryable<MainEnitity>(x).Max(y => y.ColumnInt));
        }

        [Fact]
        public void Aggregation_Average_CorrectResult()
        {
            var q = new SqlQuerySelect();
            var t = q.From.Add<MainEnitityTable>();
            q.Select.Add(SqlFunction.Avg(t.ColumnInt));

            SingleTest(q, x => new EntityQueryable<MainEnitity>(x).Average(y => y.ColumnInt));
        }

        [Fact]
        public void Aggregation_Count_CorrectResult()
        {
            var countQuery = new SqlQuerySelect();
            countQuery.Select.Add(SqlFunction.Count(Literal("*")));
            countQuery.From.Add(GetMainQuery().Item1);

            var (countExpressionBaseQuery, countExpressionBaseQueryTable) = GetMainQuery();
            countExpressionBaseQuery.Where.Add(IsGreater(countExpressionBaseQueryTable.ColumnInt, Argument(5)));
            var countExpressionQuery = new SqlQuerySelect();
            countExpressionQuery.Select.Add(SqlFunction.Count(Literal("*")));
            countExpressionQuery.From.Add(countExpressionBaseQuery);

            SingleTest(countQuery, x => new EntityQueryable<MainEnitity>(x).Count());
            SingleTest(countExpressionQuery, x => new EntityQueryable<MainEnitity>(x).Count(y => y.ColumnInt > 5));

            SingleTest(countQuery, x => new EntityQueryable<MainEnitity>(x).LongCount());
            SingleTest(countExpressionQuery, x => new EntityQueryable<MainEnitity>(x).LongCount(y => y.ColumnInt > 5));
        }

        #endregion

        #region Qualifier

        [Fact]
        public void Qualifier_Any_CorrectResult()
        {
            var subQuery = new SqlQuerySelect();
            var qt = subQuery.From.Add<MainEnitityTable>();
            subQuery.Select.Add(Argument(1));
            var query = new SqlQuerySelect();
            query.Select.Add(Exists((SqlExpression)subQuery));

            SingleWithInitializationTest(query, x => new EntityQueryable<MainEnitity>(x).Any(), Assert.False, new MainEnitity[] { });

            subQuery.Where.Add(IsGreater(qt.ColumnInt, Argument(2)));
            SingleWithInitializationTest(query, x => new EntityQueryable<MainEnitity>(x).Any(y => y.ColumnInt > 2), Assert.False, new MainEnitity[] { });
        }

        [Fact]
        public void Qualifier_All_CorrectResult()
        {
            var subquery = new SqlQuerySelect();
            var qt = subquery.From.Add<MainEnitityTable>();
            subquery.Select.Add(Argument(1));
            subquery.Where.Add(Not(IsGreater(qt.ColumnInt, Argument(2))));
            var query = new SqlQuerySelect();
            query.Select.Add(NotExists((SqlExpression)subquery));

            SingleWithInitializationTest(query, x => new EntityQueryable<MainEnitity>(x).All(y => y.ColumnInt > 2), Assert.True, new MainEnitity[] { });
        }

        [Fact]
        public void Qualifier_Contains_CorrectResult()
        {
            var entity = new MainEnitity();
            var subquery = new SqlQuerySelect();
            var qt = subquery.From.Add<MainEnitityTable>();
            subquery.Select.Add(Argument(1));
            subquery.Where.AddEquals(qt.ColumnInt, Argument(2));
            var query = new SqlQuerySelect();
            query.Select.Add(Exists((SqlExpression)subquery));

            SingleWithInitializationTest(query, x => new EntityQueryable<MainEnitity>(x).Select(y => y.ColumnInt).Contains(2), Assert.False, new MainEnitity[] { });
            SingleWithInitializationTest(query, x => new EntityQueryable<MainEnitity>(x).Select(y => y.ColumnInt).Contains(2), Assert.True, new MainEnitity[] { new MainEnitity() { ColumnInt = 2 } });
            SingleWithInitializationTest(GetMainQuery().Item1, x => new EntityQueryable<MainEnitity>(x).Contains(entity), Assert.True, new MainEnitity[] { entity });
            SingleWithInitializationTest(GetMainQuery().Item1, x => new EntityQueryable<MainEnitity>(x).Contains(entity), Assert.False, new MainEnitity[] { });
            SingleTestExceptionMethod(x => new EntityQueryable<MainEnitity>(x).Contains(entity, m_mainEntityEqualityComparer));
        }

        #endregion

        #region Equality

        [Fact]
        public void Equality_SequenceEqual_CorrectResult()
        {
            var source = new List<MainEnitity>();
            SingleTestExceptionMethod(x => new EntityQueryable<MainEnitity>(x).SequenceEqual(source));
            SingleTestExceptionMethod(x => new EntityQueryable<MainEnitity>(x).SequenceEqual(source, m_mainEntityEqualityComparer));
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

        private void CollectionTest<TResult>(SqlQuerySelect querySelect, Func<EntityQueryProvider, IQueryable<TResult>> entityQuery, Action<IEnumerable<TResult>> resultAsserts = null)
        {
            StandardTestCore<object>(querySelect, x =>
            {
                var result = entityQuery(x).ToList();
                resultAsserts?.Invoke(result);
            });
        }
        private void CollectionWithInitializationTest<TResult, TEntity>(SqlQuerySelect querySelect, Func<EntityQueryProvider, IQueryable<TResult>> entityQuery, Action<IEnumerable<TResult>> resultAsserts = null, IEnumerable<TEntity> values = null)
            where TEntity : class
        {
            StandardTestCore(querySelect, x =>
            {
                var result = entityQuery(x).ToList();
                resultAsserts?.Invoke(result);
            }, values);
        }
        private void SingleTest<TResult>(SqlQuerySelect querySelect, Func<EntityQueryProvider, TResult> entityQuery, Action<TResult> resultAsserts = null)
        {
            StandardTestCore<object>(querySelect, x =>
            {
                var result = entityQuery(x);
                resultAsserts?.Invoke(result);
            });
        }
        private void SingleWithInitializationTest<TResult, TEntity>(SqlQuerySelect querySelect, Func<EntityQueryProvider, TResult> entityQuery, Action<TResult> resultAsserts = null, IEnumerable<TEntity> values = null)
            where TEntity : class
        {
            StandardTestCore(querySelect, x =>
            {
                var result = entityQuery(x);
                resultAsserts?.Invoke(result);
            }, values);
        }

        private void CollectionTestException<TResult>(Func<EntityQueryProvider, IQueryable<TResult>> entityQuery)
        {
            TestExceptionCore<NotSupportedException, object>(null, x => entityQuery(x).GetEnumerator().MoveNext());
        }
        private void SingleTestExceptionMethod<TResult>(Func<EntityQueryProvider, TResult> entityQuery)
        {
            TestExceptionCore<NotSupportedException, object>(null, x => entityQuery(x));
        }
        private void SingleTestExceptionResult<TResult, TEntity>(SqlQuerySelect querySelect, Func<EntityQueryProvider, TResult> entityQuery, IEnumerable<TEntity> values = null)
            where TEntity : class
        {
            TestExceptionCore<TargetInvocationException, TEntity>(querySelect, x => entityQuery(x), values);
        }

        private void StandardTestCore<TEntity>(SqlQuerySelect querySelect, Action<EntityQueryProvider> action, IEnumerable<TEntity> values = null)
             where TEntity : class
        {
            TestCore(querySelect, action, values);
        }
        private void TestExceptionCore<TException, TEntity>(SqlQuerySelect querySelect, Action<EntityQueryProvider> action, IEnumerable<TEntity> values = null)
            where TException : Exception
            where TEntity : class
        {
            TestCore(querySelect, x => Assert.Throws<TException>(() => action(x)), values);
        }
        private void TestCore<TEntity>(SqlQuerySelect querySelect, Action<EntityQueryProvider> action, IEnumerable<TEntity> values = null)
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

                var provider = new EntityQueryProvider(link);
                if (querySelect != null)
                    provider.SqlQueryGenerated = s => Assert.Equal(querySelectString, s.ToString());

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
