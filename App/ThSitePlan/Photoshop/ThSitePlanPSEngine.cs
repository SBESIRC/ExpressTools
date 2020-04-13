using System.Collections.Generic;
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
            foreach (var item in jobs.Items)
            {
                Run(path, item);
            }
            foreach (var group in jobs.Groups)
            {
                Run(path, group);
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
