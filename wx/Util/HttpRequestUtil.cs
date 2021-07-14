using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace wx.Util
{
    public class HttpRequestUtil
    {
        #region 请求Url，不发送数据
        /// <summary>
        /// 请求Url，不发送数据
        /// </summary>
        public static string RequestUrl(string url)
        {
            return RequestUrl(url, "POST");
        }
        #endregion

        #region 请求Url，不发送数据
        /// <summary>
        /// 请求Url，不发送数据
        /// </summary>
        public static string RequestUrl(string url, string method)
        {
            // 设置参数
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            CookieContainer cookieContainer = new CookieContainer();
            request.CookieContainer = cookieContainer;
            request.AllowAutoRedirect = true;
            request.Method = method;
            request.ContentType = "text/html";
            request.Headers.Add("charset", "utf-8");

            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            //返回结果网页（html）代码
            string content = sr.ReadToEnd();
            return content;
        }
        #endregion

        #region 请求Url，发送json数据
        /// <summary>
        /// 请求Url，发送json数据
        /// </summary>
        public static string RequestUrlSendMsg(string url, string method, string JSONData)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JSONData);
            // 设置参数
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            CookieContainer cookieContainer = new CookieContainer();
            request.CookieContainer = cookieContainer;
            request.AllowAutoRedirect = true;
            request.Method = method;
            request.ContentType = "text/html";
            request.Headers.Add("charset", "utf-8");
            Stream reqstream = request.GetRequestStream();
            reqstream.Write(bytes, 0, bytes.Length);
            //声明一个HttpWebRequest请求  
            request.Timeout = 90000;
            //设置连接超时时间  
            request.Headers.Set("Pragma", "no-cache");
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(responseStream, Encoding.Default);
            //返回结果网页（html）代码
            string content = sr.ReadToEnd();
            return content;
        }
        #endregion
    }
}