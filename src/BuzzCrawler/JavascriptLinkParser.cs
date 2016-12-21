using Abot.Core;
using Abot.Poco;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using BuzzCrawler.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace BuzzCrawler
{
    public class JavascriptLinkParser : IHyperLinkParser
    {
        private CrawleTarget target { get; set; }
        private JavascriptLinkSelectors linkSelectors { get; set; }
        private int pagerButtonCount { get; set; }

        public JavascriptLinkParser(CrawleTarget target, JavascriptLinkSelectors linkSelectors, int pagerButtonCount)
        {
            this.target = target;
            this.linkSelectors = linkSelectors;
            this.pagerButtonCount = pagerButtonCount;
        }

        public IEnumerable<Uri> GetLinks(CrawledPage crawledPage)
        {
            var urls = new List<Uri>();
            var document = new HtmlParser().Parse(crawledPage.Content.Text);

            urls.AddRange(extractArticlePageLinks(document));
            urls.AddRange(extractPagerLinks(document));
            urls.AddRange(extractNextPageLinks(document, crawledPage.Uri));

            return urls;
        }

        private IEnumerable<Uri> extractArticlePageLinks(IHtmlDocument htmlDocument)
        {
            return from e in htmlDocument.QuerySelectorAll(linkSelectors.ArticleLinkElementSelector)
                   where e.Attributes["href"].Value.Contains(linkSelectors.ArticleLinkJavascriptFunctionName)
                   select new Uri(QueryStringHelper.FillQueryString(target.ArticleUrl, target.ArticleNoQueryString, getLinkArticleNo(e)));
        }

        private IEnumerable<Uri> extractPagerLinks(IHtmlDocument htmlDocument)
        {
            int dummy;
            return (from e in htmlDocument.QuerySelectorAll(linkSelectors.PagerElementSelector)
                    where int.TryParse(e.TextContent, out dummy)
                    select new Uri(QueryStringHelper.FillQueryString(target.ArticleListUrl, target.PageNoQueryString, e.TextContent.Trim())));

        }

        private IEnumerable<Uri> extractNextPageLinks(IHtmlDocument htmlDocument, Uri uri)
        {
            return from e in htmlDocument.QuerySelectorAll(linkSelectors.PagerElementSelector)
                   where e.TextContent.Contains(linkSelectors.PagerNextElementText)
                   select new Uri(QueryStringHelper.FillQueryString(target.ArticleListUrl, target.PageNoQueryString, getNextPageNumber(uri).ToString()));
        }

        private string getLinkArticleNo(AngleSharp.Dom.IElement element)
        {
            return Regex.Match(element.Attributes["href"].Value, $@"{linkSelectors.ArticleLinkJavascriptFunctionName}\(([^,]*),").Groups[1].Value;
        }

        private int getNextPageNumber(Uri uri)
        {
            int currentPageNo;
            if (!int.TryParse(HttpUtility.ParseQueryString(uri.Query).Get(target.PageNoQueryString), out currentPageNo))
            {
                currentPageNo = 0;
            }
            return currentPageNo + pagerButtonCount;
        }
    }
}
