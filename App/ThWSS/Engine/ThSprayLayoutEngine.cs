using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;
using ThWSS.Model;
using ThWSS.Utlis;
using Linq2Acad;
using Autodesk.AutoCAD.Geometry;

namespace ThWSS.Engine
{
    public class ThSprayLayoutEngine
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSprayLayoutEngine instance = new ThSprayLayoutEngine();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSprayLayoutEngine() { }
        internal ThSprayLayoutEngine() { }
        public static ThSprayLayoutEngine Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public ThSprayLayoutWorker Workers = new ThSprayLayoutWorker();
        public ThBeamRecognitionEngine BeamEngine = new ThBeamRecognitionEngine();
        public ThRoomRecognitionEngine RoomEngine = new ThRoomRecognitionEngine();
        public ThColumnRecognitionEngine ColumnEngine = new ThColumnRecognitionEngine();

        /// <summary>
        /// 喷淋布置引擎
        /// </summary>
        /// <param name="database"></param>
        /// <param name="polygon"></param>
        public void Layout(Database database, Polyline polygon, SprayLayoutModel layoutModel)
        {
            try
            {
                // 从房间引擎中获取房间信息
                RoomEngine.Acquire(database, polygon);
                // 遍历房间，对每个房间进行布置
                RoomEngine.Elements.Cast<ThRoom>().ForEach(o => Layout(o, layoutModel));
            }
            catch (System.Exception ex)
            {
                return;
            }
        }

        /// <summary>
        /// 喷淋布置引擎
        /// </summary>
        /// <param name="polylines"></param>
        public void Layout(List<Polyline> polylines, SprayLayoutModel layoutModel)
        {
            try
            {
                RoomEngine.Elements = new List<ThModelElement>();
                foreach (var pLine in polylines)
                {
                    var thRoom = new ThRoom();
                    thRoom.Properties = new Dictionary<string, object>() { { "room", pLine } };
                    RoomEngine.Elements.Add(thRoom);
                }
                
                // 遍历房间，对每个房间进行布置
                RoomEngine.Elements.Cast<ThRoom>().ForEach(o => Layout(o, layoutModel));
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 在一个房间内布置喷淋
        /// </summary>
        /// <param name="room"></param>
        private void Layout(ThRoom room, SprayLayoutModel layoutModel)
        {
            //var polygon = room.Properties.Values.Cast<Polyline>().ToList();
            //foreach (var item in polygon)
            //{
            //    //去掉线上多余的点
            //    var polyBounding = GeUtils.ReovePointOnLine(new List<Polyline>() { item }, new Tolerance(0.1, 0.1)).First();
            //    List<KeyValuePair<Line, double>> polyDic = new List<KeyValuePair<Line, double>>();
            //    for (int i = 0; i < polyBounding.NumberOfVertices; i++)
            //    {
            //        polyDic.Add(new KeyValuePair<Line, double>(new Line(polyBounding.GetPoint3dAt(i), polyBounding.GetPoint3dAt((i + 1) % polyBounding.NumberOfVertices)), 300));
            //    }

            //    var res = GeUtils.PolygonBuffer(polyBounding, polyDic);
            //    using (AcadDatabase acdb = AcadDatabase.Active())
            //    {
            //        acdb.ModelSpace.Add(res);
            //    }
            //}

            Workers.DoLayout(room, layoutModel);
        }
    }
}
