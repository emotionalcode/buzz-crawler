using BuzzCrawler.Discuz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abot.Poco;
using FluentAssertions;
using System.Text.RegularExpressions;
using Xunit;
using System.Threading;

namespace BuzzCrawler.Discuz.Tests
{
    public class DiscuzCrawlerTests
    {
        private CrawleTarget getTestTarget(int maxListPageNo = 10, int startArticleNo = 0)
        {
            return new CrawleTarget(
                "http://www.discuzsample.com/view.php?forumId=&articleNo=", "articleNo",
                "http://www.discuzsample.com/list.php?forumId=&pageNo=", "pageNo",
                "testForum",
                "forumId", maxListPageNo, startArticleNo);
        }

        [Fact]
        public void pageCrawlCompleted_unnecessaryHiddenDiv_removed()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent() { Text = @"
                <html><body>
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
                    </div>
                </body></html>" }
            };

            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);

            var crawler = new DiscuzCrawler(DiscuzVersion.X2, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                var refinedContent = getTrimmedText(result.Content);
                refinedContent.ShouldBeEquivalentTo("test<span>test</span>", "because hidden div tag was removed");
                stopWaitHandle.Set();
            };

            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            stopWaitHandle.WaitOne();
        }

        [Fact]
        public void pageCrawlCompleted_unnecessaryJammerText_removed()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent() { Text = @"
                <html><body>
                    <div id=""thread_subject"">title</div>
                    <div class=""authi""><a>writerId</a></div>
                    <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div>
                    <div class=""t_f"">
                        test
                        <div class=""jammer"">jammerText</div>
                        <div class='jammer anotherClass'>jammerText</div>
                        <span>test</span>
                    </div>
                </body></html>" }
            };

            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);

            var crawler = new DiscuzCrawler(DiscuzVersion.X2, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                var refinedContent = getTrimmedText(result.Content);
                refinedContent.ShouldBeEquivalentTo("test<span>test</span>", "because jammer text was removed");
                stopWaitHandle.Set();
            };

            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            stopWaitHandle.WaitOne();
        }
        
        [Fact]
        public void pageCrawlCompleted_unnecessarySignitureText_removed()
        {
            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent() { Text = @"
                <html><body>
                    <div id=""thread_subject"">title</div>
                    <div class=""authi""><a>writerId</a></div>
                    <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div>
                    <div class=""t_f"">
                        test
                        <div class=""pstatus"">jammerText</div>
                        <div class='pstatus anotherClass'>jammerText</div>
                        <span>test</span>
                    </div>
                </body></html>" }
            };

            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);

            var crawler = new DiscuzCrawler(DiscuzVersion.X2, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                var refinedContent = getTrimmedText(result.Content);
                refinedContent.ShouldBeEquivalentTo("test<span>test</span>", "because signiture text was removed");
                stopWaitHandle.Set();
            };

            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            stopWaitHandle.WaitOne();
        }

        [Fact]
        public void pageCrawlCompleted_imgTagSrcAttr_replace()
        {
            var expected = @"如题有些人就是矫情<br>
                            <img id=""aimg_37446495"" file=""http://www.discuzsample.com/forum/201610/24/060757jwdhc5b8wwp4e8je.jpg"" class=""zoom"" onclick=""zoom(this, this.src)"" 
                                width=""519"" inpost=""1"" alt=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" title=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" 
                                initialized=""true"" src=""http://www.discuzsample.com/forum/201610/24/060757jwdhc5b8wwp4e8je.jpg"">";

            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=1"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent() { Text = @"
                <html><body>
                    <div id=""thread_subject"">title</div>
                    <div class=""authi""><a>writerId</a></div>
                    <div class=""authi""><span title=""2016-10-24 00:00:00""></span></div>
                    <table><tr><td class=""t_f"" id=""postmessage_684661805"">
                        如题有些人就是矫情<br>
                        <img id=""aimg_37446495"" file=""http://www.discuzsample.com/forum/201610/24/060757jwdhc5b8wwp4e8je.jpg"" class=""zoom"" onclick=""zoom(this, this.src)"" 
                        width=""519"" inpost=""1"" alt=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" title=""4OHC@Q(2`3RW`77]{X[UVTK.jpg"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" 
                        initialized=""true""></td></tr></table>
                </body></html>" }
            };

            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);

            var crawler = new DiscuzCrawler(DiscuzVersion.X2, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                var refinedContent = getTrimmedText(result.Content);
                refinedContent.ShouldBeEquivalentTo(getTrimmedText(expected), "because img tag's src was filled");
                stopWaitHandle.Set();
            };

            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            stopWaitHandle.WaitOne();
        }

        [Fact]
        public void pageCrawlCompleted_discuzVersionX2()
        {
            var expected = new Buzz()
            {
                Content = @"<ignore_js_op>
                                <img id=""aimg_37835968"" zoomfile=""http://att.bbs.duowan.com/forum/201612/20/2132435cylz40225tylma2.jpg"" 
                                    file=""http://att.bbs.duowan.com/forum/201612/20/2132435cylz40225tylma2.jpg"" class=""zoom"" onclick=""zoom(this, this.src)"" width=""800"" inpost=""1"" alt=""1482239257.jpg"" 
                                    title=""1482239257.jpg"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" initialized=""true"" src=""http://att.bbs.duowan.com/forum/201612/20/2132435cylz40225tylma2.jpg"">
                            </ignore_js_op>&nbsp; &nbsp;&nbsp; &nbsp;&nbsp; &nbsp;&nbsp; &nbsp;上次说等出来在玩 结果又错过了&nbsp; &nbsp;&nbsp;&nbsp;好久没玩今天上去玩一下真是一点意思都没有啊！",
                ContentsNo = 10,
                Link = "",
                Title = "追忆合成器什么时候会再出？",
                WriteDate = DateTime.Parse("2016-12-20 22:33:37"),
                WriterId = "你老点我干嘛"
            };

            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=10"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent()
                {
                    Text = @"<html><body>
                        <a href=""thread-45326639-1-1.html"" id=""thread_subject"">追忆合成器什么时候会再出？ </a>
                        <div class=""authi"" id=""authi_688043981""><a href=""space-uid-59730690.html"" target=""_blank"" class=""xw1"">你老点我干嘛</a>
                        <div class=""authi"">
                            <img class=""authicn vm"" id=""authicon688043981"" src=""http://att.bbs.duowan.com/static/image/common/online_member.gif"" />
                            <em id=""authorposton688043981"">发表于 <span title=""2016-12-20 21:33:37"">昨天&nbsp;21:33</span></em>
                            <a href=""http://dnf.duowan.com/hezi/"" target=""_blank""><span class=""xg1"">发表自DNF盒子论坛</span></a>
                            <span class=""pipe"">|</span>
                            <a href=""forum.php?mod=viewthread&amp;tid=45326639&amp;page=1&amp;authorid=59730690"" rel=""nofollow"">只看该作者</a>
                            <span class=""pipe"">|</span>
                            <a href=""forum.php?mod=viewthread&amp;tid=45326639&amp;extra=page%3D1&amp;ordertype=1"">倒序浏览</a>
                        </div>
                        <table cellspacing=""0"" cellpadding=""0""><tbody><tr>
                            <td class=""t_f"" id=""postmessage_688043981""><ignore_js_op>
                                <img id=""aimg_37835968"" zoomfile=""http://att.bbs.duowan.com/forum/201612/20/2132435cylz40225tylma2.jpg"" 
                                    file=""http://att.bbs.duowan.com/forum/201612/20/2132435cylz40225tylma2.jpg"" class=""zoom"" onclick=""zoom(this, this.src)"" width=""800"" inpost=""1"" alt=""1482239257.jpg"" 
                                    title=""1482239257.jpg"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" initialized=""true"">
                                <div class=""tip tip_4 aimg_tip"" id=""aimg_37835968_menu"" style=""position: absolute; z-index: 301; left: 215.375px; top: 419px; display: none;"" initialized=""true"">
                                    <div class=""tip_c xs0"">
                                        <div class=""y""><span title=""2016-12-20 21:32:43"">昨天&nbsp;21:32</span> 上传</div>
                                        <a href=""forum.php?mod=attachment&amp;aid=Mzc4MzU5Njh8MTI5MDNmNzV8MTQ4MjI5NDQ5OXwwfDQ1MzI2NjM5&amp;nothumb=yes"" title=""1482239257.jpg 下载次数:0"" target=""_blank"">
                                            <strong>下载附件</strong> <span class=""xs0"">(265.78 KB)</span>
                                        </a>
                                    </div>
                                    <div class=""tip_horn""></div>
                                </div>
                                </ignore_js_op>&nbsp; &nbsp;&nbsp; &nbsp;&nbsp; &nbsp;&nbsp; &nbsp;上次说等出来在玩 结果又错过了&nbsp; &nbsp;&nbsp;&nbsp;好久没玩今天上去玩一下真是一点意思都没有啊！
                            </td></tr></tbody>
                        </table>
                    </body></html>"
                }
            };

            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);
            var crawler = new DiscuzCrawler(DiscuzVersion.X2, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                getTrimmedText(result.Content).ShouldBeEquivalentTo(getTrimmedText(expected.Content));
                result.ContentsNo.ShouldBeEquivalentTo(expected.ContentsNo);
                result.Title.ShouldBeEquivalentTo(expected.Title);
                result.WriteDate.ShouldBeEquivalentTo(expected.WriteDate);
                result.WriterId.ShouldBeEquivalentTo(expected.WriterId);
                stopWaitHandle.Set();
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            stopWaitHandle.WaitOne();
        }

        [Fact]
        public void pageCrawlCompleted_discuzVersionX3_1()
        {
            var expected = new Buzz()
            {
                Content = @"<img id=""aimg_je99L"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://ww4.sinaimg.cn/large/aadaeaeegw1faxfzoiz9fj20m80go1bs.jpg"" 
                            onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" src=""http://ww4.sinaimg.cn/large/aadaeaeegw1faxfzoiz9fj20m80go1bs.jpg""><br><br>
                            不晓得特效伤害怎么样。。靠光头之路的炉子上了个锻7.。",
                ContentsNo = 10,
                Link = "",
                Title = "光兵出了这个。。能升级为舰炮C吗。。。",
                WriteDate = DateTime.Parse("2016-12-20 19:41"),
                WriterId = "封兽鵺°"
            };

            var crawledPage = new CrawledPage(new Uri("http://www.discuzsample.com/view.php?forumId=testForum&articleNo=10"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent()
                {
                    Text = @"<html><body>
                        <span id=""thread_subject"">光兵出了这个。。能升级为舰炮C吗。。。</span>
                        <div class=""authi""><a href=""home.php?mod=space&amp;uid=1183054"" target=""_blank"" class=""xw1"">封兽鵺°</a></div>
                        <div class=""authi"">
                            <img class=""authicn vm"" id=""authicon81725783"" src=""http://static.colg.cn/image/common/online_member.gif"" />
                            <em id = ""authorposton81725783"" > 发表于 2016-12-20 18:41</em>
                            <span class=""pipe"">|</span>
                            <a href = ""forum.php?mod=viewthread&amp;tid=5137807&amp;page=1&amp;authorid=1183054"" rel=""nofollow"">只看该作者</a>
                            <span class=""none""><img src = ""http://static.colg.cn/image/common/arw_r.gif"" class=""vm"" alt=""回帖奖励"" /></span>
                            <span class=""pipe show"">|</span>
                            <a href = ""forum.php?mod=viewthread&amp;tid=5137807&amp;extra=&amp;ordertype=1""  class=""show"">倒序浏览</a>
                            <span class=""pipe show"">|</span>
                            <a href = ""javascript:;"" onclick=""readmode($('thread_subject').innerHTML, 81725783);"" class=""show"">阅读模式</a>
                        </div>
                        <table cellspacing=""0"" cellpadding=""0""><tr><td class=""t_f"" id=""postmessage_81725783"">
                            <img id=""aimg_je99L"" onclick=""zoom(this, this.src, 0, 0, 0)"" class=""zoom"" file=""http://ww4.sinaimg.cn/large/aadaeaeegw1faxfzoiz9fj20m80go1bs.jpg"" 
                            onmouseover=""img_onmouseoverfunc(this)"" lazyloadthumb=""1"" border=""0"" alt="""" /><br /><br />
                            不晓得特效伤害怎么样。。靠光头之路的炉子上了个锻7.。</td></tr>
                        </table>
                    </body></html>"
                }
            };

            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);
            var crawler = new DiscuzCrawler(DiscuzVersion.X3_1, getTestTarget(), null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                getTrimmedText(result.Content).ShouldBeEquivalentTo(getTrimmedText(expected.Content));
                result.ContentsNo.ShouldBeEquivalentTo(expected.ContentsNo);
                result.Title.ShouldBeEquivalentTo(expected.Title);
                result.WriteDate.ShouldBeEquivalentTo(expected.WriteDate);
                result.WriterId.ShouldBeEquivalentTo(expected.WriterId);
                stopWaitHandle.Set();
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            stopWaitHandle.WaitOne();
        }

        [Fact]
        public void pageCrawlCompleted_discuzVersionX3_2()
        {
            var expected = new Buzz()
            {
                Content = @"<strong>听说最近要推出国产DNF动漫，感觉画风完全不同啊！</strong><font size=""4""><font color=""#ff0000"">
                            <strong>&nbsp; &nbsp; 有点失落~~~鬼剑士就像个高中生，没有野性;</strong></font></font><br>
                            <font size=""4""><font color=""#ff0000""><strong>&nbsp; &nbsp; 魔法师不够可爱，</strong></font></font><br>
                            <font size=""4""><font color=""#ff0000""><strong>&nbsp; &nbsp; 女格斗本应看起来更霸气一点，</strong></font></font><br>
                            <font size=""4""><font color=""#ff0000""><strong>&nbsp; &nbsp; 啊~我是不是中毒太深了？</strong></font></font><br>
                            <font size=""4""><font color=""#ff0000""><strong>&nbsp; &nbsp; 吓得我赶紧重温了一遍原版阿拉德战纪</strong></font></font><br>
                            <ignore_js_op>
                            <img id=""aimg_95544"" aid=""95544"" src=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131543sjwjj42ef7ju7y7v.png/0""
                                zoomfile=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131543sjwjj42ef7ju7y7v.png/0"" 
                                file=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131543sjwjj42ef7ju7y7v.png/0"" class=""zoom"" 
                                onclick=""zoom(this, this.src, 0, 0, 0)"" width=""535"" inpost=""1"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" >
                            </ignore_js_op>
                            <ignore_js_op>
                            <img id=""aimg_95545"" aid=""95545"" src=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131544n6jmiwwqw9h6ftwm.png/0""
                                zoomfile=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131544n6jmiwwqw9h6ftwm.png/0"" 
                                file=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131544n6jmiwwqw9h6ftwm.png/0"" 
                                class=""zoom"" onclick=""zoom(this, this.src, 0, 0, 0)"" width=""600"" inpost=""1"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" >
                            </ignore_js_op>
                            <ignore_js_op>
                            <img id=""aimg_95546"" aid=""95546"" src=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131545pr4m11133ju1s48c.png/0""
                                zoomfile=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131545pr4m11133ju1s48c.png/0"" 
                                file=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131545pr4m11133ju1s48c.png/0"" class=""zoom"" onclick=""zoom(this, this.src, 0, 0, 0)"" 
                                width=""522"" inpost=""1"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" >
                            </ignore_js_op><br>",
                ContentsNo = 10,
                Link = "",
                Title = "DNF最初的动画阿拉德战纪你还记得吗？",
                WriteDate = DateTime.Parse("2016-12-21 14:21:09"),
                WriterId = "独白下的月光"
            };

            var crawledPage = new CrawledPage(new Uri("http://dnf.gamebbs.qq.com/forum.php?mod=viewthread&tid=10&extra=page%3D1%26filter%3Dauthor%26orderby%3Ddateline"))
            {
                HttpWebResponse = new HttpWebResponseWrapper(System.Net.HttpStatusCode.OK, "html", null, null),
                Content = new PageContent()
                {
                    Text = @"<html><body>
                        <span id=""thread_subject"">DNF最初的动画阿拉德战纪你还记得吗？</span>
                        <div class=""authi""><a href=""space-uid-279440.html"" target=""_blank"" class=""xw1"">独白下的月光</a></div>
                        <div class=""authi"">
                            <img class=""authicn vm"" id=""authicon1077595"" src=""static/image/common/ico_lz.png"" />
                            &nbsp;楼主<span class=""pipe"">|</span>
                            <em id=""authorposton1077595"">发表于 <span title=""2016-12-21 13:21:09"">1&nbsp;小时前</span></em>
                            <span class=""pipe"">|</span>
                            <a href=""forum.php?mod=viewthread&amp;tid=88917&amp;page=1&amp;authorid=279440"" rel=""nofollow"">只看该作者</a>
                            <span class=""pipe"">|</span>
                            <a href=""forum.php?mod=viewthread&amp;tid=88917&amp;from=album"">只看大图</a>
                            <span class=""none""><img src=""static/image/common/arw_r.gif"" class=""vm"" alt=""回帖奖励"" /></span>
                            <span class=""pipe show"">|</span>
                            <a href=""forum.php?mod=viewthread&amp;tid=88917&amp;extra=page%3D1%26filter%3Dauthor%26orderby%3Ddateline&amp;ordertype=1""  class=""show"">倒序浏览</a>
                            <span class=""pipe show"">|</span>
                            <a href=""javascript:;"" onclick=""readmode($('thread_subject').innerHTML, 1077595);"" class=""show"">阅读模式</a>
                        </div>
                        <table cellspacing=""0"" cellpadding=""0""><tr>
                            <td class=""t_f"" id=""postmessage_1077595"">
                                <strong>听说最近要推出国产DNF动漫，感觉画风完全不同啊！</strong><font size=""4""><font color=""#ff0000"">
                                <strong>&nbsp; &nbsp; 有点失落~~~鬼剑士就像个高中生，没有野性;</strong></font></font><br />
                                <font size=""4""><font color=""#ff0000""><strong>&nbsp; &nbsp; 魔法师不够可爱，</strong></font></font><br />
                                <font size=""4""><font color=""#ff0000""><strong>&nbsp; &nbsp; 女格斗本应看起来更霸气一点，</strong></font></font><br />
                                <font size=""4""><font color=""#ff0000""><strong>&nbsp; &nbsp; 啊~我是不是中毒太深了？</strong></font></font><br />
                                <font size=""4""><font color=""#ff0000""><strong>&nbsp; &nbsp; 吓得我赶紧重温了一遍原版阿拉德战纪</strong></font></font><br />
                                <ignore_js_op>
                                <img id=""aimg_95544"" aid=""95544"" src=""static/image/common/none.gif"" 
                                    zoomfile=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131543sjwjj42ef7ju7y7v.png/0"" 
                                    file=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131543sjwjj42ef7ju7y7v.png/0"" class=""zoom"" 
                                    onclick=""zoom(this, this.src, 0, 0, 0)"" width=""535"" id=""aimg_95544"" inpost=""1"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" />

                                <div class=""tip tip_4 aimg_tip"" id=""aimg_95544_menu"" style=""position: absolute; display: none"" disautofocus=""true"">
                                    <div class=""xs0"">
                                        <p><strong>1.png</strong> <em class=""xg1"">(0 Bytes, 下载次数: 0)</em></p>
                                        <p><a href=""forum.php?mod=attachment&amp;aid=OTU1NDR8MjZiYWQwZjJ8MTQ4MjMwMzY0OXwyOTMyMDB8ODg5MTc%3D&amp;nothumb=yes"" target=""_blank"">下载附件</a></p>
                                        <p class=""xg1 y""><span title=""2016-12-21 13:15"">1&nbsp;小时前</span> 上传</p>
                                    </div>
                                    <div class=""tip_horn""></div>
                                </div>
                                </ignore_js_op>
                                <ignore_js_op>
                                <img id=""aimg_95545"" aid=""95545"" src=""static/image/common/none.gif"" 
                                    zoomfile=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131544n6jmiwwqw9h6ftwm.png/0"" 
                                    file=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131544n6jmiwwqw9h6ftwm.png/0"" 
                                    class=""zoom"" onclick=""zoom(this, this.src, 0, 0, 0)"" width=""600"" id=""aimg_95545"" inpost=""1"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" />
                                <div class=""tip tip_4 aimg_tip"" id=""aimg_95545_menu"" style=""position: absolute; display: none"" disautofocus=""true"">
                                    <div class=""xs0"">
                                        <p><strong>2.png</strong> <em class=""xg1"">(0 Bytes, 下载次数: 0)</em></p>
                                        <p><a href=""forum.php?mod=attachment&amp;aid=OTU1NDV8ZGNmZWNhMTR8MTQ4MjMwMzY0OXwyOTMyMDB8ODg5MTc%3D&amp;nothumb=yes"" target=""_blank"">下载附件</a></p>
                                        <p class=""xg1 y""><span title=""2016-12-21 13:15"">1&nbsp;小时前</span> 上传</p>
                                    </div>
                                    <div class=""tip_horn""></div>
                                </div>
                                </ignore_js_op>
                                <ignore_js_op>
                                <img id=""aimg_95546"" aid=""95546"" src=""static/image/common/none.gif"" 
                                    zoomfile=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131545pr4m11133ju1s48c.png/0"" 
                                    file=""http://p.qpic.cn/dnfbbspic/0/dnfbbs_dnfbbs_dnf_gamebbs_qq_com_forum_201612_21_131545pr4m11133ju1s48c.png/0"" class=""zoom"" onclick=""zoom(this, this.src, 0, 0, 0)"" 
                                    width=""522"" id=""aimg_95546"" inpost=""1"" onmouseover=""showMenu({'ctrlid':this.id,'pos':'12'})"" />

                                <div class=""tip tip_4 aimg_tip"" id=""aimg_95546_menu"" style=""position: absolute; display: none"" disautofocus=""true"">
                                    <div class=""xs0"">
                                        <p><strong>3.png</strong> <em class=""xg1"">(0 Bytes, 下载次数: 0)</em></p>
                                        <p><a href=""forum.php?mod=attachment&amp;aid=OTU1NDZ8Zjk0ZDFhNzV8MTQ4MjMwMzY0OXwyOTMyMDB8ODg5MTc%3D&amp;nothumb=yes"" target=""_blank"">下载附件</a></p>
                                        <p class=""xg1 y""><span title=""2016-12-21 13:15"">1&nbsp;小时前</span> 上传</p>
                                    </div>
                                    <div class=""tip_horn""></div>
                                </div>
                                </ignore_js_op><br /></td></tr>
                        </table>	
                    </body></html>"
                }
            };

            AutoResetEvent stopWaitHandle = new AutoResetEvent(false);
            var crawler = new DiscuzCrawler(
                DiscuzVersion.X3_2, 
                new CrawleTarget(
                    "http://dnf.gamebbs.qq.com/forum.php?mod=viewthread&tid=&extra=page%3D1%26filter%3Dauthor%26orderby%3Ddateline", "tid",
                    "http://dnf.gamebbs.qq.com/forum.php?mod=forumdisplay&fid=43&orderby=dateline&filter=author&orderby=dateline&page=", "page",
                    "43",
                    "fid"), 
                null);
            crawler.OnNewBuzzCrawled += (result) =>
            {
                getTrimmedText(result.Content).ShouldBeEquivalentTo(getTrimmedText(expected.Content));
                result.ContentsNo.ShouldBeEquivalentTo(expected.ContentsNo);
                result.Title.ShouldBeEquivalentTo(expected.Title);
                result.WriteDate.ShouldBeEquivalentTo(expected.WriteDate);
                result.WriterId.ShouldBeEquivalentTo(expected.WriterId);
                stopWaitHandle.Set();
            };
            crawler.pageCrawlCompleted(null, new Abot.Crawler.PageCrawlCompletedArgs(new CrawlContext(), crawledPage));
            stopWaitHandle.WaitOne();
        }

        private string getTrimmedText(string str)
        {
            return Regex.Replace(str, @"\t|\n|\r", string.Empty).Replace(" ", string.Empty);
        }
    }
}