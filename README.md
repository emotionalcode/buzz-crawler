
# buzz-crawler
AppVeyor: [![Build status](https://ci.appveyor.com/api/projects/status/1xu16kovcr7do9xh?svg=true)](https://ci.appveyor.com/project/emotionalcode/buzz-crawler)

Travis:  [![Build Status](https://travis-ci.org/emotionalcode/buzz-crawler.svg?branch=master)](https://travis-ci.org/emotionalcode/buzz-crawler)

### Install
To install BuzzCrawler, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console)
```
PM> Install-Package BuzzCrawler
```

### Features
1. crawling forum page that generated by [discuz](https://en.wikipedia.org/wiki/Discuz!)
2. crawling rss feed

### Quick start


```c#
DiscuzCrawler crawler = new DiscuzCrawler(new DiscuzCrawleOption()
{
    BaseUrl = "http://www.example.com",
    CrawleHistoryRepository = new InMemoryRepository(),
    CrawleRange = new CrawleRange() { MaxListPageNo = 2, StartArticleNo = 10 },
    DiscuzVersion = DiscuzVersion.X3_1,
    ForumId = 171
});
crawler.OnCrawleComplete += (crawledCount, lastBuzzWriteDate) => Console.WriteLine($"crawled complete. {crawledCount} buzz crawled. last writedate is {lastBuzzWriteDate}");
crawler.OnNewBuzzCrawled += buzz => Console.WriteLine($"buzz crawled{Environment.NewLine}{JsonConvert.SerializeObject(buzz)}");

crawler.Crawle();
```
