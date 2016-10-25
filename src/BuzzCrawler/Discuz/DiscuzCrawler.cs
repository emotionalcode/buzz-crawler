using Abot.Crawler;
using Abot.Poco;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;

namespace BuzzCrawler.Discuz
{
    public class DiscuzCrawler : IBuzzCrawler
    {
        private const string HIDDEN_DIV_SELECTOR = @"[style*=""display:none""], [style*=""display: none""]";
        private const string JAMMER_TEXT_SELECTOR = ".jammer";
        private const string SIGNITURE_TEXT_SELECTOR = ".pstatus";

        private object _syncRoot = new object();
        private DateTime? lastestArticleWriteDate;
        private int crawledCount = 0;
        private readonly DiscuzCrawleOption option;

        /// <summary>
        /// event that is fired when an individual buzz has been crawled.
        /// </summary>
        public event Action<int, DateTime?> OnCrawleComplete;
        /// <summary>
        /// event that is fired when all buzz pages has been crawle completed.
        /// </summary>
        public event Action<Buzz> OnNewBuzzCrawled;
        
        public DiscuzCrawler(DiscuzCrawleOption option)
        {
            this.option = option;
        }

        public void Crawle()
        {
            DebugLog("start");

            PoliteWebCrawler crawler = new PoliteWebCrawler(null, null, null, null, null, new Abot.Core.CSQueryHyperlinkParser(), null, null, null);
            crawler.ShouldCrawlPage(shouldCrawlPage);
            crawler.PageCrawlCompletedAsync += pageCrawlCompleted;

            CrawlResult result = crawler.Crawl(new Uri(option.ArticleListPageUrl + "1"));
            if (result.ErrorOccurred)
            {
                DebugLog($"Crawl of {result.RootUri.AbsoluteUri} completed with error: {result.ErrorException.Message}");
            }
            else
            {
                DebugLog($"Crawl of {result.RootUri.AbsoluteUri} completed without error.");
                OnCrawleComplete?.Invoke(crawledCount, lastestArticleWriteDate);
            }
        }

        internal CrawlDecision shouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            var uri = pageToCrawl.Uri;

            if (isListPage(uri) && isListPageWithinTheScopeOfCrawle(uri))
            {
                return new CrawlDecision { Allow = true };
            }

            if (isArticlePage(uri) && isArticleWithinTheScopeOfCrawle(uri) && !option.CrawleHistoryRepository.IsAlreadyCrawled(uri))
            {
                return new CrawlDecision { Allow = true };
            }

            return new CrawlDecision { Allow = false, Reason = "not a target page" };
        }

        internal void pageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            if(!isValid(crawledPage))
            {
                return;
            }

            DebugLog($"pageToCrawl : {crawledPage.Uri.AbsoluteUri}");

            var document = new HtmlParser().Parse(crawledPage.Content.Text);
            removeUnnecessaryElements(document);
            replaceImgTagSrcAttribute(document);

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
                if (option.CrawleHistoryRepository.IsAlreadyCrawled(crawledPage.Uri))
                {
                    DebugLog($"already crawled : {crawledPage.Uri.AbsoluteUri}");
                    return;
                }
                option.CrawleHistoryRepository.SetCrawleComplete(crawledPage.Uri);
            }

            OnNewBuzzCrawled?.Invoke(crawledBuzz);

            crawledCount++;
            updateLastestArticleWriteDate(crawledBuzz);
        }


        #region private methods
        private bool isArticlePage(Uri uri)
        {
            var isArticlePage = uri.AbsoluteUri.StartsWith(option.ArticlePageUrl);
            var isArticlePageWithComments = uri.AbsoluteUri.Contains("&page=");
            return isArticlePage && !isArticlePageWithComments;
        }

        private bool isListPage(Uri uri)
        {
            return uri.AbsoluteUri.StartsWith(option.ArticleListPageUrl);
        }

        private bool isListPageWithinTheScopeOfCrawle(Uri uri)
        {
            var pageNo = int.Parse(HttpUtility.ParseQueryString(uri.Query).Get("page"));
            return option.CrawleRange.MaxListPageNo >= pageNo;
        }

        private bool isArticleWithinTheScopeOfCrawle(Uri uri)
        {
            return option.CrawleRange.StartArticleNo < getContentsNo(uri);
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

            return true;
        }

        private void removeUnnecessaryElements(IHtmlDocument document)
        {
            var unnecessaryElements = document.QuerySelectorAll($"{HIDDEN_DIV_SELECTOR}, {JAMMER_TEXT_SELECTOR}, {SIGNITURE_TEXT_SELECTOR}");
            foreach (var element in unnecessaryElements)
            {
                element.Remove();
            }
        }

        private void replaceImgTagSrcAttribute(IHtmlDocument document)
        {
            var imgTags = document.QuerySelector(".t_f").QuerySelectorAll("img[file]");
            foreach (var item in imgTags)
            {
                item.SetAttribute("src", item.Attributes["file"].Value);
            }
        }

        private int getContentsNo(Uri uri)
        {
            return int.Parse(HttpUtility.ParseQueryString(uri.Query).Get("tid"));
        }

        private Buzz getBuzzFromDocument(IHtmlDocument document, Uri uri)
        {
            var crawledBuzz = new Buzz()
            {
                Content = document.QuerySelector(".t_f")?.InnerHtml,
                ContentsNo = getContentsNo(uri),
                Link = "",
                Title = document.QuerySelectorAll("#thread_subject").FirstOrDefault()?.InnerHtml,
                WriteDate = DateTime.Parse(option.WriteDateSelector(document)).AddHours(1),
                WriterId = document.QuerySelectorAll(".authi a").First().InnerHtml
            };
            return crawledBuzz;
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
        #endregion
    }
}
