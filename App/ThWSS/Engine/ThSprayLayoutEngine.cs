using System.Linq;
using ThWSS.Model;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

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
        public static ThRoomRecognitionEngine RoomEngine = new ThRoomRecognitionEngine();
        public ThColumnRecognitionEngine ColumnEngine = new ThColumnRecognitionEngine();

        /// <summary>
        /// 喷淋布置引擎
        /// </summary>
        /// <param name="database"></param>
        /// <param name="polygon"></param>
        public void Layout(Database database, Polyline polygon, SprayLayoutModel layoutModel)
        {
            // 从房间引擎中获取房间信息
            RoomEngine.Acquire(database, polygon);
            // 遍历房间，对每个房间进行布置
            RoomEngine.Elements.Cast<ThRoom>().ForEach(o => Layout(o, layoutModel));
        }

        /// <summary>
        /// 喷淋布置引擎
        /// </summary>
        /// <param name="polylines"></param>
        public void Layout(List<Polyline> polylines, SprayLayoutModel layoutModel)
        {
            RoomEngine.Elements = new List<ThModelElement>();
            foreach (var pLine in polylines)
            {
                var thRoom = new ThRoom();
                thRoom.Properties = new Dictionary<string, object>() { { "room", pLine } };
                RoomEngine.Elements.Add(thRoom);
            }

            // 遍历房间，对每个房间进行布置
            RoomEngine.Elements.Where(x=>x is ThRoom).Cast<ThRoom>().ForEach(o => Layout(o, layoutModel));
        }

        /// <summary>
        /// 在一个房间内布置喷淋
        /// </summary>
        /// <param name="room"></param>
        private void Layout(ThRoom room, SprayLayoutModel layoutModel)
        {
            Workers.DoLayout(room, layoutModel);
        }
    }
}
