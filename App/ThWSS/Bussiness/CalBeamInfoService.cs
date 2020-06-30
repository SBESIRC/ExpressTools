using AcHelper;
using Linq2Acad;
using System.Linq;
using ThWSS.Beam;
using ThWSS.Utlis;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using ThStructure.BeamInfo.Command;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Bussiness
{
    public class CalBeamInfoService
    {
        public List<ThStructure.BeamInfo.Model.Beam> GetAllBeamInfo(Polyline room, Polyline floor)
        {
            List<ThStructure.BeamInfo.Model.Beam> beamInfo = new List<ThStructure.BeamInfo.Model.Beam>();
            List<Point3d> bPts = GetBoundingPoints(room);

            using (ThBeamDbManager beamManager = new ThBeamDbManager(Active.Database))
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                // 只提取指定区域（楼层）内的梁信息
                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                var beamCurves = ThBeamGeometryService.Instance.BeamCurves(beamManager.HostDb, floor);
                var allBeam = thDisBeamCommand.CalBeamStruc(beamCurves);
                
                //筛选出房间中匹配的梁
                var curves = ThBeamGeometryService.Instance.BeamCurves(beamManager.HostDb, bPts[0], bPts[1]).Cast<Curve>();
                Tolerance tol = new Tolerance(0.1, 01);
                beamInfo = allBeam.Where(x => curves.Any(y=> {
                    var beamUp3dLine = new LineSegment3d(x.UpStartPoint, x.UpEndPoint);
                    var beamDowm3dLine = new LineSegment3d(x.UpStartPoint, x.UpEndPoint);
                    var curveLine = new LineSegment3d(y.StartPoint, y.EndPoint);
                    if (beamUp3dLine.Overlap(curveLine, tol) != null || curveLine.Overlap(beamUp3dLine, tol) != null ||
                        beamDowm3dLine.Overlap(curveLine, tol) != null || curveLine.Overlap(beamDowm3dLine, tol) != null)
                    {
                        return true;
                    }
                    return false;
                })).ToList();
            }

            return beamInfo;
        }

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
