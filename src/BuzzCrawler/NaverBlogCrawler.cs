using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BuzzCrawler
{
    public class NaverBlogCrawler : IBuzzCrawler
    {
        public event Action<int, DateTime?> OnCrawleComplete;
        public event Action<Buzz> OnNewBuzzCrawled;

        private ICrawleHistoryRepository crawleHistoryRepository;
        private string searchText;
        private string clientId;
        private string clientSecret;

        //private HttpClient apiClient;
        //private HttpClient blogPageClient;

        private HttpClient httpClient;

        public NaverBlogCrawler(string searchText, string clientId, string clientSecret, ICrawleHistoryRepository crawleHistoryRepository = null)
        {
            this.searchText = searchText;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.crawleHistoryRepository = crawleHistoryRepository ?? new InMemoryCrawleHistoryRepository();
            
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://openapi.naver.com");
        }

        async Task<NaverBlogResponse> GetProductAsync(string path)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Naver-Client-Id", this.clientId);
            httpClient.DefaultRequestHeaders.Add("X-Naver-Client-Secret", this.clientSecret);
            NaverBlogResponse product = null;
            HttpResponseMessage response = await httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                product = await response.Content.ReadAsAsync<NaverBlogResponse>();
            }
            return product;
        }

        async Task<string> GetArticleContent(string path)
        {
            string result = string.Empty;
            httpClient.DefaultRequestHeaders.Clear();
            HttpResponseMessage response = await httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            return result;
        }

        public void Crawle()
        {
            var res = GetProductAsync("https://openapi.naver.com/v1/search/blog?sort=date&query=" + this.searchText);
            res.Wait();

            foreach (var article in res.Result.Items)
            {
                article.Link = HttpUtility.HtmlDecode(article.Link);
                if(article.Link.Contains("blog.naver.com") == false)
                {
                    continue;
                }
                var articleNo = getContentsNo(article.Link);
                if (crawleHistoryRepository.IsAlreadyCrawled(articleNo))
                {
                    Console.WriteLine("duplicated");
                    continue;
                }
                var framePage = GetArticleContent(article.Link);
                framePage.Wait();

                var document = new HtmlParser().Parse(framePage.Result);
                var articleUrl = document.QuerySelector("#mainFrame").Attributes["src"].Value;
                Console.WriteLine(articleUrl);

                var contentPage = GetArticleContent(new Uri(article.Link).GetLeftPart(UriPartial.Authority) + "/" + articleUrl);
                contentPage.Wait();

                var articleDoc = new HtmlParser().Parse(contentPage.Result);
                var crawledBuzz = getBuzzFromDocument(article, articleDoc);

                crawleHistoryRepository.SetCrawleComplete(articleNo);
                OnNewBuzzCrawled?.Invoke(crawledBuzz);
            }
        }

        private long getContentsNo(string uriString)
        {
            //blog.me, tistory.com  egloos.com/7293950
            var uri = new Uri(uriString);
            return long.Parse(HttpUtility.ParseQueryString(uri.Query).Get("logNo"));
        }

        private Buzz getBuzzFromDocument(NaverBlogItem item, IHtmlDocument document)
        {
            var contentsNo = getContentsNo(item.Link);
            var crawledBuzz = new Buzz()
            {
                Content = document.QuerySelector("#post-view" + contentsNo)?.InnerHtml.Trim(),
                ContentsNo = contentsNo,
                Link = item.Link,
                Title = item.Title,
                WriteDate = DateTime.Parse(document.QuerySelector("._postAddDate, .se_publishDate")?.InnerHtml.Trim()),
                WriterId = item.BloggerName
            };
            return crawledBuzz;
        }
    }

    public class NaverBlogResponse
    {
        public DateTime LastBuildDate { get; set; }
        public int Total { get; set; }
        public int Start { get; set; }
        public int Display { get; set; }
        public List<NaverBlogItem> Items { get; set; }
    }

    public class NaverBlogItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string BloggerName { get; set; }
        public string BloggerLink { get; set; }
        public string PostDate { get; set; }
    }
}
