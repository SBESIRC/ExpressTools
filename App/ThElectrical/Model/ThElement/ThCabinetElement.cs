using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThElectrical.Model.ThElement
{
    public class ThCabinetElement : ThElement
    {
        public Point3d CabinetCenter { get; set; }
        public Point3d MinPoint { get; set; }
        public Point3d MaxPoint { get; set; }
        public string CabinetName { get; set; }//配电箱名称
        public ThCabinetElement(ObjectId id) : base(id)
        {
            using (var tr = id.Database.TransactionManager.StartOpenCloseTransaction())
            {
                this.MinPoint = id.GetObjectByID<Table>(tr).GeometricExtents.MinPoint;
                this.MaxPoint = id.GetObjectByID<Table>(tr).GeometricExtents.MaxPoint;
                this.CabinetCenter = id.GetObjectByID<Table>(tr).Get3DCenter();
                this.CabinetName = id.GetObjectByID<Table>(tr).Cells[0, 0].GetRealTextString();

                tr.Commit();
            }
        }

    }
}
