using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace wx.Models
{
    public class WxUser
    {
        [Key]
        public int id { get; set; }
        [StringLength(200)]
        public string openid { get; set; }//用户的唯一标识
        [StringLength(200)]
        public string nickname { get; set; }//用户昵称
        [StringLength(200)]
        public string sex { get; set; }//用户的性别，值为1时是男性，值为2时是女性，值为0时是未知
        [StringLength(200)]
        public string province { get; set; }//用户个人资料填写的省份
        [StringLength(200)]
        public string city { get; set; }//普通用户个人资料填写的城市
        [StringLength(200)]
        public string country { get; set; }//国家，如中国为CN
        [StringLength(200)]
        public string headimgurl { get; set; }//用户头像，最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，
                                              //0代表640*640正方形头像），用户没有头像时该项为空。若用户更换头像，原有头像URL将失效。
        [StringLength(200)]
        public string unionid { get; set; }//只有在用户将公众号绑定到微信开放平台帐号后，才会出现该字段。

    }
}