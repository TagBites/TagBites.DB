using System.Text;
using TagBites.DB;
using Xunit;

namespace TagBites.Sql
{
    public class SqlQueryParserTests
    {
        [Theory]
        [InlineData("select $$@NOARG$$;")]
        [InlineData("select $tag$@NOARG$tag$;")]
        [InlineData("select $tag$ sd @NOARG sd $tag$;")]
        [InlineData("select '@NOARG'")]
        [InlineData("select '''@NOARG'''")]
        [InlineData("select E'\\'@NOARG'")]
        [InlineData("select E'@NOARG'")]
        [InlineData("select \"@NOARG\"")]
        [InlineData("select \"\"\"@NOARG\"\"\"")]
        [InlineData("select '' || @ARG || '';", "select '' || 'ARGUMENT' || '';")]
        [InlineData("select @ARG||'String constant @NOARG';", "select 'ARGUMENT'||'String constant @NOARG';")]
        [InlineData("select @ARG ;--@NOARG @NOARG;", "select 'ARGUMENT' ;--@NOARG @NOARG;")]
        [InlineData("select @ARG /*@NOARG @NOARG", "select 'ARGUMENT' /*@NOARG @NOARG")]
        public void ResolveSqlQueryParametersTest(string testQuery, string expected = null)
        {
            Assert.Equal(expected ?? testQuery, Test(testQuery));

            static string Test(string s)
            {
                var sb = new StringBuilder();
                Query.ResolveSqlQueryParameters(sb, s, '@', x =>
                {
                    Assert.Equal("ARG", x);
                    return "'ARGUMENT'";
                });
                return sb.ToString();
            }
        }
    }
}
