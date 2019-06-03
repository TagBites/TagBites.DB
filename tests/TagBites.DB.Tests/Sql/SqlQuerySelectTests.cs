using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBS.Data.DB;
using TBS.Data.UnitTests.DB;
using TBS.Sql;
using static TBS.Sql.SqlExpression;

namespace TBS.Data.UnitTests.Sql
{
    [TestClass]
    public class SqlQuerySelectTests : DbTestBase
    {
        [TestMethod]
        public void GeneralTest()
        {
            var t1 = new SqlQueryValues();
            t1.Values.Add(1, 2);
            t1.Values.Add(1, 3);

            var t2 = new SqlQueryValues();
            t2.Values.Add(2, 3);
            t2.Values.Add(2, 4);
            t2.Values.Add(2, 4);
            t2.Values.Add(2, 5);

            var q = new SqlQuerySelect();
            q.With.Add("t1", new[] { "a", "b" }, t1);
            q.With.Add("t2", new[] { "b", "c" }, t2);
            q.Distinct.Enabled = true;

            var tf = q.From.Add("t1");
            var tj = q.Join.AddOn(SqlClauseJoinEntryType.InnerJoin, "t2", "b", tf, "b");

            q.Select.Add(tf.Column("a"));
            q.Select.Add(tf.Column("b"));
            q.Select.Add(tj.Column("c"));
            q.Select.Add(tf.Column("a") + tj.Column("c"));

            q.Where.Add(AreNotEquals(tj.Column("c"), Argument(5)));
            q.OrderBy.Add(tf.Column("a") + tj.Column("c"), SqlClauseOrderByEntryType.Ascending);

            q.Offset = 1;
            q.Limit = 3;

            using (var link = CreateLink())
            {
                var r = link.Execute(q);
                Assert.AreEqual(1, r.RowCount);
                Assert.AreEqual(4, r.ColumnCount);
                Assert.AreEqual(4, r.GetValue<int>(0, "c"));
            }
        }

        [TestMethod]
        public void WithRecursiveTest()
        {
            const int count = 10;

            var qrr = new SqlQuerySelect();
            qrr.Select.Add(One);

            var qrn = new SqlQuerySelect();
            var qrnf = qrn.From.Add("a");
            qrn.Select.Add(qrnf.Column("i") + One);
            qrn.Where.Add(qrnf.Column("i") < Argument(count));

            var q = new SqlQuerySelect();
            var w = q.With.Add("a", new[] { "i" }, qrr);
            w.Recursive(qrn, SqlClauseUnionEntryType.All);
            q.From.Add(w);
            q.Select.AddAll();

            using (var link = CreateLink())
            {
                var sum = link.ExecuteColumnScalars<int>(q).Sum();
                Assert.AreEqual((1 + count) * count / 2, sum);
            }
        }

        [TestMethod]
        public void DistinctTest()
        {
            var qv = new SqlQueryValues();
            qv.Values.Add(1, 1);
            qv.Values.Add(1, 2);
            qv.Values.Add(1, 2);

            var q0 = new SqlQuerySelect();
            q0.Select.AddAll();
            q0.From.Add(qv, "qv", new[] { "a", "b" });

            var q1 = new SqlQuerySelect();
            q1.Distinct.Enabled = true;
            q1.Select.AddAll();
            q1.From.Add(qv, "qv", new[] { "a", "b" });

            var q2 = new SqlQuerySelect();
            q2.Distinct.Add(Literal("a"));
            q2.Select.AddAll();
            q2.From.Add(qv, "qv", new[] { "a", "b" });

            var q3 = new SqlQuerySelect();
            q3.Distinct.Add(Literal("a"));
            q3.Distinct.Add(Literal("b"));
            q3.Distinct.Enabled = true;
            q3.Select.AddAll();
            q3.From.Add(qv, "qv", new[] { "a", "b" });

            using (var link = CreateLink())
            {
                var result0 = link.Execute(q0);
                Assert.AreEqual(3, result0.RowCount);

                var result1 = link.Execute(q1);
                Assert.AreEqual(2, result1.RowCount);

                //var result2 = link.Execute(q2);
                //Assert.AreEqual(1, result2.RowCount);

                //var result3 = link.Execute(q3);
                //Assert.AreEqual(2, result3.RowCount);
            }
        }

        [TestMethod]
        public void InTest()
        {
            Assert.AreEqual(true, ExecuteScalar<bool>(In(Argument(1), new[] { 1, 2 })));
            Assert.AreEqual(false, ExecuteScalar<bool>(In(Argument(3), new[] { 1, 2 })));
            Assert.AreEqual(false, ExecuteScalar<bool>(In(Argument(3), new int[0])));

            Assert.AreEqual(true, ExecuteScalar<bool>(In(Argument(DateTime.Today), new[] { Argument(DateTime.Now), Argument(DateTime.Today) })));
            Assert.AreEqual(false, ExecuteScalar<bool>(In(Argument(DateTime.Today.AddDays(2)), new[] { Argument(DateTime.Now), Argument(DateTime.Today) })));
        }

        [TestMethod]
        public void DistinctFromTest()
        {
            Assert.AreEqual(true, ExecuteScalar<bool>(AreDistinct(Argument(1), Null)));
            Assert.AreEqual(true, ExecuteScalar<bool>(AreDistinct(Argument(1), Argument(2))));
            Assert.AreEqual(false, ExecuteScalar<bool>(AreDistinct(Argument(1), Argument(1))));
            Assert.AreEqual(false, ExecuteScalar<bool>(AreNotDistinct(Argument(1), Null)));
            Assert.AreEqual(false, ExecuteScalar<bool>(AreNotDistinct(Argument(1), Argument(2))));
            Assert.AreEqual(true, ExecuteScalar<bool>(AreNotDistinct(Argument(1), Argument(1))));
        }

        [TestMethod]
        public void CastTest()
        {
            Assert.AreEqual(null, ExecuteScalar<string>(Cast(Null, typeof(string))));
            Assert.AreEqual("1", ExecuteScalar<string>(Cast(One, typeof(string))));
        }
    }
}
