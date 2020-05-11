using System;
using System.Collections.Generic;
using System.Linq;

using AcHelper;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using Autodesk.AutoCAD.BoundaryRepresentation;
using AcBr = Autodesk.AutoCAD.BoundaryRepresentation;

namespace ThSitePlan.Engine
{
    public class ThSitePlanRoadHatchWorker : ThSitePlanCADWorker
    {
        public ThSitePlanRoadHatchWorker(string[] dxfNames,
    PolygonSelectionMode mode = PolygonSelectionMode.Window) : base(dxfNames, mode)
        {
            //
        }

        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            string roadcenterlay = "P-TRAF-CITY";
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                Polyline framepl = acadDatabase.Element<Polyline>((ObjectId)options.Options["Frame"]);
                using (var objs = SelectByLayer(roadcenterlay, (ObjectId)options.Options["Frame"]))
                {
                    Dictionary<string, double> po = GetPointInRoad(objs, framepl);
                    if (objs.Count == 0 || po == null)
                    {
                        return false;
                    }
                    Point3d pointinroad = new Point3d(Convert.ToDouble(po["X"]), Convert.ToDouble(po["Y"]), Convert.ToDouble(po["Z"]));
                    Active.Editor.EraseCmd(objs);
                    Active.Editor.CreateHatchWithPoint(framepl, pointinroad);
                }
            }
            return true;
        }

        private ObjectIdCollection SelectByLayer(string layername,ObjectId frameid)
        {
            TypedValue[] values = { new TypedValue((int)DxfCode.LayerName, layername) };
            var filter = new SelectionFilter(values);
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(frameid,PolygonSelectionMode.Crossing,filter);
            if (psr.Status == PromptStatus.OK)
            {
                ObjectIdCollection roadcenter = psr.Value.GetObjectIds().ToObjectIdCollection();
                return roadcenter;
            }
            else
            {
                return new ObjectIdCollection();
            }
        }

        //获取道路中心线上的一点
        private Dictionary<string , double> GetPointInRoad(ObjectIdCollection oids,Polyline frameline)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (ObjectId item in oids)
                {
                    var centerlineitem = acadDatabase.Element<Curve>(item);
                    var lineendpoint = centerlineitem.EndPoint;
                    var linestartpoint = centerlineitem.StartPoint;
                    double endtostartdist = lineendpoint.DistanceTo(linestartpoint);
                    var innerlinepoint = centerlineitem.GetPointAtDist(endtostartdist*0.5);
                    PointContainment isendpointin = GetPointContainment(frameline, innerlinepoint);
                    if (isendpointin == PointContainment.Inside)
                    {
                        var dc = new Dictionary<string, double> { { "X", innerlinepoint.X }, { "Y", innerlinepoint.Y }, { "Z", innerlinepoint.Z } };
                        return dc;
                    }
                }
                return null;
            }
        }

        //判断点是否在一个封闭的PolyLine构成的面域内
        private PointContainment GetPointContainment(Polyline pl, Point3d point)
        {
            var rgs = GetRegionFromPolyline(pl).First();
            PointContainment result = PointContainment.Outside;
            using (Brep brep = new Brep(rgs))
            {
                if (brep != null)
                {
                    using (BrepEntity ent = brep.GetPointContainment(point, out result))
                    {
                        if (ent is AcBr.Face)
                        {
                            result = PointContainment.Inside;
                        }
                    }
                }
            }
            return result;
        }

        //通过一Polyline获取面域
        private List<Region> GetRegionFromPolyline(Polyline poly)
        {
            var regions = new List<Region>();

            var sourceCol = new DBObjectCollection();
            var dbObj = poly.Clone() as Polyline;
            dbObj.Closed = true;
            sourceCol.Add(dbObj);

            var dbObjs = Region.CreateFromCurves(sourceCol);
            foreach (var obj in dbObjs)
            {
                if (obj is Region) regions.Add(obj as Region);
            }

            return regions;
        }

    }
}
