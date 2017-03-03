using BuzzCrawler.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler
{
    public class BBSCrawleTarget
    {
        private string articleUrl;
        private string listUrl;
        private string bbsInstanceKey;
        private string bbsInstanceKeyQuerystring;

        public string ArticleUrl
        {
            get
            {
                return QueryStringHelper.FillQueryString(articleUrl, bbsInstanceKeyQuerystring, bbsInstanceKey);
            }
        }

        public string ArticleListUrl
        {
            get
            {
                return QueryStringHelper.FillQueryString(listUrl, bbsInstanceKeyQuerystring, bbsInstanceKey);
            }
        }

        public string PageNoQueryString { get; }
        public string ArticleNoQueryString { get; }

        public int MaxListPageNo { get; set; }
        public int StartArticleNo { get; set; }

        public BBSCrawleTarget(string articleUrl, string articleNoQuerystring, string listUrl, string pageNoQuerystring, string bbsInstanceKey, string bbsInstanceKeyQuerystring, int maxListPageNo = 10, int startArticleNo = 0)
        {
            this.articleUrl = articleUrl;
            this.listUrl = listUrl;
            this.bbsInstanceKey = bbsInstanceKey;
            this.bbsInstanceKeyQuerystring = bbsInstanceKeyQuerystring;

            ArticleNoQueryString = articleNoQuerystring;
            PageNoQueryString = pageNoQuerystring;
            this.MaxListPageNo = maxListPageNo;
            this.StartArticleNo = startArticleNo;
        }
    }
}
