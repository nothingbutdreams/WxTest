using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wx.Util
{
    public class WxApiHelper
    {
        public static string Get_Access_Token_Url =
            "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";

        public static string Get_User_Info_Url =
            "https://api.weixin.qq.com/cgi-bin/user/info?access_token={0}&openid={1}&lang=zh_CN";
    }
}