using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ThSitePlan.Engine
{
    public class ThSitePlanBuildingAnnotations : IDisposable
    {
        public List<string> Annotations { get; set; }

        public ThSitePlanBuildingAnnotations(string[] annotations)
        {
            Annotations = new List<string>(annotations);
        }

        public void Dispose()
        {
            //
        }

        public UInt32 Floor()
        {
            UInt32 floor = 0;
            foreach(var annotation in Annotations)
            {
                Match match = Regex.Match(annotation, @"^([0-9]+)[Ff]$");
                if (match.Success)
                {
                    floor = Math.Max(floor, UInt32.Parse(match.Groups[1].Value));
                }
            }
            foreach(var annotation in Annotations)
            {
                Match match = Regex.Match(annotation, @"^([0-9]+)\+([0-9]+)[Ff]$");
                if (match.Success)
                {
                    floor = Math.Max(floor, UInt32.Parse(match.Groups[1].Value) + UInt32.Parse(match.Groups[2].Value));
                }
            }
            return floor;
        }
    }
}
