using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using Autodesk.AutoCAD.Runtime;

namespace ThSitePlan.Engine
{
    public class ThSitePlanHatchGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        private Dictionary<string, ThSitePlanWorker> Workers { get; set; }
        public ThSitePlanHatchGenerator()
        {
            Workers = new Dictionary<string, ThSitePlanWorker>()
            {
                {"树木-景观树-树木色块", new ThSitePlanCADWorker(new string[] {
                    RXClass.GetClass(typeof(Circle)).DxfName,
                    RXClass.GetClass(typeof(Spline)).DxfName,
                })},
                {"树木-行道树-树木色块", new ThSitePlanCADWorker(new string[] {
                    RXClass.GetClass(typeof(Circle)).DxfName,
                    RXClass.GetClass(typeof(Spline)).DxfName,
                })},
                {"建筑物-场地外建筑-建筑色块", new ThSitePlanRegionWorker(new string[] {
                    RXClass.GetClass(typeof(Region)).DxfName,
                })},
                {"建筑物-场地内建筑-建筑色块", new ThSitePlanRegionWorker(new string[] {
                    RXClass.GetClass(typeof(Region)).DxfName,
                })},
                {"场地-停车场地-场地色块", new ThSitePlanCADWorker(new string[]{
                    RXClass.GetClass(typeof(Polyline)).DxfName,
                })},
                {"场地-消防登高场地-场地色块", new ThSitePlanRegionWorker(new string[] {
                    RXClass.GetClass(typeof(Region)).DxfName,
                })},
                {"场地-活动场地-场地色块", new ThSitePlanRegionWorker(new string[] {
                    RXClass.GetClass(typeof(Region)).DxfName,
                })},
            };
        }

        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            var options = new ThSitePlanOptions()
            {
                Options = new Dictionary<string, object>()
                {
                    {"Frame", Frame.Item1},
                    {"Offset",  Frame.Item2},
                    {"OriginFrame", OriginFrame},
                }
            };

            var key = (string)configItem.Properties["Name"];
            if (Workers.ContainsKey(key))
            {
                Workers[key].DoProcess(database, configItem, options);
            }
            return true;
        }
    }
}
