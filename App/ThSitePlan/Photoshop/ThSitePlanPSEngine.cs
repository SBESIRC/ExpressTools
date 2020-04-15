﻿using System.Collections.Generic;
using ThSitePlan.Configuration;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSEngine
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanPSEngine instance = new ThSitePlanPSEngine();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanPSEngine() { }
        internal ThSitePlanPSEngine() { }
        public static ThSitePlanPSEngine Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public List<ThSitePlanPSGenerator> Generators { get; set; }

        public void Run(string path, ThSitePlanConfigItemGroup jobs)
        {
            while (jobs.Items.Count !=0)
            {
                var obj = jobs.Items.Dequeue();
                if (obj is ThSitePlanConfigItem item)
                {
                    Run(path, item);
                }
                else if (obj is ThSitePlanConfigItemGroup group)
                {
                    Run(path, group);
                }
            }
        }

        private void Run(string path, ThSitePlanConfigItem job)
        {
            foreach (var generator in Generators)
            {
                generator.Generate(path, job);
            }
        }
    }
}
