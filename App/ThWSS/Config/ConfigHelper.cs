using System;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ThWSS.Config
{
    public class ConfigHelper
    {
        readonly string configText = Properties.Settings.Default.SprayConfig;
        XElement config;
        public XElement elementNode;

        public ConfigHelper()
        {
            if (string.IsNullOrEmpty(configText))
            {
                CreateXmlConfig();
            }
            config = XElement.Parse(configText);
            elementNode = config;
        }

        /// <summary>
        /// 读取节点
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public IEnumerable<XElement> ReadNode(string nodeName)
        {
            if (config == null)
            {
                return new List<XElement>();
            }
            return elementNode.DescendantsAndSelf(nodeName);
        }

        /// <summary>
        /// 获取节点值
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public string GetValue(string nodeName)
        {
            var xElems = ReadNode(nodeName).ToList();
            return xElems.Count > 0 ? xElems.First().Value : "";
        }

        /// <summary>
        /// 插入节点值
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="nodeValue"></param>
        public void SetValue(string nodeName, string nodeValue)
        {
            var xElems = ReadNode(nodeName).ToList();
            if (xElems.Count > 0)
            {
                xElems.First().SetValue(nodeValue);
            }

            SaveValue();
        }

        /// <summary>
        /// 存储新配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        public void SaveConfig<T>(T t)
        {
            CreateClassConfig(t, elementNode);
            Serialize(t);
        }

        private void SaveValue()
        {
            Properties.Settings.Default.SprayConfig = config.ToString();
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 序列化xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        public void Serialize<T>(T t)
        {
            PropertyInfo[] ps = t.GetType().GetProperties();
            foreach (var item in ps)
            {
                if (!(item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String")))
                {
                    var value = item.GetValue(t);
                    if (value != null)
                    {
                        Serialize(value);
                    }
                }
                else
                {
                    SetValue(item.Name, item.GetValue(t).ToString());
                }
            }
        }

        /// <summary>
        /// 反序列化xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Deserialize<T>()
        {
            T t = Activator.CreateInstance<T>();
            AnalysisClass(t);
            return t;
        }

        private void AnalysisClass<T>(T obj)
        {
            PropertyInfo[] ps = obj.GetType().GetProperties();
            foreach (var item in ps)
            {
                if (!(item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String")))
                {
                    var temp = Activator.CreateInstance(item.PropertyType);
                    AnalysisClass(temp);
                    item.SetValue(obj, temp);
                }
                else
                {
                    item.SetValue(obj, GetValue(item.Name));
                }
            }
        }
        
        /// <summary>
        /// 创建xml配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void CreateXmlConfig()
        {
            config = new XElement("layoutSettings");
            config.Add(new XElement("tab1"));
            config.Add(new XElement("tab2"));
            config.Add(new XElement("tab3"));
            config.Add(new XElement("tab4"));
            SaveValue();
        }

        /// <summary>
        /// 动态创建xml配置节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="xElem"></param>
        private void CreateClassConfig<T>(T t, XElement xElem)
        {
            PropertyInfo[] ps = t.GetType().GetProperties();
            foreach (var item in ps)
            {
                XElement xElement = new XElement(item.Name);
                xElem.Add(xElement);
                if (!(item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String")))
                {
                    var temp = Activator.CreateInstance(item.PropertyType);
                    CreateClassConfig(temp, xElement);   
                }
            }
        }
    }
}
