using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using NFox.Cad.Collections;

namespace ThElectrical.Model.ThColumn
{
    public class ThCabinetColumn : ThColumn
    {
        public ThCabinetColumn(ObjectId id, Point3d pt, double left, double right, Point3d drawPt1, Point3d drawPt2) : base(id, pt, left, right, drawPt1, drawPt2)
        {
        }

        protected override Func<OpFilter.Op, OpFilter.Op> OpFunc()
        {
            //这里运用一个技巧：在拿表格的同时，把表头文字也拿出来，这样确保source有值，不会报NULL错误
            return new Func<OpFilter.Op, OpFilter.Op>(fil => fil.Dxf(0) == "acad_table,text");
        }

        protected override Func<Entity, bool> PredicateFunc()
        {
            //****目前是表格就拿出来,判读表头是否是合并单元格
            return new Func<Entity, bool>(ent => ent is Table && (ent as Table).Cells[0, 0].IsMerged == true);
        }
    }
}
