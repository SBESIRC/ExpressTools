using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThXClip
{    
    public class EntPropertyInfo
    {
        public string Layer { get; set; }
        public int ColorIndex { get; set;}
        public LineWeight Lw { get; set; }
    }
    public class ThXClipUtils
    {
        public static void ChangeEntityProperty(Entity ent, EntPropertyInfo entPropertyInf)
        {
            ent.Layer = entPropertyInf.Layer;
            ent.ColorIndex = entPropertyInf.ColorIndex;
            ent.LineWeight = entPropertyInf.Lw;
        }
        public static string ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            //ts3.ToString("g").Substring(0, 8) 0:00:07.1
            //ts3.ToString("c").Substring(0, 8) 00:00:07
            //ts3.ToString("G").Substring(0, 8) 0:00:00
            return ts3.ToString("G").Substring(0, 8);
        }
    }
}
