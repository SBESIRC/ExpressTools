using GeoAPI.Geometries;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.NTS
{
    public class ThSitePlanNTSService
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanNTSService instance = new ThSitePlanNTSService();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanNTSService() { }
        internal ThSitePlanNTSService() { }
        public static ThSitePlanNTSService Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        private IGeometryFactory geometryFactory;
        public IGeometryFactory GeometryFactory
        {
            get
            {
                if (geometryFactory == null)
                {
                    geometryFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory();
                }
                return geometryFactory;
            }
        }

        public IPrecisionModel PrecisionModel
        {
            get
            {
                return GeometryFactory.PrecisionModel;
            }
        }
    }
}
