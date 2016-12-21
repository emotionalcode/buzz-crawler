
# buzz-crawler
[![Build status](https://ci.appveyor.com/api/projects/status/1xu16kovcr7do9xh?svg=true)](https://ci.appveyor.com/project/emotionalcode/buzz-crawler)
[![Build Status](https://travis-ci.org/emotionalcode/buzz-crawler.svg?branch=master)](https://travis-ci.org/emotionalcode/buzz-crawler)
[![Coverage Status](https://coveralls.io/repos/github/emotionalcode/buzz-crawler/badge.svg?branch=master)](https://coveralls.io/github/emotionalcode/buzz-crawler?branch=master)
[![NuGet Badge](https://buildstats.info/nuget/BuzzCrawler)](https://www.nuget.org/packages/BuzzCrawler/)

### Install
To install BuzzCrawler, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console)
```
PM> Install-Package BuzzCrawler
```

### Quick start


```c#
var crawleTarget = new CrawleTarget(
    articleUrl: "http://bbs.colg.cn/forum.php?mod=viewthread&tid=",
    articleNoQuerystring: "tid",
    listUrl: "http://bbs.colg.cn/forum.php?mod=forumdisplay&fid=&orderby=dateline&filter=author&page=",
    pageNoQuerystring: "page",
    forumId: "393",
    forumIdQuerystring: "fid");

var crawler = new DiscuzCrawler(DiscuzVersion.X3_1, crawleTarget);
crawler.OnCrawleComplete += (crawledCount, lastBuzzWriteDate) => Console.WriteLine($"crawled complete. {crawledCount} buzz crawled. last writedate is {lastBuzzWriteDate}");
crawler.OnNewBuzzCrawled += buzz => Console.WriteLine($"buzz crawled{Environment.NewLine}{JsonConvert.SerializeObject(buzz)}");
crawler.Crawle();
```
