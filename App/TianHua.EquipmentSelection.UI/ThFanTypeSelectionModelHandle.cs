using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Dreambuild.AutoCAD;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static Dictionary<string, List<double>> GetTypePolylineFromModel(List<FanParameters> jasonmodels, Point3d typepoint)
        {
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
    }
}
