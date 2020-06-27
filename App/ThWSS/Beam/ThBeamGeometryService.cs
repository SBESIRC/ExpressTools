using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using TopoNode;
using ThCADCore.NTS;

namespace ThWSS.Beam
{
    public class ThBeamGeometryService
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThBeamGeometryService instance = new ThBeamGeometryService();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThBeamGeometryService() { }
        internal ThBeamGeometryService() { }
        public static ThBeamGeometryService Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        /// <summary>
        /// 获取指定图纸中的所有梁图元
        /// </summary>
        /// <param name="dbManager"></param>
        /// <returns></returns>
        /// 通过图层信息获取图纸中的所有梁图元
        /// 其中也包括块里面的以及外部参照里面的梁图元
        public DBObjectCollection BeamCurves(Database database)
        {
            var objs = new DBObjectCollection();
            var layers = ThBeamLayerManager.GeometryLayers(database);
            foreach(var curve in Utils.GetAllCurvesFromLayerNames(layers))
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
        public DBObjectCollection BeamCurves(Database database, Polyline frame)
        {
            return BeamCurves(database, 
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
        public DBObjectCollection BeamCurves(Database database, Point3d pt1, Point3d pt2)
        {
            var curves = BeamCurves(database);
            using (var spatialIndex = new ThCADCoreNTSSpatialIndex(curves))
            {
                return spatialIndex.SelectCrossingWindow(pt1, pt2);
            }
        }
    }
} 