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
        /// <summary>
        /// Dwg识别的梁关联Yjk中的梁段
        /// </summary>
        public List<Tuple<Beam, List<YjkEntityInfo>>> DwgBeamMatchYjkBeams => dwgBeamMatchYjkBeams;
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
            this.dwgBeams.ForEach(o => Match(o));
        }
        private void Match(Beam dwgBeam)
        {
            if(dwgBeam is LineBeam lineBeam)
            {
                Match(lineBeam);
            }
            else if(dwgBeam is ArcBeam arcBeam)
            {
                Match(arcBeam);
            }
        }
        private void Match(LineBeam lineBeam)
        {
            var res=this.yjkBeams.Where(o =>
            {
                if (o.Item1 is ModelLineBeamSeg)
                {
                    if(IsIntersectWith(lineBeam,o.Item2))
                    {
                        if(IsParallel(lineBeam.UpStartPoint,lineBeam.UpEndPoint, o.Item3.StartPoint,o.Item3.EndPoint))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }).Select(o => o.Item1);
            if(res.Count()>0)
            {
                this.dwgBeamMatchYjkBeams.Add(Tuple.Create<Beam, List<YjkEntityInfo>>(lineBeam, res.ToList()));
            }
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
        private void Match(ArcBeam arcBeam)
        {
            //ToDo
        }
    }
}
