using System;
using System.Linq;
using GeoAPI.Geometries;
using Dreambuild.AutoCAD;
using TianHua.Publics.BaseCode;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThCADCore.NTS;

namespace TianHua.FanSelection.UI
{
    public static class ThFanTypeSelectionModelHandle
    {
        public static Dictionary<string, Polyline> GetpolylineFromeModel(List<FanParameters> jasonmodels)
        {
            Dictionary<string, List<Point3d>> fanpoints = new Dictionary<string, List<Point3d>>();
            foreach (var group in jasonmodels.GroupBy(d => d.CCCF_Spec))
            {
                fanpoints.Add(group.First().CCCF_Spec, new List<Point3d>());
                foreach (var item in group)
                {
                    if (!string.IsNullOrEmpty(item.Gears) && item.Gears == "低")
                    {
                        continue;
                    }
                    fanpoints[item.CCCF_Spec].Add(new Point3d(Convert.ToDouble(item.AirVolume), Convert.ToDouble(item.Pa), 0));
                }
            }
            Dictionary<string, Polyline> typepolylines = new Dictionary<string, Polyline>();
            foreach (var item in fanpoints)
            {
                List<Point3d> verticcoll = new List<Point3d>();
                verticcoll = item.Value.OrderBy(p => p.X).ToList();
                Polyline tppoly = new Polyline();
                for (int i = 0; i < verticcoll.Count; i++)
                {
                    tppoly.AddVertexAt(i, verticcoll[i].ToPoint2d(), 0, 0, 0);
                }
                typepolylines.Add(item.Key, tppoly);
            }
            return typepolylines;
        }

        public static Dictionary<string, ILineString> GetpolylineFromeModelEx(List<FanParameters> jasonmodels)
        {
            var fanpoints = new Dictionary<string, List<Coordinate>>();
            foreach (var group in jasonmodels.GroupBy(d => d.CCCF_Spec))
            {
                fanpoints.Add(group.First().CCCF_Spec, new List<Coordinate>());
                foreach (var item in group)
                {
                    if (!string.IsNullOrEmpty(item.Gears) && item.Gears == "低")
                    {
                        continue;
                    }

                    var coordinate = new Coordinate(
                        ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(Convert.ToDouble(item.AirVolume)),
                        ThCADCoreNTSService.Instance.PrecisionModel.MakePrecise(Convert.ToDouble(item.Pa)));
                    fanpoints[item.CCCF_Spec].Add(coordinate);
                }
            }

            var models = new Dictionary<string, ILineString>();
            foreach (var item in fanpoints)
            {
                models.Add(item.Key,
                    ThCADCoreNTSService.Instance.GeometryFactory.CreateLineString(item.Value.OrderBy(p => p.X).ToArray()));
            }
            return models;
        }

        public static Dictionary<string, List<double>> GetTypePolylineFromModel(List<FanParameters> jasonmodels, List<double> pointxyz)
        {
            Point3d typepoint = new Point3d(pointxyz[0], pointxyz[1], pointxyz[2]);
            Dictionary<string, Polyline> pardic = GetpolylineFromeModel(jasonmodels);
            List<string> typename = ThFanSelectionTypeSelect.GetTypePolyline(pardic, typepoint);

            Dictionary<string, List<double>> result = new Dictionary<string, List<double>>();
            foreach (var item in typename)
            {
                Point3d ptinline = ThFanSelectionTypeSelect.CloseVertice(pardic[item], typepoint);
                List<double> ptxyz = new List<double> { ptinline.X, ptinline.Y, ptinline.Z };
                result.Add(item, ptxyz);
            }

            return result;
        }

        //传入性能点坐标判断该点是否安全，返回否即不安全
        public static bool IfPointSafe(List<FanParameters> jasonmodels, List<double> typepointxyz)
        {
            Dictionary<string, List<double>> typepoly = GetTypePolylineFromModel(jasonmodels, typepointxyz);
            Dictionary<string, Polyline> allpolys = GetpolylineFromeModel(jasonmodels);
            if (allpolys == null) { return false; }
            Polyline currenttypepoly = allpolys[typepoly.First().Key];

            return !ThFanSelectionTypeSelect.IfTypepoSmaller(currenttypepoly, new Point3d(typepointxyz[0], typepointxyz[1], typepointxyz[2]));
        }

        public static Dictionary<string, Polyline> GetAxialPolyFromeModel(List<AxialFanParameters> jasonmodels)
        {
            Dictionary<string, List<Point3d>> fanpoints = new Dictionary<string, List<Point3d>>();
            foreach (var group in jasonmodels.GroupBy(d => d.ModelNum))
            {
                fanpoints.Add(group.First().ModelNum, new List<Point3d>());
                foreach (var item in group)
                {
                    if (!string.IsNullOrEmpty(item.Gears) && item.Gears == "低")
                    {
                        continue;
                    }
                    fanpoints[item.ModelNum].Add(new Point3d( FuncStr.NullToDouble(item.AirVolume), FuncStr.NullToDouble(item.Pa), 0));
                }
            }
            Dictionary<string, Polyline> typepolylines = new Dictionary<string, Polyline>();
            foreach (var item in fanpoints)
            {
                List<Point3d> verticcoll = new List<Point3d>();
                verticcoll = item.Value.OrderBy(p => p.X).ToList();
                Polyline tppoly = new Polyline();
                for (int i = 0; i < verticcoll.Count; i++)
                {
                    tppoly.AddVertexAt(i, verticcoll[i].ToPoint2d(), 0, 0, 0);
                }
                typepolylines.Add(item.Key, tppoly);
            }
            return typepolylines;
        }

        public static Dictionary<string, List<double>> GetAxialTypePolyFromModel(List<AxialFanParameters> jasonmodels, List<double> pointxyz)
        {
            Point3d typepoint = new Point3d(pointxyz[0], pointxyz[1], pointxyz[2]);
            Dictionary<string, Polyline> pardic = GetAxialPolyFromeModel(jasonmodels);
            List<string> typename = ThFanSelectionTypeSelect.GetTypePolyline(pardic, typepoint);

            Dictionary<string, List<double>> result = new Dictionary<string, List<double>>();
            foreach (var item in typename)
            {
                Point3d ptinline = ThFanSelectionTypeSelect.CloseVertice(pardic[item], typepoint);
                List<double> ptxyz = new List<double> { ptinline.X, ptinline.Y, ptinline.Z };
                result.Add(item, ptxyz);
            }

            return result;
        }

        //传入性能点坐标判断该点是否安全，返回否即不安全
        public static bool IfPointSafe(List<AxialFanParameters> jasonmodels, List<double> typepointxyz)
        {
            Dictionary<string, List<double>> typepoly = GetAxialTypePolyFromModel(jasonmodels, typepointxyz);
            Dictionary<string, Polyline> allpolys = GetAxialPolyFromeModel(jasonmodels);
            if (allpolys == null) { return false; }
            Polyline currenttypepoly = allpolys[typepoly.First().Key];

            return !ThFanSelectionTypeSelect.IfTypepoSmaller(currenttypepoly,new Point3d(typepointxyz[0], typepointxyz[1], typepointxyz[2]));
        }

    }
}
