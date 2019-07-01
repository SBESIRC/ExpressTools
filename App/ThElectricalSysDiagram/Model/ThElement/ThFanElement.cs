using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThElectricalSysDiagram
{
    public class ThFanElement : ThElement
    {
        public ThRelationFanInfo FanInfo { get; set; }

        public ThFanElement(ObjectId objectId, string name, ThRelationFanInfo fanInfo) : base(objectId, name)
        {
            this.FanInfo = fanInfo;
        }
    }
}
