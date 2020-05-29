using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Runtime;

namespace ThStructureCheck.ThBeamInfo
{
    public class ThBeamInfoApp : IExtensionApplication
    {
        public void Initialize()
        {            
        }

        public void Terminate()
        {
        }
    }
    public class ThColumnInfoCommands
    {
        [CommandMethod("ThTestBeamRelate")]
        public void TestBeamRelate()
        {
        }
    }
}
