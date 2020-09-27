using Linq2Acad;
using ThWSS.Beam;
using ThWSS.Utlis;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using ThStructure.BeamInfo.Command;
using Autodesk.AutoCAD.DatabaseServices;
using ThStructure.BeamInfo.Model;
using ThStructure.BeamInfo.Business;
using AcHelper;
using ThCADCore.NTS;
using Dreambuild.AutoCAD;

namespace ThWSS.Bussiness
{
    public class CalBeamInfoService
    {
        public List<Polyline> GetAllBeamInfo(Polyline room, Polyline floor, List<Polyline> columnCurves = null,  bool mainBeam = false)
        {
            List<Polyline> beamInfo = new List<Polyline>();
            List<Point3d> bPts = GetBoundingPoints(room);

            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                // 只提取指定区域（楼层）内的梁信息
                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                var beamCurves = ThBeamGeometryService.Instance.BeamCurves(acdb.Database, floor);
                var allBeam = thDisBeamCommand.CalBeamStruc(beamCurves);
                
                //筛选出房间中匹配的梁
                var curves = ThBeamGeometryService.Instance.BeamCurves(acdb.Database, bPts[0], bPts[1]).Cast<Curve>();
                Tolerance tol = new Tolerance(0.1, 0.1);
                beamInfo = allBeam.Where(x => curves.Any(y => {
                    var beamUp3dLine = new LineSegment3d(x.UpStartPoint, x.UpEndPoint);
                    var beamDowm3dLine = new LineSegment3d(x.DownStartPoint, x.DownEndPoint);
                    var curveLine = new LineSegment3d(y.StartPoint, y.EndPoint);
                    if (beamUp3dLine.Overlap(curveLine, tol) != null || curveLine.Overlap(beamUp3dLine, tol) != null ||
                        beamDowm3dLine.Overlap(curveLine, tol) != null || curveLine.Overlap(beamDowm3dLine, tol) != null)
                    {
                        return true;
                    }
                    return false;
                })).Select(x => x.BeamBoundary).ToList();

                if (mainBeam)
                {
                    beamInfo = CalBeamIntersectInfo(beamInfo, columnCurves);
                }
            }

            return beamInfo;
        }

        /// <summary>
        /// 计算梁的搭接信息
        /// </summary>
        /// <param name="allBeam"></param>
        /// <param name="columnCurves"></param>
        public List<Polyline> CalBeamIntersectInfo(List<Polyline> allBeam, List<Polyline> columnCurves)
        {
            List<Polyline> beamPolys = new List<Polyline>();
            foreach (var cCurve in columnCurves)
            {
                DBObjectCollection dBObject = new DBObjectCollection();
                foreach (var beam in allBeam)
                {
                    dBObject.Add(beam);
                }
                ThCADCoreNTSSpatialIndex thPatialIndex = new ThCADCoreNTSSpatialIndex(dBObject);

                while (true)
                {
                    var neighbourCurve = thPatialIndex.NearestNeighbourRemove(cCurve);
                    if (neighbourCurve == null)
                    {
                        break;
                    }
                    
                    var neighbourBeam = GeUtils.ExtendPolygons(new List<Polyline>() { neighbourCurve as Polyline }, 20).First();
                    if (cCurve.ToNTSPolygon().Intersects(neighbourBeam.ToNTSPolygon()))
                    {
                        beamPolys.Add(neighbourBeam);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return beamPolys;
        }

        /// <summary>
        /// 获取房间boundingbox的两点
        /// </summary>
        /// <param name="room"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private List<Point3d> GetBoundingPoints(Polyline room, double offset = 0)
        {
            var roomOOB = OrientedBoundingBox.Calculate(room);
            List<Point3d> allPts = new List<Point3d>();
            for (int i = 0; i < roomOOB.NumberOfVertices; i++)
            {
                allPts.Add(roomOOB.GetPoint3dAt(i));
            }

            if (offset < 0)
            {
                offset = -offset;
            }
            double minX = allPts.OrderBy(x => x.X).First().X - offset;
            double minY = allPts.OrderBy(x => x.Y).First().Y - offset;
            double minZ = allPts.OrderBy(x => x.X).First().Z - offset;
            double maxX = allPts.OrderByDescending(x => x.X).First().X + offset;
            double maxY = allPts.OrderByDescending(x => x.Y).First().Y + offset;
            double maxZ = allPts.OrderByDescending(x => x.Z).First().Z + offset;
            List<Point3d> pts = new List<Point3d>()
            {
                new Point3d(minX, minY, minZ),
                new Point3d(maxX, maxY, maxZ),
            };

            return pts;
        }
    }
}
