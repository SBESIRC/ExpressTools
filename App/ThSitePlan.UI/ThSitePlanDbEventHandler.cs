using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThSitePlan.UI
{
    public class ThSitePlanDbEventHandler
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThSitePlanDbEventHandler instance = new ThSitePlanDbEventHandler();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThSitePlanDbEventHandler() { }
        internal ThSitePlanDbEventHandler() { }
        public static ThSitePlanDbEventHandler Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        /// <summary>
        /// 需要更新的图层
        /// </summary>
        private readonly HashSet<string> Contents = new HashSet<string>();
        public HashSet<string> UpdatedContents
        {
            get
            {
                return Contents;
            }
        }

        public void Clear()
        {
            Contents.Clear();
        }

        public void SubscribeToDb(Database db)
        {
            Contents.Clear();
            db.ObjectErased += DbEvent_ObjectErased_Handler;
            db.ObjectAppended += DbEvent_ObjectAppended_Handler;
            db.ObjectModified += DbEvent_ObjectModified_Handler;
        }

        public void UnsubscribeFromDb(Database db)
        {
            Contents.Clear();
            db.ObjectErased -= DbEvent_ObjectErased_Handler;
            db.ObjectAppended -= DbEvent_ObjectAppended_Handler;
            db.ObjectModified -= DbEvent_ObjectModified_Handler;
        }

        public void DbEvent_ObjectAppended_Handler(object sender, ObjectEventArgs e)
        {
            if (e.DBObject is Entity entity)
            {
                Contents.Add(entity.Layer);
            }
        }

        public void DbEvent_ObjectErased_Handler(object sender, ObjectErasedEventArgs e)
        {
            if (e.DBObject is Entity entity)
            {
                Contents.Add(entity.Layer);
            }
        }

        public void DbEvent_ObjectModified_Handler(object sender, ObjectEventArgs e)
        {
            if (e.DBObject is Entity entity)
            {
                Contents.Add(entity.Layer);
            }
        }
    }
}
