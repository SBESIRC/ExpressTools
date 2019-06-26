using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.ThCui
{
    public class ThCommandInfo
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string HelpString { get; set; }
        public string Suffix { get; set; }

        public ThCommandInfo(string name, string command, string helpString, bool icoSuffix)
        {
            this.Name = name;
            this.Command = command;
            this.HelpString = helpString;
            this.Suffix = icoSuffix ? @".ico" : @".png";
        }
    }
}
