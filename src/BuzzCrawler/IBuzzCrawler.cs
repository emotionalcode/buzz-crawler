using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuzzCrawler
{
    interface IBuzzCrawler
    {
        event Action<int, DateTime?> OnCrawleComplete;
        event Action<Buzz> OnNewBuzzCrawled;

        void Crawle();
    }
}
