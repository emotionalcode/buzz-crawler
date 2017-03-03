using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using BuzzCrawler.Discuz;
using Abot.Poco;
using System.Threading;
using BuzzCrawler.Shitaraba;

namespace BuzzCrawler.Tests.Shitaraba
{
    public class ShitarabaParserTest
    {
        [Fact]
        public void GetContentsNo()
        {
            var parser = new ShitarabaThreadParser(new Uri("http://jbbs.shitaraba.net/netgame/1111"), "http://jbbs.shitaraba.net/bbs/read.cgi/netgame", null, null, null, null);
            parser.GetContentsNo(new Uri("http://jbbs.shitaraba.net/bbs/read.cgi/netgame/1111/12345/123")).Should().Be(12345123L);
            parser.GetContentsNo(new Uri("http://jbbs.shitaraba.net/bbs/read.cgi/netgame/1111/5432/42")).Should().Be(5432042L);
        }

        [Fact]
        public void IsArticlePage()
        {
            var parser = new ShitarabaThreadParser(new Uri("http://jbbs.shitaraba.net/netgame/1111"), "http://jbbs.shitaraba.net/bbs/read.cgi/netgame", null, null, null, null);
            parser.IsArticlePage(new Uri("http://jbbs.shitaraba.net/bbs/read.cgi/netgame/1111/12345/123")).Should().BeTrue();
            parser.IsArticlePage(new Uri("http://jbbs.shitaraba.net/bbs/read.cgi/netgame/1111/5432/42")).Should().BeTrue();
            parser.IsArticlePage(new Uri("http://jbbs.shitaraba.net/bbs/read.cgi/netgame/1111/5432/test")).Should().BeFalse("because thread id is not a number");
            parser.IsArticlePage(new Uri("http://jbbs.shitaraba.net/bbs/read.cgi/netgame/1111/5432/l50")).Should().BeFalse("because this url is 'view last 50 item' page");
            parser.IsArticlePage(new Uri("http://jbbs.shitaraba.net/bbs/read.cgi/netgame/1111/5432")).Should().BeFalse("because thread id not included");
        }

        [Fact]
        public void IsListPage()
        {
            var parser = new ShitarabaThreadParser(new Uri("http://jbbs.shitaraba.net/netgame/1111"), "http://jbbs.shitaraba.net/bbs/read.cgi/netgame", null, null, null, null);
            parser.IsListPage(new Uri("http://jbbs.shitaraba.net/bbs/read.cgi/netgame/1111/12345/123")).Should().BeFalse();
            parser.IsListPage(new Uri("http://jbbs.shitaraba.net/bbs/read.cgi/netgame/1111/5432/test")).Should().BeFalse();
            parser.IsListPage(new Uri("http://jbbs.shitaraba.net/bbs/read.cgi/netgame/1111/5432/l50")).Should().BeTrue("because this url is 'view last 50 item' page");
        }
    }

}
