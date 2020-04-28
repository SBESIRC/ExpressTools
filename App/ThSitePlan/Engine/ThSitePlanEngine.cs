using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan
{
    public class ThSitePlanEngine
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanEngine instance = new ThSitePlanEngine();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanEngine() { }
        internal ThSitePlanEngine() { }
        public static ThSitePlanEngine Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public ObjectId OriginFrame { get; set; }
        public List<ThSitePlanGenerator> Generators { get; set; }
        public Queue<Tuple<ObjectId, Vector3d>> Containers { get; set; }

        public void Run(Database database, ThSitePlanConfigItemGroup jobs)
        {
            if (!jobs.IsEnabled)
            {
                return;
            }

            while (jobs.Items.Count != 0)
            {
                var obj = jobs.Items.Dequeue();
                if (obj is ThSitePlanConfigItem item)
                {
                    Run(database, item);
                }
                else if (obj is ThSitePlanConfigItemGroup group)
                {
                    Run(database, group);
                }
            }
        }

        private void Run(Database database, ThSitePlanConfigItem job)
        {
            if (!job.IsEnabled)
            {
                return;
            }

            if (Containers.Count == 0)
            {
                return;
            }

            var frame = Containers.Dequeue();
            foreach (var generator in Generators)
            {
                generator.Frame = frame;
                generator.OriginFrame = OriginFrame;
                generator.Generate(database, job);
            }
        }
    }
}