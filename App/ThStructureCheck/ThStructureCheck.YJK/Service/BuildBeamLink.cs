using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Model;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Service
{
    public class BuildBeamLink : IBeamLink
    {
        //主梁一定有一端是连在柱子上
        private List<BeamLink> beamLinks = new List<BeamLink>();
        private List<CalcBeamSeg> calcBeamSegs; //计算书某一层中的所有梁
        private string dtlCalcPath;
        private int flrNo;
        public BuildBeamLink(string dtlCalcPath, int flrNo )
        {
            this.dtlCalcPath = dtlCalcPath;
            this.flrNo = flrNo;
            this.calcBeamSegs = new YjkBeamQuery(dtlCalcPath).GetFloorCalcBeamSeg(flrNo);
        }
        public List<BeamLink> BeamLinks => this.beamLinks;
        public void Build()
        {
            while(this.calcBeamSegs.Count>0)
            {
                BeamLink beamLink = new BeamLink();
                List<CalcBeamSeg> linkBeams = new List<CalcBeamSeg> { this.calcBeamSegs[0] };
                this.calcBeamSegs.RemoveAt(0);
                int startFindJt = ForwardFindBeam(linkBeams, this.calcBeamSegs[0].Jt1);
                bool startIsPrimary = false;
                bool endIsPrimary = false;
                beamLink.Start = GetEndPortLinks(linkBeams[0], startFindJt,out startIsPrimary);
                int endFindJt = BackupFindBeam(linkBeams, this.calcBeamSegs[0].Jt2);
                beamLink.End = GetEndPortLinks(linkBeams[linkBeams.Count-1], endFindJt,out endIsPrimary);
                if(startIsPrimary || endIsPrimary)
                {
                    beamLink.Status = BeamStatus.Primary;
                }
                else if(beamLink.Start.Count>0 && beamLink.End.Count > 0)
                {
                    beamLink.Status = BeamStatus.Secondary;
                }
                else
                {
                    beamLink.Status = BeamStatus.Unknown;
                }
                beamLink.Beams = linkBeams;
                this.beamLinks.Add(beamLink);
                foreach (var item in beamLink.Beams)
                {
                    var findRes=this.calcBeamSegs.Where(i => i.ID == item.ID).Select(i => i);
                    if(findRes!=null && findRes.Count()>0)
                    {
                        this.calcBeamSegs.Remove(findRes.First());
                    }
                }
            }
        }
        private List<YjkEntityInfo> GetEndPortLinks(CalcBeamSeg calcBeamSeg,int jt,out bool isPrimary)
        {
            List<YjkEntityInfo> yjkEntities = new List<YjkEntityInfo>();
            isPrimary = false;
            List<CalcColumnSeg> linkedColumns = GetBeamLinkedColumns(calcBeamSeg.FlrNo, jt);
            if(linkedColumns.Count>0)
            {
                isPrimary = true;
                return linkedColumns.Cast<YjkEntityInfo>().ToList();
            }
            List<CalcWallBeam> linkedWallBeam = GetBeamLinkedWallBeam(calcBeamSeg.FlrNo, jt);
            if(linkedWallBeam.Count>0)
            {
                isPrimary = true;
                return linkedWallBeam.Cast<YjkEntityInfo>().ToList();
            }
            List<CalcWallCol> linkedWallCol = GetBeamLinkedWallCol(calcBeamSeg.FlrNo, jt);
            if(linkedWallCol.Count > 0)
            {
                isPrimary = true;
                return linkedWallCol.Cast<YjkEntityInfo>().ToList();
            }
            List<CalcBeamSeg> linkedBeams = GetBeamLinkedBeam(calcBeamSeg, jt,false);
            if(linkedBeams.Count>0)
            {
                return linkedBeams.Cast<YjkEntityInfo>().ToList();
            }
            return yjkEntities;
        }
        private List<CalcColumnSeg> GetBeamLinkedColumns(int flrNo,int jt)
        {
            return new YjkColumnQuery(this.dtlCalcPath).GetBeamLinkColumns(flrNo, jt);
        }
        private List<CalcWallBeam> GetBeamLinkedWallBeam(int flrNo,int jt)
        {
            return new YjkWallBeamQuery(this.dtlCalcPath).GetBeamLinkWalls(flrNo, jt);
        }
        private List<CalcWallCol> GetBeamLinkedWallCol(int flrNo,int jt)
        {
            return new YjkWallColQuery(this.dtlCalcPath).GetBeamLinkWalls(flrNo, jt);
        }
        private List<CalcBeamSeg> GetBeamLinkedBeam(CalcBeamSeg beamSeg,int jtID,bool isCollinear)
        {
            List<CalcBeamSeg> linkBeamSegs = new YjkBeamQuery(this.dtlCalcPath).GetBeamLinkBeams(beamSeg, jtID);
            for (int i = 0; i < linkBeamSegs.Count; i++)
            {
                bool res = beamSeg.IsCollinear(linkBeamSegs[i],this.dtlCalcPath);
                if(isCollinear) //要共线
                {
                    if(!res)
                    {
                        linkBeamSegs.RemoveAt(i);
                        i = i - 1;
                    }
                }
                else  //不要共线
                {
                    if(res)
                    {
                        linkBeamSegs.RemoveAt(i);
                        i = i - 1;
                    }
                }
            }
            return linkBeamSegs;
        }
        private int ForwardFindBeam(List<CalcBeamSeg> linkBeams,int jtID)
        {
            List<CalcBeamSeg> linkBeamSegs = GetBeamLinkedBeam(linkBeams[0], jtID,true);
            if (linkBeamSegs.Count==1)
            {
                linkBeams.Insert(0,linkBeamSegs[0]);
                int findJt = linkBeamSegs[0].Jt1;
                if (linkBeamSegs[0].Jt1== jtID)
                {
                    findJt = linkBeamSegs[0].Jt2;
                }
                ForwardFindBeam(linkBeams, findJt);
            }
            else if(linkBeamSegs.Count > 1)
            {
                throw new Exception("查找到了"+ linkBeamSegs.Count + "个以上共线的梁");
            }
            return jtID;
        }
        private int BackupFindBeam(List<CalcBeamSeg> linkBeams, int jtID)
        {
            List<CalcBeamSeg> linkBeamSegs = GetBeamLinkedBeam(linkBeams[linkBeams.Count-1], jtID,true);
            if (linkBeamSegs.Count == 1)
            {
                linkBeams.Add(linkBeamSegs[0]);
                int findJt = linkBeamSegs[0].Jt2;
                if (linkBeamSegs[0].Jt2 == jtID)
                {
                    findJt = linkBeamSegs[0].Jt1;
                }
                BackupFindBeam(linkBeams, findJt);
            }
            else if (linkBeamSegs.Count > 1)
            {
                throw new Exception("查找到了" + linkBeamSegs.Count + "个以上共线的梁");
            }
            return jtID;
        }
    }
}
