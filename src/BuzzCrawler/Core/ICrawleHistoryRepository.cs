using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler
{
    public interface ICrawleHistoryRepository
    {
        bool IsAlreadyCrawled(long identifier);
        void SetCrawleComplete(long identifier);
    }
}
