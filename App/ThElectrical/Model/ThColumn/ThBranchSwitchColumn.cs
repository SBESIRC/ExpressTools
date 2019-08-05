using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using NFox.Cad.Collections;

namespace ThElectrical.Model.ThColumn
{
    public class ThBranchSwitchColumn : ThColumn
    {
        public ThBranchSwitchColumn(ObjectId id, Point3d pt, double left, double right, Point3d drawPt1, Point3d drawPt2) : base(id, pt, left, right, drawPt1, drawPt2)
        {
        }

        protected override Func<OpFilter.Op, OpFilter.Op> OpFunc()
        {
            return new Func<OpFilter.Op, OpFilter.Op>(fil => fil.Dxf(0) == "text");
        }

        protected override Func<Entity, bool> PredicateFunc()
        {
            //去除所有汉字，具体逻辑规则，还待进一步确认
            return new Func<Entity, bool>(txt => Regex.Match((txt as DBText).TextString, @"[^\u4E00-\u9FFF]+").Success);
        }
    }
}
