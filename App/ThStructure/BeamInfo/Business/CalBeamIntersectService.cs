using Linq2Acad;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using ThStructure.BeamInfo.Model;
using ThStructure.BeamInfo.Utils;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThStructure.BeamInfo.Business
{
    public class CalBeamIntersectService
    {
        private readonly double searchBeamPolylineDis = 20.0;
        private readonly string colLayerName = "__覆盖_S30-colu_TEN25CUZ_设计区$0$S_COLU";//"__覆盖_S30-colu_TEN25CUZ_设计区$0$S_COLU";
        private readonly string wallLayerName = "__覆盖_S30-colu_TEN25CUZ_设计区$0$S_WALL";
        private readonly string beamLayerName = "__覆盖_S20-平面_TEN25CUZ_设计区$0$S_BEAM";//"__覆盖_S20-平面_TEN25CUZ_设计区$0$S_BEAM";
        private Document _doc;

        public CalBeamIntersectService(Document document)
        {
            _doc = document;
        }

        /// <summary>
        /// 计算梁的搭接信息
        /// </summary>
        /// <param name="allBeam"></param>
        public void CalBeamIntersectInfo(List<Beam> allBeam, AcadDatabase acdb)
        {
            SelectionFilter columnFilter = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, "Polyline,LWPOLYLINE"),
                                                                                      new TypedValue((int)DxfCode.LayerName, colLayerName)});
            SelectionFilter wallFilter = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, "Polyline,LWPOLYLINE"),
                                                                                      new TypedValue((int)DxfCode.LayerName, wallLayerName)});
            SelectionFilter beamFilter = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, "ARC,LINE,Polyline,LWPOLYLINE"),
                                                                                      new TypedValue((int)DxfCode.LayerName, beamLayerName)});
            foreach (var beam in allBeam)
            {
                CalIntersectLine(beam, columnFilter, acdb, out List<Curve> columnCurve);
                SupplementBeamIntersInfo(allBeam, beam, columnCurve, IntersectType.Column, 10);

                CalIntersectLine(beam, wallFilter, acdb, out List<Curve> wallCurve);
                SupplementBeamIntersInfo(allBeam, beam, wallCurve, IntersectType.Wall, 10);

                CalIntersectLine(beam, beamFilter, acdb, out List<Curve> beamCurve);
                beamCurve = beamCurve.Except(new List<Curve>() { beam.UpBeamLine, beam.DownBeamLine }).ToList();
                SupplementBeamIntersInfo(allBeam, beam, beamCurve, IntersectType.Beam, 10);
            }
        }

        /// <summary>
        /// 填充梁的搭接信息
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="sCurve"></param>
        /// <param name="eCurve"></param>
        /// <param name="intersectType"></param>
        /// <param name="offset"></param>
        private void SupplementBeamIntersInfo(List<Beam> allBeams, Beam beam, List<Curve> sCurve, IntersectType intersectType, double offset = 0)
        {
            BeamIntersectInfo intersectSInfo = new BeamIntersectInfo();
            foreach (var curve in sCurve)
            {
                if (intersectType != IntersectType.Beam && beam.StartIntersect != null)
                {
                    break;
                }
                bool sRes = CalIntersect(allBeams, beam, curve, true, offset);
                if (sRes)
                {
                    intersectSInfo.EntityType = intersectType;
                    intersectSInfo.EntityCurve.Add(curve);
                    beam.StartIntersect = intersectSInfo;
                    continue;
                }
            }

            BeamIntersectInfo intersectEInfo = new BeamIntersectInfo();
            foreach (var curve in sCurve)
            {
                if (intersectType != IntersectType.Beam && beam.EndIntersect != null)
                {
                    break;
                }
                bool eRes = CalIntersect(allBeams, beam, curve, false, offset);
                if (eRes)
                {
                    intersectEInfo.EntityType = intersectType;
                    intersectEInfo.EntityCurve.Add(curve);
                    beam.EndIntersect = intersectEInfo;
                }
            }
        }

        /// <summary>
        /// 判断是否有碰撞
        /// </summary>
        /// <param name="allBeams"></param>
        /// <param name="beam"></param>
        /// <param name="curve"></param>
        /// <param name="isStart"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private bool CalIntersect(List<Beam> allBeams, Beam beam, Curve curve, bool isStart, double offset)
        {
            bool res = false;
            Polyline polyline;
            if (isStart)
            {
                polyline = beam.BeamSPointSolid;
            }
            else
            {
                polyline = beam.BeamEPointSolid;
            }
            if (curve is Polyline)
            {
                res = CalBeamIntersect.JudgeBeamIntersect(polyline, curve as Polyline, offset);
            }
            else
            {
                var beamSolid = allBeams.Where(y => (beam != y) && (y.UpBeamLine == curve || y.DownBeamLine == curve))
                    .Select(x=>x.BeamBoundary)
                    .ToList();
                foreach (var beamCuv in beamSolid)
                {
                    res = CalBeamIntersect.JudgeBeamIntersect(polyline, beamCuv, offset);
                    if (res)
                    {
                        break;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// 计算与梁相交的曲线
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="filter"></param>
        /// <param name="acdb"></param>
        /// <param name="sInterCurve"></param>
        /// <param name="eInterCurve"></param>
        private void CalIntersectLine(Beam beam, SelectionFilter filter, AcadDatabase acdb, out List<Curve> interCurve)
        {
            interCurve = new List<Curve>();

            Point3d sMoveP1 = beam.UpStartPoint - beam.BeamNormal * searchBeamPolylineDis;
            Point3d sMoveP2 = beam.DownEndPoint + beam.BeamNormal * searchBeamPolylineDis;
            var colStartRes = GetObjectUtils.GetObjectWithBounding(_doc.Editor, sMoveP1, sMoveP2, filter);
            if (colStartRes.Status == PromptStatus.OK)
            {
                foreach (ObjectId obj in colStartRes.Value.GetObjectIds())
                {
                    interCurve.Add(acdb.Element<Curve>(obj));
                }
            }
        }
    }
}
