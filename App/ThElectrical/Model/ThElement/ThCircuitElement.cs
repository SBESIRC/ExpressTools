using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThElectrical.Model.ThElement
{
    public class ThCircuitElement : ThElement
    {
        public string Number { get; set; }//回路所有值
        public string CircuitType { get; set; }//回路类型
        public string CircuitNumber { get; set; }//回路编号
        public Point3d Center { get; set; }//中心位置
        public ThCircuitElement(ObjectId id) : base(id)
        {
            using (var tr = id.Database.TransactionManager.StartOpenCloseTransaction())
            {
                this.Number = id.GetObjectByID<DBText>(tr).TextString;
                //数字就是值
                this.CircuitNumber = Regex.Match(this.Number, @"\d+").Value;
                //字母就是类型
                this.CircuitType = Regex.Match(this.Number, @"[a-zA-Z]+").Value;

                this.Center = id.GetObjectByID<DBText>(tr).Get3DCenter();
                tr.Commit();
            }

        }
    }
}
