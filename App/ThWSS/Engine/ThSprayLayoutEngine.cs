using ThWSS.Model;
using System.Linq;
using ThWSS.Bussiness;
using Dreambuild.AutoCAD;
using Autodesk.AutoCAD.DatabaseServices;

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

        public ThSprayLayoutWorker Worker = new ThSprayLayoutWorker();
        public ThRoomRecognitionEngine RoomEngine = new ThRoomRecognitionEngine();

        /// <summary>
        /// 按防火分区布置喷淋
        /// </summary>
        /// <param name="database"></param>
        /// <param name="fire"></param>
        public void Layout(Database database, Polyline floor, ObjectId fire, SprayLayoutModel layoutModel)
        {
            RoomEngine.Acquire(database, floor, fire);
            RoomEngine.Elements.Where(x => x is ThRoom).Cast<ThRoom>().ForEach(o => DoLayout(o, floor, layoutModel));
        }

        /// <summary>
        /// 按房间轮廓线布置喷淋
        /// </summary>
        /// <param name="database"></param>
        /// <param name="fire"></param>
        public void Layout(Database database, Polyline floor, ObjectIdCollection frames, SprayLayoutModel layoutModel)
        {
            RoomEngine.Acquire(database, floor, frames);
            RoomEngine.Elements.Where(x => x is ThRoom).Cast<ThRoom>().ForEach(o => DoLayout(o, floor, layoutModel));
        }

        /// <summary>
        ///按自定义区域布置喷淋
        /// </summary>
        /// <param name="polylines"></param>
        public void Layout(Database database, Polyline floor, DBObjectCollection frames, SprayLayoutModel layoutModel)
        {
            RoomEngine.Acquire(database, floor, frames);
            RoomEngine.Elements.Where(x => x is ThRoom).Cast<ThRoom>().ForEach(o => DoLayout(o, floor, layoutModel));
        }

        /// <summary>
        /// 在一个房间内布置喷淋
        /// </summary>
        /// <param name="room"></param>
        private void DoLayout(ThRoom room, Polyline floor, SprayLayoutModel layoutModel)
        {
            Worker.DoLayout(room, floor, layoutModel);
        }
    }
}
