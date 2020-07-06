using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.DatabaseServices;
using acdb = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using ThColumnInfo.View;

namespace ThColumnInfo
{
    public class ColumnDrawRule:DrawableOverrule
    {
        public ColumnDrawRule()
        {

        }
        public override bool WorldDraw(Drawable drawable, WorldDraw wd)
        {
#if ACAD2012
            return drawable.WorldDraw(wd);
#else
            acdb.Polyline2d polyline2d = drawable as acdb.Polyline2d;
            //if (DrawableOverruleController.handleList.IndexOf(polyline2d.ObjectId.Handle.ToString())<0)
            //{
            //    return base.WorldDraw(drawable, wd);
            //}
          
            Point3d minPt = Point3d.Origin;
            Point3d maxPt = Point3d.Origin;
            if(polyline2d.GeometricExtents!=null)
            {
                minPt = polyline2d.GeometricExtents.MinPoint;
                maxPt = polyline2d.GeometricExtents.MaxPoint;
            }
           
            Point2d pt1 = new Point2d(minPt.X, minPt.Y);
            Point2d pt2 = new Point2d(maxPt.X, minPt.Y);
            Point2d pt3 = new Point2d(maxPt.X, maxPt.Y);
            Point2d pt4 = new Point2d(minPt.X, maxPt.Y);
            Point2dCollection p2ds = new Point2dCollection();
            p2ds.Add(pt1);
            p2ds.Add(pt2);
            p2ds.Add(pt3);
            p2ds.Add(pt4);
            p2ds.Add(pt1);

            DoubleCollection doubleCol = new DoubleCollection();
            doubleCol.Add(0.0);
            doubleCol.Add(0.0);
            doubleCol.Add(0.0);
            doubleCol.Add(0.0);
            doubleCol.Add(0.0);

            Hatch hatch = new Hatch();
            hatch.PatternScale = 5;
            hatch.SetDatabaseDefaults();
            hatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
            hatch.ColorIndex = 3;
            hatch.AppendLoop(HatchLoopTypes.Polyline, p2ds, doubleCol);
            hatch.EvaluateHatch(true);


            Point3dCollection pts = new Point3dCollection();
            pts.Add(minPt);
            pts.Add(new Point3d(maxPt.X, minPt.Y, minPt.Z));
            pts.Add(new Point3d(maxPt.X, maxPt.Y, minPt.Z));
            pts.Add(new Point3d(minPt.X, maxPt.Y, maxPt.Z));
            pts.Add(minPt);

            wd.Geometry.Polygon(pts);
            hatch.WorldDraw(wd);
            return true;
#endif
        }
        public override bool IsApplicable(RXObject overruledSubject)
        {
            //Polyline2d polyline = overruledSubject as Polyline2d;
            //if(polyline!=null )
            //{
            //    if(polyline.Layer== CheckResult._searchFields.ColumnRangeLayerName)
            //    {
            //        return true;
            //    }
            //}
            return false;
        }
    }
}
