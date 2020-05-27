using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.LayerState
{
    /// <summary>
    /// 预定义图层状态
    /// </summary>
    public enum State
    {
        /// <summary>
        /// 水平通风
        /// </summary>
        VENTILATE = 1,

        /// <summary>
        /// 水管平面
        /// </summary>
        PIPE = 2,

        /// <summary>
        /// 消防平面
        /// </summary>
        EXTINGUISHMENT = 4,

        /// <summary>
        /// 全显
        /// </summary>
        ALL = VENTILATE | PIPE | EXTINGUISHMENT,
    }

    public class ThLayerStateManager : IDisposable
    {
        private Database HostDatabase { get; set; }
        private LayerStateManager Manager { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database"></param>
        public ThLayerStateManager(Database database)
        {
            HostDatabase = database;
            Manager = database.LayerStateManager;
        }

        /// <summary>
        /// Dispose()
        /// </summary>
        public void Dispose()
        {
            //
        }

        public bool HasLayerState(string name)
        {
            return Manager.HasLayerState(name);
        }

        public void RestoreLayerState(string name)
        {
            Manager.RestoreLayerState(name, ObjectId.Null, 0, LayerStateMasks.On | LayerStateMasks.Frozen | LayerStateMasks.Locked);
        }

        public void ImportLayerStateFromDb(string name, Database database)
        {
            Manager.ImportLayerStateFromDb(name, database);
        }
    }
}
