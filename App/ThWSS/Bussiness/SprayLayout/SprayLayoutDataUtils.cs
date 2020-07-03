using System.Linq;
using ThCADCore.NTS;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    public class SprayLayoutDataUtils
    {
        public static DBObjectCollection Radii(List<SprayLayoutData> sprays)
        {
            var objs = new DBObjectCollection();
            foreach(var curve in sprays.Select(o => o.Radii))
            {
                objs.Add(curve);
            }

            return objs.Union();
        }

        /// <summary>
        /// 计算出需要调整的点
        /// </summary>
        /// <param name="sprays"></param>
        /// <param name="blindRegions"></param>
        /// <returns></returns>
        public static Dictionary<Polyline, List<SprayLayoutData>> CalAdjustmentSpray(List<SprayLayoutData> sprays, DBObjectCollection blindRegions)
        {
            Dictionary<Polyline, List<SprayLayoutData>> resDic = new Dictionary<Polyline, List<SprayLayoutData>>();
            foreach (var spray in sprays)
            {
                foreach (var bRegion in blindRegions.Cast<Polyline>())
                {
                    if((spray.Radii as Polyline).ToNTSPolygon().Intersects(bRegion.ToNTSPolygon()))
                    {
                        if (resDic.Keys.Contains(bRegion))
                        {
                            resDic[bRegion].Add(spray);
                        }
                        else
                        {
                            resDic.Add(bRegion, new List<SprayLayoutData>() { spray });
                        }
                    }
                }
            }

            return resDic;
        }
    }
}
