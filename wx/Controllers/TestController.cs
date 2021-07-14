using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using wx.Models;

namespace wx.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        const string Token = "hellowb";//你的token
        #region 以下代码只用于第一次验证  验证完后请注释
        protected void Page_load(object sender, EventArgs e)
        {
            string poststr = "";
            if (Request.HttpMethod.ToLower() == "post")
            {
                System.IO.Stream s = System.Web.HttpContext.Current.Request.InputStream;
                byte[] b = new byte[s.Length];
                s.Read(b, 0, (int)s.Length);
                poststr = System.Text.Encoding.UTF8.GetString(b);
                if (!string.IsNullOrEmpty(poststr))
                {
                    Response.End();
                }
                //writelog("poststr:" + poststr);
            }
            else
            {
                Valid();
            }
        }
        #endregion

        #region 以下是正常使用时的pageload  请在验证时将其注释  并保证在正常使用时可用
        /// <summary>
        /// 以下是正常使用时的pageload  请在验证时将其注释  并保证在正常使用时可用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected void Page_Load(object sender, EventArgs e)
        //{

        //    if (Request.HttpMethod == "POST")
        //    {
        //        string weixin = "";
        //        weixin = PostInput();//获取xml数据
        //        if (!string.IsNullOrEmpty(weixin))
        //        {
        //            ResponseMsg(weixin);//调用消息适配器
        //        }
        //    }
        //}
        #endregion

        #region 获取post请求数据
        /// <summary>
        /// 获取post请求数据
        /// </summary>
        /// <returns></returns>
        private string PostInput()
        {
            Stream s = System.Web.HttpContext.Current.Request.InputStream;
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            return Encoding.UTF8.GetString(b);
        }
        #endregion

        #region 消息类型适配器
        private void ResponseMsg(string weixin)// 服务器响应微信请求
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(weixin);//读取xml字符串
            XmlElement root = doc.DocumentElement;
            ExmlMsg xmlMsg = GetExmlMsg(root);
            //XmlNode MsgType = root.SelectSingleNode("MsgType");
            //string messageType = MsgType.InnerText;
            string messageType = xmlMsg.MsgType;//获取收到的消息类型。文本(text)，图片(image)，语音等。


            try
            {

                switch (messageType)
                {
                    //当消息为文本时
                    case "text":
                        textCase(xmlMsg);
                        break;
                    case "event":
                        if (!string.IsNullOrEmpty(xmlMsg.Event) && xmlMsg.Event.Trim() == "subscribe")
                        {
                            //刚关注时的时间，用于欢迎词  
                            int nowtime = ConvertDateTimeInt(DateTime.Now);
                            string msg = "你要关注我，我有什么办法。随便发点什么试试吧~~~";
                            string resxml = "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                            Response.Write(resxml);
                        }
                        break;
                    case "image":
                        break;
                    case "voice":
                        break;
                    case "vedio":
                        break;
                    case "location":
                        break;
                    case "link":
                        break;
                    default:
                        break;
                }
                Response.End();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        private string getText(ExmlMsg xmlMsg)
        {
            string con = xmlMsg.Content.Trim();

            System.Text.StringBuilder retsb = new StringBuilder(200);
            retsb.Append("这里放你的业务逻辑");
            retsb.Append("接收到的消息：" + xmlMsg.Content);
            retsb.Append("用户的OPEANID：" + xmlMsg.FromUserName);

            return retsb.ToString();
        }


        #region 操作文本消息 + void textCase(XmlElement root)
        private void textCase(ExmlMsg xmlMsg)
        {
            int nowtime = ConvertDateTimeInt(DateTime.Now);
            string msg = "";
            msg = getText(xmlMsg);
            string resxml = "<xml><ToUserName><![CDATA[" + xmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + xmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";
            Response.Write(resxml);

        }
        #endregion

        #region 将datetime.now 转换为 int类型的秒
        /// <summary>
        /// datetime转换为unixtime
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
        private int converDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        /// <summary>
        /// unix时间转换为datetime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private DateTime UnixTimeToTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        #endregion

        #region 验证微信签名 保持默认即可
        /// <summary>
        /// 验证微信签名
        /// </summary>
        /// * 将token、timestamp、nonce三个参数进行字典序排序
        /// * 将三个参数字符串拼接成一个字符串进行sha1加密
        /// * 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信。
        /// <returns></returns>
        private bool CheckSignature()
        {
            string signature = Request.QueryString["signature"].ToString();
            string timestamp = Request.QueryString["timestamp"].ToString();
            string nonce = Request.QueryString["nonce"].ToString();
            string[] ArrTmp = { Token, timestamp, nonce };
            Array.Sort(ArrTmp);     //字典排序
            string tmpStr = string.Join("", ArrTmp);
            tmpStr = FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1");
            tmpStr = tmpStr.ToLower();
            if (tmpStr == signature)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Valid()
        {
            string echoStr = Request.QueryString["echoStr"].ToString();
            //if (CheckSignature())
            //{
            //    if (!string.IsNullOrEmpty(echoStr))
            //    {
                    Response.Write(echoStr);
                    Response.End();
            //    }
            //}
        }
        #endregion

        #region 写日志(用于跟踪) ＋　WriteLog(string strMemo, string path = "*****")
        /// <summary>
        /// 写日志(用于跟踪)
        /// 如果log的路径修改,更改path的默认值
        /// </summary>
        private void WriteLog(string strMemo, string path = "wx.txt")
        {
            string filename = Server.MapPath(path);
            StreamWriter sr = null;
            try
            {
                if (!System.IO.File.Exists(filename))
                {
                    sr = System.IO.File.CreateText(filename);
                }
                else
                {
                    sr = System.IO.File.AppendText(filename);
                }
                sr.WriteLine(strMemo);
            }
            catch
            {

            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
        }
        //#endregion 
        #endregion

        #region 接收的消息实体类 以及 填充方法
        //private class ExmlMsg
        //{
        //    /// <summary>
        //    /// 本公众账号
        //    /// </summary>
        //    public string ToUserName { get; set; }
        //    /// <summary>
        //    /// 用户账号
        //    /// </summary>
        //    public string FromUserName { get; set; }
        //    /// <summary>
        //    /// 发送时间戳
        //    /// </summary>
        //    public string CreateTime { get; set; }
        //    /// <summary>
        //    /// 发送的文本内容
        //    /// </summary>
        //    public string Content { get; set; }
        //    /// <summary>
        //    /// 消息的类型
        //    /// </summary>
        //    public string MsgType { get; set; }
        //    /// <summary>
        //    /// 事件名称
        //    /// </summary>
        //    public string EventName { get; set; }

        //}

        private ExmlMsg GetExmlMsg(XmlElement root)
        {
            ExmlMsg xmlMsg = new ExmlMsg()
            {
                FromUserName = root.SelectSingleNode("FromUserName").InnerText,
                ToUserName = root.SelectSingleNode("ToUserName").InnerText,
                CreateTime = root.SelectSingleNode("CreateTime").InnerText,
                MsgType = root.SelectSingleNode("MsgType").InnerText,
            };
            if (xmlMsg.MsgType.Trim().ToLower() == "text")
            {
                xmlMsg.Content = root.SelectSingleNode("Content").InnerText;
            }
            else if (xmlMsg.MsgType.Trim().ToLower() == "event")
            {
                xmlMsg.Event = root.SelectSingleNode("Event").InnerText;
            }
            return xmlMsg;
        }
        #endregion

    }
}