using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThElectrical.Model.ThColumn
{
    public static class ThColumnFactory
    {
        public static Tolerance columneDistinctTol = new Tolerance(0, 5);//去重的容差
        public const double columnTolerance = 1000;//列高的容差
        public const string powerCapacityName = "容量";
        public const string circuitName = "回路";
        public const string outCableName = "出线电缆及导线型号规格";
        public const string branchSwitchName = "支路开关型号及规格";
        public const string cabinetName = "配电箱名称";

        public static List<string> columnNames = new List<string> { powerCapacityName, circuitName, outCableName, branchSwitchName, cabinetName };
        public static ThColumn CreateColumn(string type, ObjectId id, Point3d pt, Point3d minDrawPt, Point3d maxDrawPt)
        {
            switch (type)
            {
                case powerCapacityName:
                    return new ThPowerCapacityColumn(id, pt, 540, 540, minDrawPt, maxDrawPt);
                case circuitName:
                    return new ThCircuitColumn(id, pt, 555, 526, minDrawPt, maxDrawPt);
                case outCableName:
                    return new ThOutCableColumn(id, pt, 2700, 2700, minDrawPt, maxDrawPt);
                case branchSwitchName:
                    return new ThBranchSwitchColumn(id, pt, 3151, 3151, minDrawPt, maxDrawPt);
                case cabinetName:
                    return new ThCabinetColumn(id, pt, 1200, 1200, minDrawPt, maxDrawPt);
                default:
                    return null;
            }

        }
    }
}
