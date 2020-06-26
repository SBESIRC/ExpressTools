using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using TopoNode;
using ThCADCore.NTS;

namespace ThWSS.Column
{
    public class ThColumnGeometryService
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThColumnGeometryService instance = new ThColumnGeometryService();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThColumnGeometryService() { }
        internal ThColumnGeometryService() { }
        public static ThColumnGeometryService Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        /// <summary>
        /// 获取指定图纸中的所有柱图元
        /// </summary>
        /// <param name="dbManager"></param>
        /// <returns></returns>
        /// 通过图层信息获取图纸中的所有梁图元
        /// 其中也包括块里面的以及外部参照里面的梁图元
        public DBObjectCollection ColumnCurves(ThColumnDbManager dbManager)
        {
            var objs = new DBObjectCollection();
            var layers = ThColumnLayerManager.GeometryLayers(dbManager.HostDb);
            foreach (var curve in Utils.GetAllCurvesFromLayerNames(layers))
            {
                objs.Add(curve);
            }
            return objs;
        }

        /// <summary>
        /// 获取指定图纸中指定区域内的所有梁图元
        /// </summary>
        /// <param name="dbManager"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public DBObjectCollection ColumnCurves(ThColumnDbManager dbManager, Polyline frame)
        {
            return BeamCurves(dbManager,
                frame.GeometricExtents.MinPoint,
                frame.GeometricExtents.MaxPoint);
        }

        /// <summary>
        /// 获取指定图纸中指定窗口内的所有梁图元
        /// </summary>
        /// <param name="database"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public DBObjectCollection BeamCurves(ThColumnDbManager dbManager, Point3d pt1, Point3d pt2)
        {
            var curves = ColumnCurves(dbManager);
            using (var spatialIndex = new ThCADCoreNTSSpatialIndex(curves))
            {
                return spatialIndex.SelectCrossingWindow(pt1, pt2);
            }
        }
    }
}
