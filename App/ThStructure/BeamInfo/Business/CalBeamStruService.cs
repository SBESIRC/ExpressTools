using System;
using Linq2Acad;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using ThStructure.BeamInfo.Model;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThStructure.BeamInfo.Business
{
    public class CalBeamStruService
    {
        /// <summary>
        /// 将梁线按照规则分类成很多类
        /// </summary>
        /// <param name="dBObjects"></param>
        /// <returns></returns>
        public List<Beam> GetBeamInfo(DBObjectCollection dBObjects)
        {
            List<Beam> allBeam = new List<Beam>();
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                Dictionary<Vector3d, Dictionary<Curve, Line>> groupDic = new Dictionary<Vector3d, Dictionary<Curve, Line>>();
                Dictionary<Point3d, List<Arc>> arcGroupDic = new Dictionary<Point3d, List<Arc>>();
                foreach (DBObject obj in dBObjects)
                {
                    if (obj is Line line)
                    {
                        // 忽略Z值不为零的情况
                        var lNormal = line.Delta.GetNormal();
                        if (!lNormal.IsEqualTo(new Vector3d(lNormal.X, lNormal.Y, 0.0)))
                        {
                            continue;
                        }

                        var norComp = groupDic.Keys.Where(x => x.IsParallelToEx(lNormal)).ToList();
                        if (norComp.Count > 0)
                        {
                            groupDic[norComp.First()].Add(line, line);
                        }
                        else
                        {
                            groupDic.Add(lNormal, new Dictionary<Curve, Line>() { { line, line } });
                        }
                    }
                    else if (obj is Arc arcLine)
                    {
                        // TODO: 检查Z值不为零的情况

                        var arcLst = arcGroupDic.Keys.Where(x => x.IsEqualTo(arcLine.Center)).ToList();
                        if (arcLst.Count > 0)
                        {
                            arcGroupDic[arcLst.First()].Add(arcLine);
                        }
                        else
                        {
                            arcGroupDic.Add(arcLine.Center, new List<Arc>() { arcLine });
                        }
                    }
                }

                // TODO: Z归零应该在前面预处理时完成
                //将所有线的法相z值归零（不为0构建坐标系会出错
                groupDic = groupDic.ToDictionary(x => x.Key.Z == 0 ? x.Key : new Vector3d(x.Key.X, x.Key.Y, 0), k => k.Value);

                foreach (var lineDic in groupDic)
                {
                    var res = GetLineBeamObject(lineDic.Value, lineDic.Key, 100);
                    allBeam.AddRange(res);
                }

                foreach (var arcDic in arcGroupDic)
                {
                    var res = GetArcBeamObject(arcDic.Value);
                    allBeam.AddRange(res);
                }
            }

            return allBeam;
        }

        /// <summary>
        /// 获取直线梁
        /// </summary>
        /// <param name="linDic"></param>
        /// <param name="lineDir"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private List<LineBeam> GetLineBeamObject(Dictionary<Curve, Line> linDic, Vector3d lineDir, double tolerance)
        {
            Vector3d zDir = Vector3d.ZAxis;
            Vector3d yDir = Vector3d.ZAxis.CrossProduct(lineDir);
            Matrix3d trans = new Matrix3d(new double[]{
                    lineDir.X, yDir.X, zDir.X, 0,
                    lineDir.Y, yDir.Y, zDir.Y, 0,
                    lineDir.Z, yDir.Z, zDir.Z, 0,
                    0.0, 0.0, 0.0, 1.0});

            //将所有线转到自建坐标系,方便比较
            linDic = linDic.OrderBy(x =>
            {
                x.Value.TransformBy(trans.Inverse());
                return x.Value.StartPoint.Y;
            }).ToDictionary(x => x.Key, x => x.Value);

            List<LineBeam> beamLst = new List<LineBeam>();
            var linePair = linDic.First();
            while (linDic.Count > 0)
            {
                linDic.Remove(linePair.Key);
                Line firLine = linePair.Value;
                double lMaxX = firLine.StartPoint.X;
                double lMinX = firLine.EndPoint.X;
                if (firLine.StartPoint.X < firLine.EndPoint.X)
                {
                    lMaxX = firLine.EndPoint.X;
                    lMinX = firLine.StartPoint.X;
                }
                var paraLines = linDic.Where(x =>
                {
                    double xMaxX = x.Value.StartPoint.X;
                    double xMinX = x.Value.EndPoint.X;
                    if (x.Value.StartPoint.X < x.Value.EndPoint.X)
                    {
                        xMaxX = x.Value.EndPoint.X;
                        xMinX = x.Value.StartPoint.X;
                    }

                    if (Math.Abs(xMaxX - lMaxX) < tolerance || Math.Abs(xMinX - lMinX) < tolerance || (xMinX > lMinX && xMaxX < lMaxX) || (xMinX < lMinX && xMaxX > lMaxX))
                    {
                        return true;
                    }
                    return false;
                }).ToList();

                if (paraLines.Count > 0)
                {
                    if (paraLines.First().Value.Length > firLine.Length)  //如果梁下边线长度大于上边线，那么就会有其他上边线遗漏
                    {
                        linDic.Add(linePair.Key, linePair.Value);
                        linDic = linDic.OrderBy(x => x.Value.StartPoint.Y).ToDictionary(x => x.Key, x => x.Value);
                        linePair = paraLines.First();
                        continue;
                    }

                    firLine.TransformBy(trans);
                    double sum = 0;
                    List<Line> matchLines = new List<Line>();
                    foreach (var plineDic in paraLines)
                    {
                        var thisLine = plineDic.Value;
                        sum += thisLine.Length;
                        if ((sum > firLine.Length && Math.Abs(sum - firLine.Length) > tolerance))
                        {
                            break;
                        }

                        var tempMacthNum = matchLines.Where(x =>
                        {
                            double xMaxX = x.StartPoint.X;
                            double xMinX = x.EndPoint.X;
                            if (x.StartPoint.X < x.EndPoint.X)
                            {
                                xMaxX = x.EndPoint.X;
                                xMinX = x.StartPoint.X;
                            }

                            double mMaxX = thisLine.StartPoint.X;
                            double mMinX = thisLine.EndPoint.X;
                            if (thisLine.StartPoint.X < thisLine.EndPoint.X)
                            {
                                mMaxX = thisLine.EndPoint.X;
                                mMinX = thisLine.StartPoint.X;
                            }

                            if (mMaxX <= xMinX || mMinX >= xMaxX)
                            {
                                return false;
                            }
                            return true;
                        });
                        if (tempMacthNum.Count() > 0)
                        {
                            break;
                        }

                        matchLines.Add(thisLine.Clone() as Line);
                        thisLine.TransformBy(trans);
                        LineBeam beam = new LineBeam(firLine, thisLine);
                        beam.UpBeamLine = linePair.Key;
                        beam.DownBeamLine = plineDic.Key;
                        beamLst.Add(beam);
                        linDic.Remove(plineDic.Key);
                    }
                }
                else
                {
                    firLine.TransformBy(trans);
                }

                if (linDic.Count > 0)
                {
                    linePair = linDic.First();
                }
            }

            return beamLst;
        }

        /// <summary>
        /// 获取弧梁
        /// </summary>
        /// <param name="arcs"></param>
        /// <returns></returns>
        private List<ArcBeam> GetArcBeamObject(List<Arc> arcs)
        {
            List<ArcBeam> beamLst = new List<ArcBeam>();
            arcs = arcs.OrderByDescending(x => x.Length).ToList();
            Arc firArc = arcs.First();
            while (arcs.Count > 0)
            {
                Point3d sP = firArc.StartPoint;
                Point3d eP = firArc.EndPoint;
                Point3d midP = new Point3d((sP.X + eP.X) / 2, (sP.Y + eP.Y) / 2, (sP.Z + eP.Z) / 2);
                Vector3d xDir = (sP - eP).GetNormal();
                Vector3d zDir = Vector3d.ZAxis;
                Vector3d yDir = Vector3d.ZAxis.CrossProduct(xDir);
                Matrix3d trans = new Matrix3d(new double[]{
                    xDir.X, yDir.X, zDir.X, 0,
                    xDir.Y, yDir.Y, zDir.Y, 0,
                    xDir.Z, yDir.Z, zDir.Z, 0,
                    0.0, 0.0, 0.0, 1.0});
                arcs.ForEach(x => x.TransformBy(trans.Inverse()));
                arcs.Remove(firArc);

                double maxX = firArc.StartPoint.X;
                double minX = firArc.EndPoint.X;
                if (maxX < minX)
                {
                    maxX = firArc.EndPoint.X;
                    minX = firArc.StartPoint.X;
                }

                firArc.TransformBy(trans);
                var arcLst = arcs.Where(x => minX < x.StartPoint.X && x.StartPoint.X < maxX
                    && minX < x.EndPoint.X && x.EndPoint.X < maxX).ToList();
                foreach (var thisLine in arcLst)
                {
                    thisLine.TransformBy(trans);
                    ArcBeam beam = new ArcBeam(firArc, thisLine);
                    beamLst.Add(beam);
                    arcs.Remove(thisLine);
                }
                arcs.ForEach(x => x.TransformBy(trans));
                if (arcs.Count > 0)
                {
                    firArc = arcs.First();
                }
            }

            return beamLst;
        }
    }
}
