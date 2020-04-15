using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class PropertyInfo:CNotifyPropertyChange
    {
        private string text = "";
        private string name = "";
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                NotifyPropertyChange("Text");
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyPropertyChange("Name");
            }
        }
    }
}
