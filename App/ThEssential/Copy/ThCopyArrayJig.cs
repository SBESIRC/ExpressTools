using AcHelper;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;

namespace ThEssential.Copy
{
    public class ThCopyArrayJig : DrawJig
    {
        public uint Parameter { get; set; }
        private Point3d BasePoint { get; set; }
        public Vector3d Displacement { get; set; }
        private List<Entity> Entities { get; set; }
        private Matrix3d UCS
        {
            get
            {
                return Active.Editor.CurrentUserCoordinateSystem;
            }
        }

        public ThCopyArrayJig(Point3d basePt)
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
                Cursor = CursorType.RubberBand
            };
            PromptPointResult prResult = prompts.AcquirePoint(prOptions);
            if (prResult.Status == PromptStatus.Cancel || prResult.Status == PromptStatus.Error)
            {
                return SamplerStatus.Cancel;
            }
            Point3d tmpPt = prResult.Value.TransformBy(UCS.Inverse());
            Displacement = tmpPt - BasePoint;
            if (!Displacement.IsZeroLength())
            {
                return SamplerStatus.OK;
            }
            else
            {
                return SamplerStatus.NoChange;
            }
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            WorldGeometry geo = draw.Geometry;
            if (geo != null)
            {
                for(uint i = 1; i < Parameter; i++)
                {
                    geo.PushModelTransform(Matrix3d.Displacement(Displacement * i));
                    foreach (Entity ent in Entities)
                    {
                        geo.Draw(ent);
                    }
                    geo.PopModelTransform();
                }
            }

            return true;
        }
    }
}
