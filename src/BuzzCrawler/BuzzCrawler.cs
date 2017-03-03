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
        public event Action<int, DateTime?> OnCrawleComplete;
        public event Action<Buzz> OnNewBuzzCrawled;
        public Action<IHtmlDocument> DocumentModify;

        private Uri crawleStartUri;
        private IBuzzParser buzzParser;
        private IHyperLinkParser hyperLinkParser;
        private ICrawleHistoryRepository crawleHistoryRepository;

        private int crawledCount = 0;
        private DateTime? lastestArticleWriteDate;
        private CrawlConfiguration abotConfig;
        private object _syncRoot = new object();

        public BuzzCrawler(Uri crawleStartUri, IBuzzParser parser, 
            IHyperLinkParser hyperlinkParser = null, ICrawleHistoryRepository crawleHistoryRepository = null, CrawlConfiguration abotConfig = null)
        {
            this.crawleStartUri = crawleStartUri;
            buzzParser = parser;
            this.hyperLinkParser = hyperlinkParser ?? new AngleSharpHyperlinkParser();
            this.crawleHistoryRepository = crawleHistoryRepository ?? new InMemoryCrawleHistoryRepository();
            this.abotConfig = abotConfig;
        }

        public void Crawle()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            DebugLog("start");

            PoliteWebCrawler crawler = new PoliteWebCrawler(abotConfig, null, null, null, null, hyperLinkParser, null, null, null);
            crawler.ShouldCrawlPage(shouldCrawlPage);
            crawler.PageCrawlCompletedAsync += pageCrawlCompleted;

            CrawlResult result = crawler.Crawl(crawleStartUri);
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

            if (buzzParser.IsListPage(uri))
            {
                DebugLog(uri.AbsoluteUri + " : list page, so crwale");
                return new CrawlDecision { Allow = true };
            }

            if (buzzParser.IsArticlePage(uri) && !crawleHistoryRepository.IsAlreadyCrawled(buzzParser.GetContentsNo(uri)))
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

            Buzz crawledBuzz = null;
            try
            {
                crawledBuzz = GetBuzzFromDocument(crawledPage.Uri, crawledPage.Content.Text);
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

        public Buzz GetBuzzFromDocument(Uri uri, string text)
        {
            var document = new HtmlParser().Parse(text);
            this.DocumentModify?.Invoke(document);

            var crawledBuzz = new Buzz()
            {
                Content = buzzParser.ContentSelector(document),
                ContentsNo = buzzParser.GetContentsNo(uri),
                Link = "",
                Title = buzzParser.TitleSelector(document),
                WriteDate = buzzParser.WriteDateSelector(document),
                WriterId = buzzParser.WriterIdSelector(document)
            };
            return crawledBuzz;
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
            if (buzzParser.IsListPage(crawledPage.Uri))
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