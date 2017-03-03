using AngleSharp.Dom.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler
{
    public interface IBuzzParser
    {
        bool IsListPage(Uri uri);
        bool IsArticlePage(Uri uri);
        long GetContentsNo(Uri uri);
        Func<IHtmlDocument, string> ContentSelector { get; set; }
        Func<IHtmlDocument, string> TitleSelector { get; set; }
        Func<IHtmlDocument, DateTime> WriteDateSelector { get; set; }
        Func<IHtmlDocument, string> WriterIdSelector { get; set; }
    }
}
