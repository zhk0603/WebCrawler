using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class UrlHelper
    {
        private readonly string _domainUrl;

        public UrlHelper(Site site)
        {
            _domainUrl = site.Host;
        }

        public UrlHelper(string domainUrl)
        {
            _domainUrl = domainUrl;
        }

        public string Content(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            if (path.StartsWith("~/") || path.StartsWith("/"))
            {
                return _domainUrl + path;
            }
            return "";
        }
    }
}
