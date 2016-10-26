using BuzzCrawler.Discuz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abot.Poco;
using System.Text.RegularExpressions;
using Xunit;

namespace BuzzCrawler.Discuz.Tests
{
    public class DiscuzCrawlerTests
    {
        #region shouldCrawlPage
        [Fact]
        public void shouldCrawlPage_neitherArticleOrListPage_allowFalse()
        {
            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { MaxListPageNo = 1000, StartArticleNo = 0 }
            });

            var actual = crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://test.com") }, null);
            Assert.False(actual.Allow);
        }

        [Fact]
        public void shouldCrawlPage_listPage_outOfScope_allowFalse()
        {
            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { MaxListPageNo = 10 }
            });

            var actual = crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/forum.php?mod=forumdisplay&fid=777&orderby=dateline&filter=author&page=11") }, null);
            Assert.False(actual.Allow);
            var actual2 = crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/forum.php?mod=forumdisplay&fid=777&orderby=dateline&filter=author&page=12") }, null);
            Assert.False(actual2.Allow);
        }

        [Fact]
        public void shouldCrawlPage_listPage_inScope_allowTrue()
        {
            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { MaxListPageNo = 10 }
            });
            var actual = crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/forum.php?mod=forumdisplay&fid=777&orderby=dateline&filter=author&page=10") }, null);
            Assert.True(actual.Allow);
            var actual2 = crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/forum.php?mod=forumdisplay&fid=777&orderby=dateline&filter=author&page=9") }, null);
            Assert.True(actual2.Allow);
        }

        [Fact]
        public void shouldCrawlPage_articlePage_outOfScope_allowFalse()
        {
            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 10 }
            });

            var actual = crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=10") }, null);
            Assert.False(actual.Allow);

            var actual2 = crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=9") }, null);
            Assert.False(actual2.Allow);
        }

        [Fact]
        public void shouldCrawlPage_articlePage_inScope_allowTrue()
        {
            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 10 }
            });

            var actual = crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=11") }, null);
            Assert.True(actual.Allow);
        }

        [Fact]
        public void shouldCrawlPage_articlePage_alreadyCrawled_allowFalse()
        {
            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(true),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0 }
            });

            var actual = crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=1") }, null);
            Assert.False(actual.Allow);
        }

        [Fact]
        public void shouldCrawlPage_articlePage_withComments_allowFalse()
        {
            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0 }
            });

            var actual = crawler.shouldCrawlPage(new PageToCrawl() { Uri = new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=1&page=2") }, null);
            Assert.False(actual.Allow);
        }
        #endregion

        #region pageCrawlCompletedAsync
        [Fact]
        public void pageCrawlCompleted_webException_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com"));
            crawledPage.WebException = new System.Net.WebException("test");
            
            //using (ShimsContext.Create())
            //{
            //    ShimDiscuzCrawler.AllInstances.DebugLogString = (discuzCrawler, txt) =>
            //    {
            //        Assert.Equal($"Crawl of page failed - web exception {crawledPage.Uri.AbsoluteUri} : {crawledPage.WebException.ToString()}", txt);
            //    };

                var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
                {
                    BaseUrl = "http://www.discuzsample.com",
                    ForumId = 777,
                    CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                    DiscuzVersion = DiscuzVersion.X2,
                    CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
                });

                crawler.OnNewBuzzCrawled += (result) =>
                {
                    Assert.True(false, "web exception throwed, should be ignored");
                };

                crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            //}
        }

        [Fact]
        public void pageCrawlCompleted_httpResponse502_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com"));
            crawledPage.HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.BadGateway, "html", null, null);

            //using (ShimsContext.Create())
            //{
            //    ShimDiscuzCrawler.AllInstances.DebugLogString = (discuzCrawler, txt) =>
                //{
                //    Assert.Equal($"Crawl of page failed - status {crawledPage.Uri.AbsoluteUri} : {crawledPage.HttpWebResponse.StatusCode.ToString()}", txt);
                //};

                var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
                {
                    BaseUrl = "http://www.discuzsample.com",
                    ForumId = 777,
                    CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                    DiscuzVersion = DiscuzVersion.X2,
                    CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
                });

                crawler.OnNewBuzzCrawled += (result) =>
                {
                    Assert.True(false, "http response was 502, should be ignored");
                };
                crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            //}
        }

        [Fact]
        public void pageCrawlCompleted_listPage_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/forum.php?mod=forumdisplay&fid=777&orderby=dateline&filter=author&page=1"));
            crawledPage.HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null);
            
            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
            });

            crawler.OnNewBuzzCrawled += a =>
            {
                Assert.True(false, "list page, should be ignored");
            };

            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_documentIsNull_ignore()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com"));
            crawledPage.HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null);


            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
            });

            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.True(false, "document is null, should be ignored");
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_unnecessaryHiddenDiv_removed()
        {
            var testContent = @"
                            <div id=""thread_subject"">title</div>
                            <div class=""authi""><a>writerId</a></div>
                            <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div>
                            <div class=""t_f"">
                                test
                                <div style=""display:none"">hiddenDiv</div>
                                <div style=""display:none;"">hiddenDiv</div>
                                <div style=""display:none;background-color:red;"">hiddenDiv</div>
                                <div style=""display:none; background-color:red;"">hiddenDiv</div>
                                <div style=""background-color:red;display:none;"">hiddenDiv</div>
                                <div style=""background-color:red;display:none"">hiddenDiv</div>
                                <div style=""background-color:red; display:none;"">hiddenDiv</div>
                                <div style=""background-color:red; display:none"">hiddenDiv</div>
                                <div style='display:none'>hiddenDiv</div>
                                <div style='display:none;'>hiddenDiv</div>
                                <div style='display:none;background-color:red;'>hiddenDiv</div>
                                <div style='display:none; background-color:red;'>hiddenDiv</div>
                                <div style='background-color:red;display:none;'>hiddenDiv</div>
                                <div style='background-color:red;display:none'>hiddenDiv</div>
                                <div style='background-color:red; display:none;'>hiddenDiv</div>
                                <div style='background-color:red; display:none'>hiddenDiv</div>
                                
                                <div style=""display: none"">hiddenDiv</div>
                                <div style=""display: none;"">hiddenDiv</div>
                                <div style=""display: none;background-color:red;"">hiddenDiv</div>
                                <div style=""display: none; background-color:red;"">hiddenDiv</div>
                                <div style=""background-color:red;display: none;"">hiddenDiv</div>
                                <div style=""background-color:red;display: none"">hiddenDiv</div>
                                <div style=""background-color:red; display: none;"">hiddenDiv</div>
                                <div style=""background-color:red; display: none"">hiddenDiv</div>
                                <div style='display: none'>hiddenDiv</div>
                                <div style='display: none;'>hiddenDiv</div>
                                <div style='display: none;background-color:red;'>hiddenDiv</div>
                                <div style='display: none; background-color:red;'>hiddenDiv</div>
                                <div style='background-color:red;display: none;'>hiddenDiv</div>
                                <div style='background-color:red;display: none'>hiddenDiv</div>
                                <div style='background-color:red; display: none;'>hiddenDiv</div>
                                <div style='background-color:red; display: none'>hiddenDiv</div>
                                <span>test</span>
                            </div>";
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=10"));
            crawledPage.HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null);
            crawledPage.Content.Text = testContent;


            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
            });

            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.Equal("test<span>test</span>", Regex.Replace(result.Content.Trim(), @"\t|\n|\r", string.Empty).Replace(" ", string.Empty));
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_unnecessaryJammerText_removed()
        {
            var testContent = @"
                            <div id=""thread_subject"">title</div>
                            <div class=""authi""><a>writerId</a></div>
                            <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div>
                            <div class=""t_f"">
                                test
                                <div class=""jammer"">jammerText</div>
                                <div class='jammer anotherClass'>jammerText</div>
                                <span>test</span>
                            </div>";
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=10"));
            crawledPage.HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null);
            crawledPage.Content.Text = testContent;

            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
            });

            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.Equal("test<span>test</span>", Regex.Replace(result.Content.Trim(), @"\t|\n|\r", string.Empty).Replace(" ", string.Empty));
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_unnecessarySignitureText_removed()
        {
            var testContent = @"
                            <div id=""thread_subject"">title</div>
                            <div class=""authi""><a>writerId</a></div>
                            <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div>
                            <div class=""t_f"">
                                test
                                <div class=""pstatus"">jammerText</div>
                                <div class='pstatus anotherClass'>jammerText</div>
                                <span>test</span>
                            </div>";
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=10"));
            crawledPage.HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null);
            crawledPage.Content.Text = testContent;

            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
            });

            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.Equal("test<span>test</span>", Regex.Replace(result.Content.Trim(), @"\t|\n|\r", string.Empty).Replace(" ", string.Empty));
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_imgTagSrcAttr_replace()
        {
            var expected = @"如题有些人就是矫情<br>
                    <img id=""aimg_37446495"" file=""http://www.discuzsample.com/forum/201610/24/060757jwdhc5b8wwp4e8je.jpg"" class=""zoom"" onclick=""zoom(this, this.src)"" width=""519"" inpost=""1"" alt=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" title=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" initialized=""true"" src=""http://www.discuzsample.com/forum/201610/24/060757jwdhc5b8wwp4e8je.jpg"">";
            var testContent = @"<html><body><div id=""thread_subject"">title</div>
                            <div class=""authi""><a>writerId</a></div>
                            <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div><table><tr><td class=""t_f"" id=""postmessage_684661805"">
                    如题有些人就是矫情<br><img id=""aimg_37446495"" file=""http://www.discuzsample.com/forum/201610/24/060757jwdhc5b8wwp4e8je.jpg"" class=""zoom"" onclick=""zoom(this, this.src)"" width=""519"" inpost=""1"" alt=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" title=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" initialized=""true""></td></tr></table></body></html>";
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=10"));
            crawledPage.HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null);
            crawledPage.Content.Text = testContent;

            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
            });

            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.Equal(getTrimmedText(expected), getTrimmedText(result.Content));
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }
        #endregion

        [Fact]
        public void pageCrawlCompleted_parseError_ignore()
        {
            var expected = @"如题有些人就是矫情<br>
                    <img id=""aimg_37446495"" file=""http://www.discuzsample.com/forum/201610/24/060757jwdhc5b8wwp4e8je.jpg"" class=""zoom"" onclick=""zoom(this, this.src)"" width=""519"" inpost=""1"" alt=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" title=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" initialized=""true"" src=""http://www.discuzsample.com/forum/201610/24/060757jwdhc5b8wwp4e8je.jpg"">";
            var testContent = @"<html><body><div id=""thread_subject"">title</div>
                            <div class=""authi""></div>
                            <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div><table><tr><td class=""t_f"" id=""postmessage_684661805"">
                    如题有些人就是矫情<br><img id=""aimg_37446495"" file=""http://www.discuzsample.com/forum/201610/24/060757jwdhc5b8wwp4e8je.jpg"" class=""zoom"" onclick=""zoom(this, this.src)"" width=""519"" inpost=""1"" alt=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" title=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" initialized=""true""></td></tr></table></body></html>";
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=10"));
            crawledPage.HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null);
            crawledPage.Content.Text = testContent;


            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
            });

            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.True(false, "when parse error, should be ignored");
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_realDuowanBuzz()
        {
            var expected = new Buzz()
            {
                Content = @"如题有些人就是矫情<br>
                    1、一被制裁360小时就吵着脱坑，好像谁没被制裁一样。（我就不信你不回来）<br>
                    2、我有荒古和无用带哪个好？萌新求解。（花式装逼）<br>
                    3、我第一把爆手搓光可是我刚玩剑魂我不会搓垃圾。（矫情有本事别捡啊）<br>
                    4、都被制裁15天了还去刷深渊的而且出货了。有可能还是四缺一的还没法捡（纯属作死）<br>
                    还有什么请大家补充<br>
                    <br>",
                ContentsNo = 10,
                Link = "",
                Title = "title",
                WriteDate = DateTime.Parse("2016-10-24 01:00:00"),
                WriterId = "writerId"
            };
            var testContent = @"<html><body><div id=""thread_subject"">title</div>
                            <div class=""authi""><a>writerId</a></div>
                            <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div><table><tr><td class=""t_f"" id=""postmessage_684661805"">
                    如题有些人就是矫情<br>
                    <span style=""display:none"">, z/ ^' \/ F! @' p0 ~$ W. E3 [</span>1、一被制裁360小时就吵着脱坑，好像谁没被制裁一样。（我就不信你不回来）<font class=""jammer"">- ]$ }- C! m/ ?3 k/ O+ x"" t</font><br>
                    2、我有荒古和无用带哪个好？萌新求解。（花式装逼）<font class=""jammer"">9 ?$ I7 m3 [( d9 |4 T</font><br>
                    3、我第一把爆手搓光可是我刚玩剑魂我不会搓垃圾。（矫情有本事别捡啊）<br>
                    <span style=""display:none""># m1 P0 m7 W8 }. X</span>4、都被制裁15天了还去刷深渊的而且出货了。有可能还是四缺一的还没法捡（纯属作死）<font class=""jammer"">&amp; \"" q&nbsp;&nbsp;l; L&amp; r9 R2 j</font><br>
                    还有什么请大家补充<br>
                    <span style=""display:none"">"" l7 _+ M2 _6 O$ ["" d6 O$ Y/ M</span><i class=""pstatus""> 本帖最后由 血牛233 于 2016-10-11 15:59 编辑 </i><br>
                    <span style=""display:none"">&nbsp;&nbsp;]; f"" s/ X. ?1 ^, w&nbsp;&nbsp;k</span></td></tr></table></body></html>";
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=10"));
            crawledPage.HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null);
            crawledPage.Content.Text = testContent;

            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
            });

            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.Equal(getTrimmedText(expected.Content), getTrimmedText(result.Content));
                Assert.Equal(expected.ContentsNo, result.ContentsNo);
                Assert.Equal(expected.Link, result.Link);
                Assert.Equal(expected.Title, result.Title);
                Assert.Equal(expected.WriteDate, result.WriteDate);
                Assert.Equal(expected.WriterId, result.WriterId);
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        [Fact]
        public void pageCrawlCompleted_realColgBuzz()
        {
            var expected = new Buzz()
            {
                Content = @"<table cellspacing=""0"" class=""t_table"" style=""width: 75%;"" bgcolor=""mediumslateblue""><tbody><tr><td><table cellspacing=""0"" class=""t_table"" style=""width: 98%;"" bgcolor=""mistyrose""><tbody><tr><td><img id=""aimg_i4i4E"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20161025/07/52350242201610250756233290499415347_000.jpg""><br>
</td></tr></tbody></table><br>
<br>
<table cellspacing=""0"" class=""t_table"" style=""width: 98%;"" bgcolor=""silver""><tbody><tr><td><img id=""aimg_LA4vJ"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20130620/12/52350242201306201232562837070490111_002.jpg""></td><td><strong>高级钻石硬币 </strong></td><td>1</td></tr><tr><td><img id=""aimg_B2ahj"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_020.jpg""></td><td><strong>钻石硬币 </strong></td><td>5</td></tr><tr><td><img id=""aimg_if5fz"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20150921/07/52350242201509210723501907234578385_001.jpg?30x31_120""></td><td><strong>数据芯片</strong></td><td>25</td></tr><tr><td><img id=""aimg_imZjC"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20150624/20/52350242201506242020452424355124162_001.jpg?30x29_120""></td><td><strong>遗忘陨石</strong></td><td>91</td></tr><tr><td><img id=""aimg_CI4F5"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20130203/17/52350242201302031725539505679242199_001.jpg""></td><td><strong>无尽的永恒</strong></td><td>23</td></tr><tr><td><img id=""aimg_Z54jz"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20130203/17/52350242201302031725539505679242199_004.jpg""></td><td><strong>深渊派对邀请函</strong></td><td>24</td></tr><tr><td><img id=""aimg_RV0I2"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20130203/17/52350242201302031725539505679242199_000.jpg""></td><td><strong>深渊派对邀请书</strong></td><td>5</td></tr><tr><td><img id=""aimg_y1xf4"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_008.jpg""></td><td><strong>赫仑皇帝的印章</strong></td><td>15</td></tr><tr><td><img id=""aimg_B4x5Z"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_004.jpg""></td><td><strong>金刚石</strong></td><td>0</td></tr><tr><td><img id=""aimg_k2f40"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_002.jpg""></td><td><strong>血滴石</strong></td><td>0</td></tr><tr><td><img id=""aimg_vJX5M"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_003.jpg""></td><td><strong>海蓝宝石 </strong></td><td>1</td></tr><tr><td><img id=""aimg_PmF01"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_001.jpg""></td><td><strong>紫玛瑙</strong></td><td>2</td></tr><tr><td><img id=""aimg_yZa1J"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_000.jpg""></td><td><strong>黑曜石</strong></td><td>0</td></tr><tr><td><img id=""aimg_QHjXZ"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_006.jpg""></td><td><strong>碎布片</strong></td><td>12</td></tr><tr><td><img id=""aimg_g4a4F"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_018.jpg""></td><td><strong>最下级硬化剂</strong></td><td>8</td></tr><tr><td><img id=""aimg_I4Qm4"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_007.jpg""></td><td><strong>破旧的皮革</strong></td><td>12</td></tr><tr><td><img id=""aimg_L4M1H"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_017.jpg""></td><td><strong>风化的碎骨</strong></td><td>8</td></tr><tr><td><img id=""aimg_B5QJ5"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_005.jpg""></td><td><strong>生锈的铁片</strong></td><td>10</td></tr><tr><td><img id=""aimg_z5MFJ"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_019.jpg""></td><td><strong>最下级砥石</strong></td><td>9</td></tr><tr><td><img id=""aimg_j22z4"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191532373030237679919_000.jpg""></td><td><strong>诺顿的达人HP药剂</strong></td><td>3</td></tr><tr><td><img id=""aimg_GcczV"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191532373030237679919_001.jpg""></td><td><strong>罗莉安的达人MP药剂</strong> </td><td>5</td></tr></tbody></table></td></tr></tbody></table><br>
<table cellspacing=""0"" class=""t_table"" style=""width: 400px;"" bgcolor=""palevioletred""><tbody><tr><td><br>
<table cellspacing=""0"" class=""t_table"" style=""width: 388px;"" bgcolor=""silver""><tbody><tr><td><font size=""4""><font color=""darkorchid""><strong>晶体</strong></font></font></td></tr><tr><td><img id=""aimg_rma0X"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20140703/20/52350242201407032004091683128078344_000.jpg?27x28_120""></td><td><strong>无色小晶体</strong></td><td>-1250</td></tr><tr><td><img id=""aimg_e0Ami"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20140703/20/52350242201407032004091683128078344_002.jpg?29x29_120""></td><td><strong>白色小晶体</strong></td><td>+</td></tr><tr><td><img id=""aimg_W4q22"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20140703/20/52350242201407032004091683128078344_001.jpg?27x28_120""></td><td><strong>红色小晶体</strong></td><td>+</td></tr><tr><td><img id=""aimg_fq44i"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20140703/20/52350242201407032004091683128078344_004.jpg?29x29_120""></td><td><strong>蓝色小晶体</strong></td><td>+</td></tr><tr><td><img id=""aimg_Umvqv"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20140703/20/52350242201407032004091683128078344_003.jpg?29x29_120""></td><td><strong>黑色小晶体</strong></td><td>+</td></tr><tr><td></td><td> </td><td> </td></tr><tr><td> </td><td> </td><td> </td></tr></tbody></table></td></tr></tbody></table><br>
<table cellspacing=""0"" class=""t_table"" style=""width: 400px;"" bgcolor=""palevioletred""><tbody><tr><td><br>
<table cellspacing=""0"" class=""t_table"" style=""width: 388px;"" bgcolor=""silver""><tbody><tr><td><strong><font size=""4""><font color=""darkorchid"">翻牌+怪物掉落获得封装:</font></font></strong></td></tr><tr><td>封印<strong style=""background-color: yellow;"">装备</strong>X135</td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr></tbody></table></td></tr></tbody></table><br>
<font size=""4""><font color=""royalblue""><strong>怪物掉落金钱+翻拍获得金钱+翻拍获得白色<strong style=""background-color: yellow;"">装备</strong>+蓝色<strong style=""background-color: yellow;"">装备</strong>出售商店后获得金币，总数为：<br>
</strong></font></font><img id=""aimg_o5CM4"" class=""zoom"" border=""0"" alt="""" src=""http://www.discuzsample.com/mypoco/myphoto/20130203/17/52350242201302031722253896862635633_005.jpg"">：<strong><font size=""6""><font color=""#ff0000"">1157102</font></font></strong><br>
<br>",
                ContentsNo = 4591192,
                Link = "",
                Title = "【刷图狂魔日记】10月25日，星期2——噩梦级钢铁马骝深渊（日常)",
                WriteDate = DateTime.Parse("2016-10-25 08:58:00"),
                WriterId = "莉露兜"
            };
            var testContent = @"<html><body><div class=""authi"">
<img class=""authicn vm"" id=""authicon80077484"" src=""http://www.discuzsample.com/image/common/online_member.gif"" />
<em id=""authorposton80077484"">发表于 2016-10-25 07:58</em>
<table cellspacing=""0"" cellpadding=""0""><tr><td class=""t_f"" id=""postmessage_80077484"">
<table cellspacing=""0"" class=""t_table"" style=""width:75%"" bgcolor=""mediumslateblue""><tr><td><table cellspacing=""0"" class=""t_table"" style=""width:98%"" bgcolor=""mistyrose""><tr><td><img id=""aimg_wbswe"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20161025/07/52350242201610250756233290499415347_000.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /><br />
</td></tr></td></tr></table><br />
<br />
<table cellspacing=""0"" class=""t_table"" style=""width:98%"" bgcolor=""silver""><tr><td><img id=""aimg_XEGel"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20130620/12/52350242201306201232562837070490111_002.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>高级钻石硬币 </strong></td><td>1</td></tr><tr><td><img id=""aimg_RGSYs"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_020.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>钻石硬币 </strong></td><td>5</td></tr><tr><td><img id=""aimg_oSGeY"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20150921/07/52350242201509210723501907234578385_001.jpg?30x31_120"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>数据芯片</strong></td><td>25</td></tr><tr><td><img id=""aimg_vGLzq"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20150624/20/52350242201506242020452424355124162_001.jpg?30x29_120"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>遗忘陨石</strong></td><td>91</td></tr><tr><td><img id=""aimg_bqgg1"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20130203/17/52350242201302031725539505679242199_001.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>无尽的永恒</strong></td><td>23</td></tr><tr><td><img id=""aimg_YqW0o"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20130203/17/52350242201302031725539505679242199_004.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>深渊派对邀请函</strong></td><td>24</td></tr><tr><td><img id=""aimg_RgsOQ"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20130203/17/52350242201302031725539505679242199_000.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>深渊派对邀请书</strong></td><td>5</td></tr><tr><td><img id=""aimg_fEsSG"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_008.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>赫仑皇帝的印章</strong></td><td>15</td></tr><tr><td><img id=""aimg_y409t"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_004.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>金刚石</strong></td><td>0</td></tr><tr><td><img id=""aimg_RT1oZ"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_002.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>血滴石</strong></td><td>0</td></tr><tr><td><img id=""aimg_dwZOe"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_003.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>海蓝宝石 </strong></td><td>1</td></tr><tr><td><img id=""aimg_dfS4Q"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_001.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>紫玛瑙</strong></td><td>2</td></tr><tr><td><img id=""aimg_sbgqY"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_000.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>黑曜石</strong></td><td>0</td></tr><tr><td><img id=""aimg_QbQoo"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_006.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>碎布片</strong></td><td>12</td></tr><tr><td><img id=""aimg_GbQs9"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_018.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>最下级硬化剂</strong></td><td>8</td></tr><tr><td><img id=""aimg_T5OQL"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_007.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>破旧的皮革</strong></td><td>12</td></tr><tr><td><img id=""aimg_na6g4"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_017.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>风化的碎骨</strong></td><td>8</td></tr><tr><td><img id=""aimg_J06SF"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_005.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>生锈的铁片</strong></td><td>10</td></tr><tr><td><img id=""aimg_kYg0Y"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191528484091821336508_019.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>最下级砥石</strong></td><td>9</td></tr><tr><td><img id=""aimg_IOee6"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191532373030237679919_000.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>诺顿的达人HP药剂</strong></td><td>3</td></tr><tr><td><img id=""aimg_ffYbo"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20110119/15/52350242201101191532373030237679919_001.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>罗莉安的达人MP药剂</strong> </td><td>5</td></tr></table></td></tr></table><br />
<table cellspacing=""0"" class=""t_table"" style=""width:400px"" bgcolor=""palevioletred""><tr><td><br />
<table cellspacing=""0"" class=""t_table"" style=""width:388px"" bgcolor=""silver""><tr><td><font size=""4""><font color=""darkorchid""><strong>晶体</strong></font></font></td></tr><tr><td><img id=""aimg_xTQfW"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20140703/20/52350242201407032004091683128078344_000.jpg?27x28_120"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>无色小晶体</strong></td><td>-1250</td></tr><tr><td><img id=""aimg_xy5Qy"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20140703/20/52350242201407032004091683128078344_002.jpg?29x29_120"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>白色小晶体</strong></td><td>+</td></tr><tr><td><img id=""aimg_ugGso"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20140703/20/52350242201407032004091683128078344_001.jpg?27x28_120"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>红色小晶体</strong></td><td>+</td></tr><tr><td><img id=""aimg_aGBzL"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20140703/20/52350242201407032004091683128078344_004.jpg?29x29_120"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>蓝色小晶体</strong></td><td>+</td></tr><tr><td><img id=""aimg_gS6gt"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20140703/20/52350242201407032004091683128078344_003.jpg?29x29_120"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /></td><td><strong>黑色小晶体</strong></td><td>+</td></tr><tr><td></td><td> </td><td> </td></tr><tr><td> </td><td> </td><td> </td></tr></table></td></tr></table><br />
<table cellspacing=""0"" class=""t_table"" style=""width:400px"" bgcolor=""palevioletred""><tr><td><br />
<table cellspacing=""0"" class=""t_table"" style=""width:388px"" bgcolor=""silver""><tr><td><strong><font size=""4""><font color=""darkorchid"">翻牌+怪物掉落获得封装:</font></font></strong></td></tr><tr><td>封印装备X135</td></tr><tr><td></td></tr><tr><td></td></tr><tr><td></td></tr></table></td></tr></table><br />
<font size=""4""><font color=""royalblue""><strong>怪物掉落金钱+翻拍获得金钱+翻拍获得白色装备+蓝色装备出售商店后获得金币，总数为：<br />
</strong></font></font><img id=""aimg_VE5G9"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://www.discuzsample.com/mypoco/myphoto/20130203/17/52350242201302031722253896862635633_005.jpg"" onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" />：<strong><font size=""6""><font color=""#ff0000"">1157102</font></font></strong><br />
<br />
</body></html>";
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/forum.php?mod=viewthread&tid=10"));
            crawledPage.HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null);
            crawledPage.Content.Text = testContent;

            var crawler = new DiscuzCrawler(new DiscuzCrawleOption()
            {
                BaseUrl = "http://www.discuzsample.com",
                ForumId = 777,
                CrawleHistoryRepository = new DummyCrawleHistoryRepository(false),
                DiscuzVersion = DiscuzVersion.X2,
                CrawleRange = new CrawleRange() { StartArticleNo = 0, MaxListPageNo = 10 }
            });

            crawler.OnNewBuzzCrawled += (result) =>
            {
                Assert.Equal(getTrimmedText(expected.Content), getTrimmedText(result.Content));
                Assert.Equal(expected.ContentsNo, result.ContentsNo);
                Assert.Equal(expected.Link, result.Link);
                Assert.Equal(expected.Title, result.Title);
                Assert.Equal(expected.WriteDate, result.WriteDate);
                Assert.Equal(expected.WriterId, result.WriterId);
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
        }

        private string getTrimmedText(string str)
        {
            return Regex.Replace(str, @"\t|\n|\r", string.Empty).Replace(" ", string.Empty);
        }
    }

    public class DummyCrawleHistoryRepository : ICrawleHistoryRepository
    {
        private bool returnValue;
        public DummyCrawleHistoryRepository(bool returnValue)
        {
            this.returnValue = returnValue;
        }

        public bool IsAlreadyCrawled(Uri uri)
        {
            return returnValue;
        }

        public void SetCrawleComplete(Uri uri)
        {
            return;
        }

        public DateTime GetLastCrawledArticleWriteDate()
        {
            throw new NotImplementedException();
        }

        public void SetLastCrawledArticleWriteDate(DateTime newLastCrawledArticleWriteDate)
        {
            throw new NotImplementedException();
        }
    }
}