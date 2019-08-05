using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using NFox.Cad.Collections;
using ThElectrical.Model.ThElement;
using TianHua.AutoCAD.Utility.ExtensionTools;
using static NFox.Cad.Collections.OpFilter;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThElectrical.Model.ThColumn
{
    public abstract class ThColumn
    {
        public ObjectId ElementId { get; set; }
        public Point3d Center { get; set; }//中心位置
        public double LeftOffset { get; set; }
        public double RightOffset { get; set; }
        public Point3d MinPoint { get; set; }//列边界点
        public Point3d MaxPoint { get; set; }


        public ThColumn(ObjectId id, Point3d pt, double left, double right, Point3d drawPt1, Point3d drawPt2)
        {
            this.ElementId = id;
            this.Center = pt;
            this.LeftOffset = left;
            this.RightOffset = right;

            //求列边界
            var pts = GetBoundaryPoints(drawPt1, drawPt2);
            this.MinPoint = pts[0];
            this.MaxPoint = pts[1];
        }


        protected Point3dCollection GetBoundaryPoints(Point3d drawPt1, Point3d drawPt2)
        {
            //左偏为X，图纸边界为Y
            var minPt = new Point3d(this.Center.X - this.LeftOffset, drawPt1.Y, 0);

            //右偏为X，图纸边界为Y
            var maxPt = new Point3d(this.Center.X + this.RightOffset, drawPt2.Y, 0);

            return new Point3dCollection() { minPt, maxPt };
        }

        //过滤函数
        protected abstract Func<Op, Op> OpFunc();

        //条件函数
        protected abstract Func<Entity, bool> PredicateFunc();

        public List<ThElement.ThElement> GetThElements()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            //在列范围内，找各种内容
            var txts = SelectionTool.DocChoose<Entity>(() => ed.SelectWindow(this.MinPoint, this.MaxPoint, OpFilter.Bulid(OpFunc()))).Where(PredicateFunc());

            //实例化列元素
            return txts.Select(txt => ThElementFactory.CreateElement(this.GetType(), txt.ObjectId)).ToList();
        }

    }
}
