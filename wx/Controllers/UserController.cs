using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using wx.Context;
using wx.Models;
using wx.Util;

namespace wx.Controllers
{
    public class UserController : Controller
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: User
        public ActionResult Index()
        {
            //string appid = ConfigurationManager.AppSettings["appID"];
            //string redirect_uri = "http://zengzhixiang.iask.in/Home/Index";
            //string codeUrl = Oauth2.GetCodeUrl(appid, Url.Encode(redirect_uri), "snsapi_base");
            //string code = HttpRequestUtil.RequestUrl(codeUrl);
            return View();
        }

        // GET: User/Details/5

        public ActionResult Video(string code)
        {
            return View();
        }

        public ActionResult Hls()
        {
            return View();
        }

        public ActionResult Wenjuan()
        {
            return View();
        }

        public ActionResult Tiao()
        {
            log.Info("-----tiao-----");
            //string appid = ConfigurationManager.AppSettings["appID"];
            //string redirect_uri = "http://jiahao.azeng1990.top/User/Wenjuan";
            //string codeUrl = Oauth2.GetCodeUrl(appid, Url.Encode(redirect_uri), "snsapi_base");
            //return Redirect(codeUrl);
            return Redirect("/User/Wenjuan");
        }

        public ActionResult UserInfo(string code)
        {
            log.Info("---code---" + code);
            string appid = ConfigurationManager.AppSettings["appID"];
            string appsecret = ConfigurationManager.AppSettings["appsecret"];

            string openid = Oauth2.CodeGetOpenid(appid, appsecret, code);

            SqlDbContext sqlDbContext = new SqlDbContext();
            WxUser user = sqlDbContext.WxUser.Where(wx => wx.openid == openid).SingleOrDefault();

            ViewBag.nickName = user.nickname;
            ViewBag.headimgurl = user.headimgurl;
            ViewBag.openid = user.openid;
            ViewBag.country = user.country;
            ViewBag.province = user.province;
            ViewBag.city = user.city;

            //ViewBag.nickName = "123";
            //ViewBag.headimgurl = "https://fuss10.elemecdn.com/e/5d/4a731a90594a4af544c0c25941171jpeg.jpeg";
            //ViewBag.openid = "456";
            //ViewBag.country = "789";
            //ViewBag.province = "0";
            //ViewBag.city = "111";

            return View();
        }

        public ActionResult ToUserInfo() 
        {
            string appid = ConfigurationManager.AppSettings["appID"];
            log.Info("---appid---" + appid);
            string redirect_uri = "http://jiahao.azeng1990.top/User/UserInfo";
            log.Info("---redirect_uri---" + redirect_uri);
            string codeUrl = Oauth2.GetCodeUrl(appid, Url.Encode(redirect_uri), "snsapi_base");
            log.Info("---codeUrl---" + codeUrl);
            return Redirect(codeUrl); //HttpRequestUtil.RequestUrl(codeUrl);
        }

        public ActionResult ToVideo()
        {
            string appid = ConfigurationManager.AppSettings["appID"];
            string redirect_uri = "http://jiahao.azeng1990.top/User/Video";
            string codeUrl = Oauth2.GetCodeUrl(appid, Url.Encode(redirect_uri), "snsapi_base");
            return Redirect(codeUrl); 
        }

        public string getSessionKey(string code) 
        {
            string url = "https://api.weixin.qq.com/sns/jscode2session?appid=wx8d97aba8b49a93ea&secret=ab61030fc80aeace3f1f61103994a1da&js_code=" + code + "&grant_type=authorization_code";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=utf-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
            string retString = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            response.Close();
            WxCode2Session rep = JsonConvert.DeserializeObject<WxCode2Session>(retString);
            string session_key = rep.session_key;
            //string phoneNumber = decrypPhone(session_key, encryptedData, iv);
            return session_key;
        }

        public string decrypPhone(string session_key, string encryptedData, string iv) {
            try
            {
                byte[] encryData = Convert.FromBase64String(encryptedData);
                RijndaelManaged rijndaelCipher = new RijndaelManaged();
                rijndaelCipher.Key = Convert.FromBase64String(session_key);
                rijndaelCipher.IV = Convert.FromBase64String(iv);
                rijndaelCipher.Mode = CipherMode.CBC;
                rijndaelCipher.Padding = PaddingMode.PKCS7;
                ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
                byte[] plainText = transform.TransformFinalBlock(encryData, 0, encryData.Length);
                string result = Encoding.Default.GetString(plainText);

                dynamic model = Newtonsoft.Json.Linq.JToken.Parse(result);
                return model.phoneNumber;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }
}
