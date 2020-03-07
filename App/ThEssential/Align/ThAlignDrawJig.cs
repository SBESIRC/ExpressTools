using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;

namespace ThEssential.Align
{
    public class ThAlignDrawJig : DrawJig
    {
        #region Fields
        private Point3d mBase;
        private Point3d mLocation;
        List<Entity> mEntities;
        #endregion
        public ThAlignDrawJig(Point3d basePt)
        {
            mBase = basePt.TransformBy(UCS);
            mEntities = new List<Entity>();
        }
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
                return Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
            }
        }

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
        #region Overrides

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
            JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\n请指定参考点：");
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
        #endregion
    }
}
