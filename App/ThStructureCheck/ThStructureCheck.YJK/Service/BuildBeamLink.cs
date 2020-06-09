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
    public class BuildCalcBeamLink : IBeamLink
    {
        //主梁一定有一端是连在柱子上
        private List<BeamLink> beamLinks = new List<BeamLink>();
        private List<CalcBeamSeg> calcBeamSegs; //计算书某一层中的所有梁
        private string dtlCalcPath;
        private int flrNo;
        public BuildCalcBeamLink(string dtlCalcPath, int flrNo )
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
                int startFindJt = ForwardFindBeam(linkBeams, linkBeams[0].Jt1);
                bool startIsPrimary = false;
                bool endIsPrimary = false;
                beamLink.Start = GetEndPortLinks(linkBeams[0], startFindJt,out startIsPrimary);
                int endFindJt = BackupFindBeam(linkBeams, linkBeams[linkBeams.Count-1].Jt2);
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
                beamLink.Beams = linkBeams.Cast<YjkEntityInfo>().ToList();
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
            return new YjkColumnQuery(this.dtlCalcPath).GetCalcBeamLinkColumns(flrNo, jt);
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
                bool res = beamSeg.IsCollinear(linkBeamSegs[i]);
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
                return ForwardFindBeam(linkBeams, findJt);
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
    public class BuildModelBeamLink : IBeamLink
    {
        //主梁一定有一端是连在柱子上
        private List<BeamLink> beamLinks = new List<BeamLink>();
        private List<ModelBeamSeg> modelBeamSegs; //计算书某一层中的所有梁
        private string dtlModelPath;
        private int flrNo;
        private YjkGridQuery yjkGridQuery;
        public BuildModelBeamLink(string dtlModelPath, int flrNo)
        {
            this.dtlModelPath = dtlModelPath;
            this.flrNo = flrNo;
            this.modelBeamSegs = new YjkBeamQuery(dtlModelPath).GetFloorModelBeamSeg(flrNo);
            yjkGridQuery = new YjkGridQuery(this.dtlModelPath);
        }
        public List<BeamLink> BeamLinks => this.beamLinks;
        public void Build()
        {
            while (this.modelBeamSegs.Count > 0)
            {
                BeamLink beamLink = new BeamLink();
                List<ModelBeamSeg> linkBeams = new List<ModelBeamSeg> { this.modelBeamSegs[0] };
                this.modelBeamSegs.RemoveAt(0);
                ModelGrid currentBeamGrid = yjkGridQuery.GetModelGrid(linkBeams[0].GridID);
                int startFindJt = ForwardFindBeam(linkBeams, currentBeamGrid.Jt1ID);
                bool startIsPrimary = false;
                bool endIsPrimary = false;
                beamLink.Start = GetEndPortLinks(linkBeams[0], startFindJt, out startIsPrimary);
                int endFindJt = BackupFindBeam(linkBeams, currentBeamGrid.Jt2ID);
                beamLink.End = GetEndPortLinks(linkBeams[linkBeams.Count - 1], endFindJt, out endIsPrimary);
                if (startIsPrimary || endIsPrimary)
                {
                    beamLink.Status = BeamStatus.Primary;
                }
                else if (beamLink.Start.Count > 0 && beamLink.End.Count > 0)
                {
                    beamLink.Status = BeamStatus.Secondary;
                }
                else
                {
                    beamLink.Status = BeamStatus.Unknown;
                }
                beamLink.Beams = linkBeams.Cast<YjkEntityInfo>().ToList();
                this.beamLinks.Add(beamLink);
                foreach (var item in beamLink.Beams)
                {
                    var findRes = this.modelBeamSegs.Where(i => i.ID == item.ID).Select(i => i);
                    if (findRes != null && findRes.Count() > 0)
                    {
                        this.modelBeamSegs.Remove(findRes.First());
                    }
                }
            }
        }
        private List<YjkEntityInfo> GetEndPortLinks(ModelBeamSeg modelBeamSeg, int jt, out bool isPrimary)
        {
            List<YjkEntityInfo> yjkEntities = new List<YjkEntityInfo>();
            isPrimary = false;
            List<ModelColumnSeg> linkedColumns = GetBeamLinkedColumns(modelBeamSeg.StdFlrID, jt);
            if (linkedColumns.Count > 0)
            {
                isPrimary = true;
                return linkedColumns.Cast<YjkEntityInfo>().ToList();
            }
            List<ModelWallSeg> linkedWall = GetBeamLinkedWall(modelBeamSeg, jt);
            if (linkedWall.Count > 0)
            {
                isPrimary = true;
                return linkedWall.Cast<YjkEntityInfo>().ToList();
            }
            List<ModelBeamSeg> linkedBeams = GetBeamLinkedBeam(modelBeamSeg, jt, false);
            if (linkedBeams.Count > 0)
            {
                return linkedBeams.Cast<YjkEntityInfo>().ToList();
            }
            return yjkEntities;
        }
        private List<ModelColumnSeg> GetBeamLinkedColumns(int stdFlrId, int jt)
        {
            List<ModelColumnSeg> results = new List<ModelColumnSeg>();
            results = new YjkColumnQuery(this.dtlModelPath).GetModelBeamLinkColumns(stdFlrId, jt);
            return results;
        }
        private List<ModelWallSeg> GetBeamLinkedWall(ModelBeamSeg modelBeamSeg, int jt)
        {
            return new YjkWallSegQuery(this.dtlModelPath).GetBeamLinkedWall(modelBeamSeg, jt);
        }
        private List<ModelBeamSeg> GetBeamLinkedBeam(ModelBeamSeg beamSeg, int jtID, bool isCollinear)
        {
            List<ModelBeamSeg> linkBeamSegs = new YjkBeamQuery(this.dtlModelPath).GetBeamLinkBeams(beamSeg, jtID);
            for (int i = 0; i < linkBeamSegs.Count; i++)
            {
                bool res = beamSeg.IsCollinear(linkBeamSegs[i]);
                if (isCollinear) //要共线
                {
                    if (!res)
                    {
                        linkBeamSegs.RemoveAt(i);
                        i = i - 1;
                    }
                }
                else  //不要共线
                {
                    if (res)
                    {
                        linkBeamSegs.RemoveAt(i);
                        i = i - 1;
                    }
                }
            }
            return linkBeamSegs;
        }
        private int ForwardFindBeam(List<ModelBeamSeg> linkBeams, int jtID)
        {
            List<ModelBeamSeg> linkBeamSegs = GetBeamLinkedBeam(linkBeams[0], jtID, true);
            if (linkBeamSegs.Count == 1)
            {
                linkBeams.Insert(0, linkBeamSegs[0]);
                ModelGrid mg = linkBeamSegs[0].Grid;
                int findJt = mg.Jt1ID;
                if (mg.Jt1ID == jtID)
                {
                    findJt = mg.Jt2ID;
                }
                return ForwardFindBeam(linkBeams, findJt);
            }
            else if (linkBeamSegs.Count > 1)
            {
                throw new Exception("查找到了" + linkBeamSegs.Count + "个以上共线的梁");
            }
            return jtID;
        }
        private int BackupFindBeam(List<ModelBeamSeg> linkBeams, int jtID)
        {
            List<ModelBeamSeg> linkBeamSegs = GetBeamLinkedBeam(linkBeams[linkBeams.Count - 1], jtID, true);
            if (linkBeamSegs.Count == 1)
            {
                linkBeams.Add(linkBeamSegs[0]);
                ModelGrid mg = linkBeamSegs[0].Grid;
                int findJt = mg.Jt2ID;
                if (mg.Jt2ID == jtID)
                {
                    findJt = mg.Jt1ID;
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
