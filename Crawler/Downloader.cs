using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler
{
    /// <summary>
    ///     Http连接操作帮助类
    /// </summary>
    public class Downloader
    {
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
        /// <param name="objhttpitem">参数类对象</param>
        /// <returns>返回HttpResult类型</returns>
        public HttpResult GetHtml(HttpItem objhttpitem)
        {
            //返回参数
            var result = new HttpResult();
            try
            {
                //准备参数
                SetRequest(objhttpitem);
            }
            catch (Exception ex)
            {
                result = new HttpResult
                {
                    Cookie = "",
                    Header = null,
                    Html = ex.Message,
                    StatusDescription = "配置参数时出错：" + ex.Message
                };
                return result;
            }
            try
            {
                #region 得到请求的response

                using (_response = (HttpWebResponse) _request.GetResponse())
                {
                    result.Uri = _response.ResponseUri;
                    result.StatusCode = _response.StatusCode;
                    result.StatusDescription = _response.StatusDescription;
                    result.Header = _response.Headers;
                    if (_response.Cookies != null)
                    {
                        result.CookieCollection = _response.Cookies;
                    }
                    if (_response.Headers["set-cookie"] != null)
                    {
                        result.Cookie = _response.Headers["set-cookie"];
                    }
                    var _stream = new MemoryStream();
                    //GZIIP处理
                    if (_response.ContentEncoding != null &&
                        _response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //开始读取流并设置编码方式
                        //new GZipStream(_response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                        //.net4.0以下写法
                        _stream =
                            GetMemoryStream(new GZipStream(_response.GetResponseStream(), CompressionMode.Decompress));
                    }
                    else
                    {
                        //开始读取流并设置编码方式
                        //_response.GetResponseStream().CopyTo(_stream, 10240);
                        //.net4.0以下写法
                        _stream = GetMemoryStream(_response.GetResponseStream());
                    }
                    //获取Byte
                    var RawResponse = _stream.ToArray();
                    _stream.Close();
                    //是否返回Byte类型数据
                    if (objhttpitem.ResultType == ResultType.Byte)
                    {
                        result.ResultByte = RawResponse;
                    }
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
                            {
                                _encoding = Encoding.UTF8;
                            }
                            else
                            {
                                _encoding = Encoding.GetEncoding(_response.CharacterSet);
                            }
                        }
                    }
                    //得到返回的HTML
                    result.Html = _encoding.GetString(RawResponse);
                }

                #endregion
            }
            catch (WebException ex)
            {
                //这里是在发生异常时返回的错误信息
                _response = (HttpWebResponse) ex.Response;
                result.Html = ex.Message;
                if (_response != null)
                {
                    result.Uri = _response.ResponseUri;
                    result.StatusCode = _response.StatusCode;
                    result.StatusDescription = _response.StatusDescription;
                }
            }
            catch (Exception ex)
            {
                result.Html = ex.Message;
            }
            if (objhttpitem.IsToLower)
            {
                result.Html = result.Html.ToLower();
            }
            return result;
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
        /// <param name="objhttpItem">参数列表</param>
        private void SetRequest(HttpItem objhttpItem)
        {
            //设置安全协议
            ServicePointManager.SecurityProtocol = objhttpItem.SecurityProtocolType;
            // 验证证书
            SetCer(objhttpItem);
            //设置Header参数
            if (objhttpItem.Header != null && objhttpItem.Header.Count > 0)
            {
                foreach (var item in objhttpItem.Header.AllKeys)
                {
                    _request.Headers.Add(item, objhttpItem.Header[item]);
                }
            }
            // 设置代理
            SetProxy(objhttpItem);
            if (objhttpItem.ProtocolVersion != null)
            {
                _request.ProtocolVersion = objhttpItem.ProtocolVersion;
            }
            _request.ServicePoint.Expect100Continue = objhttpItem.Expect100Continue;
            //请求方式Get或者Post
            _request.Method = objhttpItem.Method;
            _request.Timeout = objhttpItem.Timeout;
            _request.ReadWriteTimeout = objhttpItem.ReadWriteTimeout;
            //Accept
            _request.Accept = objhttpItem.Accept;
            //ContentType返回类型
            _request.ContentType = objhttpItem.ContentType;
            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息
            _request.UserAgent = objhttpItem.UserAgent;
            // 编码
            _encoding = objhttpItem.Encoding;
            //设置Cookie
            SetCookie(objhttpItem);
            //来源地址
            _request.Referer = objhttpItem.Referer;
            //是否执行跳转功能
            _request.AllowAutoRedirect = objhttpItem.Allowautoredirect;
            //设置Post数据
            SetPostData(objhttpItem);
            //设置最大连接
            if (objhttpItem.Connectionlimit > 0)
            {
                _request.ServicePoint.ConnectionLimit = objhttpItem.Connectionlimit;
            }
            //设置host
            SetHost(objhttpItem);
        }

        /// <summary>
        ///     设置证书
        /// </summary>
        /// <param name="objhttpItem"></param>
        private void SetCer(HttpItem objhttpItem)
        {
            //这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。
            ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;

            if (!string.IsNullOrEmpty(objhttpItem.CerPath))
            {
                //初始化对像，并设置请求的URL地址
                _request = (HttpWebRequest) WebRequest.Create(objhttpItem.Url);
                SetCerList(objhttpItem);
                //将证书添加到请求里
                _request.ClientCertificates.Add(new X509Certificate(objhttpItem.CerPath));
            }
            else
            {
                //初始化对像，并设置请求的URL地址
                _request = (HttpWebRequest) WebRequest.Create(objhttpItem.Url);
                SetCerList(objhttpItem);
            }
        }

        /// <summary>
        ///     设置多个证书
        /// </summary>
        /// <param name="objhttpItem"></param>
        private void SetCerList(HttpItem objhttpItem)
        {
            if (objhttpItem.ClentCertificates != null && objhttpItem.ClentCertificates.Count > 0)
            {
                foreach (var item in objhttpItem.ClentCertificates)
                {
                    _request.ClientCertificates.Add(item);
                }
            }
        }

        /// <summary>
        ///     设置Cookie
        /// </summary>
        /// <param name="objhttpItem">Http参数</param>
        private void SetCookie(HttpItem objhttpItem)
        {
            if (!string.IsNullOrEmpty(objhttpItem.Cookie))
                //Cookie
            {
                _request.Headers[HttpRequestHeader.Cookie] = objhttpItem.Cookie;
            }
            //设置Cookie
            if (objhttpItem.CookieCollection != null)
            {
                _request.CookieContainer = new CookieContainer();
                _request.CookieContainer.Add(objhttpItem.CookieCollection);
            }
        }

        /// <summary>
        ///     设置Post数据
        /// </summary>
        /// <param name="objhttpItem">Http参数</param>
        private void SetPostData(HttpItem objhttpItem)
        {
            //验证在得到结果时是否有传入数据
            if (_request.Method.Trim().ToLower().Contains("post"))
            {
                if (objhttpItem.PostEncoding != null)
                {
                    _postencoding = objhttpItem.PostEncoding;
                }
                byte[] buffer = null;
                //写入Byte类型
                if (objhttpItem.PostDataType == PostDataType.Byte && objhttpItem.PostdataByte != null &&
                    objhttpItem.PostdataByte.Length > 0)
                {
                    //验证在得到结果时是否有传入数据
                    buffer = objhttpItem.PostdataByte;
                } //写入文件
                else if (objhttpItem.PostDataType == PostDataType.FilePath &&
                         !string.IsNullOrEmpty(objhttpItem.Postdata))
                {
                    var r = new StreamReader(objhttpItem.Postdata, _postencoding);
                    buffer = _postencoding.GetBytes(r.ReadToEnd());
                    r.Close();
                } //写入字符串
                else if (!string.IsNullOrEmpty(objhttpItem.Postdata))
                {
                    buffer = _postencoding.GetBytes(objhttpItem.Postdata);
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
        /// <param name="objhttpItem">参数对象</param>
        private void SetProxy(HttpItem objhttpItem)
        {
            if (_webProxy != null)
            {
                _request.Proxy = _webProxy;
                //设置安全凭证
                _request.Credentials = CredentialCache.DefaultNetworkCredentials;
                return;
            }

            if (!string.IsNullOrEmpty(objhttpItem.ProxyIp))
            {
                //设置代理服务器
                if (objhttpItem.ProxyIp.Contains(":"))
                {
                    var plist = objhttpItem.ProxyIp.Split(':');
                    var myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()));
                    //建议连接
                    myProxy.Credentials = new NetworkCredential(objhttpItem.ProxyUserName, objhttpItem.ProxyPwd);
                    //给当前请求对象
                    _request.Proxy = _webProxy ?? myProxy;
                }
                else
                {
                    var myProxy = new WebProxy(objhttpItem.ProxyIp, false);
                    //建议连接
                    myProxy.Credentials = new NetworkCredential(objhttpItem.ProxyUserName, objhttpItem.ProxyPwd);
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
        /// <param name="objhttpItem"></param>
        private void SetHost(HttpItem objhttpItem)
        {
            if (!string.IsNullOrEmpty(objhttpItem.Host))
            {
                var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var collection = property?.GetValue(_request.Headers, null) as NameValueCollection;
                if (collection != null)
                {
                    collection["Host"] = objhttpItem.Host;
                }
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
                {
                    _webProxy.Credentials = new NetworkCredential(userName, password);
                }
            }
            else
            {
                _webProxy = new WebProxy(ip, false);
                //建议连接
                if (!string.IsNullOrEmpty(userName))
                {
                    _webProxy.Credentials = new NetworkCredential(userName, password);
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///     Http请求参考类
    /// </summary>
    public class HttpItem
    {
        /// <summary>
        ///     获取或设置Host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///     请求URL必须填写
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        ///     请求方式默认为GET方式,当为POST方式时必须设置Postdata的值
        /// </summary>
        public string Method { get; set; } = "GET";

        /// <summary>
        ///     默认请求超时时间
        /// </summary>
        public int Timeout { get; set; } = 100000;

        /// <summary>
        ///     默认写入Post数据超时间
        /// </summary>
        public int ReadWriteTimeout { get; set; } = 30000;

        /// <summary>
        ///     请求标头值 默认为text/html, application/xhtml+xml, */*
        /// </summary>
        public string Accept { get; set; } = "text/html, application/xhtml+xml, */*";

        /// <summary>
        ///     请求返回类型默认 text/html
        /// </summary>
        public string ContentType { get; set; } = "text/html";

        /// <summary>
        ///     客户端访问信息默认Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";

        /// <summary>
        ///     返回数据编码默认为NUll,可以自动识别,一般为utf-8,gbk,gb2312
        /// </summary>
        public Encoding Encoding { get; set; } = null;

        /// <summary>
        ///     Post的数据类型
        /// </summary>
        public PostDataType PostDataType { get; set; } = PostDataType.String;

        /// <summary>
        ///     Post请求时要发送的字符串Post数据
        /// </summary>
        public string Postdata { get; set; } = string.Empty;

        /// <summary>
        ///     Post请求时要发送的Byte类型的Post数据
        /// </summary>
        public byte[] PostdataByte { get; set; } = null;

        /// <summary>
        ///     Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { get; set; } = null;

        /// <summary>
        ///     请求时的Cookie
        /// </summary>
        public string Cookie { get; set; } = string.Empty;

        /// <summary>
        ///     来源地址，上次访问地址
        /// </summary>
        public string Referer { get; set; } = string.Empty;

        /// <summary>
        ///     证书绝对路径
        /// </summary>
        public string CerPath { get; set; } = string.Empty;

        /// <summary>
        ///     是否设置为全文小写，默认为不转化
        /// </summary>
        public bool IsToLower { get; set; } = false;

        /// <summary>
        ///     支持跳转页面，查询结果将是跳转后的页面，默认是不跳转
        /// </summary>
        public bool Allowautoredirect { get; set; } = false;

        /// <summary>
        ///     最大连接数
        /// </summary>
        public int Connectionlimit { get; set; } = 1024;

        /// <summary>
        ///     代理Proxy 服务器用户名
        /// </summary>
        public string ProxyUserName { get; set; } = string.Empty;

        /// <summary>
        ///     代理 服务器密码
        /// </summary>
        public string ProxyPwd { get; set; } = string.Empty;

        /// <summary>
        ///     代理 服务IP
        /// </summary>
        public string ProxyIp { get; set; } = string.Empty;

        /// <summary>
        ///     设置返回类型String和Byte
        /// </summary>
        public ResultType ResultType { get; set; } = ResultType.String;

        /// <summary>
        ///     header对象
        /// </summary>
        public WebHeaderCollection Header { get; set; } = new WebHeaderCollection();

        //     获取或设置用于请求的 HTTP 版本。返回结果:用于请求的 HTTP 版本。默认为 System.Net.HttpVersion.Version11。
        /// <summary>
        /// </summary>
        public Version ProtocolVersion { get; set; }

        /// <summary>
        ///     获取或设置一个 System.Boolean 值，该值确定是否使用 100-Continue 行为。如果 POST 请求需要 100-Continue 响应，则为 true；否则为 false。默认值为 true。
        /// </summary>
        public bool Expect100Continue { get; set; } = true;

        /// <summary>
        ///     设置509证书集合
        /// </summary>
        public X509CertificateCollection ClentCertificates { get; set; }

        /// <summary>
        ///     指定Schannel安全包支持的安全协议
        /// </summary>
        public SecurityProtocolType SecurityProtocolType { get; set; }

        /// <summary>
        ///     设置或获取Post参数编码,默认的为Default编码
        /// </summary>
        public Encoding PostEncoding { get; set; }
    }

    /// <summary>
    ///     Http返回参数类
    /// </summary>
    public class HttpResult
    {
        /// <summary>
        ///     Http请求地址
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        ///     Http请求返回的Cookie
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        ///     Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { get; set; }

        /// <summary>
        ///     返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        ///     返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空
        /// </summary>
        public byte[] ResultByte { get; set; }

        /// <summary>
        ///     header对象
        /// </summary>
        public WebHeaderCollection Header { get; set; }

        /// <summary>
        ///     返回状态说明
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        ///     返回状态码,默认为OK
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
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
