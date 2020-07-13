using ThWSS.Model;
using ThWSS.Bussiness;
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

        /// <summary>
        /// 按房间轮廓线布置喷淋
        /// </summary>
        /// <param name="database"></param>
        /// <param name="fire"></param>
        public void Layout(Database database, Polyline floor, ObjectIdCollection frames, SprayLayoutModel layoutModel)
        {
            using (var explodeManager = new ThSprayDbExplodeManager(database))
            {
                using (var roomEngine = new ThRoomRecognitionEngine())
                {
                    roomEngine.Acquire(database, floor, frames);
                    roomEngine.Rooms.ForEach(o => DoLayout(o, floor, layoutModel));
                }
            }
        }

        /// <summary>
        /// 在一个房间内布置喷淋
        /// </summary>
        /// <param name="room"></param>
        private void DoLayout(ThRoom room, Polyline floor, SprayLayoutModel layoutModel)
        {
            SparyLayoutService service = null;
            if (layoutModel.UseBeam)
            {
                service = new SprayLayoutByBeamService();
            }
            else
            {
                service = new SprayLayoutNoBeamService();
            }
            service.CleanSpray(room);
            service.LayoutSpray(room, floor, layoutModel);
        }
    }
}
