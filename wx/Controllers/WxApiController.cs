using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using wx.Models;
using wx.Util;

namespace wx.Controllers
{
    public class WxApiController : Controller
    {
        // GET: WxApi
        public ActionResult Index()
        {
            return View();
        }

        public void GetUserInfo(string openId) {
            string accessToken = GetAccessToken();
            string userJson = BasicApi.GetUersInfo(accessToken, openId);

            WxUser user = JsonHelper.DeserializeJsonToObject<WxUser>(userJson);

            string nickname = user.nickname;
            string openid = user.openid;
            string headimgurl = user.headimgurl;
        }

        public string GetAccessToken() {
            string appid = ConfigurationManager.AppSettings["appID"]; 
            string appsecret = ConfigurationManager.AppSettings["appsecret"];
            string accessToken = BasicApi.GetTokenSession(appid, appsecret);
            return accessToken;
        }

    }
}