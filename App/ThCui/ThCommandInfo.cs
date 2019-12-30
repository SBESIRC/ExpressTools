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
        public string MacroId
        {
            get
            {
                return "ID_" + Command;
            }
        }

        public ThCommandInfo(string name, string command)
        {
            this.Name = name;
            this.Command = command;
        }
    }
}
