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
            //var crawleTarget = new CrawleTarget(
            //    articleUrl: "http://www.hungryapp.co.kr/bbs/bbs_view.php?bcode=&pid=",
            //    articleNoQuerystring: "pid",
            //    listUrl: "http://www.hungryapp.co.kr/bbs/list.php?bcode=&page=",
            //    pageNoQuerystring: "page",
            //    forumId: "lineagerk",
            //    forumIdQuerystring: "bcode");

            //var crawler = new BuzzCrawler(
            //    crawleTarget,
            //    new ElementSelectors()
            //    {
            //        ContentSelector = ".bbs_cont.contents",
            //        TitleSelector = ".info-area h3 font",
            //        WriteDateSelector = d => DateTime.Parse(d.QuerySelectorAll(".info02 li").FirstOrDefault()?.InnerHtml.Trim().Replace("등록일 : ", "")),
            //        WriterIdSelector = ".info03 .user a"
            //    },
            //    new JavascriptLinkParser(crawleTarget, new JavascriptLinkSelectors()
            //    {
            //        ArticleLinkElementSelector = ".tablef .td02 a",
            //        ArticleLinkJavascriptFunctionName = "sendView",
            //        PagerElementSelector = ".ppaging a",
            //        PagerNextElementText = "다음"
            //    }, 10));

            var crawleTarget = new CrawleTarget(
                articleUrl: "http://bbs.colg.cn/forum.php?mod=viewthread&tid=",
                articleNoQuerystring: "tid",
                listUrl: "http://bbs.colg.cn/forum.php?mod=forumdisplay&fid=&orderby=dateline&filter=author&page=",
                pageNoQuerystring: "page",
                forumId: "393",
                forumIdQuerystring: "fid");

            var crawler = new DiscuzCrawler(DiscuzVersion.X3_1, crawleTarget);
            crawler.OnCrawleComplete += (crawledCount, lastBuzzWriteDate) => Console.WriteLine($"crawled complete. {crawledCount} buzz crawled. last writedate is {lastBuzzWriteDate}");
            crawler.OnNewBuzzCrawled += buzz => Console.WriteLine($"buzz crawled{Environment.NewLine}{JsonConvert.SerializeObject(buzz)}");
            crawler.Crawle();

            Console.ReadLine();
        }
    }
}
