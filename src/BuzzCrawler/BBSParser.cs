using AngleSharp.Dom.Html;
using BuzzCrawler.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BuzzCrawler
{
    public class BBSParser : IBuzzParser
    {
        public Func<IHtmlDocument, string> ContentSelector { get; set; }
        public Func<IHtmlDocument, string> TitleSelector { get; set; }
        public Func<IHtmlDocument, DateTime> WriteDateSelector { get; set; }
        public Func<IHtmlDocument, string> WriterIdSelector { get; set; }

        BBSCrawleTarget target;
        public BBSParser(BBSCrawleTarget target,
            Func<IHtmlDocument, string> ContentSelector,
            Func<IHtmlDocument, string> TitleSelector,
            Func<IHtmlDocument, DateTime> WriteDateSelector,
            Func<IHtmlDocument, string> WriterIdSelector)
        {
            this.target = target;
            this.ContentSelector = ContentSelector;
            this.TitleSelector = TitleSelector;
            this.WriteDateSelector = WriteDateSelector;
            this.WriterIdSelector = WriterIdSelector;
        }

        public long GetContentsNo(Uri uri)
        {
            return int.Parse(HttpUtility.ParseQueryString(uri.Query).Get(target.ArticleNoQueryString));
        }

        public bool IsArticlePage(Uri uri)
        {
            return isArticlePage(uri) && isArticleWithinTheScopeOfCrawle(uri);
        }

        public bool IsListPage(Uri uri)
        {
            return isListPage(uri) && isListPageWithinTheScopeOfCrawle(uri);
        }

        private bool isListPage(Uri uri)
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
            return target.StartArticleNo <= GetContentsNo(uri);
        }
    }
}
