using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructure.BeamInfo.Model;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Service;
using ThStructureCheck.YJK.Model;
using ThStructureCheck.YJK.Service;

namespace ThStructureCheck.ThBeamInfo.Service
{
    /// <summary>
    /// 匹配Yjk中的梁断和图纸中的梁断
    /// </summary>
    public class ThBeamMatch
    {
        private double angleRange = 1.0;
        private List<Tuple<YjkEntityInfo, Polyline, Curve>> yjkBeams =new List<Tuple<YjkEntityInfo, Polyline, Curve>>();
        private List<Beam> dwgBeams = new List<Beam>();
        private List<Tuple<Beam, List<YjkEntityInfo>>> dwgBeamMatchYjkBeams = new List<Tuple<Beam, List<YjkEntityInfo>>>();

        private List<Tuple<Beam, BeamLink>> validDwgBeamMatchYjkBeams = new List<Tuple<Beam, BeamLink>>();
        /// <summary>
        /// 可以匹配的有效梁端
        /// </summary>
        public List<Tuple<Beam, BeamLink>> ValidDwgBeamMatchYjkBeams => validDwgBeamMatchYjkBeams;
        public ThBeamMatch(List<Tuple<YjkEntityInfo, Polyline,Curve>> yjkBeams, List<Beam> dwgBeams)
        {
            this.yjkBeams = yjkBeams;
            this.dwgBeams = dwgBeams;
        }
        public void Match()
        {
            if(this.yjkBeams.Count==0 || this.dwgBeams.Count==0)
            {
                return;
            }
            //关联Dwg图纸中的梁和Yjk导入的梁
            this.dwgBeams.ForEach(o => Match(o));

            //如果Dwg中的梁关联到Yjk多根梁，且YJK的某根梁被Dwg的Beam一对一绑定，则要把该梁从其它集合中移除
            HandleUniqueBinding();

            //Dwg关联的Yjk梁段，再查找梁端前后连接的物体
            BuildBeamLink();
        }

        private void Match(Beam dwgBeam)
        {
            if (dwgBeam is LineBeam lineBeam)
            {
                Match(lineBeam);
            }
            else if (dwgBeam is ArcBeam arcBeam)
            {
                Match(arcBeam);
            }
        }
        private void Match(LineBeam lineBeam)
        {
            var res = this.yjkBeams.Where(o =>
            {
                if (o.Item1 is ModelLineBeamSeg)
                {
                    if (IsIntersectWith(lineBeam, o.Item2))
                    {
                        if (IsParallel(lineBeam.UpStartPoint, lineBeam.UpEndPoint, o.Item3.StartPoint, o.Item3.EndPoint))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }).Select(o => o.Item1);
            if (res.Count() > 0)
            {
                this.dwgBeamMatchYjkBeams.Add(Tuple.Create<Beam, List<YjkEntityInfo>>(lineBeam, res.ToList()));
            }
        }
        private void Match(ArcBeam arcBeam)
        {
            //ToDo
        }
        private void HandleUniqueBinding()
        {
            this.dwgBeamMatchYjkBeams.ForEach(o =>
            {
                if (o.Item2.Count > 1)
                {
                    for (int i = 0; i < o.Item2.Count; i++)
                    {
                        var res = this.dwgBeamMatchYjkBeams.Where(m =>
                        {
                            if (m.Item2.Count == 1)
                            {
                                if (m.Item2[0].ID == o.Item2[i].ID)
                                {
                                    return true;
                                }
                            }
                            return false;
                        });
                        if (res.Count() > 0)
                        {
                            o.Item2.RemoveAt(i);
                            i = i - 1;
                        }
                    }
                }
            });
        }
        private void BuildBeamLink()
        {
            this.dwgBeamMatchYjkBeams.ForEach(o => 
            {
                BuildBeamLink buildBeamLink = new BuildBeamLink(o.Item2);
                buildBeamLink.Build();
                if(buildBeamLink.BL.Beams.Count>0)
                {
                    this.validDwgBeamMatchYjkBeams.Add(Tuple.Create<Beam, BeamLink>(o.Item1, buildBeamLink.BL));
                }
            });
        }
        /// <summary>
        /// 生成计算指标
        /// </summary>
        /// <param name="dtlCalPath"></param>
        public void PrintCalculationIndicator(string dtlCalPath)
        {
            this.validDwgBeamMatchYjkBeams.ForEach(o => o.Item2.GenerateBeamCalculationIndex(dtlCalPath));
        }       
        private bool IsIntersectWith(LineBeam lineBeam,Polyline modelLineBeamOutline)
        {
            CloseCurveOverlap closeOverlap = new CloseCurveOverlap(lineBeam.BeamBoundary, modelLineBeamOutline);
            closeOverlap.Check();
            return closeOverlap.IsOverlap;
        }
        private bool IsParallel(Point3d dwgLineBeamSpt,Point3d dwgLineBeamEpt,
            Point3d yjkLineBeamSpt,Point3d yjkLineBeamEpt)
        {
            Vector3d dwgVec = dwgLineBeamSpt.GetVectorTo(dwgLineBeamEpt);
            Vector3d yjkVec = yjkLineBeamSpt.GetVectorTo(yjkLineBeamEpt);
            double angle = Utils.RadToAng(dwgVec.GetAngleTo(yjkVec));
            if(angle<this.angleRange)
            {
                return true;
            }
            return false;
        }
        #region ----------Print Calculation IndicatorText
        private double textHeight = 250.0;
        private List<Entity> DrawTexts(BeamLink beamLink)
        {
            List<Entity> texts = new List<Entity>();
            List<Point3d> pts = new List<Point3d>();
            for (int i = 0; i < beamLink.Beams.Count; i++)
            {
                if (beamLink.Beams[i] is ModelBeamSeg modelBeamSeg)
                {
                    var res = this.yjkBeams.Where(o => o.Item1.ID == modelBeamSeg.ID).Select(o => o);
                    if(res.Count()>0)
                    {
                        pts.Add(res.First().Item3.StartPoint);
                        pts.Add(res.First().Item3.EndPoint);
                    }
                }
            }
            int m = 0;
            int n = 1;
            double maxDis = pts[m].DistanceTo(pts[n]);
            for (int i = 0; i < pts.Count; i++)
            {
                for (int j = 0; j < pts.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    if (pts[i].DistanceTo(pts[j]) > maxDis)
                    {
                        m = i;
                        n = j;
                        maxDis = pts[i].DistanceTo(pts[j]);
                    }
                }
            }
            Point3d beamSpt = pts[m];
            Point3d beamEpt = pts[n];
            double textRotation = TextRotateAngle(beamSpt, beamEpt);
            Point3d midPt = CadTool.GetMidPt(beamSpt, beamEpt);
            Vector3d perpendVec = GeometricCalculation.GetOffsetDirection(beamSpt, beamEpt);
            texts.Add(CreateText(beamLink.BeamCalIndex.AsuFormat, midPt, textHeight * 0.0, perpendVec, textRotation));
            texts.Add(CreateText(beamLink.BeamCalIndex.GFormat, midPt, textHeight * 1.0, perpendVec, textRotation));
            texts.Add(CreateText(beamLink.BeamCalIndex.Asd.ToString(), midPt, textHeight * -1.0, perpendVec, textRotation));
            texts.Add(CreateText(beamLink.BeamCalIndex.VtFormat, midPt, textHeight * -2.0, perpendVec, textRotation));
            texts.Add(CreateText(beamLink.BeamCalIndex.Spec, midPt, textHeight * -3.0, perpendVec, textRotation));
            return texts;
        }
        private DBText CreateText(string content, Point3d basePt, double offsetHeight, Vector3d offsetVec, double textRotation)
        {
            DBText dBText = new DBText();
            dBText.TextString = content;
            dBText.Position = Point3d.Origin;
            Matrix3d mt = Matrix3d.Rotation(textRotation, Vector3d.ZAxis, Point3d.Origin);
            dBText.TransformBy(mt);
            Point3d position = basePt + offsetVec.GetNormal().MultiplyBy(offsetHeight);
            Vector3d vec = Point3d.Origin.GetVectorTo(position);
            Matrix3d moveMt = Matrix3d.Displacement(vec);
            dBText.TransformBy(moveMt);
            return dBText;
        }

        private double TextRotateAngle(Point3d firstPt, Point3d secondPt)
        {
            Vector3d vec = secondPt - firstPt;
            double rad = vec.GetAngleTo(Vector3d.XAxis);
            double ang = Utils.RadToAng(rad);
            if (ang == 0.0 || ang == 180.0 || ang == 360.0)
            {
                return 0.0;
            }
            if (ang == 90.0 || ang == 270.0)
            {
                return Math.PI / 2.0;
            }
            if (ang > 0 && ang < 90)
            {
                return rad;
            }
            if (ang > 90 && ang < 180)
            {
                return rad - Math.PI;
            }
            if (ang > 180 && ang < 270)
            {
                return rad - Math.PI;
            }
            if (ang > 270 && ang < 360)
            {
                return rad - Math.PI * 2;
            }
            return rad;
        }
    }
}
