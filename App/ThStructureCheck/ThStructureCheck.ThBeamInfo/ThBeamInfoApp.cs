using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using ThStructureCheck.YJK;
using ThStructureCheck.YJK.Service;

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
            string dtlCalcPath = @"D:\柱校核\实例 - Send 1023\A3#楼 - 伪原位\计算模型\施工图\dtlCalc.ydb";
            BuildBeamLink buildBeamLink = new BuildBeamLink(dtlCalcPath, 3);
            buildBeamLink.Build();
        }
    }
}
