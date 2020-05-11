using System;
using Linq2Acad;
using System.Text;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using DotNetARX;

using Autodesk.AutoCAD.EditorInput;
using AcHelper;



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
                if (NameByFrame(frame) == name)
                {
                    return frame;
                }
            }
            return ObjectId.Null;
        }

        public string NameByFrame(ObjectId frame)
        {
            TypedValueList tvList = frame.GetXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name);
            if (tvList == null)
            {
                return "";
            }
            var tv = tvList.Where(o => o.TypeCode == (int)DxfCode.ExtendedDataBinaryChunk).First();
           return Encoding.UTF8.GetString(tv.Value as byte[]);
        }

        public void EraseItemInFrame(ObjectId FrameId , PolygonSelectionMode mode)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                PromptSelectionResult  psr = Active.Editor.SelectByPolyline(FrameId,  mode, null);
                if (psr.Status == PromptStatus.OK)
                {
                    SelectionSet Selset = psr.Value;
                    ObjectIdCollection SelObjIdCol = Selset.GetObjectIds().ToObjectIdCollection();
                    SelObjIdCol.Remove(FrameId);
                    Active.Editor.EraseCmd(SelObjIdCol);
                }
            }
        }
    }
}
