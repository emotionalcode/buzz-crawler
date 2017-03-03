using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using BuzzCrawler.Discuz;
using Abot.Poco;
using System.Threading;

namespace BuzzCrawler.Tests
{
    public class BuzzParserTest
    {
        BBSCrawleTarget testTarget = new BBSCrawleTarget(
            articleUrl: "http://bbs.dizcuzsample.com/forum.php?mod=viewthread&tid=",
            articleNoQuerystring: "tid",
            listUrl: "http://bbs.dizcuzsample.com/forum.php?mod=forumdisplay&fid=&orderby=dateline&filter=author&page=",
            pageNoQuerystring: "page",
            bbsInstanceKey: "333",
            bbsInstanceKeyQuerystring: "fid",
            maxListPageNo: 999,
            startArticleNo: 10
        );

        [Fact]
        public void IsArticlePage_outOfScope_false()
        {
            var parser = new BBSParser(testTarget, null, null, null, null);

            parser.IsArticlePage(
                new Uri("http://bbs.dizcuzsample.com/forum.php?mod=viewthread&tid=8")
                ).Should().BeFalse("because tid is smaller then startArticleNo");
            parser.IsArticlePage(
                new Uri("http://bbs.dizcuzsample.com/forum.php?mod=viewthread&tid=9")
                ).Should().BeFalse("because tid is smaller then startArticleNo");

        }

        [Fact]
        public void IsArticlePage_notArticlePage_false()
        {
            var parser = new BBSParser(testTarget, null, null, null, null);

            parser.IsArticlePage(
                new Uri("http://bbs.dizcuzsample.com/forum.php?mod=forumdisplay&fid=333&orderby=dateline&filter=author&page=1")
                ).Should().BeFalse("because this url is not an article url");
            parser.IsArticlePage(
                new Uri("http://www.dummy.com")
                ).Should().BeFalse("because this url is not an article url");
        }

        [Fact]
        public void IsArticlePage_articlePageInScope_false()
        {
            var parser = new BBSParser(testTarget, null, null, null, null);
            parser.IsArticlePage(new Uri("http://bbs.dizcuzsample.com/forum.php?mod=viewthread&tid=10")).Should().BeTrue();
            parser.IsArticlePage(new Uri("http://bbs.dizcuzsample.com/forum.php?mod=viewthread&tid=11")).Should().BeTrue();
        }

        [Fact]
        public void IsListPage_outOfScope_false()
        {
            var parser = new BBSParser(testTarget, null, null, null, null);

            parser.IsListPage(
                new Uri("http://bbs.dizcuzsample.com/forum.php?mod=forumdisplay&fid=333&orderby=dateline&filter=author&page=1000")
                ).Should().BeFalse("because pageNo is greater then maxListPageNo");
            parser.IsListPage(
                new Uri("http://bbs.dizcuzsample.com/forum.php?mod=forumdisplay&fid=333&orderby=dateline&filter=author&page=1001")
                ).Should().BeFalse("because pageNo is greater then maxListPageNo");

        }

        [Fact]
        public void IsListPage_notListPage_false()
        {
            var parser = new BBSParser(testTarget, null, null, null, null);

            parser.IsListPage(
                new Uri("http://bbs.dizcuzsample.com/forum.php?mod=viewthread&tid=8")
                ).Should().BeFalse("because this url is not a list page");
            parser.IsListPage(
                new Uri("http://www.dummy.com")
                ).Should().BeFalse("because this url is not a list page");

        }

        [Fact]
        public void IsListPage_listPageInScope_true()
        {
            var parser = new BBSParser(testTarget, null, null, null, null);

            parser.IsListPage(new Uri("http://bbs.dizcuzsample.com/forum.php?mod=forumdisplay&fid=333&orderby=dateline&filter=author&page=999")).Should().BeTrue();
            parser.IsListPage(new Uri("http://bbs.dizcuzsample.com/forum.php?mod=forumdisplay&fid=333&orderby=dateline&filter=author&page=998")).Should().BeTrue();

        }

        [Fact]
        public void GetContentsNo()
        {
            var parser = new BBSParser(testTarget, null, null, null, null);

            parser.GetContentsNo(new Uri("http://bbs.dizcuzsample.com/forum.php?mod=viewthread&tid=11")).Should().Be(11L);
            parser.GetContentsNo(new Uri("http://bbs.dizcuzsample.com/forum.php?mod=viewthread&tid=110")).Should().Be(110L);
            Action getContentsNoFromInValidUrl = () =>
            {
                parser.GetContentsNo(new Uri("http://www.dummy.com"));
            };

            getContentsNoFromInValidUrl.ShouldThrow<ArgumentNullException>("because contents key was not included in url");
        }
    }
}
