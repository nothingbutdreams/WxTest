using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace wx.Util
{
    public class BasicApi
    {
        public static string SessionAccessToken = "";//access_token缓存 其他接口的通行证

        public BasicApi() { }

        #region 获取access_token缓存
        public static string GetTokenSession(string AppID, string AppSecret)
        {
            string TokenSession = "";

            if (System.Web.HttpContext.Current.Session[SessionAccessToken] == null)
            {
                TokenSession = AddTokenSession(AppID, AppSecret);
            }
            else
            {
                TokenSession = System.Web.HttpContext.Current.Session[SessionAccessToken].ToString();
            }

            return TokenSession;
        }
        /// <summary>
        /// 添加AccessToken缓存
        /// </summary>
        /// <param name="AppID"></param>
        /// <param name="AppSecret"></param>
        /// <returns></returns>
        public static string AddTokenSession(string AppID, string AppSecret)
        {
            //获取AccessToken
            string AccessToken = GetAccessToken(AppID, AppSecret);
            if (AccessToken != "")
            {
                HttpContext.Current.Session[SessionAccessToken] = AccessToken;
                HttpContext.Current.Session.Timeout = 7200;
            }
            return AccessToken;
        }

        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <param name="AppID"></param>
        /// <param name="AppSecret"></param>
        /// <returns></returns>
        public static string GetAccessToken(string AppID, string AppSecret)
        {
            JavaScriptSerializer Jss = new JavaScriptSerializer();
            string getAccessTokenUrl = WxApiHelper.Get_Access_Token_Url;
            string strJson = HttpRequestUtil.RequestUrl(string.Format(getAccessTokenUrl, AppID, AppSecret));
            Dictionary<string, object> DicText = (Dictionary<string, object>)Jss.DeserializeObject(strJson);
            if (!DicText.ContainsKey("access_token"))
                return "";
            return DicText["access_token"].ToString();
        }
        #endregion

        public static string GetUersInfo(string AccessToken, string OpenId) {
            string getAccessTokenUrl = WxApiHelper.Get_User_Info_Url;
            string userJson = HttpRequestUtil.RequestUrl(string.Format(getAccessTokenUrl, AccessToken, OpenId));

            return userJson;
        }
    }
}