using AcHelper;
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

            // 只提取指定区域（楼层）内的梁信息
            ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
            var beamCurves = ThBeamGeometryService.Instance.BeamCurves(Active.Database, floor);
            var allBeam = thDisBeamCommand.CalBeamStruc(beamCurves);

            //筛选出房间中匹配的梁
            var curves = ThBeamGeometryService.Instance.BeamCurves(Active.Database, bPts[0], bPts[1]).Cast<Curve>();
            beamInfo = allBeam.Where(x => curves.Where(y => (y.StartPoint.IsEqualTo(x.UpBeamLine.StartPoint, new Tolerance(0.1, 0.1)) && y.EndPoint.IsEqualTo(x.UpBeamLine.EndPoint, new Tolerance(0.1, 0.1)))
                                 || (y.StartPoint.IsEqualTo(x.DownBeamLine.StartPoint, new Tolerance(0.1, 0.1)) && y.EndPoint.IsEqualTo(x.DownBeamLine.EndPoint, new Tolerance(0.1, 0.1)))).Count() > 0).ToList();

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
