﻿using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BuzzCrawler.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            crawleDuowanSite();

            //crawleColgSite();

            //crawleQqSite();

            //crawleHungryAppSite();

            //crawleShitarabaSite();

            //crawleNaverBlog();

            Console.ReadLine();
        }

        

        private static void crawleDuowanSite()
        {
            var duowanCrawler = BuzzCrawlerFactory.GetBBSCrawler(
                BBSType.Discuz_v2,
                new BBSCrawleTarget(
                    articleUrl: "http://bbs.duowan.com/forum.php?mod=viewthread&tid=",
                    articleNoQuerystring: "tid",
                    listUrl: "http://bbs.duowan.com/forum.php?mod=forumdisplay&fid=&orderby=dateline&filter=author&page=",
                    pageNoQuerystring: "page",
                    forumId: "774",
                    forumIdQuerystring: "fid",
                    maxListPageNo: 999,
                    startArticleNo: 45480215));
            duowanCrawler.OnCrawleComplete += (crawledCount, lastBuzzWriteDate) => Console.WriteLine($"crawled complete. {crawledCount} buzz crawled. last writedate is {lastBuzzWriteDate}");
            duowanCrawler.OnNewBuzzCrawled += buzz => Console.WriteLine($"buzz crawled{Environment.NewLine}{JsonConvert.SerializeObject(buzz)}");
            duowanCrawler.Crawle();
        }

        private static void crawleColgSite()
        {
            var colgCrawler = BuzzCrawlerFactory.GetBBSCrawler(
                BBSType.Discuz_v3_1,
                new BBSCrawleTarget(
                    articleUrl: "http://bbs.colg.cn/forum.php?mod=viewthread&tid=",
                    articleNoQuerystring: "tid",
                    listUrl: "http://bbs.colg.cn/forum.php?mod=forumdisplay&fid=&orderby=dateline&filter=author&page=",
                    pageNoQuerystring: "page",
                    forumId: "171",
                    forumIdQuerystring: "fid",
                    maxListPageNo: 999,
                    startArticleNo: 5836878));
            colgCrawler.OnCrawleComplete += (crawledCount, lastBuzzWriteDate) => Console.WriteLine($"crawled complete. {crawledCount} buzz crawled. last writedate is {lastBuzzWriteDate}");
            colgCrawler.OnNewBuzzCrawled += buzz => Console.WriteLine($"buzz crawled{Environment.NewLine}{JsonConvert.SerializeObject(buzz)}");
            colgCrawler.Crawle();
        }

        private static void crawleQqSite()
        {
            var qqCrawler = BuzzCrawlerFactory.GetBBSCrawler(
                BBSType.Discuz_v3_2,
                new BBSCrawleTarget(
                    articleUrl: "http://dnf.gamebbs.qq.com/forum.php?mod=viewthread&tid=",
                    articleNoQuerystring: "tid",
                    listUrl: "http://dnf.gamebbs.qq.com/forum.php?mod=forumdisplay&fid=&orderby=dateline&filter=author&page=",
                    pageNoQuerystring: "page",
                    forumId: "43",
                    forumIdQuerystring: "fid",
                    maxListPageNo: 999,
                    startArticleNo: 117805));
            qqCrawler.OnCrawleComplete += (crawledCount, lastBuzzWriteDate) => Console.WriteLine($"crawled complete. {crawledCount} buzz crawled. last writedate is {lastBuzzWriteDate}");
            qqCrawler.OnNewBuzzCrawled += buzz => Console.WriteLine($"buzz crawled{Environment.NewLine}{JsonConvert.SerializeObject(buzz)}");
            qqCrawler.Crawle();
        }

        private static void crawleHungryAppSite()
        {
            var hungryAppCrawler = BuzzCrawlerFactory.GetBBSCrawler(
                BBSType.HungryApp,
                new BBSCrawleTarget(
                    articleUrl: "http://www.hungryapp.co.kr/bbs/bbs_view.php?bcode=&pid=",
                    articleNoQuerystring: "pid",
                    listUrl: "http://www.hungryapp.co.kr/bbs/list.php?bcode=&page=",
                    pageNoQuerystring: "page",
                    forumId: "dnfsoul",
                    maxListPageNo: 999,
                    forumIdQuerystring: "bcode",
                    startArticleNo: 4781));
            hungryAppCrawler.OnCrawleComplete += (crawledCount, lastBuzzWriteDate) => Console.WriteLine($"crawled complete. {crawledCount} buzz crawled. last writedate is {lastBuzzWriteDate}");
            hungryAppCrawler.OnNewBuzzCrawled += buzz => Console.WriteLine($"buzz crawled{Environment.NewLine}{JsonConvert.SerializeObject(buzz)}");
            hungryAppCrawler.Crawle();
        }

        private static void crawleShitarabaSite()
        {
            var shitarabaCrawler = BuzzCrawlerFactory.GetShitarabaCrawler(new Uri("http://jbbs.shitaraba.net/netgame/6969"), "http://jbbs.shitaraba.net/bbs/read.cgi/netgame");
            shitarabaCrawler.OnCrawleComplete += (crawledCount, lastBuzzWriteDate) => Console.WriteLine($"crawled complete. {crawledCount} buzz crawled. last writedate is {lastBuzzWriteDate}");
            shitarabaCrawler.OnNewBuzzCrawled += buzz => Console.WriteLine($"buzz crawled{Environment.NewLine}{JsonConvert.SerializeObject(buzz)}");
            shitarabaCrawler.Crawle();
        }

        private static void crawleNaverBlog()
        {
            var crawler = new NaverBlogCrawler("keyword", "your-client-id", "your-client-secret");

            crawler.OnNewBuzzCrawled += buzz => Console.WriteLine($"buzz crawled{Environment.NewLine}{JsonConvert.SerializeObject(buzz)}");
            crawler.Crawle();
        }

    }
}
