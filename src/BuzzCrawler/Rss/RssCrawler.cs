using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace BuzzCrawler.Rss
{
    public class RssCrawler : IBuzzCrawler
    {
        private const string SIGNITURE_BLOCK_PREFIX = "&#32; submitted by &#32;";
        private const string SIGNITURE_BLOCK_SUFFIX = "[comments]</a></span>";

        private readonly string url;
        private readonly ICrawleHistoryRepository crawleHistory;

        private int crawledCount = 0;

        public event Action<int, DateTime?> OnCrawleComplete;
        public event Action<Buzz> OnNewBuzzCrawled;
        
        public RssCrawler(string url, ICrawleHistoryRepository crawleHistory)
        {
            this.url = url;
            this.crawleHistory = crawleHistory;
        }

        public void Crawle()
        {
            DebugLog("start");

            var reader = XmlReader.Create(url);
            var articles = SyndicationFeed.Load(reader);
            reader.Close();

            var lastCrawledArticleWriteDate = crawleHistory.GetLastCrawledArticleWriteDate();

            foreach (SyndicationItem article in articles.Items)
            {
                var writeDate = article.LastUpdatedTime.DateTime.ToLocalTime();
                if (writeDate <= lastCrawledArticleWriteDate)
                {
                    continue;
                }

                crawledCount++;
                OnNewBuzzCrawled?.Invoke(new Buzz()
                {
                    ContentsNo = 0,
                    Title = article.Title.Text,
                    Content = getPureContent(article),
                    WriterId = article.Authors.First().Name.Replace("/u/", ""),
                    WriteDate = writeDate,
                    Link = article.Links.Any() ? article.Links[0].Uri.ToString() : null
                });
            }

            var newLastCrawledArticleWriteDate = articles.Items.OrderByDescending(i => i.LastUpdatedTime).First().LastUpdatedTime.DateTime.ToLocalTime();
            crawleHistory.SetLastCrawledArticleWriteDate(newLastCrawledArticleWriteDate);
            OnCrawleComplete?.Invoke(crawledCount, newLastCrawledArticleWriteDate);
        }

        string getPureContent(SyndicationItem article)
        {
            var text = (article.Content as TextSyndicationContent).Text;

            var prefixIdx = text.IndexOf(SIGNITURE_BLOCK_PREFIX);
            var hasSignitureBlock = prefixIdx > 0;
            if (hasSignitureBlock)
            {
                var suffixIdx = text.IndexOf(SIGNITURE_BLOCK_SUFFIX);
                return HttpUtility.HtmlDecode(text.Remove(prefixIdx, suffixIdx - prefixIdx + SIGNITURE_BLOCK_SUFFIX.Length));
            }
            else
            {
                return HttpUtility.HtmlDecode(text);
            }
        }

        [Conditional("DEBUG")]
        void DebugLog(string txt)
        {
            Console.WriteLine(txt);
        }
    }
}
