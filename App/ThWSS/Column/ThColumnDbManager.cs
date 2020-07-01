using System;
using TopoNode;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using ThStructure.BeamInfo.Utils;

namespace ThWSS.Column
{
    public class ThColumnDbManager : IDisposable
    {
        public Database HostDb { get; set; }
        private ObjectIdCollection Geometries { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database"></param>
        public ThColumnDbManager(Database database)
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
            // 处理图纸，获取处理后的构成梁的所有图元（曲线）
            Geometries = new ObjectIdCollection();
            var geometryLayers = ThColumnLayerManager.GeometryLayers(this.HostDb);
            List<Entity> ents = ThStructureUtils.Explode(this.HostDb);
            List<Entity> geometryEnts = ThStructureUtils.FilterCurveByLayers(ents, geometryLayers);
            Geometries = ThStructureUtils.AddToDatabase(geometryEnts);
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
