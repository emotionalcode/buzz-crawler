using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler
{
    public class NaverBlogResponse
    {
        public DateTime LastBuildDate { get; set; }
        public int Total { get; set; }
        public int Start { get; set; }
        public int Display { get; set; }
        public List<NaverBlogItem> Items { get; set; }
    }
}
