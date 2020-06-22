using System;
using TopoNode;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using ThStructure.BeamInfo.Utils;
using Autodesk.AutoCAD.Interop.Common;

namespace ThWSS.Beam
{
    public class ThBeamDbManager : IDisposable
    {
        public Database HostDb { get; set; }
        private DBObjectCollection Annotations { get; set; }
        private DBObjectCollection Geometries { get; set; }
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
            Geometries = new DBObjectCollection();
            Annotations = new DBObjectCollection();
            var geometryLayers = ThBeamLayerManager.GeometryLayers(this.HostDb);
            var annotationLayers= ThBeamLayerManager.AnnotationLayers(this.HostDb);
            List<Entity> ents = ThStructureUtils.Explode(ExplodeType.All);
            List<Entity> geometryEnts = ThStructureUtils.FilterCurveByLayers(ents, geometryLayers);
            geometryEnts.ForEach(i => Geometries.Add(i));
            List<Entity> annoationEnts = ThStructureUtils.FilterAnnotationByLayers(ents, annotationLayers);
            annoationEnts.ForEach(i => Annotations.Add(i));
            ThStructureUtils.AddToDatabase(geometryEnts);
            ThStructureUtils.AddToDatabase(annoationEnts);
        }

        public void PostProcess()
        {
            Utils.PostProcess(Geometries.Cast<Entity>().ToList());
            Utils.PostProcess(Annotations.Cast<Entity>().ToList());
        }
    }
}