using AngleSharp.Dom.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler
{
    public class ElementSelectors
    {
        public Func<IHtmlDocument, DateTime> WriteDateSelector { get; set; }

        public string ContentSelector { get; set; }
        public string TitleSelector { get; set; }
        public string WriterIdSelector { get; set; }
    }
}
