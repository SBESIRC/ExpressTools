using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace ThElectricalSysDiagram
{
    /// <summary>
    /// 块转换关系的类
    /// </summary>
    public class ThRelationBlockInfo
    {
        public ThBlockInfo UpstreamBlockInfo { get; set; }//上游专业块信息
        public ThBlockInfo DownstreamBlockInfo { get; set; }//下游专业块信息


        public ThRelationBlockInfo(ThBlockInfo upstreamBlockInfo, ThBlockInfo downstreamBlockInfo)
        {
            this.UpstreamBlockInfo = upstreamBlockInfo;
            this.DownstreamBlockInfo = downstreamBlockInfo;
        }

        //public ThRelationBlockInfo() { }
    }
}
