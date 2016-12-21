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
    public class BuzzCrawlerTest
    {
        private CrawleTarget getTestTarget(int maxListPageNo = 10, int startArticleNo = 0)
        {
            return new CrawleTarget(
                "http://www.discuzsample.com/view.php?forumId=&articleNo=", "articleNo",
                "http://www.discuzsample.com/list.php?forumId=&pageNo=", "pageNo",
                "testForum",
                "forumId", maxListPageNo, startArticleNo);
        }

        private static PageToCrawl targetPage(string url)
        {
            return new PageToCrawl() { Uri = new Uri(url) };
        }

        #region shouldCrawlPage
        [Fact]
        public void shouldCrawlPage_neitherArticleOrListPage_allowFalse()
        {
            var crawler = new BuzzCrawler(getTestTarget(), null);

            crawler.shouldCrawlPage(targetPage("http://test.com"), null)
                .Allow.Should().BeFalse("because this is neither ArticlePage and ListPage");
        }

        

        [Fact]
        public void shouldCrawlPage_listPage_outOfScope_allowFalse()
        {
            var crawler = new BuzzCrawler(getTestTarget(maxListPageNo: 3), null);

            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=4"), null)
                .Allow.Should().BeFalse("because 4 page is out of crawle range");
            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=5"), null)
                .Allow.Should().BeFalse("because 5 page is out of crawle range");
        }

        [Fact]
        public void shouldCrawlPage_listPage_inScope_allowTrue()
        {
            var crawler = new BuzzCrawler(getTestTarget(maxListPageNo: 3), null);

            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=2"), null)
                .Allow.Should().BeTrue("because crawle range include 2 page ");
            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=3"), null)
                .Allow.Should().BeTrue("because crawle range include 3 page ");
        }

        [Fact]
        public void shouldCrawlPage_articlePage_outOfScope_allowFalse()
        {
            var crawler = new BuzzCrawler(getTestTarget(startArticleNo: 10), null);

            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=9"), null)
                .Allow.Should().BeFalse("because articleNo 9 page is out of crawle range");
            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=8"), null)
                .Allow.Should().BeFalse("because articleNo 8 page is out of crawle range");
        }

        [Fact]
        public void shouldCrawlPage_articlePage_inScope_allowTrue()
        {
            var crawler = new BuzzCrawler(getTestTarget(startArticleNo: 10), null);

            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=10"), null)
                .Allow.Should().BeTrue("because crawle range include articleNo 10 page ");
            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=11"), null)
                .Allow.Should().BeTrue("because crawle range include articleNo 11 page ");
        }

        [Fact]
        public void shouldCrawlPage_articlePage_alreadyCrawled_allowFalse()
        {
            var repository = new InMemoryCrawleHistoryRepository();
            var crawler = new BuzzCrawler(getTestTarget(), null, crawleHistoryRepository: repository);

            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=10"), null)
                .Allow.Should().BeTrue("because this page never been crawled before");

            repository.SetCrawleComplete(10);

            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=10"), null)
                .Allow.Should().BeFalse("because this page already crawled");
        }

        [Fact]
        public void shouldCrawlPage_articlePage_withComments_allowFalse()
        {
            var crawler = new BuzzCrawler(getTestTarget(), null);

            crawler.shouldCrawlPage(targetPage("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=10&page=2"), null)
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
                Content = new PageContent() { Text = @"
                <html><body>
                    <div id=""thread_subject"">title</div>
                    <div class=""authi""><a>writerId</a></div>
                    <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div>
                    <table><tr><td class=""t_f"" id=""postmessage_684661805"">article contents</td></tr></table>
                </body></html>" },
                WebException = new System.Net.WebException("exception for test")
            };
            
            var crawler = new DiscuzCrawler(DiscuzVersion.X2, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.True(false, "because web exception throwed, this buzz should ignored. should not fired OnNewBuzzCrawled");
            };

            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_httpResponse502_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.BadGateway, "html", null, null),
                Content = new PageContent() { Text = @"
                <html><body>
                    <div id=""thread_subject"">title</div>
                    <div class=""authi""><a>writerId</a></div>
                    <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div>
                    <table><tr><td class=""t_f"" id=""postmessage_684661805"">article contents</td></tr></table>
                </body></html>" }
            };
            
            var crawler = new DiscuzCrawler(DiscuzVersion.X2, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.True(false, "because response status is not an OK, this buzz should ignored. should not fired OnNewBuzzCrawled");
            };

            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_listPage_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent() { Text = @"
                <html><body>
                    <div id=""thread_subject"">title</div>
                    <div class=""authi""><a>writerId</a></div>
                    <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div>
                    <table><tr><td class=""t_f"" id=""postmessage_684661805"">article contents</td></tr></table>
                </body></html>" }
            };
            
            var crawler = new DiscuzCrawler(DiscuzVersion.X2, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.True(false, "because this is list page, should not fired OnNewBuzzCrawled");
            };

            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_documentIsNull_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/list.php?forumId=testForum&pageNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null)
            };
            
            var crawler = new DiscuzCrawler(DiscuzVersion.X2, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.True(false, "because document is empty, should not fired OnNewBuzzCrawled");
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_parseError_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent() { Text = @"
                <html><body>
                    <div id=""thread_subject"">title</div>
                    <div class=""authi""><a>writerId</a></div>
                    <div class=""authi""><span title=""aaaa""></span></div>
                    <div class=""t_f"">
                        test<span>test</span>
                    </div>
                </body></html>" }
            };

            var crawler = new DiscuzCrawler(DiscuzVersion.X2, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.True(false, "when parse error, should be ignored");
            };

            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }
        #endregion

        [Fact]
        public void isListPage()
        {
            var crawleTarget = new CrawleTarget(
                articleUrl: "http://bbs.colg.cn/forum.php?mod=viewthread&tid=",
                articleNoQuerystring: "tid",
                listUrl: "http://bbs.colg.cn/forum.php?mod=forumdisplay&fid=&orderby=dateline&filter=author&page=",
                pageNoQuerystring: "page",
                forumId: "171",
                forumIdQuerystring: "fid",
                maxListPageNo: 10,
                startArticleNo: 5136100);

            var crawler = new DiscuzCrawler(DiscuzVersion.X3_1, crawleTarget);
            var result = crawler.isListPage(new Uri("http://bbs.colg.cn/forum.php?mod=forumdisplay&fid=171&orderby=dateline&orderby=dateline&filter=author&page=2"));
            result.Should().BeTrue();
        }
    }
}
