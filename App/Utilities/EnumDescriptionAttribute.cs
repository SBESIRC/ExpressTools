using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public class EnumDescriptionAttribute : Attribute
    {
        private string m_strDescription;
        public EnumDescriptionAttribute(string strPrinterName)
        {
            m_strDescription = strPrinterName;
        }

        public string Description
        {
            get { return m_strDescription; }
        }
    }
}
