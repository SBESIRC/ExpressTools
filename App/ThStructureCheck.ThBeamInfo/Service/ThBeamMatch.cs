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
    }
}
