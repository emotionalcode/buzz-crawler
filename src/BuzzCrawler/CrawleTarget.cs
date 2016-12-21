using BuzzCrawler.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler
{
    public class CrawleTarget
    {
        private string articleUrl;
        private string listUrl;
        private string forumId;
        private string forumIdQuerystring;

        public string ArticleUrl
        {
            get
            {
                return QueryStringHelper.FillQueryString(articleUrl, forumIdQuerystring, forumId);
            }
        }

        public string ArticleListUrl
        {
            get
            {
                return QueryStringHelper.FillQueryString(listUrl, forumIdQuerystring, forumId);
            }
        }

        public string PageNoQueryString { get; }
        public string ArticleNoQueryString { get; }

        public int MaxListPageNo { get; set; }
        public int StartArticleNo { get; set; }

        public CrawleTarget(string articleUrl, string articleNoQuerystring, string listUrl, string pageNoQuerystring, string forumId, string forumIdQuerystring, int maxListPageNo = 10, int startArticleNo = 0)
        {
            this.articleUrl = articleUrl;
            this.listUrl = listUrl;
            this.forumId = forumId;
            this.forumIdQuerystring = forumIdQuerystring;

            ArticleNoQueryString = articleNoQuerystring;
            PageNoQueryString = pageNoQuerystring;
            this.MaxListPageNo = maxListPageNo;
            this.StartArticleNo = startArticleNo;
        }
    }
}
