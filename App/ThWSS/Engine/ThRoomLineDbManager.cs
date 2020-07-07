using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThStructure.BeamInfo.Utils;

namespace ThWSS.Engine
{
    public class ThRoomLineDbManager : IDisposable
    {
        public Database HostDb { get; set; }
        public ObjectIdCollection Geometries { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ThRoomLineDbManager(Database database)
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
            Geometries = new ObjectIdCollection();
            var geometryLayers = ThRoomLineLayerManager.GeometryLayers(this.HostDb);
            List<Entity> ents = ThStructureUtils.Explode(this.HostDb);
            List<Entity> geometryEnts = ThStructureUtils.FilterCurveByLayers(ents, geometryLayers);
            Geometries = ThStructureUtils.AddToDatabase(geometryEnts);
        }

        /// <summary>
        /// 对图纸后处理
        /// </summary>
        public void PostProcess()
        {
            TopoNode.Utils.PostProcess(Geometries);
        }
    }
}
