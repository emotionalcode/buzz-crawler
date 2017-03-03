using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BuzzCrawler.Shitaraba
{
    public class ShitarabaThreadParser : IBuzzParser
    {
        public Uri CrawleStartUri { get; set; }

        public Func<IHtmlDocument, string> ContentSelector { get; set; }
        public Func<IHtmlDocument, string> TitleSelector { get; set; }
        public Func<IHtmlDocument, DateTime> WriteDateSelector { get; set; }
        public Func<IHtmlDocument, string> WriterIdSelector { get; set; }

        private string baseUrl;

        public ShitarabaThreadParser(
            Uri crawleStartUri,
            string baseUrl, 
            Func<IHtmlDocument, string> ContentSelector,
            Func<IHtmlDocument, string> TitleSelector,
            Func<IHtmlDocument, DateTime> WriteDateSelector,
            Func<IHtmlDocument, string> WriterIdSelector)
        {
            CrawleStartUri = crawleStartUri;
            this.ContentSelector = ContentSelector;
            this.TitleSelector = TitleSelector;
            this.WriteDateSelector = WriteDateSelector;
            this.WriterIdSelector = WriterIdSelector;
            this.baseUrl = baseUrl;
        }

        public long GetContentsNo(Uri uri)
        {
            var paths = uri.LocalPath.Split('/').Skip(5).ToArray();
            long threadId = long.Parse(paths[0]);
            var seq = long.Parse(paths[1]);
            return (threadId * 1000) + seq;
        }

        public bool IsArticlePage(Uri uri)
        {
            Regex regex = new Regex($@"^{this.baseUrl}/\d+/\d+/\d+$");
            return regex.IsMatch(uri.ToString());
        }

        public bool IsListPage(Uri uri)
        {
            if (uri.AbsoluteUri == this.CrawleStartUri.AbsoluteUri)
            {
                return true;
            }
            Regex regex = new Regex($@"^{this.baseUrl}/\d+/\d+/l50$");
            return regex.IsMatch(uri.ToString());
        }
    }
}
