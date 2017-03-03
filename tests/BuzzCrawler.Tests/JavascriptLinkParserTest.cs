using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace BuzzCrawler.Tests
{
    public class JavascriptLinkParserTest
    {
        private BBSCrawleTarget target = new BBSCrawleTarget(
                "http://www.test.com/view.php?q1=v1&f=&articleNo=",
                "articleNo",
                "http://www.test.com/list.php?q1=v1&f=&pageNo=",
                "pageNo",
                "f1",
                "f");

        private JavascriptLinkSelectors selectors = new JavascriptLinkSelectors()
        {
            ArticleLinkElementSelector = ".articleLink a",
            ArticleLinkJavascriptFunctionName = "open",
            PagerElementSelector = ".pager a",
            PagerNextElementText = "next page"
        };

        private int pagerButtonCount = 3;

        [Fact]
        public void GetLinks_pagerLinks()
        {
            var expected = new List<Uri>()
            {
                new Uri("http://www.test.com/list.php?q1=v1&f=f1&pageNo=2"),
                new Uri("http://www.test.com/list.php?q1=v1&f=f1&pageNo=3"),
                new Uri("http://www.test.com/list.php?q1=v1&f=f1&pageNo=4")
            };

            var parser = new JavascriptLinkParser(target, selectors, pagerButtonCount);
            var parsedLinks = parser.GetLinks(new Abot.Poco.CrawledPage(new Uri("http://www.test.com/list.php?q1=v1&f=&pageNo=1"))
            {
                Content = new Abot.Poco.PageContent()
                {
                    Text = @"<html><head></head><body><div class=""tablef"">
                                <div class=""pager"">1</div>
                                <div class=""pager""><a href=""javascript:sendList(2)"">2</a></div>
                                <div class=""pager""><a href=""javascript:sendList(3)"">3</a></div>
                                <div class=""pager""><a href=""javascript:sendList(4)"">4</a></div>
                            </div></body></html>"
                }
            });

            parsedLinks.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetLinks_nextPageLink()
        {
            var expected = new List<Uri>()
            {
                new Uri("http://www.test.com/list.php?q1=v1&f=f1&pageNo=5")
            };

            var parser = new JavascriptLinkParser(target, selectors, pagerButtonCount);
            var parsedLinks = parser.GetLinks(new Abot.Poco.CrawledPage(new Uri("http://www.test.com/list.php?q1=v1&f=&pageNo=2"))
            {
                Content = new Abot.Poco.PageContent()
                {
                    Text = @"<html><head></head><body><div class=""tablef"">
                                <div class=""pager"">1</div>
                                <div class=""pager""><a href=""javascript:sendList(4)"">next page</a></div>
                            </div></body></html>"
                }
            });

            parsedLinks.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetLinks_articleLinks()
        {
            var expected = new List<Uri>()
            {
                new Uri("http://www.test.com/view.php?q1=v1&f=f1&articleNo=111"),
                new Uri("http://www.test.com/view.php?q1=v1&f=f1&articleNo=112"),
                new Uri("http://www.test.com/view.php?q1=v1&f=f1&articleNo=113")
            };

            var parser = new JavascriptLinkParser(target, selectors, pagerButtonCount);
            var parsedLinks = parser.GetLinks(new Abot.Poco.CrawledPage(new Uri("http://www.test.com/list.php?q1=v1&f=&pageNo=1"))
            {
                Content = new Abot.Poco.PageContent()
                {
                    Text = @"<html><head></head><body><div class=""tablef"">
                                <div class=""articleLink""><a href=""javascript:$('#btype').val('');$('#bcode2').val('');open(111,'')"">title1</a></div>
                                <div class=""articleLink""><a href=""javascript:$('#btype').val('');$('#bcode2').val('');open(112,'')"">title2</a></div>
                                <div class=""articleLink""><a href=""javascript:$('#btype').val('');$('#bcode2').val('');open(113,'')"">title3</a></div>
                            </div></body></html>"
                }
            });

            parsedLinks.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetLinks_allTypeofLinks()
        {
            var expected = new List<Uri>()
            {
                new Uri("http://www.test.com/view.php?q1=v1&f=f1&articleNo=111"),
                new Uri("http://www.test.com/view.php?q1=v1&f=f1&articleNo=112"),
                new Uri("http://www.test.com/view.php?q1=v1&f=f1&articleNo=113"),
                new Uri("http://www.test.com/list.php?q1=v1&f=f1&pageNo=1"),
                new Uri("http://www.test.com/list.php?q1=v1&f=f1&pageNo=3"),
                new Uri("http://www.test.com/list.php?q1=v1&f=f1&pageNo=4"),
                new Uri("http://www.test.com/list.php?q1=v1&f=f1&pageNo=5")
            };

            var parser = new JavascriptLinkParser(target, selectors, pagerButtonCount);
            var parsedLinks = parser.GetLinks(new Abot.Poco.CrawledPage(new Uri("http://www.test.com/list.php?q1=v1&f=&pageNo=2"))
            {
                Content = new Abot.Poco.PageContent()
                {
                    Text = @"<html><head></head><body>
                                <div class=""tablef"">
                                    <div class=""articleLink""><a href=""javascript:$('#btype').val('');$('#bcode2').val('');open(111,'')"">title1</a></div>
                                    <div class=""articleLink""><a href=""javascript:$('#btype').val('');$('#bcode2').val('');open(112,'')"">title2</a></div>
                                    <div class=""articleLink""><a href=""javascript:$('#btype').val('');$('#bcode2').val('');open(113,'')"">title3</a></div>
                                </div>
                                <div class=""tablef"">
                                    <div class=""pager""><a href=""javascript:sendList(1)"">1</a></div>
                                    <div class=""pager"">2</div>
                                    <div class=""pager""><a href=""javascript:sendList(3)"">3</a></div>
                                    <div class=""pager""><a href=""javascript:sendList(4)"">4</a></div>
                                    <div class=""pager""><a href=""javascript:sendList(5)"">next page</a></div>
                                </div>
                            </body></html>"
                }
            });

            parsedLinks.Should().BeEquivalentTo(expected);
        }
    }
}
