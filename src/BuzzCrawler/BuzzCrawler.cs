using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using BuzzCrawler.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BuzzCrawler
{
    public class BuzzCrawler : IBuzzCrawler
    {
        private object _syncRoot = new object();
        private DateTime? lastestArticleWriteDate;
        private int crawledCount = 0;
        private CrawleTarget target;
        private IHyperLinkParser hyperLinkParser;
        private ICrawleHistoryRepository crawleHistoryRepository;

        protected ElementSelectors selectors;
        protected Action<IHtmlDocument> documentModify;

        public event Action<int, DateTime?> OnCrawleComplete;
        public event Action<Buzz> OnNewBuzzCrawled;

        public BuzzCrawler(CrawleTarget target, ElementSelectors selectors, IHyperLinkParser hyperlinkParser = null, ICrawleHistoryRepository crawleHistoryRepository = null, 
            Action<IHtmlDocument> documentModify = null)
        {
            this.target = target;
            this.selectors = selectors;
            this.hyperLinkParser = hyperlinkParser ?? new AngleSharpHyperlinkParser();
            this.crawleHistoryRepository = crawleHistoryRepository ?? new InMemoryCrawleHistoryRepository();
            this.documentModify = documentModify;
        }

        public void Crawle()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            DebugLog("start");

            PoliteWebCrawler crawler = new PoliteWebCrawler(null, null, null, null, null, hyperLinkParser, null, null, null);
            crawler.ShouldCrawlPage(shouldCrawlPage);
            crawler.PageCrawlCompletedAsync += pageCrawlCompleted;

            CrawlResult result = crawler.Crawl(new Uri(QueryStringHelper.FillQueryString(target.ArticleListUrl, target.PageNoQueryString, "1")));
            if (result.ErrorOccurred)
            {
                DebugLog($"Crawl of {result.RootUri.AbsoluteUri} completed with error: {result.ErrorException.Message}");
            }
            else
            {
                DebugLog($"Crawl of {result.RootUri.AbsoluteUri} completed without error.");
                OnCrawleComplete?.Invoke(crawledCount, lastestArticleWriteDate);
            }
            DebugLog(sw.Elapsed.TotalSeconds.ToString());
        }

        internal CrawlDecision shouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            var uri = pageToCrawl.Uri;

            if (isListPage(uri) && isListPageWithinTheScopeOfCrawle(uri))
            {
                DebugLog(uri.AbsoluteUri + " : list page, so crwale");
                return new CrawlDecision { Allow = true };
            }

            if (isArticlePage(uri) && isArticleWithinTheScopeOfCrawle(uri) && !crawleHistoryRepository.IsAlreadyCrawled(getContentsNo(uri)))
            {
                return new CrawlDecision { Allow = true };
            }

            return new CrawlDecision { Allow = false, Reason = "not a target page" };
        }

        internal void pageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            if (!isValid(crawledPage))
            {
                return;
            }

            DebugLog($"pageToCrawl : {crawledPage.Uri.AbsoluteUri}");

            var document = new HtmlParser().Parse(crawledPage.Content.Text);
            this.documentModify?.Invoke(document);

            Buzz crawledBuzz = null;
            try
            {
                crawledBuzz = getBuzzFromDocument(document, crawledPage.Uri);
            }
            catch (Exception ex)
            {
                DebugLog($"buzz parsing error. exception:{ex.ToString()}");
                return;
            }
            
            lock (_syncRoot)
            {
                if (crawleHistoryRepository.IsAlreadyCrawled(crawledBuzz.ContentsNo))
                {
                    DebugLog($"already crawled : {crawledPage.Uri.AbsoluteUri}");
                    return;
                }
                crawleHistoryRepository.SetCrawleComplete(crawledBuzz.ContentsNo);
            }

            OnNewBuzzCrawled?.Invoke(crawledBuzz);

            crawledCount++;
            updateLastestArticleWriteDate(crawledBuzz);
        }

        private bool isValid(CrawledPage crawledPage)
        {
            if (crawledPage.WebException != null)
            {
                DebugLog($"Crawl of page failed - web exception {crawledPage.Uri.AbsoluteUri} : {crawledPage.WebException.ToString()}");
                return false;
            }
            if (crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
            {
                DebugLog($"Crawl of page failed - status {crawledPage.Uri.AbsoluteUri} : {crawledPage.HttpWebResponse.StatusCode.ToString()}");
                return false;
            }
            if (isListPage(crawledPage.Uri))
            {
                DebugLog($"this is list page, so pass analysis: {crawledPage.Uri.AbsoluteUri}");
                return false;
            }
            if (string.IsNullOrEmpty(crawledPage.Content.Text))
            {
                return false;
            }
            if (crawledPage.Content.Text.Contains("alert('게시물이 존재하지 않습니다.')"))
            {
                return false;
            }
            return true;
        }

        internal bool isListPage(Uri uri)
        {
            return
                Uri.Compare(
                    new Uri(QueryStringHelper.RemoveQueryStringByKey(uri.AbsoluteUri, target.PageNoQueryString)),
                    new Uri(QueryStringHelper.RemoveQueryStringByKey(target.ArticleListUrl, target.PageNoQueryString)),
                    UriComponents.Host | UriComponents.PathAndQuery,
                    UriFormat.SafeUnescaped,
                    StringComparison.OrdinalIgnoreCase) == 0;
        }

        private bool isListPageWithinTheScopeOfCrawle(Uri uri)
        {
            var pageNo = int.Parse(HttpUtility.ParseQueryString(uri.Query).Get(target.PageNoQueryString));
            return target.MaxListPageNo >= pageNo;
        }

        private bool isArticlePage(Uri uri)
        {
            var isArticlePage = uri.AbsoluteUri.StartsWith(target.ArticleUrl);
            var isArticlePageWithComments = uri.AbsoluteUri.Contains("&page=");
            return isArticlePage && !isArticlePageWithComments;
        }

        private bool isArticleWithinTheScopeOfCrawle(Uri uri)
        {
            return target.StartArticleNo <= getContentsNo(uri);
        }

        private Buzz getBuzzFromDocument(IHtmlDocument document, Uri uri)
        {
            var crawledBuzz = new Buzz()
            {
                Content = document.QuerySelector(selectors.ContentSelector)?.InnerHtml.Trim(),
                ContentsNo = getContentsNo(uri),
                Link = "",
                Title = document.QuerySelectorAll(selectors.TitleSelector).FirstOrDefault()?.InnerHtml.Trim(),
                WriteDate = selectors.WriteDateSelector(document),
                WriterId = document.QuerySelectorAll(selectors.WriterIdSelector).First().InnerHtml
            };
            return crawledBuzz;
        }

        private int getContentsNo(Uri uri)
        {
            return int.Parse(HttpUtility.ParseQueryString(uri.Query).Get(target.ArticleNoQueryString));
        }

        private void updateLastestArticleWriteDate(Buzz crawledResult)
        {
            if (lastestArticleWriteDate.HasValue == false || lastestArticleWriteDate.Value < crawledResult.WriteDate)
            {
                lastestArticleWriteDate = crawledResult.WriteDate;
            }
        }

        [Conditional("DEBUG")]
        void DebugLog(string txt)
        {
            Console.WriteLine(txt);
        }
    }
}
