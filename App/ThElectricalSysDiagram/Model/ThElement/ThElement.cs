using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThElectricalSysDiagram
{
    public class ThElement
    {
        public ObjectId ElementId { get; set; }//转换元素的id
        public string Name { get; set; }//转换元素的名字
        public ThElement(ObjectId objectId,string name)
        {
            this.ElementId = objectId;
            this.Name = name;
        }
    }
}
