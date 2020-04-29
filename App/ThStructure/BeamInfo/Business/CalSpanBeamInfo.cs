using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThStructure.BeamInfo.Model;

namespace ThStructure.BeamInfo.Business
{
    public class CalSpanBeamInfo
    {
        public void FindBeamOfCentralizeMarking(ref List<Beam> allBeams)
        {
            //找到带有集中标注的梁，并将他们按照先y后x排序。
            List<Beam> cMarkBeams = allBeams.Where(x => x.CentralizeMarkings != null && x.CentralizeMarkings.Count > 0)
                .Where(x => x.CentralizeMarkings.Where(y => (y.Marking as DBText).TextString.Contains("L")).Count() > 0)
                .OrderByDescending(x => x.CentralizeMarkings.First(y => (y.Marking as DBText).TextString.Contains("L")).AlignmentPoint.Y)
                .ThenBy(x => x.CentralizeMarkings.First(y => (y.Marking as DBText).TextString.Contains("L")).AlignmentPoint.X)
                .ToList();
            List<Beam> noCMarkBeams = allBeams.Except(cMarkBeams).ToList();
            List<Beam> mergeBeams = new List<Beam>();
            foreach (var beam in cMarkBeams)
            {
                string text = beam.CentralizeMarkings.Select(x => x.Marking as DBText).First(x => x.TextString.Contains("L")).TextString;
                if (text.Contains("(") && text.Contains(")"))
                {
                    string inText = Regex.Replace(text, @"(.*\()(.*)(\).*)", "$2");
                    int num = int.Parse(Regex.Replace(inText, @"[^0-9]+", ""));
                    if (Regex.Matches(inText, "[a-zA-Z]").Count > 0)
                    {
                        num = num + 1;
                    }

                    Beam curBeam = beam;
                    int index = 1;
                    while (index < num)  //因为排序的时候是按照从左往右排序集中标注的，所以先往左边寻找梁，以免遗漏
                    {
                        Beam matchBeam = FindMatchBeam(curBeam, noCMarkBeams, allBeams, true);
                        if (matchBeam != null)
                        {
                            noCMarkBeams.Remove(matchBeam);
                            matchBeam.CentralizeMarkings = beam.CentralizeMarkings;
                            curBeam = matchBeam;
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    curBeam = beam;
                    while (index < num)  //向右找
                    {
                        Beam matchBeam = FindMatchBeam(curBeam, noCMarkBeams, allBeams, false);
                        if (matchBeam != null)
                        {
                            noCMarkBeams.Remove(matchBeam);
                            matchBeam.CentralizeMarkings = beam.CentralizeMarkings;
                            curBeam = matchBeam;
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (beam.BeamType == BeamStandardsType.SecondaryBeam)
                    {
                        mergeBeams.Add(beam);
                    }
                }
            }

            List<Beam> secBeams = noCMarkBeams.Where(x => x.BeamType == BeamStandardsType.SecondaryBeam).ToList();
            foreach (var beam in mergeBeams)
            {
                List<Beam> matchBeams = CalMatchMergeBeams(secBeams, allBeams, beam);
                if (matchBeams.Count > 0)
                {
                    Beam mergeBeam = MergeBeams(matchBeams, beam);
                    allBeams.Remove(beam);
                    allBeams = allBeams.Except(matchBeams).ToList();
                    allBeams.Add(mergeBeam);
                }
            }
        }

        private List<Beam> CalMatchMergeBeams(List<Beam> secBeams, List<Beam> allBeams, Beam beam)
        {
            List<Beam> matchBeams = new List<Beam>();
            Beam curBeam = beam;
            while (true)  //因为排序的时候是按照从左往右排序集中标注的，所以先往左边寻找梁，以免遗漏
            {
                Beam matchBeam = FindMatchBeam(curBeam, secBeams, allBeams, true);
                if (matchBeam != null)
                {
                    secBeams.Remove(matchBeam);
                    matchBeams.Add(matchBeam);
                    curBeam = matchBeam;
                }
                else
                {
                    break;
                }
            }

            curBeam = beam;
            while (true)  //向右找
            {
                Beam matchBeam = FindMatchBeam(curBeam, secBeams, allBeams, false);
                if (matchBeam != null)
                {
                    secBeams.Remove(matchBeam);
                    matchBeams.Add(matchBeam);
                    curBeam = matchBeam;
                }
                else
                {
                    break;
                }
            }

            return matchBeams;
        }

        private MergeLineBeam MergeBeams(List<Beam> beams, Beam cBeam)
        {
            MergeLineBeam mergeBeam = new MergeLineBeam(beams);
            mergeBeam.mergeBeams.Add(cBeam);
            mergeBeam.UpBeamLines.Add(cBeam.UpBeamLine);
            mergeBeam.DownBeamLines.Add(cBeam.DownBeamLine);
            mergeBeam.BeamNormal = cBeam.BeamNormal;
            mergeBeam.CentralizeMarkings = cBeam.CentralizeMarkings;
            mergeBeam.OriginMarkings = beams.SelectMany(x => x.OriginMarkings).ToList();

            foreach (var beam in beams)
            {
                if (mergeBeam.UpBeamLines.Contains(beam.UpBeamLine))
                {
                    mergeBeam.DownBeamLines.Add(beam.DownBeamLine);
                    continue;
                }
                if (mergeBeam.UpBeamLines.Contains(beam.DownBeamLine))
                {
                    mergeBeam.DownBeamLines.Add(beam.UpBeamLine);
                    continue;
                }
                if (mergeBeam.DownBeamLines.Contains(beam.UpBeamLine))
                {
                    mergeBeam.UpBeamLines.Add(beam.DownBeamLine);
                    continue;
                }
                if (mergeBeam.DownBeamLines.Contains(beam.DownBeamLine))
                {
                    mergeBeam.UpBeamLines.Add(beam.UpBeamLine);
                    continue;
                }

                Point3d sp = mergeBeam.UpBeamLines.First().StartPoint;
                double dis1 = beam.UpBeamLine.StartPoint.DistanceTo(sp);
                double dis2 = beam.UpBeamLine.EndPoint.DistanceTo(sp);
                double upDis = dis1 > dis2 ? dis2 : dis1;
                double dis3 = beam.DownBeamLine.StartPoint.DistanceTo(sp);
                double dis4 = beam.DownBeamLine.EndPoint.DistanceTo(sp);
                double downDis = dis3 > dis4 ? dis4 : dis3;
                if (upDis < downDis)
                {
                    mergeBeam.UpBeamLines.Add(beam.UpBeamLine);
                    mergeBeam.DownBeamLines.Add(beam.DownBeamLine);
                }
                else
                {
                    mergeBeam.UpBeamLines.Add(beam.DownBeamLine);
                    mergeBeam.DownBeamLines.Add(beam.UpBeamLine);
                }
            }

            return mergeBeam;
        }

        private Beam FindMatchBeam(Beam beam, List<Beam> beamsLst, List<Beam> allBeams, bool isLeft)
        {
            Vector3d normal = beam.BeamNormal;
            Vector3d findXDir = isLeft ? -Vector3d.XAxis : Vector3d.XAxis;
            Vector3d findYDir = isLeft ? Vector3d.YAxis : -Vector3d.YAxis;
            Curve intersectCuv = null;
            if (normal.DotProduct(findXDir) > 0.001 || (-0.001 < normal.DotProduct(findXDir) && findYDir.DotProduct(normal) > 0))
            {
                intersectCuv = beam.EndIntersect != null && beam.EndIntersect.EntityCurve.Count > 0 ? beam.EndIntersect.EntityCurve.First() : null;
            }
            else
            {
                intersectCuv = beam.StartIntersect != null && beam.StartIntersect.EntityCurve.Count > 0 ? beam.StartIntersect.EntityCurve.First() : null;
            }
            if (intersectCuv == null)
            {
                return null;
            }

            List<Beam> findBeam = new List<Beam>();
            if (intersectCuv is Polyline)
            {
                findBeam = beamsLst.Where(x => ((x.StartIntersect != null && x.StartIntersect.EntityCurve.Where(y => y == intersectCuv).Count() > 0) ||
                                   (x.EndIntersect != null && x.EndIntersect.EntityCurve.Where(y => y == intersectCuv).Count() > 0)) &&
                                   x.BeamNormal.IsParallelTo(normal, new Tolerance(0.0001, 0.0001)))
                                   .ToList();
            }
            else
            {
                foreach (var curBeam in allBeams)
                {
                    if (curBeam.UpBeamLine == intersectCuv)
                    {
                        findBeam.AddRange(beamsLst.Where(x => ((x.StartIntersect != null && x.StartIntersect.EntityCurve.Where(y => y == curBeam.DownBeamLine).Count() > 0) ||
                                             (x.EndIntersect != null && x.EndIntersect.EntityCurve.Where(y => y == curBeam.DownBeamLine).Count() > 0)) &&
                                             x.BeamNormal.IsParallelTo(normal, new Tolerance(0.0001, 0.0001)))
                                             .ToList());
                    }

                    if (curBeam.DownBeamLine == intersectCuv)
                    {
                        findBeam.AddRange(beamsLst.Where(x => ((x.StartIntersect != null && x.StartIntersect.EntityCurve.Where(y => y == curBeam.UpBeamLine).Count() > 0) ||
                                             (x.EndIntersect != null && x.EndIntersect.EntityCurve.Where(y => y == curBeam.UpBeamLine).Count() > 0)) &&
                                             x.BeamNormal.IsParallelTo(normal, new Tolerance(0.0001, 0.0001)))
                                             .ToList());
                    }
                }
            }

            return FindConnectBeam(findBeam, beam);
        }

        private Beam FindConnectBeam(List<Beam> beams, Beam beam)
        {
            if (beams.Count <= 0)
            {
                return null;
            }

            Beam matchBeam = null;
            Vector3d zDir = Vector3d.ZAxis;
            Vector3d yDir = Vector3d.ZAxis.CrossProduct(beam.BeamNormal);
            Matrix3d trans = new Matrix3d(new double[]{
                    beam.BeamNormal.X, yDir.X, zDir.X, 0,
                    beam.BeamNormal.Y, yDir.Y, zDir.Y, 0,
                    beam.BeamNormal.Z, yDir.Z, zDir.Z, 0,
                    0.0, 0.0, 0.0, 1.0});

            beam.UpBeamLine.TransformBy(trans.Inverse());
            beam.DownBeamLine.TransformBy(trans.Inverse());
            double y1 = beam.UpBeamLine.StartPoint.Y >= beam.DownBeamLine.StartPoint.Y ? beam.UpBeamLine.StartPoint.Y : beam.DownBeamLine.StartPoint.Y;
            double y2 = beam.UpBeamLine.StartPoint.Y < beam.DownBeamLine.StartPoint.Y ? beam.UpBeamLine.StartPoint.Y : beam.DownBeamLine.StartPoint.Y;
            beams.ForEach(x =>
            {
                x.UpBeamLine.TransformBy(trans.Inverse());
                x.DownBeamLine.TransformBy(trans.Inverse());
            });
            var paraBeams = beams.Where(x =>
            {
                double y3 = x.UpBeamLine.StartPoint.Y >= x.DownBeamLine.StartPoint.Y ? x.UpBeamLine.StartPoint.Y : x.DownBeamLine.StartPoint.Y;
                double y4 = x.UpBeamLine.StartPoint.Y < x.DownBeamLine.StartPoint.Y ? x.UpBeamLine.StartPoint.Y : x.DownBeamLine.StartPoint.Y;
                if (y1 >= y3 && y3 > y2)
                {
                    return true;
                }
                else if (y3 >= y1 && y1 > y4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }).ToList();
            if (paraBeams.Count > 0)
            {
                matchBeam = paraBeams.First();
            }

            beam.UpBeamLine.TransformBy(trans);
            beam.DownBeamLine.TransformBy(trans);
            beams.ForEach(x =>
            {
                x.UpBeamLine.TransformBy(trans);
                x.DownBeamLine.TransformBy(trans);
            });
            return matchBeam;
        }
    }
}
