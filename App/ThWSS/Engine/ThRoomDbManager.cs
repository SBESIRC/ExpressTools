using System;
using TopoNode;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Engine
{
    public class ThRoomDbManager : IDisposable
    {
        public Extents3d Extents { get; set; }
        public Database HostDb { get; private set; }
        private ObjectIdCollection Geometries { get; set; }
        public ThRoomLayerManager LayerManger { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ThRoomDbManager(Database database, Extents3d extents)
        {
            HostDb = database;
            Extents = extents;
            LayerManger = new ThRoomLayerManager();
            LayerManger.Initialize();
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
            Geometries = Utils.PreProcess2(LayerManger.ValidLayers());
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
