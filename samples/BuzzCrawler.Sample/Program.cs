using BuzzCrawler.Discuz;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscuzCrawler crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://bbs.colg.cn",
                CrawleHistoryRepository = new InMemoryRepository(),
                CrawleRange = new CrawleRange() { MaxListPageNo = 2, StartArticleNo = 4591192 },
                DiscuzVersion = DiscuzVersion.X3_1,
                ForumId = 171
            });
            crawler.OnCrawleComplete += (crawledCount, lastBuzzWriteDate) => Console.WriteLine($"crawled complete. {crawledCount} buzz crawled. last writedate is {lastBuzzWriteDate}");
            crawler.OnNewBuzzCrawled += buzz => Console.WriteLine($"buzz crawled{Environment.NewLine}{JsonConvert.SerializeObject(buzz)}");

            crawler.Crawle();
        }
    }

    public class InMemoryRepository : ICrawleHistoryRepository
    {
        HashSet<string> history = new HashSet<string>();

        public bool IsAlreadyCrawled(Uri uri)
        {
            return history.Contains(uri.AbsoluteUri);
        }

        public void SetCrawleComplete(Uri uri)
        {
            history.Add(uri.AbsoluteUri);
        }

        public DateTime GetLastCrawledArticleWriteDate()
        {
            throw new NotImplementedException();
        }

        public void SetLastCrawledArticleWriteDate(DateTime newLastCrawledArticleWriteDate)
        {
            throw new NotImplementedException();
        }
    }
}
