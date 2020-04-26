﻿using GeoAPI.Geometries;
using NetTopologySuite;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSService
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThCADCoreNTSService instance = new ThCADCoreNTSService();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThCADCoreNTSService() { }
        internal ThCADCoreNTSService() { }
        public static ThCADCoreNTSService Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        private IGeometryFactory geometryFactory;
        public IGeometryFactory GeometryFactory
        {
            get
            {
                if (geometryFactory == null)
                {
                    geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory();
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