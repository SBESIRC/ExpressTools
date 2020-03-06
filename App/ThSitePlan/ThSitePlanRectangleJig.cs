using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using AcHelper;

namespace ThSitePlan
{
    public class ThSitePlanRectangleJig : DrawJig
    {
        public Point3d Corner1 { get; set; }
        public Point3d Corner2 { get; set; }


        public ThSitePlanRectangleJig(Point3d basePt)
        {
            Corner1 = basePt;
        }

        public Point3dCollection Corners
        {
            get
            {
                return new Point3dCollection(
                    new Point3d[]
                    {
                        Corner1,
                        new Point3d(Corner1.X, Corner2.Y, 0),
                        Corner2,
                        new Point3d(Corner2.X, Corner1.Y, 0)
                    });
            }
        }

        public Matrix3d UCS
        {
            get
            {
                return Active.Editor.CurrentUserCoordinateSystem;
            }
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions prOptions2 = new JigPromptPointOptions("\n第二角：")
            {
                UseBasePoint = false
            };

            PromptPointResult prResult2 = prompts.AcquirePoint(prOptions2);
            if (prResult2.Status == PromptStatus.Cancel || prResult2.Status == PromptStatus.Error)
            {
                return SamplerStatus.Cancel;
            }

            Point3d tmpPt = prResult2.Value.TransformBy(UCS.Inverse());
            if (!Corner2.IsEqualTo(tmpPt, ThSitePlanCommon.global_tolerance))
            {
                Corner2 = tmpPt;
                return SamplerStatus.OK;
            }
            else
            {
                return SamplerStatus.NoChange;
            }
        }

        protected override bool WorldDraw(WorldDraw draw)
        {
            WorldGeometry geometry = draw.Geometry;
            if (geometry != null)
            {
                geometry.PushModelTransform(UCS);
                geometry.Polygon(Corners);
                geometry.PopModelTransform();
            }

            return true;
        }
    }
}
