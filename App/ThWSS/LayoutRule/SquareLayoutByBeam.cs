using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThCADCore.NTS;
using ThWSS.Bussiness;
using ThWSS.Model;
using ThWSS.Utlis;

namespace ThWSS.LayoutRule
{
    public class SquareLayoutByBeam : SquareLayout
    {
        double height = 1000;
        double sprayHeight = 550;
        double floorHeight = 220;
        public SquareLayoutByBeam(SprayLayoutModel layoutModel) : base(layoutModel)
        {

        }

        public List<List<SprayLayoutData>> Layout(List<Line> roomLines, Polyline diviRoom, List<ThStructure.BeamInfo.Model.Beam> beamInfo)
        {
            var beamDic = CalLayoutSpaceByBeam(diviRoom, roomLines, beamInfo);
            var bufferRes = diviRoom.Buffer(-beamDic.Max(x => x.Value));
            Polyline resPoly = diviRoom;
            bool hasBeam = true;
            if (bufferRes.Count > 0)
            {
                using (AcadDatabase acdb = AcadDatabase.Active())
                {
                    acdb.ModelSpace.Add(diviRoom);
                    //acdb.ModelSpace.Add(resPoly);
                }
                resPoly = diviRoom.Buffer(-beamDic.Max(x => x.Value))[0] as Polyline;
                hasBeam = false;
            }

            //DBObjectCollection dBObject = new DBObjectCollection();
            //for (int i = 0; i < resPoly.NumberOfVertices; i++)
            //{
            //    dBObject.Add(new Line(resPoly.GetPoint3dAt(i), resPoly.GetPoint3dAt((i + 1) % resPoly.NumberOfVertices)));
            //}
            //var objCollection = dBObject.Polygons();
            //Polyline polygon = objCollection.Cast<Polyline>().OrderByDescending(x => x.Area).FirstOrDefault();
            //if (polygon == null)
            //{
            //    return null;
            //}

            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                //acdb.ModelSpace.Add(diviRoom);
                acdb.ModelSpace.Add(resPoly);
            }

            //计算房间走向
            var roomOOB = OrientedBoundingBox.Calculate(resPoly);
            return base.Layout(resPoly, roomOOB, hasBeam);
        }

        /// <summary>
        /// 计算可布置区域间距
        /// </summary>
        /// <param name="diviRoom"></param>
        /// <param name="roomLines"></param>
        /// <param name="beamInfo"></param>
        /// <returns></returns>
        public List<KeyValuePair<Line, double>> CalLayoutSpaceByBeam(Polyline diviRoom, List<Line> roomLines, List<ThStructure.BeamInfo.Model.Beam> beamInfo)
        {
            List<Line> diviRoomLines = new List<Line>();
            for (int i = 0; i < diviRoom.NumberOfVertices; i++)
            {
                diviRoomLines.Add(new Line(diviRoom.GetPoint3dAt(i), diviRoom.GetPoint3dAt((i + 1) % diviRoom.NumberOfVertices)));
            }
            
            List<KeyValuePair<Line, double>> dic = new List<KeyValuePair<Line, double>>();
            foreach (var line in diviRoomLines)
            {
                if (line.Length < 50)
                {
                    dic.Add(new KeyValuePair<Line, double>(line, 0));
                    continue;
                }

                double length = CalSpaceLength(line, roomLines, beamInfo);
                dic.Add(new KeyValuePair<Line, double>(line, length));
            }

            double maxValue = dic.Max(x => x.Value);
            //有误差导致一根线被打成两根处理
            for (int i = 0; i < dic.Count; i++)
            {
                if (dic[i].Value == 0)
                {
                    dic[i] = new KeyValuePair<Line, double>(dic[i].Key, maxValue);
                    int j = i - 1;
                    if (j < 0)
                    {
                        j = dic.Count - 1;
                    }

                    var next = dic[(i + 1) % dic.Count];
                    var pre = dic[j];

                    if (next.Key.Delta.GetNormal().IsParallelTo(pre.Key.Delta.GetNormal(), new Tolerance(0.1, 0.1)))
                    {
                        if (pre.Value <= next.Value)
                        {
                            dic[j] = new KeyValuePair<Line, double>(pre.Key, next.Value);
                        }
                        else
                        {
                            dic[(i + 1) % dic.Count] = new KeyValuePair<Line, double>(next.Key, pre.Value);
                        }
                    }
                }
            }

            return dic;
        }

        /// <summary>
        /// 计算每条线移动距离
        /// </summary>
        /// <param name="line"></param>
        /// <param name="roomLines"></param>
        /// <param name="beamInfo"></param>
        /// <returns></returns>
        public double CalSpaceLength(Line line, List<Line> roomLines, List<ThStructure.BeamInfo.Model.Beam> beamInfo)
        {
            var lineSeg = new LineSegment3d(line.StartPoint, line.EndPoint);
            double length = 300;
            foreach (var beam in beamInfo)
            {
                List<LineSegment3d> beamLine = new List<LineSegment3d>()
                    {
                        new LineSegment3d(beam.UpStartPoint, beam.UpEndPoint),
                        new LineSegment3d(beam.DownStartPoint, beam.DownEndPoint),
                    };
                foreach (var beamSeg in beamLine)
                {
                    var overCuv = lineSeg.Overlap(beamSeg, new Tolerance(0.5, 0.5));
                    if (overCuv == null)
                    {
                        overCuv = beamSeg.Overlap(lineSeg, new Tolerance(0.5, 0.5));
                    }

                    if (overCuv != null)
                    {
                        int res = GetBeamRelationWithLine(beam, roomLines, new LineSegment3d(overCuv.StartPoint, overCuv.EndPoint), out List<Line> matchRoomLines);
                        if (res == 0 || res == 2)
                        {
                            double cLength = CalSapceByCenterBeam(beam);
                            if (cLength > length)
                            {
                                length = cLength;
                            }
                        }
                        if (res == 1 || res == 2)
                        {
                            double sLength = CalSpaceBySideBeam(beam, matchRoomLines, line);
                            if (sLength > length)
                            {
                                length = sLength;
                            }
                        }
                    }
                }
            }

            return length;
        }

        /// <summary>
        /// 计算分割区域的边界关系（0.只做为中间梁；1.只做为边梁；2.既做为边梁又做为中间梁）
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="roomLines"></param>
        /// <param name="diviLine"></param>
        /// <returns></returns>
        private int GetBeamRelationWithLine(ThStructure.BeamInfo.Model.Beam beam, List<Line> roomLines, LineSegment3d segLine, out List<Line> matchRoomLines)
        {
            matchRoomLines = new List<Line>();
            List<Line> matchLines = CalWallLinesInBeam(beam, roomLines);

            double length = 0;
            foreach (var line in matchLines)
            {
                var proLine = GeUtils.LineProjectToLine(new Line(segLine.StartPoint, segLine.EndPoint), line);
                var proLineSeg = new LineSegment3d(proLine.StartPoint, proLine.EndPoint);
                var overCuv = proLineSeg.Overlap(segLine);
                if (overCuv == null)
                {
                    overCuv = segLine.Overlap(proLineSeg);
                }

                if (overCuv != null)
                {
                    matchRoomLines.Add(line);
                    length = length + (overCuv as LineSegment3d).Length;
                }
            }

            if (matchLines.Count <= 0 || length == 0)
            {
                return 0;
            }

            return segLine.Length <= length + 50 ? 1 : 2;
        }

        /// <summary>
        /// 计算与在梁线内的墙线
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="roomLines"></param>
        /// <returns></returns>
        private List<Line> CalWallLinesInBeam(ThStructure.BeamInfo.Model.Beam beam, List<Line> roomLines)
        {
            Vector3d zDir = Vector3d.ZAxis;
            Vector3d yDir = beam.BeamNormal;
            Vector3d xDir = Vector3d.ZAxis.CrossProduct(yDir);
            Matrix3d trans = new Matrix3d(new double[]{
                    xDir.X, yDir.X, zDir.X, 0,
                    xDir.Y, yDir.Y, zDir.Y, 0,
                    xDir.Z, yDir.Z, zDir.Z, 0,
                    0.0, 0.0, 0.0, 1.0});

            double minX = beam.UpStartPoint.TransformBy(trans).X;
            double maxX = beam.DownStartPoint.TransformBy(trans).X;
            if (minX > maxX)
            {
                minX = maxX;
                maxX = beam.UpStartPoint.TransformBy(trans).X;
            }

            List<Line> resLines = roomLines.Where(x =>
            {
                var startX = x.StartPoint.TransformBy(trans).X;
                var endX = x.EndPoint.TransformBy(trans).X;
                if (minX <= startX && startX <= maxX && minX <= endX && endX <= maxX)
                {
                    return true;
                }
                return false;
            }).ToList();

            return resLines;
        }

        /// <summary>
        /// 计算边梁的应移动距离
        /// </summary>
        /// <param name="beam"></param>
        /// <param name="matchRoomLines"></param>
        /// <param name="roomLine"></param>
        /// <returns></returns>
        private double CalSpaceBySideBeam(ThStructure.BeamInfo.Model.Beam beam, List<Line> matchRoomLines, Line roomLine)
        {
            double beamHeight = height;
            if (beam.ThOriginMarkingcsP != null)
            {
                if (beam.ThOriginMarkingcsP.SectionSize != null)
                {
                    var beamSize = beam.ThOriginMarkingcsP.SectionSize.Split('x');
                    if (beamSize.Count() > 1)
                    {
                        if (double.TryParse(beamSize[1], out double ht)) { beamHeight = ht; };
                    }
                }
            }

            double maxLength = 0;
            foreach (var line in matchRoomLines)
            {
                Point3d newP = line.GetClosestPointTo(roomLine.StartPoint, true);
                double length = newP.DistanceTo(roomLine.StartPoint) - 200 + beamHeight - sprayHeight;
                if (length > maxLength)
                {
                    maxLength = length;
                }
            }

            return maxLength;
        }

        /// <summary>
        /// 计算中间梁应移动距离
        /// </summary>
        /// <param name="beam"></param>
        /// <returns></returns>
        private double CalSapceByCenterBeam(ThStructure.BeamInfo.Model.Beam beam)
        {
            double beamHeight = height;
            if (beam.ThOriginMarkingcsP != null)
            {
                if (beam.ThOriginMarkingcsP.SectionSize != null)
                {
                    var beamSize = beam.ThOriginMarkingcsP.SectionSize.Split('x');
                    if (beamSize.Count() > 1)
                    {
                        if (double.TryParse(beamSize[1], out double ht)) { beamHeight = ht; };
                    }
                }
            }

            double maxLength = 0;
            double bValue = beamHeight - sprayHeight - floorHeight;
            if (bValue <= 0)
            {
                maxLength = 300;
            }
            else if (bValue <= 60)
            {
                maxLength = CalAValue(0, 60, 300, 600, bValue);
            }
            else if (bValue <= 140)
            {
                maxLength = CalAValue(60, 140, 600, 900, bValue);
            }
            else if (bValue <= 240)
            {
                maxLength = CalAValue(140, 240, 900, 1200, bValue);
            }
            else if (bValue <= 350)
            {
                maxLength = CalAValue(240, 350, 1200, 1500, bValue);
            }
            else if (bValue <= 450)
            {
                maxLength = CalAValue(350, 450, 1500, 1800, bValue);
            }
            else if (bValue <= 600)
            {
                maxLength = CalAValue(450, 600, 1800, 2100, bValue);
            }
            else
            {
                maxLength = sideLength / 2;
            }

            if (maxLength > sideLength / 2)
            {
                maxLength = sideLength / 2;
            }
            return maxLength;
        }

        /// <summary>
        /// 线性增量
        /// </summary>
        /// <param name="bMin"></param>
        /// <param name="bMax"></param>
        /// <param name="aMin"></param>
        /// <param name="aMax"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        private double CalAValue(double bMin, double bMax, double aMin, double aMax, double bValue)
        {
            double aDifference = aMax - aMin;
            double ratio = (bValue - bMin) / (bMax - bMin);

            return aMin + aDifference * ratio;
        }
    }
}
