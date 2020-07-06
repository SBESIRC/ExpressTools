using Database=Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System.Collections.Generic;
using ThStructureCheck.Common;

namespace ThStructureCheck.YJK.Service
{
    public class ThBeamJig : DrawJig
    {
        private string keyWord = "";
        private Point3d mBase;
        private Point3d mLocation;
        private List<Autodesk.AutoCAD.DatabaseServices.Entity> entities = new List<Autodesk.AutoCAD.DatabaseServices.Entity>();
        private Matrix3d currentUcs;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="polylines"></param>
        /// <param name="basePt">Ucs Point</param>
        public ThBeamJig(List<Autodesk.AutoCAD.DatabaseServices.Entity> entities, Point3d basePt)
        {
            this.currentUcs = CadTool.GetMdiActiveDocument().Editor.CurrentUserCoordinateSystem;
            this.entities = entities;
            this.keyWord = "";
            UpdateMbase(basePt);
        }
        public void UpdateMbase(Point3d basePt)
        {
            mBase = basePt.TransformBy(currentUcs);
        }
        /// <summary>
        /// Ucs Point
        /// </summary>
        public Point3d Location
        {
            get { return mLocation; }
        }
        public string KeyWord
        {
            get { return keyWord; }
        }
        public void TransformEntities(List<Database.Entity> ents)
        {
            Matrix3d mat = Matrix3d.Displacement(mBase.GetVectorTo(mLocation));
            foreach (Database.Entity ent in ents)
            {
                ent.TransformBy(mat);
            }
        }
        /// <summary>
        /// 转到最后Jig的状态
        /// </summary>
        public void TransformEntities()
        {
            Matrix3d mat = Matrix3d.Displacement(mBase.GetVectorTo(mLocation));
            foreach (Database.Entity ent in this.entities)
            {
                ent.TransformBy(mat);
            }
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\n选择安装的基点[重置基点(R)/退出(E)]:");
            prOptions1.UseBasePoint = false;
            prOptions1.Keywords.Add("R");
            prOptions1.Keywords.Add("E");
            PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
            if (prResult1.Status == PromptStatus.Cancel || prResult1.Status == PromptStatus.Error)
                return SamplerStatus.Cancel;
            mLocation = prResult1.Value;
            if (prResult1.Status == PromptStatus.Cancel)
            {
                return SamplerStatus.Cancel;
            }
            //拖拽时输入关键字
            if (prResult1.Status == PromptStatus.Keyword)
            {
                this.keyWord = prResult1.StringResult;
                return SamplerStatus.Cancel;
            }
            if (mBase.Equals(mLocation))  //Use better comparison method if necessary.
            {
                return SamplerStatus.NoChange;
            }
            return SamplerStatus.OK;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            Matrix3d mt = Matrix3d.Displacement(mBase.GetVectorTo(mLocation));
            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                geo.PushModelTransform(mt);
                for (int i = 0; i < this.entities.Count; i++)
                {
                    geo.Draw(this.entities[i]);
                }
                geo.PopModelTransform();
            }
            return true;
        }
    }
}
