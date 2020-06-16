using ThCADCore.NTS;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThStructure.BeamInfo.Business
{
    public class ThBeamGeometryPreprocessor
    {
        /// <summary>
        /// 分解曲线
        /// </summary>
        /// <param name="curves"></param>
        public static DBObjectCollection ExplodeCurves(DBObjectCollection curves)
        {
            var objs = new DBObjectCollection();
            foreach (Curve curve in curves)
            {
                if (curve is Line line)
                {
                    objs.Add(line);
                }
                else if (curve is Arc arc)
                {
                    objs.Add(arc);
                }
                else if (curve is Polyline polyline)
                {
                    using (var entitySet = new DBObjectCollection())
                    {
                        polyline.Explode(entitySet);
                        foreach (Entity entity in entitySet)
                        {
                            objs.Add(entity);
                        }
                    }
                }
            }
            return objs;
        }

        /// <summary>
        /// 合并“相交”的曲线
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static DBObjectCollection MergeCurves(DBObjectCollection curves)
        {
            // 保留2位小数
            using (var ov = new ThCADCoreNTSPrecisionReducer(100))
            using (var spatialIndex = new ThCADCoreNTSSpatialIndex(curves))
            {
                // 寻找所有"孤立"的曲线
                var objs = new DBObjectCollection();
                foreach (Curve curve in curves)
                {
                    var results = spatialIndex.SelectFence(curve);
                    // 返回的结果中默认包含“自己”
                    if (results.Count == 1)
                    {
                        objs.Add(curve);
                    }
                }

                // 这些“孤立”的曲线将会被保留
                var newCurves = new DBObjectCollection();
                foreach(DBObject obj in objs)
                {
                    newCurves.Add(obj);
                }

                // 从集合中剔除所有孤立的曲线，剩下的即为需要合并的曲线
                foreach (DBObject obj in objs)
                {
                    curves.Remove(obj);
                }
                // 合并所有非孤立的曲线，保留合并后的结果
                foreach (DBObject obj in curves.Merge())
                {
                    newCurves.Add(obj);
                }

                return newCurves;
            }
        }
    }
}
