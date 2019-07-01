using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThElectricalSysDiagram
{
    public class ThBlockElement : ThElement
    {
        public ThRelationBlockInfo BlockInfo { get; set; }//对应的块转换的关系

        public ThBlockElement(ObjectId objectId, string name, ThRelationBlockInfo blockInfo) : base(objectId, name)
        {
            this.BlockInfo = blockInfo;
        }
    }
}
