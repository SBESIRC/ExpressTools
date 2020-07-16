using AcHelper;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;

namespace ThEssential.Copy
{
    public class ThCopyDistanceJig : DrawJig
    {
        private Point3d BasePoint { get; set; }
        public Vector3d Displacement { get; set; }
        public ThCopyArrayOptions Options { get; set; }
        private List<Entity> Entities { get; set; }
        private Matrix3d UCS
        {
            get
            {
                return Active.Editor.CurrentUserCoordinateSystem;
            }
        }

        public ThCopyDistanceJig(Point3d basePt)
        {
            BasePoint = basePt;
            Entities = new List<Entity>();
        }

        public void AddEntity(Entity ent)
        {
            Entities.Add(ent);
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions prOptions = new JigPromptPointOptions("\n指定第二个点")
            {
                UseBasePoint = true,
                BasePoint = BasePoint,
                Cursor = CursorType.RubberBand,
                UserInputControls  = UserInputControls.AcceptOtherInputString,
            };
            if ((Options & ThCopyArrayOptions.Array) == ThCopyArrayOptions.Array)
            {
                prOptions.Keywords.Add("Array", "Array", "阵列(A)");
                prOptions.Keywords.Default = "Array";
            }
            if ((Options & ThCopyArrayOptions.Copy) == ThCopyArrayOptions.Copy)
            {
                prOptions.Keywords.Add("Copy", "Copy", "重复(C)");
            }
            if ((Options & ThCopyArrayOptions.Divide) == ThCopyArrayOptions.Divide)
            {
                prOptions.Keywords.Add("Divide", "Divide", "等分(D)");
            }
            PromptPointResult prResult = prompts.AcquirePoint(prOptions);
            if (prResult.Status == PromptStatus.OK)
            {
                Displacement = prResult.Value - BasePoint;
                if (!Displacement.IsZeroLength())
                {
                    return SamplerStatus.OK;
                }
                else
                {
                    return SamplerStatus.NoChange;
                }
            }
            else if (prResult.Status == PromptStatus.Keyword)
            {
                return SamplerStatus.Cancel;
            }
            else
            {
                return SamplerStatus.Cancel;
            }
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                geo.PushModelTransform(UCS);
                var offset = new Point3d(Displacement.ToArray());
                geo.PushPositionTransform(PositionBehavior.World, offset);
                foreach (Entity ent in Entities)
                {
                    geo.Draw(ent);
                }
                geo.PopModelTransform();
                geo.PopModelTransform();
            }

            return true;
        }
    }
}