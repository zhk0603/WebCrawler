using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Crawler.Downloader
{
    /// <summary>
    ///     下载器。
    /// </summary>
    public class HttpDownloader : IDownloader
    {
        #region 公共方法

        /// <summary>
        ///     设置代理
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public void SetProxy(string ip, string userName = null, string password = null)
        {
            //设置代理服务器
            if (ip.Contains(":"))
            {
                var plist = ip.Split(':');
                _webProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()));
                //建议连接
                if (!string.IsNullOrEmpty(userName))
                    _webProxy.Credentials = new NetworkCredential(userName, password);
            }
            else
            {
                _webProxy = new WebProxy(ip, false);
                //建议连接
                if (!string.IsNullOrEmpty(userName))
                    _webProxy.Credentials = new NetworkCredential(userName, password);
            }
        }

        #endregion

        #region 预定义方法或者变更

        //默认的编码
        private Encoding _encoding = Encoding.Default;

        //Post数据编码
        private Encoding _postencoding = Encoding.Default;

        //HttpWebRequest对象用来发起请求
        private HttpWebRequest _request;

        //获取影响流的数据对象
        private HttpWebResponse _response;

        // web 代理
        private WebProxy _webProxy;

        /// <summary>
        ///     根据相传入的数据，得到相应页面数据
        /// </summary>
        /// <param name="requestSite">参数类对象</param>
        /// <returns>返回HttpResult类型</returns>
        public Page GetPage(Site requestSite)
        {
            //返回参数
            var page = new Page();
            try
            {
                //准备参数
                SetRequest(requestSite);
            }
            catch (Exception ex)
            {
                page = new Page
                {
                    Cookie = "",
                    Header = null,
                    HtmlSource = ex.Message,
                    StatusDescription = "配置参数时出错：" + ex.Message
                };
                return page;
            }
            try
            {
                #region 得到请求的response

                using (_response = (HttpWebResponse) _request.GetResponse())
                {
                    page.Uri = _response.ResponseUri;
                    page.HttpStatusCode = (int) _response.StatusCode;
                    page.StatusDescription = _response.StatusDescription;
                    page.Header = _response.Headers;
                    if (_response.Cookies != null)
                        page.CookieCollection = _response.Cookies;
                    if (_response.Headers["set-cookie"] != null)
                        page.Cookie = _response.Headers["set-cookie"];
                    var _stream = new MemoryStream();
                    //GZIIP处理
                    if (_response.ContentEncoding != null &&
                        _response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                        _stream =
                            GetMemoryStream(new GZipStream(_response.GetResponseStream(), CompressionMode.Decompress));
                    else
                        _stream = GetMemoryStream(_response.GetResponseStream());
                    //获取Byte
                    var RawResponse = _stream.ToArray();
                    _stream.Close();
                    //是否返回Byte类型数据
                    if (requestSite.ResultType == ResultType.Byte)
                        page.ResultByte = RawResponse;
                    //从这里开始我们要无视编码了
                    if (_encoding == null)
                    {
                        var meta = Regex.Match(Encoding.Default.GetString(RawResponse),
                            "<meta([^<]*)charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                        var charter = meta.Groups.Count > 1 ? meta.Groups[2].Value.ToLower() : string.Empty;
                        if (charter.Length > 2)
                        {
                            _encoding =
                                Encoding.GetEncoding(
                                    charter.Trim()
                                        .Replace("\"", "")
                                        .Replace("'", "")
                                        .Replace(";", "")
                                        .Replace("iso-8859-1", "gbk"));
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(_response.CharacterSet))
                                _encoding = Encoding.UTF8;
                            else
                                _encoding = Encoding.GetEncoding(_response.CharacterSet);
                        }
                    }
                    //得到返回的HTML
                    page.HtmlSource = _encoding.GetString(RawResponse);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(page.HtmlSource);
                    page.HtmlNode = doc.DocumentNode;
                }

                #endregion
            }
            catch (WebException ex)
            {
                //这里是在发生异常时返回的错误信息
                _response = (HttpWebResponse) ex.Response;
                page.HtmlSource = ex.Message;
                if (_response != null)
                {
                    page.Uri = _response.ResponseUri;
                    page.HttpStatusCode = (int) _response.StatusCode;
                    page.StatusDescription = _response.StatusDescription;
                }
            }
            catch (Exception ex)
            {
                page.HtmlSource = ex.Message;
            }
            if (requestSite.IsToLower)
                page.HtmlSource = page.HtmlSource.ToLower();

            return page;
        }

        public Page GetPage(string requestUrl)
        {
            var site = new Site(requestUrl);
            return GetPage(site);
        }

        /// <summary>
        ///     4.0以下.net版本取数据使用
        /// </summary>
        /// <param name="streamResponse">流</param>
        private static MemoryStream GetMemoryStream(Stream streamResponse)
        {
            var stream = new MemoryStream();
            var Length = 256;
            var buffer = new byte[Length];
            var bytesRead = streamResponse.Read(buffer, 0, Length);
            while (bytesRead > 0)
            {
                stream.Write(buffer, 0, bytesRead);
                bytesRead = streamResponse.Read(buffer, 0, Length);
            }
            return stream;
        }

        /// <summary>
        ///     为请求准备参数
        /// </summary>
        /// <param name="requestSite">参数列表</param>
        private void SetRequest(Site requestSite)
        {
            //设置安全协议
            if (requestSite.SecurityProtocolType.HasValue)
            {
                ServicePointManager.SecurityProtocol = requestSite.SecurityProtocolType.Value;
            }
            // 验证证书
            SetCer(requestSite);
            //设置Header参数
            if (requestSite.Header != null && requestSite.Header.Count > 0)
                foreach (var item in requestSite.Header.AllKeys)
                    _request.Headers.Add(item, requestSite.Header[item]);
            // 设置代理
            SetProxy(requestSite);
            if (requestSite.ProtocolVersion != null)
                _request.ProtocolVersion = requestSite.ProtocolVersion;
            _request.ServicePoint.Expect100Continue = requestSite.Expect100Continue;
            //请求方式Get或者Post
            _request.Method = requestSite.Method;
            _request.Timeout = requestSite.Timeout;
            _request.ReadWriteTimeout = requestSite.ReadWriteTimeout;
            //Accept
            _request.Accept = requestSite.Accept;
            //ContentType返回类型
            _request.ContentType = requestSite.ContentType;
            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息
            _request.UserAgent = requestSite.UserAgent;
            // 编码
            _encoding = requestSite.Encoding;
            //设置Cookie
            SetCookie(requestSite);
            //来源地址
            _request.Referer = requestSite.Referer;
            //是否执行跳转功能
            _request.AllowAutoRedirect = requestSite.Allowautoredirect;
            //设置Post数据
            SetPostData(requestSite);
            //设置最大连接
            if (requestSite.Connectionlimit > 0)
                _request.ServicePoint.ConnectionLimit = requestSite.Connectionlimit;
            //设置host
            SetHost(requestSite);
        }

        /// <summary>
        ///     设置证书
        /// </summary>
        /// <param name="requestSite"></param>
        private void SetCer(Site requestSite)
        {
            //这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。
            ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;

            if (!string.IsNullOrEmpty(requestSite.CerPath))
            {
                //初始化对像，并设置请求的URL地址
                _request = (HttpWebRequest) WebRequest.Create(requestSite.Url);
                SetCerList(requestSite);
                //将证书添加到请求里
                _request.ClientCertificates.Add(new X509Certificate(requestSite.CerPath));
            }
            else
            {
                //初始化对像，并设置请求的URL地址
                _request = (HttpWebRequest) WebRequest.Create(requestSite.Url);
                SetCerList(requestSite);
            }
        }

        /// <summary>
        ///     设置多个证书
        /// </summary>
        /// <param name="requestSite"></param>
        private void SetCerList(Site requestSite)
        {
            if (requestSite.ClentCertificates != null && requestSite.ClentCertificates.Count > 0)
                foreach (var item in requestSite.ClentCertificates)
                    _request.ClientCertificates.Add(item);
        }

        /// <summary>
        ///     设置Cookie
        /// </summary>
        /// <param name="requestSite">Http参数</param>
        private void SetCookie(Site requestSite)
        {
            if (!string.IsNullOrEmpty(requestSite.Cookie))
                //Cookie
                _request.Headers[HttpRequestHeader.Cookie] = requestSite.Cookie;
            //设置Cookie
            if (requestSite.CookieCollection != null)
            {
                _request.CookieContainer = new CookieContainer();
                _request.CookieContainer.Add(requestSite.CookieCollection);
            }
        }

        /// <summary>
        ///     设置Post数据
        /// </summary>
        /// <param name="requestSite">Http参数</param>
        private void SetPostData(Site requestSite)
        {
            //验证在得到结果时是否有传入数据
            if (_request.Method.Trim().ToLower().Contains("post"))
            {
                if (requestSite.PostEncoding != null)
                    _postencoding = requestSite.PostEncoding;
                byte[] buffer = null;
                //写入Byte类型
                if (requestSite.PostDataType == PostDataType.Byte && requestSite.PostdataByte != null &&
                    requestSite.PostdataByte.Length > 0)
                {
                    //验证在得到结果时是否有传入数据
                    buffer = requestSite.PostdataByte;
                } //写入文件
                else if (requestSite.PostDataType == PostDataType.FilePath &&
                         !string.IsNullOrEmpty(requestSite.Postdata))
                {
                    var r = new StreamReader(requestSite.Postdata, _postencoding);
                    buffer = _postencoding.GetBytes(r.ReadToEnd());
                    r.Close();
                } //写入字符串
                else if (!string.IsNullOrEmpty(requestSite.Postdata))
                {
                    buffer = _postencoding.GetBytes(requestSite.Postdata);
                }
                if (buffer != null)
                {
                    _request.ContentLength = buffer.Length;
                    _request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        ///     设置代理
        /// </summary>
        /// <param name="requestSite">参数对象</param>
        private void SetProxy(Site requestSite)
        {
            if (_webProxy != null)
            {
                _request.Proxy = _webProxy;
                //设置安全凭证
                _request.Credentials = CredentialCache.DefaultNetworkCredentials;
                return;
            }

            if (!string.IsNullOrEmpty(requestSite.ProxyIp))
            {
                //设置代理服务器
                if (requestSite.ProxyIp.Contains(":"))
                {
                    var plist = requestSite.ProxyIp.Split(':');
                    var myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()));
                    //建议连接
                    myProxy.Credentials = new NetworkCredential(requestSite.ProxyUserName, requestSite.ProxyPwd);
                    //给当前请求对象
                    _request.Proxy = _webProxy ?? myProxy;
                }
                else
                {
                    var myProxy = new WebProxy(requestSite.ProxyIp, false);
                    //建议连接
                    myProxy.Credentials = new NetworkCredential(requestSite.ProxyUserName, requestSite.ProxyPwd);
                    //给当前请求对象
                    _request.Proxy = _webProxy ?? myProxy;
                }
                //设置安全凭证
                _request.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
        }

        /// <summary>
        ///     设置host
        /// </summary>
        /// <param name="requestSite"></param>
        private void SetHost(Site requestSite)
        {
            if (!string.IsNullOrEmpty(requestSite.Host))
            {
                var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var collection = property?.GetValue(_request.Headers, null) as NameValueCollection;
                if (collection != null)
                    collection["Host"] = requestSite.Host;
            }
        }

        /// <summary>
        ///     回调验证证书问题
        /// </summary>
        /// <param name="sender">流对象</param>
        /// <param name="certificate">证书</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors errors)
        {
            return true;
        }

        #endregion
    }

    /// <summary>
    ///     返回类型
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        ///     表示只返回字符串 只有Html有数据
        /// </summary>
        String,

        /// <summary>
        ///     表示返回字符串和字节流 ResultByte和Html都有数据返回
        /// </summary>
        Byte
    }

    /// <summary>
    ///     Post的数据格式默认为string
    /// </summary>
    public enum PostDataType
    {
        /// <summary>
        ///     字符串类型，这时编码Encoding可不设置
        /// </summary>
        String,

        /// <summary>
        ///     Byte类型，需要设置PostdataByte参数的值编码Encoding可设置为空
        /// </summary>
        Byte,

        /// <summary>
        ///     传文件，Postdata必须设置为文件的绝对路径，必须设置Encoding的值
        /// </summary>
        FilePath
    }
}