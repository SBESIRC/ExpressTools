using System;
using Linq2Acad;
using System.Text;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using DotNetARX;

namespace ThSitePlan.Engine
{
    public class ThSitePlanDbEngine
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanDbEngine instance = new ThSitePlanDbEngine();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanDbEngine() { }
        internal ThSitePlanDbEngine() { }
        public static ThSitePlanDbEngine Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public ObjectIdCollection Frames { get; set; }

        public void Initialize(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                Frames = new ObjectIdCollection();
                acadDatabase.ModelSpace
                    .OfType<Polyline>()
                    .Where(o => o.Layer == ThSitePlanCommon.LAYER_FRAME &&
                    o.ObjectId.GetXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name) != null)
                    .ForEachDbObject(o => Frames.Add(o.ObjectId));
            }
        }

        public ObjectId FrameByName(string name)
        {
            foreach (ObjectId frame in Frames)
            {
                TypedValueList tvList = frame.GetXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name);
                var tv = tvList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataBinaryChunk).First();
                string frameName = Encoding.UTF8.GetString(tv.Value as byte[]);
                if (frameName == name)
                {
                    return frame;
                }
            }
            return ObjectId.Null;
        }
    }
}
