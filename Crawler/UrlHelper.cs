using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class UrlHelper
    {
        public static string Combine(string currenUrl, string absUrl)
        {
            if (string.IsNullOrEmpty(absUrl))
            {
                return "";
            }
            if (absUrl.StartsWith("http"))
            {
                return absUrl;
            }
            var baseUri = new Uri(currenUrl);
            var absoluteUri = new Uri(baseUri, absUrl);
            return absoluteUri.ToString();
        }
    }
}
