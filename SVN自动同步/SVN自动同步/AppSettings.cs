using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SVN自动同步
{
    /// <summary>
    /// App.config配置类
    /// </summary>
    public class AppSettings
    {/// <summary>
        /// 获取配置文件路径
        /// </summary>
        /// <returns></returns>
        public static string AppConfig()
        {
            return System.IO.Path.Combine(Application.StartupPath, "App.config");//此处配置文件在程序目录下，或者设置为指定的配置文件路径
        }
        /// <summary>
        /// 获取配置节点值
        /// </summary>
        /// <param name="appKey">节点key值</param>
        /// <returns></returns>
        public static string GetValue(string appKey)
        {
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.Load(AppSettings.AppConfig());
                XmlNode xNode;
                XmlElement xElem;
                xNode = xDoc.SelectSingleNode("//appSettings");　　　　//补充，需要在你的app.config 文件中增加一下，<appSetting> </appSetting>
                xElem = (XmlElement)xNode.SelectSingleNode("//add[@key='" + appKey + "']");
                if (xElem != null)
                    return xElem.GetAttribute("value");
                else
                    return "";
            }
            catch (Exception)
            {
                return "";
            }
        }
        /// <summary>
        /// 设置配置节点值
        /// </summary>
        /// <param name="AppKey">key</param>
        /// <param name="AppValue">value</param>
        public static void SetValue(string AppKey, string AppValue)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(AppSettings.AppConfig());
            XmlNode xNode;
            XmlElement xElem1;
            XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//appSettings");
            xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            if (xElem1 != null)
            {
                xElem1.SetAttribute("value", AppValue);
            }
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("key", AppKey);
                xElem2.SetAttribute("value", AppValue);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(AppSettings.AppConfig());
        }
    }
}
