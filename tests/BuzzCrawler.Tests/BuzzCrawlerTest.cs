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
using AngleSharp.Dom.Html;

namespace BuzzCrawler.Tests
{
    public class BuzzCrawlerTest
    {
        private const string HTML_SAMPLE = @"<html><body>
                    <div id=""thread_subject"">title</div>
                    <div class=""authi""><a>writerId</a></div>
                    <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div>
                    <table><tr><td class=""t_f"" id=""postmessage_684661805"">article contents</td></tr></table>
                </body></html>";

        private const string HTML_SAMPLE_WITH_NO_WRITEDATE = @"<html><body>
                    <div id=""thread_subject"">title</div>
                    <div class=""authi""><a>writerId</a></div>
                    <div class=""authi""><span title=""aaaa""></span></div>
                    <div class=""t_f"">
                        test<span>test</span>
                    </div>
                </body></html>";

        private readonly BBSCrawleTarget TEST_CRAWLE_TARGET = new BBSCrawleTarget(
                "http://www.discuzsample.com/view.php?forumId=&articleNo=",
                "articleNo",
                "http://www.discuzsample.com/list.php?forumId=&pageNo=",
                "pageNo",
                "testForum",
                "forumId");

        public class DummyBuzzParser_NotAnArticlePage_NotAListPage : IBuzzParser
        {
            public Func<IHtmlDocument, string> ContentSelector { get; set; }
            public Func<IHtmlDocument, string> TitleSelector { get; set; }
            public Func<IHtmlDocument, DateTime> WriteDateSelector { get; set; }
            public Func<IHtmlDocument, string> WriterIdSelector { get; set; }

            public long GetContentsNo(Uri uri)
            {
                throw new NotImplementedException();
            }

            public bool IsArticlePage(Uri uri)
            {
                return false;
            }

            public bool IsListPage(Uri uri)
            {
                return false;
            }
        }

        #region shouldCrawlPage
        [Fact]
        public void shouldCrawlPage_neitherArticleOrListPage_allowFalse()
        {
            var crawler = new BuzzCrawler(
                new Uri("http://www.discuzsample.com/list.php?forumId=f&pageNo=1"), 
                new DummyBuzzParser_NotAnArticlePage_NotAListPage());

            crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://test.com") }, null)
                .Allow.Should().BeFalse("because this is neither ArticlePage and ListPage");
        }

        [Fact]
        public void shouldCrawlPage_articlePage_alreadyCrawled_allowFalse()
        {
            var repository = new InMemoryCrawleHistoryRepository();
            var crawler = new BuzzCrawler(
                new Uri("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=1"), 
                new BBSParser(TEST_CRAWLE_TARGET, null, null, null, null), 
                crawleHistoryRepository: repository);

            crawler.shouldCrawlPage(new PageToCrawl(){ Uri = new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=10") }, null)
                .Allow.Should().BeTrue("because this page never been crawled before");

            repository.SetCrawleComplete(10);

            crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=10") }, null)
                .Allow.Should().BeFalse("because this page already crawled");
        }

        [Fact]
        public void shouldCrawlPage_articlePage_withComments_allowFalse()
        {
            var crawler = new BuzzCrawler(
                new Uri("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=1"), 
                new BBSParser(TEST_CRAWLE_TARGET, null, null, null, null));

            crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=10&page=2") }, null)
                .Allow.Should().BeFalse("because this is article comment list page");
        }
        #endregion

        #region pageCrawlCompleted
        [Fact]
        public void pageCrawlCompleted_webException_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent() { Text = HTML_SAMPLE },
                WebException = new System.Net.WebException("exception for test")
            };

            var crawler = BuzzCrawlerFactory.GetBBSCrawler(BBSType.Discuz_v2, TEST_CRAWLE_TARGET);
            crawler.Should().BeOfType<BuzzCrawler>();
            crawler.MonitorEvents();
            (crawler as BuzzCrawler).pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            crawler.ShouldNotRaise("OnNewBuzzCrawled", "because web exception throwed");
        }

        [Fact]
        public void pageCrawlCompleted_httpResponse502_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.BadGateway, "html", null, null),
                Content = new PageContent() { Text = HTML_SAMPLE }
            };

            var crawler = BuzzCrawlerFactory.GetBBSCrawler(BBSType.Discuz_v2, TEST_CRAWLE_TARGET);
            crawler.Should().BeOfType<BuzzCrawler>();
            crawler.MonitorEvents();
            (crawler as BuzzCrawler).pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            crawler.ShouldNotRaise("OnNewBuzzCrawled", "because response status is not an OK");
        }

        [Fact]
        public void pageCrawlCompleted_listPage_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent() { Text = HTML_SAMPLE }
            };

            var crawler = BuzzCrawlerFactory.GetBBSCrawler(BBSType.Discuz_v2, TEST_CRAWLE_TARGET);
            crawler.Should().BeOfType<BuzzCrawler>();
            crawler.MonitorEvents();
            (crawler as BuzzCrawler).pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            crawler.ShouldNotRaise("OnNewBuzzCrawled", "because this is list page");
        }

        [Fact]
        public void pageCrawlCompleted_documentIsNull_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null)
            };
            
            var crawler = BuzzCrawlerFactory.GetBBSCrawler(BBSType.Discuz_v2, TEST_CRAWLE_TARGET);
            crawler.Should().BeOfType<BuzzCrawler>();
            crawler.MonitorEvents();
            (crawler as BuzzCrawler).pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            crawler.ShouldNotRaise("OnNewBuzzCrawled", "because document is empty");
        }

        [Fact]
        public void pageCrawlCompleted_parseError_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent() { Text = HTML_SAMPLE_WITH_NO_WRITEDATE }
            };

            var crawler = BuzzCrawlerFactory.GetBBSCrawler(BBSType.Discuz_v2, TEST_CRAWLE_TARGET);
            crawler.Should().BeOfType<BuzzCrawler>();
            crawler.MonitorEvents();
            (crawler as BuzzCrawler).pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            crawler.ShouldNotRaise("OnNewBuzzCrawled", "because parsing error");
        }

        [Fact]
        public void pageCrawlCompleted_httpResponseOK_eventRaise()
        {

            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent() { Text = HTML_SAMPLE }
            };

            var crawler = BuzzCrawlerFactory.GetBBSCrawler(BBSType.Discuz_v2, TEST_CRAWLE_TARGET);
            crawler.Should().BeOfType<BuzzCrawler>();
            crawler.MonitorEvents();
            (crawler as BuzzCrawler).pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            crawler.ShouldRaise("OnNewBuzzCrawled");
        }

        #endregion
    }
}
