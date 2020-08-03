using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Dreambuild.AutoCAD;
using System;
using System.Collections.Generic;
using System.Linq;
using TianHua.Publics.BaseCode;

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
            //若一个性能点对应两条性能曲线，任意一条性能曲线的对应值不满足安全条件都返回false
            if (typepoly.Any(p => p.Value[1] >= typepointxyz[1]))
            {
                return false;
            }
            return true;
        }

        public static Dictionary<string, Polyline> GetAxialPolyFromeModel(List<AxialFanParameters> jasonmodels)
        {
            Dictionary<string, List<Point3d>> fanpoints = new Dictionary<string, List<Point3d>>();
            foreach (var group in jasonmodels.GroupBy(d => d.ModelNum))
            {
                fanpoints.Add(group.First().ModelNum, new List<Point3d>());
                foreach (var item in group)
                {
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
            //若一个性能点对应两条性能曲线，任意一条性能曲线的对应值不满足安全条件都返回false
            if (typepoly.Any(p=>p.Value[1] >= typepointxyz[1]))
            {
                return false;
            }
            return true;
        }

    }
}
