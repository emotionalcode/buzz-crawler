using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace BuzzCrawler.Tests
{
    public class BuzzCrawleTargetTest
    {
        [Fact]
        public void ArticleUrl_Getter()
        {
            var target = new BBSCrawleTarget(
                "http://www.discuzsample.com/view.php?forumId=&articleNo=",
                "articleNo",
                "http://www.discuzsample.com/list.php?forumId=&pageNo=",
                "pageNo",
                "testForum",
                "forumId");

            target.ArticleUrl.Should().Be("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=");
        }

        [Fact]
        public void ArticleListUrl_Getter()
        {
            var target = new BBSCrawleTarget(
                "http://www.discuzsample.com/view.php?forumId=&articleNo=",
                "articleNo",
                "http://www.discuzsample.com/list.php?forumId=&pageNo=",
                "pageNo",
                "testForum",
                "forumId");

            target.ArticleListUrl.Should().Be("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=");
        }
    }
}
