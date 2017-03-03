using Abot.Core;
using Abot.Poco;
using AngleSharp.Dom.Html;
using BuzzCrawler.Infrastructure;
using BuzzCrawler.Shitaraba;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BuzzCrawler
{
    public static class BuzzCrawlerFactory
    {
        private const string DISCUZ_HIDDEN_DIV_SELECTOR = @"[style*=""display:none""], [style*=""display: none""]";
        private const string DISCUZ_JAMMER_TEXT_SELECTOR = ".jammer";
        private const string DISCUZ_SIGNITURE_TEXT_SELECTOR = ".pstatus";
        private const string DISCUZ_LOGIN_LINK_SELECTOR = @"a[href*=""member.php?mod=logging&action=login""]";

        public static void ModifyDiscuzDocument(IHtmlDocument document)
        {
            removeUnnecessaryElements(document);
            replaceImgTagSrcAttribute(document);
        }

        private static void removeUnnecessaryElements(IHtmlDocument document)
        {
            var unnecessaryElements = document.QuerySelectorAll($"{DISCUZ_HIDDEN_DIV_SELECTOR}, {DISCUZ_JAMMER_TEXT_SELECTOR}, {DISCUZ_SIGNITURE_TEXT_SELECTOR}, {DISCUZ_LOGIN_LINK_SELECTOR}");
            foreach (var element in unnecessaryElements)
            {
                element.Remove();
            }
        }

        private static void replaceImgTagSrcAttribute(IHtmlDocument document)
        {
            var imgTags = document.QuerySelector(".t_f").QuerySelectorAll("img[file]");
            foreach (var item in imgTags)
            {
                item.SetAttribute("src", item.Attributes["file"].Value);
            }
        }

        public static IBuzzCrawler GetBBSCrawler(BBSType version, BBSCrawleTarget target, ICrawleHistoryRepository crawleHistoryRepository = null)
        {
            switch (version)
            {
                case BBSType.Discuz_v2:
                    return new BuzzCrawler(
                        new Uri(QueryStringHelper.FillQueryString(target.ArticleListUrl, target.PageNoQueryString, "1")),
                        new BBSParser(
                            target,
                            ContentSelector: d => { return d.QuerySelector(".t_f")?.InnerHtml.Trim(); },
                            TitleSelector: d => { return d.QuerySelectorAll("#thread_subject").FirstOrDefault()?.InnerHtml.Trim(); },
                            WriteDateSelector: d => { return DateTime.Parse(d.QuerySelectorAll(".authi").Skip(1).First().QuerySelector("span[title]").Attributes["title"].Value).AddHours(1); },
                            WriterIdSelector: d => { return d.QuerySelectorAll(".authi a").First().InnerHtml; }),
                        crawleHistoryRepository: crawleHistoryRepository)
                    {
                        DocumentModify = ModifyDiscuzDocument
                    };
                case BBSType.Discuz_v3_1:
                    return new BuzzCrawler(
                        new Uri(QueryStringHelper.FillQueryString(target.ArticleListUrl, target.PageNoQueryString, "1")),
                        new BBSParser(
                            target,
                            ContentSelector: d => { return d.QuerySelector(".t_f")?.InnerHtml.Trim(); },
                            TitleSelector: d => { return d.QuerySelectorAll("#thread_subject").FirstOrDefault()?.InnerHtml.Trim(); },
                            WriteDateSelector: d => { return DateTime.Parse(d.QuerySelectorAll(".authi").Skip(1).First().QuerySelector("em").InnerHtml.Replace("发表于 ", "")).AddHours(1); },
                            WriterIdSelector: d => { return d.QuerySelectorAll(".authi a").First().InnerHtml; }),
                        crawleHistoryRepository: crawleHistoryRepository)
                        {
                            DocumentModify = ModifyDiscuzDocument
                        };
                case BBSType.Discuz_v3_2:
                    return new BuzzCrawler(
                        new Uri(QueryStringHelper.FillQueryString(target.ArticleListUrl, target.PageNoQueryString, "1")),
                        new BBSParser(
                            target,
                            ContentSelector: d => { return d.QuerySelector(".t_f")?.InnerHtml.Trim(); },
                            TitleSelector: d => { return d.QuerySelectorAll("#thread_subject").FirstOrDefault()?.InnerHtml.Trim(); },
                            WriteDateSelector: d => { return DateTime.Parse(d.QuerySelectorAll(".authi").Skip(1).First().QuerySelector("span[title]").Attributes["title"].Value).AddHours(1); },
                            WriterIdSelector: d => { return d.QuerySelectorAll(".authi a").First().InnerHtml; }),
                        crawleHistoryRepository: crawleHistoryRepository)
                        {
                            DocumentModify = ModifyDiscuzDocument
                        };
                case BBSType.HungryApp:
                    return new BuzzCrawler(
                        new Uri(QueryStringHelper.FillQueryString(target.ArticleListUrl, target.PageNoQueryString, "1")),
                        new BBSParser(
                            target,
                            ContentSelector: d => { return d.QuerySelector(".bbs_cont.contents")?.InnerHtml.Trim(); },
                            TitleSelector: d => { return d.QuerySelectorAll(".info-area h3 font").FirstOrDefault()?.InnerHtml.Trim(); },
                            WriteDateSelector: d => { return DateTime.Parse(d.QuerySelectorAll(".info02 li").FirstOrDefault()?.InnerHtml.Trim().Replace("등록일 : ", "")); },
                            WriterIdSelector: d => { return d.QuerySelectorAll(".info03 .user a").First().InnerHtml; }),
                        new JavascriptLinkParser(target, new JavascriptLinkSelectors()
                        {
                            ArticleLinkElementSelector = ".tablef .td02 a",
                            ArticleLinkJavascriptFunctionName = "sendView",
                            PagerElementSelector = ".ppaging a",
                            PagerNextElementText = "다음"
                        }, 10),
                        crawleHistoryRepository: crawleHistoryRepository);
                default:
                    throw new Exception($"{version} is not supported");
            }
        }

        public static IBuzzCrawler GetShitarabaCrawler(Uri crawleStartUri, string baseUrl, ICrawleHistoryRepository crawleHistoryRepository = null)
        {
            CrawlConfiguration crawlConfig = AbotConfigurationSectionHandler.LoadFromXml().Convert();
            crawlConfig.MinCrawlDelayPerDomainMilliSeconds = 1000;
            return new BuzzCrawler(
                        crawleStartUri,
                        new ShitarabaThreadParser(
                            crawleStartUri,
                            baseUrl,
                            ContentSelector: d => { return d.QuerySelector("#thread-body dd")?.InnerHtml.Trim(); },
                            TitleSelector: d => { return d.QuerySelectorAll("h1.thread-title").FirstOrDefault()?.InnerHtml.Trim(); },
                            WriteDateSelector: d =>
                            {
                                DateTime writeDate = DateTime.Now;
                                CultureInfo provider = new CultureInfo("ja-JP");
                                Regex regx = new Regex(@"\d\d\d\d/\d\d/\d\d\(\w\) \d\d:\d\d:\d\d");
                                var matchs = regx.Matches(d.QuerySelector("#thread-body dt").InnerHtml);
                                if (matchs.Count == 1)
                                {
                                    writeDate = DateTime.Parse(matchs[0].Value, provider);
                                }
                                return writeDate;
                            },
                            WriterIdSelector: d => { return ""; }),
                        crawleHistoryRepository: crawleHistoryRepository,
                        abotConfig: crawlConfig);
        }
    }

    public enum BBSType
    {
        /// <summary>
        /// ex) duowan
        /// </summary>
        Discuz_v2,
        /// <summary>
        /// ex) colg
        /// </summary>
        Discuz_v3_1,
        /// <summary>
        /// ex) qq
        /// </summary>
        Discuz_v3_2,
        HungryApp
    }
}
