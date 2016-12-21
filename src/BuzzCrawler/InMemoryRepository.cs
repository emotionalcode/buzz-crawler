using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler
{
    public class InMemoryCrawleHistoryRepository : ICrawleHistoryRepository
    {
        HashSet<long> history = new HashSet<long>();

        public bool IsAlreadyCrawled(long articleNo)
        {
            return history.Contains(articleNo);
        }

        public void SetCrawleComplete(long articleNo)
        {
            history.Add(articleNo);
        }
    }
}
