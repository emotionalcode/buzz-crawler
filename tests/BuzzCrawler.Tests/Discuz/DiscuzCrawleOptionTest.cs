using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BuzzCrawler.Discuz;

namespace BuzzCrawler.Tests.Discuz
{
    [TestClass]
    public class DiscuzCrawleOptionTest
    {
        [TestMethod]
        public void ArticlePageUrl_getter()
        {
            var expected = "http://www.discuzsample.com/forum.php?mod=viewthread&tid=";


            var crawleOption = new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com"
            };
            Assert.AreEqual(expected, crawleOption.ArticlePageUrl);
        }

        [TestMethod()]
        public void ArticleListPageUrl_getter()
        {
            var expected = "http://www.discuzsample.com/forum.php?mod=forumdisplay&fid=100&orderby=dateline&filter=author&page=";
            var crawleOption = new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 100
            };
            Assert.AreEqual(expected, crawleOption.ArticleListPageUrl);
        }
    }
}
