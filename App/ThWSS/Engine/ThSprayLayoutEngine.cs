using System.Linq;
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

        public List<ThSprayLayoutWorker> Workers { get; set; }
        public ThBeamRecognitionEngine BeamEngine { get; set; }
        public ThRoomRecognitionEngine RoomEngine { get; set; }
        public ThColumnRecognitionEngine ColumnEngine { get; set; }

        /// <summary>
        /// 喷淋布置引擎
        /// </summary>
        /// <param name="database"></param>
        /// <param name="polygon"></param>
        public void Layout(Database database, Polyline polygon)
        {
            // 从房间引擎中获取房间信息
            RoomEngine.Acquire(database, polygon);

            // 遍历房间，对每个房间进行布置
            RoomEngine.Elements.Cast<ThRoom>().ForEach(o => Layout(o));
        }

        /// <summary>
        /// 在一个房间内布置喷淋
        /// </summary>
        /// <param name="room"></param>
        private void Layout(ThRoom room)
        {
            foreach(var worker in Workers)
            {
                worker.DoLayout(room);
            }
        }
    }
}
