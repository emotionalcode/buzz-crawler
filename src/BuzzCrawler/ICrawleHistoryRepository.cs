using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler
{
    public interface ICrawleHistoryRepository
    {
        bool IsAlreadyCrawled(Uri uri);
        void SetCrawleComplete(Uri uri);

        DateTime GetLastCrawledArticleWriteDate();

        void SetLastCrawledArticleWriteDate(DateTime newLastCrawledArticleWriteDate);
    }
}
