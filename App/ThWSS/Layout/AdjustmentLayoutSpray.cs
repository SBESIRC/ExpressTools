using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThCADCore.NTS;
using ThWSS.Bussiness;
using ThWSS.Model;
using ThWSS.Utlis;

namespace ThWSS.Layout
{
    class AdjustmentLayoutSpray : SquareLayout
    {
        SprayLayoutModel sprayLayoutModel = null;
        public AdjustmentLayoutSpray(SprayLayoutModel layoutModel) : base(layoutModel)
        {
            sprayLayoutModel = layoutModel;
        }

        public List<SprayLayoutData> Layout(Dictionary<Polyline, List<SprayLayoutData>> roomDic)
        {
            List<SprayLayoutData> sprays = new List<SprayLayoutData>();
            foreach (var room in roomDic)
            {
                List<List<Point3d>> resPts = new List<List<Point3d>>();
                foreach (var roomSprays in room.Value)
                {
                    resPts.Add(LayoutPoints(room.Key, roomSprays.Position, roomSprays.mainDir, roomSprays.otherDir));
                    sprays.AddRange(CreateSprayModels(resPts, roomSprays.mainDir, roomSprays.otherDir).SelectMany(x => x).ToList());
                }
            }

            List<Polyline> blindRegions = roomDic.Keys.ToList();
            if (sprays.Count > 0)
            {
                // 计算房间内的所有喷淋的保护半径
                var radiis = SprayLayoutDataUtils.Radii(sprays);

                //计算保护盲区
                blindRegions = new List<Polyline>();
                foreach (var bRegions in roomDic.Keys)
                {
                    blindRegions.AddRange(bRegions.Difference(radiis).Cast<Polyline>());
                }
            }

            //计算盲区内排布点
            foreach (Polyline curve in blindRegions)
            {
                var obb = OrientedBoundingBox.Calculate(curve);
                var layout = new SquareLayoutWithRay(sprayLayoutModel);
                sprays.AddRange(layout.Layout(curve, obb).SelectMany(x=>x));
            }

            return sprays;
        }

        /// <summary>
        /// 调整喷淋点位
        /// </summary>
        /// <param name="roomPoly"></param>
        /// <param name="pt"></param>
        /// <param name="vDir"></param>
        /// <param name="tDir"></param>
        /// <returns></returns>
        private List<Point3d> LayoutPoints(Polyline roomPoly, Point3d pt, Vector3d vDir, Vector3d tDir)
        {
            List<Point3d> layoutPoints = new List<Point3d>();
            //房间线
            List<Line> roomLines = new List<Line>();
            for (int i = 0; i < roomPoly.NumberOfVertices; i++)
            {
                roomLines.Add(new Line(roomPoly.GetPoint3dAt(i), roomPoly.GetPoint3dAt((i + 1) % roomPoly.NumberOfVertices)));
            }

            List<Point3d> points = GeUtils.GetRayIntersectPoints(roomLines, pt, vDir);
            points.AddRange(GeUtils.GetRayIntersectPoints(roomLines, pt, -vDir));
            if (points.Count >= 2 && points.First().DistanceTo(points.Last()) > 200)
            {
                layoutPoints.Add(new Point3d((points.First().X + points.Last().X) / 2, (points.First().Y + points.Last().Y) / 2, 0));
            }
            else
            {
                points = GeUtils.GetRayIntersectPoints(roomLines, pt, tDir);
                points.AddRange(GeUtils.GetRayIntersectPoints(roomLines, pt, -tDir));
                if (points.Count >= 2 && points.First().DistanceTo(points.Last()) > 200)
                {
                    layoutPoints.Add(new Point3d((points.First().X + points.Last().X) / 2, (points.First().Y + points.Last().Y) / 2, 0));
                }
            }

            return layoutPoints;
        }
    }
}
