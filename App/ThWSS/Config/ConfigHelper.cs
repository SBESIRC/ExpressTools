using System;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ThWSS.Config
{
    public class ConfigHelper
    {
        readonly string configPath = @"../../Config/config.xml";
        XElement config;
        public XElement elementNode;

        public ConfigHelper()
        {
            config = XElement.Load(configPath);
            elementNode = config;
        }

        public IEnumerable<XElement> ReadNode(string nodeName)
        {
            return elementNode.DescendantsAndSelf(nodeName);
        }

        public string GetValue(string nodeName)
        {
            var xElems = ReadNode(nodeName).ToList();
            return xElems.Count > 0 ? xElems.First().Value : "";
        }

        public void SetValue(string nodeName, string nodeValue)
        {
            var xElems = ReadNode(nodeName).ToList();
            if (xElems.Count > 0)
            {
                xElems.First().SetValue(nodeValue);
            }

            SaveConfig();
        }

        public void SaveConfig()
        {
            config.Save(configPath);
        }

        public void Serialize<T>(T t)
        {
            PropertyInfo[] ps = t.GetType().GetProperties();
            foreach (var item in ps)
            {
                if (!(item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String")))
                {
                    var value = item.GetValue(t, null);
                    if (value != null)
                    {
                        Serialize(value);
                    }
                }
                else
                {
                    SetValue(item.Name, item.GetValue(t, null).ToString());
                }
            }
        }

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
                    item.SetValue(obj, temp, null);
                }
                else
                {
                    item.SetValue(obj, GetValue(item.Name), null);
                }
            }
        }
    }
}
