using AngleSharp.Dom.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler.Discuz
{
    public class DiscuzCrawleOption
    {
        /// <summary>
        /// base url to crawle.
        /// <value>ex) http://bbs.colg.cn</value>
        /// </summary>
        public string BaseUrl { get; set; }
        public int ForumId { get; set; }

        public ICrawleHistoryRepository CrawleHistoryRepository { get; set; }
        public DiscuzVersion DiscuzVersion { get; set; }
        public CrawleRange CrawleRange { get; set; }

        public string ArticlePageUrl
        {
            get
            {
                return $"{BaseUrl}/forum.php?mod=viewthread&tid=";
            }
        }

        public string ArticleListPageUrl
        {
            get
            {
                return $"{BaseUrl}/forum.php?mod=forumdisplay&fid={ForumId}&orderby=dateline&filter=author&page=";
            }
        }

        public Func<IHtmlDocument, string> WriteDateSelector
        {
            get
            {
                switch (this.DiscuzVersion)
                {
                    case DiscuzVersion.X3_1:
                        return d => d.QuerySelectorAll(".authi").Skip(1).First().QuerySelector("em").InnerHtml.Replace("发表于 ", "");
                    case DiscuzVersion.X2:
                    case DiscuzVersion.X3_2:
                        return d => d.QuerySelectorAll(".authi").Skip(1).First().QuerySelector("span").Attributes["title"].Value;
                    default:
                        throw new Exception($"not supported version : {this.DiscuzVersion}");
                }
            }
        }
    }
}
