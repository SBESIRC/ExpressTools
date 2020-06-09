using System;
using TopoNode;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Beam
{
    public class ThBeamDbManager : IDisposable
    {
        public Database HostDb { get; set; }
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
            // 首先获取构成梁的图元（曲线）所在的图层
            var layers = ThBeamLayerManager.GeometryLayers(HostDb);
            // 处理图纸，获取处理后的构成梁的所有图元（曲线）
            Geometries = new DBObjectCollection();
            foreach (var curve in Utils.PreProcess2(layers))
            {
                Geometries.Add(curve);
            }
        }

        public void PostProcess()
        {
            Utils.PostProcess(Geometries.Cast<Entity>().ToList());
        }
    }
}