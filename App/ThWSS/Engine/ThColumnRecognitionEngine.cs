﻿using System;
using Linq2Acad;
using DotNetARX;
using ThWSS.Column;
using ThCADCore.NTS;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThWSS.Model;

namespace ThWSS.Engine
{
    public class ThColumnRecognitionEngine : ThModeltRecognitionEngine, IDisposable
    {
        public Database HostDb { get; set; }
        public override List<ThModelElement> Elements { get; set; }
        public ThColumnRecognitionEngine(Database database)
        {
            HostDb = database;
        }

        public void Dispose()
        {
            //
        }

        public override bool Acquire(Database database, Polyline floor, ObjectId frame)
        {
            throw new NotImplementedException();
        }

        public override bool Acquire(Database database, Polyline floor, ObjectIdCollection frames)
        {
            var objs = new DBObjectCollection();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                foreach(ObjectId fame in frames)
                {
                    // 为了确保选择的多段线可以形成封闭的房间轮廓
                    // 把多段线的Closed状态设置成true
                    var pline = acadDatabase.Element<Polyline>(fame);
                    var clone = pline.GetTransformedCopy(Matrix3d.Identity) as Polyline;
                    clone.Closed = true;
                    objs.Add(clone);
                }
            }
            return Acquire(database, floor, objs);
        }

        public override bool Acquire(Database database, Polyline floor, DBObjectCollection frames)
        {
            int columnIndex = 0;
            Elements = new List<ThModelElement>();
            foreach (Polyline frame in frames)
            {
                var columnCurves = ThColumnGeometryService.Instance.ColumnCurves(HostDb, frame);
                // 构成柱的几何图元包括：
                //  1. 圆
                //  2. 多段线构成的矩形
                //  3. 多条多段线构成的矩形（内部有图案）
                // 对于圆形柱，通过获取其外接矩形，将其转化成矩形柱
                // 对于矩形柱，直接保留
                // 对于内部有图案的矩形柱，获取其外轮廓矩形
                var arcs = new DBObjectCollection();
                var lines = new DBObjectCollection();
                var circles = new DBObjectCollection();
                foreach (Curve curve in columnCurves)
                {
                    if (curve is Line line)
                    {
                        lines.Add(line);
                    }
                    else if (curve is Polyline polyline)
                    {
                        lines.Add(polyline);
                    }
                    else if (curve is Arc arc)
                    {
                        arcs.Add(arc);
                    }
                    else if (curve is Circle circle)
                    {
                        circles.Add(circle);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }

                // 对于线段，获取构成的轮廓，即为柱的矩形轮廓
                foreach (Polyline pline in lines.Outline())
                {
                    if (frame.Contains(pline))
                    {
                        Elements.Add(CreateColumn(columnIndex++, pline));
                    }
                }

                // 对于圆，获取其外接矩形，转化成矩形柱
                foreach (Circle circle in circles)
                {
                    var pline = new Polyline()
                    {
                        Closed = true,
                    };
                    pline.CreateRectangle(
                        circle.GeometricExtents.MinPoint.ToPoint2d(),
                        circle.GeometricExtents.MaxPoint.ToPoint2d());
                    if (frame.Contains(pline))
                    {
                        Elements.Add(CreateColumn(columnIndex++, pline));
                    }
                }

                // 对于圆弧，暂时不考虑，忽略
            }
            return true;
        }

        public override bool Acquire(ThModelElement element)
        {
            throw new NotImplementedException();
        }

        private ThColumn CreateColumn(int index, Polyline polyline)
        {
            return new ThColumn()
            {
                Properties = new Dictionary<string, object>()
                {
                    { string.Format("ThColumn{0}", index), polyline }
                }
            };
        }
    }
}
