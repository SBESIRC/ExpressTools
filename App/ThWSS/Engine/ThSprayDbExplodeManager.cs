using System;
using Linq2Acad;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThStructure.BeamInfo.Utils;
using ThWSS.Beam;
using ThWSS.Column;
using ThWSS.Utlis;

namespace ThWSS
{
    public class ThSprayDbExplodeManager : IDisposable
    {
        private Database HostDb { get; set; }
        private List<Entity> Entities { get; set; }
        private ObjectIdCollection BeamCurves { get; set; }
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
            DisposeEntities();
        }

        /// <summary>
        /// 预处理
        /// </summary>
        private void PreProcess()
        {
            Explode();
            BeamCurves = AddBeamCurvesToDatabase();
            ColumnCurves = AddColumnCurvesToDatabase();
        }

        /// <summary>
        /// 后处理
        /// </summary>
        public void PostProcess()
        {
            HostDb.EraseObjs(BeamCurves);
            HostDb.EraseObjs(ColumnCurves);
            foreach (Entity entity in Entities)
            {
                entity.Dispose();
            }
        }

        private void Explode()
        {
            Entities = ThStructureUtils.Explode(HostDb);
        }

        /// <summary>
        /// 释放不关心的图元对象
        /// </summary>
        private void DisposeEntities()
        {
            foreach(Entity entity in Entities)
            {
                entity.Dispose();
            }
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
                    // 在AutoCAD2012下炸出的图元添加到当前图纸时会报告eWrongDatabase错误
                    // 在未排除出原因之前，采用一个“变通方法”，把炸出的图元复制后将“复制”添加到当前图纸中
                    objs.Add(acadDatabase.ModelSpace.Add(entity.Clone() as Entity));
                }
                foreach (var entity in entities)
                {
                    entity.Dispose();
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
                    // 在AutoCAD2012下炸出的图元添加到当前图纸时会报告eWrongDatabase错误
                    // 在未排除出原因之前，采用一个“变通方法”，把炸出的图元复制后将“复制”添加到当前图纸中
                    objs.Add(acadDatabase.ModelSpace.Add(entity.Clone() as Entity));
                }
                foreach (var entity in entities)
                {
                    entity.Dispose();
                }
                return objs;
            }
        }
    }
}
