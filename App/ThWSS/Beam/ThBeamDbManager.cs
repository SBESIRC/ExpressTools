using System;
using TopoNode;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using ThStructure.BeamInfo.Utils;

namespace ThWSS.Beam
{
    public class ThBeamDbManager : IDisposable
    {
        public Database HostDb { get; set; }
        private DBObjectCollection Geometries { get; set; }
        private List<string> beamColumnLayers = new List<string>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="database"></param>
        public ThBeamDbManager(Database database)
        {
            beamColumnLayers.Add("S_COLU");
            beamColumnLayers.Add("S_BEAM");
            beamColumnLayers.Add("S_BEAM_SECD");
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
            List<Entity> ents = ThStructureUtils.ExplodeAllXRef();
            ents=ThStructureUtils.FilterByLayers(ents, this.beamColumnLayers);
            List<Curve> curves = ents.Where(i => i is Curve).Select(i => i as Curve).ToList();
            curves.ForEach(i => Geometries.Add(i));
            ThStructureUtils.AddToDatabase(curves.Cast<Entity>().ToList());
        }

        public void PostProcess()
        {
            Utils.PostProcess(Geometries.Cast<Entity>().ToList());
        }
    }
}