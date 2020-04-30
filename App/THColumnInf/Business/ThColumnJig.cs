using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.ApplicationServices;
namespace ThColumnInfo
{
    public class ThColumnJig : DrawJig
    {
        private string keyWord = "";
        private Point3d mBase;
        private Point3d mLocation;
        private List<Autodesk.AutoCAD.DatabaseServices.Polyline> polylines = new List<Autodesk.AutoCAD.DatabaseServices.Polyline>();
        private Matrix3d currentUcs;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="polylines"></param>
        /// <param name="basePt">Ucs Point</param>
        public ThColumnJig(List<Autodesk.AutoCAD.DatabaseServices.Polyline> polylines, Point3d basePt)
        {
            this.currentUcs = ThColumnInfoUtils.GetMdiActiveDocument().Editor.CurrentUserCoordinateSystem;        
            this.polylines = polylines;
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
        public void TransformEntities(List<Autodesk.AutoCAD.DatabaseServices.Entity> ents)
        {
            Matrix3d mat = Matrix3d.Displacement(mBase.GetVectorTo(mLocation));
            foreach (Entity ent in ents)
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
            foreach (Entity ent in this.polylines)
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
                for (int i = 0; i < this.polylines.Count; i++)
                {
                    geo.Draw(this.polylines[i]);
                }
                geo.PopModelTransform();
            }
            return true;
        }
    }

    public class DrawJigger : DrawJig
    {
        #region Fields
        private Point3d mBase;
        private Point3d mLocation;
        List<Entity> mEntities;
        private Document doc;
        #endregion
        #region Constructors
        public DrawJigger(Point3d basePt)
        {
            mBase = basePt.TransformBy(UCS);
            mEntities = new List<Entity>();
            doc = ThColumnInfoUtils.GetMdiActiveDocument();
        }
        #endregion
        #region Properties
        public Point3d Base
        {
            get { return mLocation; }
            set { mLocation = value; }
        }
        public Point3d Location
        {
            get { return mLocation; }
            set { mLocation = value; }
        }
        public Matrix3d UCS
        {
            get
            {
                return this.doc.Editor.CurrentUserCoordinateSystem;
            }
        }
        public string KeyWord
        { get; set; }
    
        #endregion
        #region Methods
        public void AddEntity(Entity ent)
        {
            mEntities.Add(ent);
        }
        public void TransformEntities()
        {
            Matrix3d mat = Matrix3d.Displacement(mBase.GetVectorTo(mLocation));
            foreach (Entity ent in mEntities)
            {
                ent.TransformBy(mat);
            }
        }
        #endregion
        protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
        {
            Matrix3d mat = Matrix3d.Displacement(mBase.GetVectorTo(mLocation));
            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                geo.PushModelTransform(mat);
                foreach (Entity ent in mEntities)
                {
                    geo.Draw(ent);
                }
                geo.PopModelTransform();
            }
            return true;
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\nNew location:");
            prOptions1.UseBasePoint = false;
            PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
            if (prResult1.Status == PromptStatus.Cancel || prResult1.Status == PromptStatus.Error)
                return SamplerStatus.Cancel;
            if (!mLocation.IsEqualTo(prResult1.Value, new Tolerance(10e-10, 10e-10)))
            {
                mLocation = prResult1.Value;
                return SamplerStatus.OK;
            }
            else
                return SamplerStatus.NoChange;
        }
    }
}
