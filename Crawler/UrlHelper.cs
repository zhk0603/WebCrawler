using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class UrlHelper
    {
        public static string Combine(string currenUri, string relativeUri)
        {
            if (string.IsNullOrEmpty(relativeUri))
            {
                return "";
            }
            if (relativeUri.StartsWith("http"))
            {
                return relativeUri;
            }
            var baseUri = new Uri(currenUri);
            var absoluteUri = new Uri(baseUri, relativeUri);
            return absoluteUri.ToString();
        }
    }
}
