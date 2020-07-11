﻿using System;
using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThStructure.BeamInfo.Utils;
using ThWSS.Beam;
using ThWSS.Column;

namespace ThWSS
{
    public class ThSprayDbExplodeManager : IDisposable
    {
        private Database HostDb { get; set; }
        private List<Entity> Entities { get; set; }
        private ObjectIdCollection BeamCurves { get; set; }
        private ObjectIdCollection BeamAnnotations { get; set; }
        private ObjectIdCollection ColumnCurves { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database"></param>
        public ThSprayDbExplodeManager(Database database)
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
        /// 预处理
        /// </summary>
        private void PreProcess()
        {
            Explode();
            BeamCurves = AddBeamCurvesToDatabase();
            ColumnCurves = AddColumnCurvesToDatabase();
            BeamAnnotations = AddBeamAnnotationsToDatabase();
        }

        /// <summary>
        /// 后处理
        /// </summary>
        public void PostProcess()
        {
            EraseObjs(BeamCurves);
            EraseObjs(ColumnCurves);
            EraseObjs(BeamAnnotations);
        }

        private void EraseObjs(ObjectIdCollection objs)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(this.HostDb))
            {
                foreach (ObjectId obj in objs)
                {
                    if (!obj.IsErased)
                    {
                        // 删除数据清理过程中临时“炸”到当前图纸中的对象
                        // 对于“炸”到锁定图层上的对象，我们仍然需要将他们删除
                        acadDatabase.Element<Entity>(obj, true, true).Erase();
                    }
                }
            }

            // A collection of object ids whose memory is to be reclaimed by deleting their objects.
            // All object ids in the collection must correspond to erased objects, which must be entirely closed
            var ids = objs.Cast<ObjectId>().Where(o => o.IsErased).ToArray();
            HostDb.ReclaimMemoryFromErasedObjects(new ObjectIdCollection(ids));
        }

        private void Explode()
        {
            Entities = ThStructureUtils.Explode(HostDb);
        }

        private ObjectIdCollection AddBeamCurvesToDatabase()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(this.HostDb))
            {
                var geometryLayers = ThBeamLayerManager.GeometryLayers(this.HostDb);
                var entities = ThStructureUtils.FilterCurveByLayers(Entities, geometryLayers);
                foreach(var entity in entities)
                {
                    Entities.Remove(entity);
                }
                var objs = new ObjectIdCollection();
                foreach (var entity in entities)
                {
                    objs.Add(acadDatabase.ModelSpace.Add(entity));
                }
                return objs;
            }
        }

        private ObjectIdCollection AddBeamAnnotationsToDatabase()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(this.HostDb))
            {
                var annotationLayers = ThBeamLayerManager.AnnotationLayers(this.HostDb);
                var entities = ThStructureUtils.FilterAnnotationByLayers(Entities, annotationLayers);
                foreach (var entity in entities)
                {
                    Entities.Remove(entity);
                }
                var objs = new ObjectIdCollection();
                foreach (var entity in entities)
                {
                    objs.Add(acadDatabase.ModelSpace.Add(entity));
                }
                return objs;
            }
        }

        private ObjectIdCollection AddColumnCurvesToDatabase()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(this.HostDb))
            {
                var geometryLayers = ThColumnLayerManager.GeometryLayers(this.HostDb);
                var entities = ThStructureUtils.FilterCurveByLayers(Entities, geometryLayers);
                foreach (var entity in entities)
                {
                    Entities.Remove(entity);
                }
                var objs = new ObjectIdCollection();
                foreach (var entity in entities)
                {
                    objs.Add(acadDatabase.ModelSpace.Add(entity));
                }
                return objs;
            }
        }
    }
}
