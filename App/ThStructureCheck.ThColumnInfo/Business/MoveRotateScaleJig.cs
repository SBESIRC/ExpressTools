using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    enum JigWay
    {
        Move,
        Rotate,
        Scale,
        None
    }
    class MoveRotateScaleJig:DrawJig
    {
        private List<Autodesk.AutoCAD.DatabaseServices.Polyline> entities = new List<Autodesk.AutoCAD.DatabaseServices.Polyline>();
        private Point3d moveStartPt;
        private Point3d moveEndPt;
        private double rotateAngle;
        private double scaleFactor;

        private JigWay jigWay= JigWay.None;
        public MoveRotateScaleJig(List<Autodesk.AutoCAD.DatabaseServices.Polyline> entities, Point3d basePt, JigWay jigWay,double rotateAngle=0,double scaleFactor=1)
        {
            this.entities = entities;
            this.moveStartPt = basePt;
            this.moveEndPt = this.moveStartPt;
            this.rotateAngle = rotateAngle;
            this.scaleFactor = scaleFactor;
            this.jigWay = jigWay;
        }
        public double RotateAngle
        {
            get
            {
                return this.rotateAngle;
            }
        }
        public Matrix3d Transformation
        {
            get
            {
                return Matrix3d.Scaling(scaleFactor, moveEndPt).
                    PostMultiplyBy(Matrix3d.Rotation(rotateAngle, Vector3d.ZAxis, moveEndPt)).
                    PostMultiplyBy(Matrix3d.Displacement(moveStartPt.GetVectorTo(moveEndPt)));
            }
        }
        public void TranformEntities()
        {
            this.entities.ForEach(i => i.TransformBy(Transformation));
        }
        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (this.jigWay)
            {
                case JigWay.Move:
                    JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\n选择移动的目标点");
                    prOptions1.UserInputControls = UserInputControls.GovernedByOrthoMode | UserInputControls.GovernedByUCSDetect;
                    PromptPointResult ppr = prompts.AcquirePoint(prOptions1);
                    if(ppr.Status!=PromptStatus.OK)
                    {
                        return SamplerStatus.Cancel;
                    }
                    if(ppr.Value.DistanceTo(this.moveEndPt)<1e-4)
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        moveEndPt = ppr.Value;
                        return SamplerStatus.OK;
                    }
                case JigWay.Rotate:
                    JigPromptAngleOptions prOptions2 = new JigPromptAngleOptions("\n旋转角度");
                    prOptions2.UseBasePoint = true;
                    prOptions2.BasePoint = this.moveEndPt;
                    prOptions2.UserInputControls = UserInputControls.GovernedByOrthoMode | 
                        UserInputControls.GovernedByUCSDetect| UserInputControls.AcceptOtherInputString;
                    PromptDoubleResult pdr1 = prompts.AcquireAngle(prOptions2);
                    if (pdr1.Status != PromptStatus.OK)
                    {
                        return SamplerStatus.Cancel;
                    }
                   if (pdr1.Value.Equals(this.rotateAngle))
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        this.rotateAngle = pdr1.Value;
                        return SamplerStatus.OK;
                    }
                case JigWay.Scale:
                    JigPromptDistanceOptions prOptions3 = new JigPromptDistanceOptions("\n缩放比例");
                    prOptions3.UseBasePoint = true;
                    prOptions3.BasePoint = this.moveEndPt;
                    prOptions3.UserInputControls = UserInputControls.GovernedByOrthoMode | UserInputControls.GovernedByUCSDetect;
                    PromptDoubleResult pdr2 = prompts.AcquireDistance(prOptions3);
                    if (pdr2.Status != PromptStatus.OK)
                    {
                        return SamplerStatus.Cancel;
                    }
                    if (pdr2.Value.Equals(this.scaleFactor))
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        this.scaleFactor = pdr2.Value;
                        return SamplerStatus.OK;
                    }
            }
            return SamplerStatus.OK;
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            if(draw.Geometry != null)
            {
                draw.Geometry.PushModelTransform(Transformation);
                this.entities.ForEach(i=> draw.Geometry.Draw(i));
                draw.Geometry.PopModelTransform();
            }
            return true;
        }
    }
}
