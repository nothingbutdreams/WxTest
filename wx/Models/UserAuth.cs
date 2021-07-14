using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace wx.Models
{
    public class UserAuth
    {
        [Key]
        public int id { get; set; }
        [StringLength(200)]
        public string openid { get; set; }//用户的唯一标识
        [StringLength(200)]
        public string mobile { get; set; }//手机号
    }
}