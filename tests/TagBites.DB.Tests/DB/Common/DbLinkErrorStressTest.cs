﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TagBites.DB.Tests.DB.Core;
using Xunit;

namespace TagBites.DB.Tests.DB.Common
{
    public class DbLinkErrorStressTest : DbTestBase
    {
        [Fact]
        public void ConnectionStaysOpenAfterQueryExceptionTest()
        {
            using (var link = CreateLink())
            {
                try
                {
                    link.ExecuteNonQuery("SELECT a");
                    Assert.True(false, "Connection was break.");
                }
                catch { }

                Assert.Equal(1, link.ExecuteScalar<int>("SELECT 1"));
            }
        }

        [Fact]
        public void TransactionFailsAfterQueryExceptionTest()
        {
            using (var link = CreateLink())
            using (var transaction = link.Begin())
            {
                try
                {
                    link.ExecuteNonQuery("SELECT a");
                    Assert.True(false);
                }
                catch { }

                Assert.Equal(DbLinkTransactionStatus.RollingBack, link.TransactionStatus);

                try
                {
                    link.ExecuteNonQuery("SELECT 1");
                    Assert.True(false);
                }
                catch { }

                try
                {
                    transaction.Commit();
                    Assert.True(false);
                }
                catch { }
            }
        }

        [Fact]
        public void TransactionFailsWhenConnectionBreaksTest()
        {
            using (var link = CreateLink())
            using (var transaction = link.Begin())
            {
                Assert.Equal(1, link.ExecuteScalar<int>("SELECT 1"));

                ((DbLinkContext)link.ConnectionContext).GetOpenConnection().Close();

                try
                {
                    link.ExecuteNonQuery("SELECT 1");
                    Assert.True(false, "Connection was break.");
                }
                catch { }

                try
                {
                    transaction.Commit();
                    Assert.True(false, "Connection was break.");
                }
                catch { }
            }
        }

        [Fact]
        public void ErrorInQueryNotBreaksTheConnection()
        {
            using (var link = CreateLink())
            {
                try
                {
                    link.ExecuteNonQuery("SELECT a");
                    Assert.True(false);
                }
                catch { }

                link.ExecuteNonQuery("SELECT 1");
                ((DbLinkContext)link.ConnectionContext).GetOpenConnection().Close();
                link.ExecuteNonQuery("SELECT 1");
            }
        }

        [Fact]
        public void AutoReopenConnection()
        {
            using (var link = CreateLink())
            {
                link.ExecuteNonQuery("SELECT 1");
                ((DbLinkContext)link.ConnectionContext).GetOpenConnection().Close();

                using (var link2 = NpgsqlProvider.CreateLink())
                    DbLinkExtensions.ExecuteNonQuery(link2, "SELECT 1");
            }
        }
    }
}
