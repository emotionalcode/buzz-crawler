using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using BuzzCrawler.Infrastructure;

namespace BuzzCrawler.Tests
{
    public class QueryStringHelperTest
    {
        [Fact]
        public void FillQueryStringTest_firstSequence()
        {
            var url = QueryStringHelper.FillQueryString("http://test.com?q1=", "q1", "value");
            url.Should().BeEquivalentTo("http://test.com?q1=value");
        }

        [Fact]
        public void FillQueryStringTest_notFirstSequence()
        {
            var url = QueryStringHelper.FillQueryString("http://test.com?q1=&q2=", "q2", "value");
            url.Should().BeEquivalentTo("http://test.com?q1=&q2=value");
        }

        [Fact]
        public void RemoveQueryStringByKey_duplicated()
        {
            var url = QueryStringHelper.RemoveQueryStringByKey("http://test.com/test.php?q1=a&q1=a&q2=", "q2");
            url.Should().BeEquivalentTo("http://test.com/test.php?q1=a");
        }

        [Fact]
        public void RemoveQueryStringByKey_targetKeyIsDuplicated()
        {
            var url = QueryStringHelper.RemoveQueryStringByKey("http://test.com/test.php?q1=a&q1=b&q2=c", "q1");
            url.Should().BeEquivalentTo("http://test.com/test.php?q2=c");
        }
    }
}
