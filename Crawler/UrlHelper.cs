using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class UrlHelper
    {
        public static string Content(string curUrl, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            if (path.StartsWith("~/") || path.StartsWith("/"))
            {
                //return _domainUrl + path;
            }
            return "";
        }
    }
}
