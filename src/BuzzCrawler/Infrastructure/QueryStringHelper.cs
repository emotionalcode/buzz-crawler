using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BuzzCrawler.Infrastructure
{
    internal static class QueryStringHelper
    {
        public static string FillQueryString(string url, string querystringName, string value)
        {
            return url.Replace($"?{querystringName}=", $"?{querystringName}={value}")
                .Replace($"&{querystringName}=", $"&{querystringName}={value}");
        }

        public static string RemoveQueryStringByKey(string url, string key)
        {
            var uri = new Uri(url);
            var queryStrings = HttpUtility.ParseQueryString(uri.Query);
            queryStrings.Remove(key);
            var newUrl = uri.GetLeftPart(UriPartial.Path);

            //return queryStrings.Count > 0 ? String.Format("{0}?{1}", newUrl, queryStrings) : newUrl;

            //detect duplicated querystring, and take one.
            //because some link url has duplicated querystring (ex: http://bbs.colg.cn/ )
            var delimiter = "?";
            foreach (string queryString in queryStrings)
            {
                var value = queryStrings[queryString];
                if (value.Contains(","))
                {
                    value = value.Split(',')[0];
                }
                newUrl += $"{delimiter}{queryString}={value}";
                delimiter = "&";
            }

            return newUrl;
        }
    }
}
