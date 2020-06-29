using System;
using System.Linq;
using Dreambuild.AutoCAD;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThStructure.BeamInfo.Utils;

namespace ThWSS.Beam
{
    public class ThBeamDbManager : IDisposable
    {
        public Database HostDb { get; set; }
        private ObjectIdCollection Annotations { get; set; }
        private ObjectIdCollection Geometries { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database"></param>
        public ThBeamDbManager(Database database)
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
            Annotations = new ObjectIdCollection();
            var geometryLayers = ThBeamLayerManager.GeometryLayers(this.HostDb);
            var annotationLayers= ThBeamLayerManager.AnnotationLayers(this.HostDb);
            List<Entity> ents = ThStructureUtils.Explode(ExplodeType.All);
            List<Entity> geometryEnts = ThStructureUtils.FilterCurveByLayers(ents, geometryLayers);
            List<Entity> annoationEnts = ThStructureUtils.FilterAnnotationByLayers(ents, annotationLayers);
            foreach(var entity in geometryEnts)
            {
                ents.Remove(entity);
            }
            foreach (var entity in annoationEnts)
            {
                ents.Remove(entity);
            }
            ents.ForEach(o => o.Dispose());
            Geometries = ThStructureUtils.AddToDatabase(geometryEnts);
            Annotations = ThStructureUtils.AddToDatabase(annoationEnts);
        }

        public void PostProcess()
        {
            TopoNode.Utils.PostProcess(Geometries);
            TopoNode.Utils.PostProcess(Annotations);
        }
    }
}