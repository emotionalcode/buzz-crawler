using System;
using BuzzCrawler.Discuz;
using Xunit;

namespace BuzzCrawler.Tests.Discuz
{
    public class DiscuzCrawleOptionTest
    {
        [Fact]
        public void ArticlePageUrl_getter()
        {
            var expected = "http://www.discuzsample.com/forum.php?mod=viewthread&tid=";


            var crawleOption = new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com"
            };
            Assert.Equal(expected, crawleOption.ArticlePageUrl);
        }

        [Fact]
        public void ArticleListPageUrl_getter()
        {
            var expected = "http://www.discuzsample.com/forum.php?mod=forumdisplay&fid=100&orderby=dateline&filter=author&page=";
            var crawleOption = new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 100
            };
            Assert.Equal(expected, crawleOption.ArticleListPageUrl);
        }
    }
}
