using AcHelper;
using DotNetARX;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThSitePlan
{
    public static class ThSitePlanFrameUtils
    {
        /// <summary>
        /// Jig生成图框
        /// </summary>
        /// <returns></returns>
        public static Polyline CreateFrame()
        {
            PromptPointOptions prOpt = new PromptPointOptions("\n第一角：");
            PromptPointResult ppr = Active.Editor.GetPoint(prOpt);
            if (ppr.Status != PromptStatus.OK)
            {
                return null;
            }

            var jigger = new ThSitePlanRectangleJig(ppr.Value);
            if (Active.Editor.Drag(jigger).Status != PromptStatus.OK)
            {
                return null;
            }

            var pline = new Polyline()
            {
                Closed = true
            };
            pline.CreatePolyline(jigger.Corners);
            return pline;
        }

        public static Vector3d PickFrameOffset(Polyline frame)
        {
            var jig = new ThSitePlanFrameJig(frame);
            if (Active.Editor.Drag(jig).Status != PromptStatus.OK)
            {
                return new Vector3d(0,0,0);
            }

            return jig.Displacement;
        }
    }
}
