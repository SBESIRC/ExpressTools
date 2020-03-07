﻿using AcHelper;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using AcDbPolyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace ThSitePlan
{
    public class ThSitePlanFrameJig : DrawJig
    {
        private AcDbPolyline Frame { get; set; }
        private Point3d BasePoint { get; set; }
        private Matrix3d UCS
        {
            get
            {
                return Active.Editor.CurrentUserCoordinateSystem;
            }
        }

        public Vector3d Displacement { get; set; }

        public ThSitePlanFrameJig(AcDbPolyline frame)
        {
            Frame = frame;
            // 设置Jig的起点为图框的左上角
            BasePoint = Frame.GeometricExtents.MinPoint + Frame.GeometricExtents.Height() * Vector3d.YAxis;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions prOptions = new JigPromptPointOptions("\n请指定解构图集的放置区")
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
            if (!Displacement.IsZeroLength(ThSitePlanCommon.global_tolerance))
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
            WorldGeometry geometry = draw.Geometry;
            if (geometry != null)
            {
                geometry.PushModelTransform(UCS);
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        double deltaX = Frame.GeometricExtents.Width() * 6.0 / 5.0 * j;
                        double deltaY = Frame.GeometricExtents.Height() * 6.0 / 5.0 * i;
                        Vector3d delta = new Vector3d(deltaX, -deltaY, 0.0);
                        geometry.PushPositionTransform(PositionBehavior.World, (Displacement + delta).Offset());
                        geometry.Polyline(Frame, 0, 4);
                        geometry.PopModelTransform();
                    }
                }
                geometry.PopModelTransform();
            }

            return true;
        }
    }
}