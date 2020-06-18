using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructure.BeamInfo.Command;
using ThWSS.Beam;
using ThStructure.BeamInfo.Model;
using Autodesk.AutoCAD.DatabaseServices;
using ThWSS.Utlis;
using Autodesk.AutoCAD.Geometry;
using ThStructure.BeamInfo.Business;

namespace ThWSS.Bussiness
{
    public class CalBeamInfoService
    {
        public List<ThStructure.BeamInfo.Model.Beam> GetAllBeamInfo(Polyline room)
        {
            List<ThStructure.BeamInfo.Model.Beam> beamInfo = new List<ThStructure.BeamInfo.Model.Beam>();
            List<Point3d> bPts = GetBoundingPoints(room);
            
            using (AcadDatabase acdb = AcadDatabase.Active())
            using (ThBeamDbManager beamManager = new ThBeamDbManager(acdb.Database))
            {
                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                // 获取所有构成梁的曲线（线，多段线，圆弧）
                var beamCurves = ThBeamGeometryService.Instance.BeamCurves(beamManager);
                // 考虑到多段线的情况，需要将多段线“炸”成线来处理
                var allBeam = thDisBeamCommand.CalBeamStruc(ThBeamGeometryPreprocessor.ExplodeCurves(beamCurves));

                //筛选出房间中匹配的梁
                var curves = ThBeamGeometryService.Instance.BeamCurves(beamManager, bPts[0], bPts[1]).Cast<Curve>();
                beamInfo = allBeam.Where(x => curves.Where(y => (y.StartPoint.IsEqualTo(x.UpBeamLine.StartPoint) && y.EndPoint.IsEqualTo(x.UpBeamLine.EndPoint))
                                     || (y.StartPoint.IsEqualTo(x.DownBeamLine.StartPoint) && y.EndPoint.IsEqualTo(x.DownBeamLine.EndPoint))).Count() > 0).ToList();
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
