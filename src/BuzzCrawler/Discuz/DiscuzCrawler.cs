using Abot.Core;
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
    public class DiscuzCrawler: BuzzCrawler
    {
        private const string HIDDEN_DIV_SELECTOR = @"[style*=""display:none""], [style*=""display: none""]";
        private const string JAMMER_TEXT_SELECTOR = ".jammer";
        private const string SIGNITURE_TEXT_SELECTOR = ".pstatus";

        public DiscuzCrawler(DiscuzVersion version, CrawleTarget target, ICrawleHistoryRepository crawleHistoryRepository = null) 
            : base(target, null, crawleHistoryRepository: crawleHistoryRepository)
        {
            base.selectors = this.getElementSelectors(version);
            base.documentModify = modifyDocument;
        }

        public void modifyDocument(IHtmlDocument document)
        {
            removeUnnecessaryElements(document);
            replaceImgTagSrcAttribute(document);
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

        private ElementSelectors getElementSelectors(DiscuzVersion version)
        {
            var selector = new ElementSelectors()
            {
                ContentSelector = ".t_f",
                TitleSelector = "#thread_subject",
                WriterIdSelector = ".authi a"
            };
            switch (version)
            {
                //case DiscuzVersion.X2:
                //    selector.WriteDateSelector = d => DateTime.Parse(d.QuerySelectorAll(".authi").Skip(1).First().QuerySelector("span").Attributes["title"].Value).AddHours(1);
                //    break;
                case DiscuzVersion.X3_1:
                    selector.WriteDateSelector = d => DateTime.Parse(d.QuerySelectorAll(".authi").Skip(1).First().QuerySelector("em").InnerHtml.Replace("发表于 ", "")).AddHours(1);
                    break;
                case DiscuzVersion.X2:
                case DiscuzVersion.X3_2:
                    selector.WriteDateSelector = d => DateTime.Parse(d.QuerySelectorAll(".authi").Skip(1).First().QuerySelector("span[title]").Attributes["title"].Value).AddHours(1);
                    break;
                default:
                    throw new Exception($"{version} is not supported");
            }
            return selector;
        }
    }
}
