using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using wx.Context;
using wx.Models;
using wx.Util;

namespace wx.Controllers
{
    public class HomeController : Controller
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ViewWx()
        {
            string name = "假面";
            SqlDbContext sqlDbContext = new SqlDbContext();
            UserAuth user = new UserAuth();
            user.mobile = "123456";
            user.openid = "111111";
            sqlDbContext.UserAuth.Add(user);
            sqlDbContext.SaveChanges();
            //SqlParameter[] param = new SqlParameter[] {
            //new SqlParameter("@name",name),
            //};
            //string sql = "select *from pers where name = @name";
            ////string sql = "select *from pers";
            //string phone = "";
            //using (SqlDataReader sdr = SqlHelper.ExecuteReader(sql, CommandType.Text, param)) {
            //    while (sdr.Read()) {
            //        phone = sdr["phone"].ToString();
            //    }
            //}


            /*SqlDb _db = new SqlDb();
            bool r = _db.OpenConnection();
            if (r)
            {
                return Content("连接成功");
            }
            else
            {
                return Content("连接失败");
            }*/

            return View();
        }

        public void Wx() {
            if (HttpContext.Request.RequestType == "GET")
            {
                log.Info("wx---get---"+ HttpContext.Request.RequestType);
                #region 验证请求来源是否是微信
                string signature = Request["signature"]?.ToString();
                string timestamp = Request["timestamp"]?.ToString();
                string nonce = Request["nonce"]?.ToString();
                string echostr = Request["echostr"]?.ToString();
                string token = "hellowb";
                List<string> list = new List<string>() { token, timestamp, nonce };
                list.Sort();
                string data = string.Join("", list);
                byte[] temp1 = Encoding.UTF8.GetBytes(data);
                SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
                byte[] temp2 = sha.ComputeHash(temp1);

                var hashCode = BitConverter.ToString(temp2);
                hashCode = hashCode.Replace("-", "").ToLower();

                if (hashCode == signature)
                {
                    Response.Write(echostr);
                    Response.End();
                }
                #endregion
            }
            else {
                log.Info("wx---post---" + HttpContext.Request.RequestType);
                //ProcessPost(Request);
                ProcessWxPost(Request);
            }
        }

        private void ProcessWxPost(HttpRequestBase request)
        {
            var istream = Request.InputStream;
            byte[] temp = new byte[istream.Length];
            istream.Read(temp, 0, (int)istream.Length);
            string xml = Encoding.UTF8.GetString(temp);
            ExmlMsg exmlMsg = XmlHelper.GetExml(xml);
            string msgType = exmlMsg.MsgType;
            log.Info("msgType---" + msgType);
            switch (msgType) {
                case "text":
                    ProcessWxTextMsg(exmlMsg);
                    break;
                case "event":
                    ProcessWxEventMsg(exmlMsg);
                    break;
                case "image":
                    break;
                case "voice":
                    break;
                case "vedio":
                    break;
                case "shortvideo":
                    break;
                case "location":
                    break;
                case "link":
                    break;
                default:
                    break;
            }
        }

        private void ProcessPost(HttpRequestBase request)
        {
            var istream = Request.InputStream;
            byte[] temp = new byte[istream.Length];
            istream.Read(temp, 0, (int)istream.Length);
            string xml = Encoding.UTF8.GetString(temp);
            var dic = XmlHelper.GetMsgEntity(xml);
            switch (dic["MsgType"])
            {
                case "text":
                    ProcessTextMsg(dic, request);
                    break;
                default:
                    break;
            }
        }

        private void ProcessTextMsg(Dictionary<string, string> dic, HttpRequestBase request)
        {
            string tempxml = "<xml><ToUserName><![CDATA[-tname]]></ToUserName><FromUserName><![CDATA[-fname]]></FromUserName><CreateTime>-time</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[-content]]></Content></xml>";
            tempxml = tempxml.Replace("-tname", dic["FromUserName"]);
            tempxml = tempxml.Replace("-fname", dic["ToUserName"]);
            tempxml = tempxml.Replace("-time", dic["CreateTime"]);
            tempxml = tempxml.Replace("-content", $"MsgType:{dic["MsgType"]}\ncontent:{dic["Content"]}\nMsgId:{dic["MsgId"]}");
            Response.Write(tempxml);
            Response.End();
        }

        private void ProcessWxTextMsg(ExmlMsg exmlMsg) {
            string tempxml = "<xml><ToUserName><![CDATA[-tname]]></ToUserName><FromUserName><![CDATA[-fname]]></FromUserName><CreateTime>-time</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[-content]]></Content></xml>";
            tempxml = tempxml.Replace("-tname", exmlMsg.FromUserName);
            tempxml = tempxml.Replace("-fname", exmlMsg.ToUserName);
            tempxml = tempxml.Replace("-time", exmlMsg.CreateTime);
            tempxml = tempxml.Replace("-content", $"MsgType:{exmlMsg.MsgType}\ncontent:{exmlMsg.Content}\nMsgId:{exmlMsg.MsgId}");
            Response.Write(tempxml);
            Response.End();
        }

        private void ProcessWxEventMsg(ExmlMsg exmlMsg) {
            log.Info("exmlMsg.Event.Trim---"+exmlMsg.Event.Trim());
            if (!string.IsNullOrEmpty(exmlMsg.Event) && exmlMsg.Event.Trim() == "subscribe")
            {
                log.Info("---subscribe---");
                //刚关注时的时间，用于欢迎词  
                int nowtime = ConvertDateTimeInt(DateTime.Now);
                string msg = "你要关注我，我有什么办法。随便发点什么试试吧~~~";
                string resxml = "<xml><ToUserName><![CDATA[" + exmlMsg.FromUserName + "]]></ToUserName><FromUserName><![CDATA[" + exmlMsg.ToUserName + "]]></FromUserName><CreateTime>" + nowtime + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + msg + "]]></Content><FuncFlag>0</FuncFlag></xml>";
                Response.Write(resxml);
                log.Info("---openid---"+ exmlMsg.FromUserName);
                //数据库新增绑定
                SaveWxUser(exmlMsg.FromUserName);
            }
            else if (!string.IsNullOrEmpty(exmlMsg.Event) && exmlMsg.Event.Trim() == "unsubscribe") {
                log.Info("---unsubscribe---");
                //数据库删除绑定
                DeleteWxUser(exmlMsg.FromUserName);
            }
        }

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

        #region 保存订阅，删除取消订阅用户
        public void SaveWxUser(string openId)
        {
            log.Info("---SaveWxUser---");
            string appid = ConfigurationManager.AppSettings["appID"];
            log.Info("---appid---"+ appid);
            string appsecret = ConfigurationManager.AppSettings["appsecret"];
            log.Info("---appsecret---" + appsecret);
            string accessToken = BasicApi.GetTokenSession(appid, appsecret);
            log.Info("---accessToken---" + accessToken);

            string userJson = BasicApi.GetUersInfo(accessToken, openId);
            log.Info("---userJson---" + userJson);

            WxUser user = JsonHelper.DeserializeJsonToObject<WxUser>(userJson);
            log.Info("---user---" + user);

            SqlDbContext sqlDbContext = new SqlDbContext();
            sqlDbContext.WxUser.Add(user);
            sqlDbContext.SaveChanges();
            log.Info("---WxUser.Add---sucess---");
        }

        public void DeleteWxUser(string openId) 
        {
            SqlDbContext sqlDbContext = new SqlDbContext();
            WxUser getUser = sqlDbContext.WxUser.Where(wx => wx.openid == openId).SingleOrDefault();
            sqlDbContext.WxUser.Remove(getUser);
            sqlDbContext.SaveChanges();
        }

        #endregion

    }
}