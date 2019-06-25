using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThElectricalSysDiagram
{
    class ThElectricalSysDiagramUtils
    {
        public static string BlockTemplateFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), 
                                @"Autodesk\ApplicationPlugins\ThCADPlugin.bundle\Contents\Support", 
                                @"块表样本.dwg");
        }
    }
}