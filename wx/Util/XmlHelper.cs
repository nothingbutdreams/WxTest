using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using wx.Models;

namespace wx.Util
{
    public class XmlHelper
    {
        public static Dictionary<string, string> GetMsgEntity(string text)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(text);
                Dictionary<string, string> dict = new Dictionary<string, string>();
                XmlNodeList xml = doc.SelectSingleNode("/xml").ChildNodes;
                foreach (XmlNode node in xml)
                {
                    dict.Add(node.Name, node.InnerText);
                }
                return dict;
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
        }

        public static ExmlMsg GetExml(string text) {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(text);

                XmlElement rootElement = doc.DocumentElement;
                ExmlMsg xmlMsg = new ExmlMsg()
                {
                    FromUserName = rootElement.SelectSingleNode("FromUserName").InnerText,
                    ToUserName = rootElement.SelectSingleNode("ToUserName").InnerText,
                    CreateTime = rootElement.SelectSingleNode("CreateTime").InnerText,
                    MsgType = rootElement.SelectSingleNode("MsgType").InnerText,
                };
                switch (xmlMsg.MsgType)
                {
                    case "text"://文本
                        xmlMsg.Content = rootElement.SelectSingleNode("Content").InnerText;
                        xmlMsg.MsgId = rootElement.SelectSingleNode("MsgId").InnerText;
                        break;
                    case "image"://图片
                        xmlMsg.PicUrl = rootElement.SelectSingleNode("PicUrl").InnerText;
                        xmlMsg.MsgId = rootElement.SelectSingleNode("MsgId").InnerText;
                        break;
                    case "event"://事件
                        xmlMsg.Event = rootElement.SelectSingleNode("Event").InnerText;
                        if (xmlMsg.Event == "subscribe")//关注类型
                        {
                            xmlMsg.EventKey = rootElement.SelectSingleNode("EventKey").InnerText;
                        }
                        break;
                    default:
                        break;
                }
                return xmlMsg;
            }
            catch (Exception)
            {
                return new ExmlMsg();
            }


        }
    }
}