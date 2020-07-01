using System;
using TopoNode;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Engine
{
    public class ThRoomDbManager : IDisposable
    {
        public Database HostDb { get; private set; }
        private ObjectIdCollection Geometries { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ThRoomDbManager(Database database)
        {
            HostDb = database;
            PreProcess();
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        public void Dispose()
        {
            PostProcess();
        }

        /// <summary>
        /// 对图纸预处理
        /// </summary>
        /// <returns></returns>
        public void PreProcess()
        {
            ThRoomLayerManager.Instance.Initialize();
            Geometries = Utils.PreProcess2(ThRoomLayerManager.Instance.ValidLayers());
        }

        /// <summary>
        /// 对图纸后处理
        /// </summary>
        public void PostProcess()
        {
            Utils.PostProcess(Geometries);
        }
    }
}
