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
    /// <summary>
    /// 创建模型库中某层的所有梁连接
    /// </summary>
    public class BuildFlrBeamLink : IBeamLink
    {
        private List<BeamLink> allBeamLinks = new List<BeamLink>();
        private List<BeamLink> mainBeamLinks = new List<BeamLink>(); //主梁一定有一端是连在柱子上或墙上
        private List<BeamLink> secondaryBeamLinks = new List<BeamLink>();//次梁集合

        private List<ModelBeamSeg> modelBeamSegs; //计算书某一层中的所有梁
        private string dtlModelPath;
        private int flrNo;
        private YjkGridQuery yjkGridQuery;
        public BuildFlrBeamLink(string dtlModelPath, int flrNo)
        {
            this.dtlModelPath = dtlModelPath;
            this.flrNo = flrNo;
            this.modelBeamSegs = new YjkBeamQuery(dtlModelPath).GetFloorModelBeamSeg(flrNo);
            yjkGridQuery = new YjkGridQuery(this.dtlModelPath);
        }
        public BuildFlrBeamLink(string dtlModelPath)
        {
            this.dtlModelPath = dtlModelPath;
            yjkGridQuery = new YjkGridQuery(this.dtlModelPath);
        }
        public List<BeamLink> MainBeamLinks => this.mainBeamLinks;
        public List<BeamLink> SecondaryBeamLinks => this.secondaryBeamLinks;

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
                this.allBeamLinks.Add(beamLink);
                foreach (var item in beamLink.Beams)
                {
                    var findRes = this.modelBeamSegs.Where(i => i.ID == item.ID).Select(i => i);
                    if (findRes != null && findRes.Count() > 0)
                    {
                        this.modelBeamSegs.Remove(findRes.First());
                    }
                }
            }
            //对主梁分类
            this.mainBeamLinks = this.allBeamLinks.Where(o => o.Status == BeamStatus.Primary).ToList();
            //对次梁分跨
            SecondaryBeamSplitSpan();
        }
        /// <summary>
        /// 次梁分跨
        /// </summary>
        private void SecondaryBeamSplitSpan()
        {
            var secondaryBeams = this.allBeamLinks.Where(o => o.Status == BeamStatus.Secondary).ToList();
            secondaryBeams.ForEach(o => this.secondaryBeamLinks.AddRange(SplitSpanBeamLink(o)));
        }
        /// <summary>
        /// 分跨梁段
        /// </summary>
        /// <param name="beamLink"></param>
        /// <returns></returns>
        private List<BeamLink> SplitSpanBeamLink(BeamLink beamLink)
        {
            List<BeamLink> splitSpanBeamLinks = new List<BeamLink>();
            List<ModelBeamSeg> modelBeamSegs = beamLink.Beams.Cast<ModelBeamSeg>().ToList();
            for (int i = 0; i < modelBeamSegs.Count; i++)
            {
                BeamLink subBeamLink = new BeamLink();
                List<YjkEntityInfo> startLink = new List<YjkEntityInfo>();
                bool startIsPrimary = false;
                if (i == 0)
                {
                    startLink = beamLink.Start;
                }
                else
                {
                    ModelGrid currentBeamGrid = yjkGridQuery.GetModelGrid(modelBeamSegs[i].GridID);
                    startLink = GetEndPortLinks(modelBeamSegs[i], currentBeamGrid.Jt1ID, out startIsPrimary);
                }
                List<YjkEntityInfo> splitBeams = new List<YjkEntityInfo>();
                List<YjkEntityInfo> endLink = new List<YjkEntityInfo>();
                for (int j = i; j < modelBeamSegs.Count; j++)
                {
                    splitBeams.Add(modelBeamSegs[j]);
                    bool endIsPrimary = false;
                    if (j == modelBeamSegs.Count - 1)
                    {
                        i = j;
                        endLink = beamLink.End;
                    }
                    else
                    {
                        ModelGrid currentBeamGrid = yjkGridQuery.GetModelGrid(modelBeamSegs[j].GridID);
                        endLink = GetEndPortLinks(modelBeamSegs[j], currentBeamGrid.Jt2ID, out endIsPrimary);
                        if(endIsPrimary || JudgeBeamPortLinkedBeamOnMainBeam(endLink))
                        {
                            //如果遇到柱子和，就停止
                            i = j;
                            break;
                        }
                    }
                }
                subBeamLink.Start = startLink;
                subBeamLink.End = endLink;
                subBeamLink.Status = beamLink.Status;
                subBeamLink.Beams = splitBeams;
                splitSpanBeamLinks.Add(subBeamLink);
            }
            return splitSpanBeamLinks;
        }
        private bool JudgeBeamPortLinkedBeamOnMainBeam(List<YjkEntityInfo> linkedBeams)
        {
            foreach (var beam in linkedBeams)
            {
                if(beam is ModelBeamSeg modelBeamSeg)
                {
                    foreach(var mainBeamLink in this.mainBeamLinks)
                    {
                        var findBeams = mainBeamLink.Beams.Where(o => o is ModelBeamSeg beamSeg && beamSeg.ID == modelBeamSeg.ID);
                        if (findBeams.Count() > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public List<YjkEntityInfo> GetEndPortLinks(ModelBeamSeg modelBeamSeg, int jt, out bool isPrimary)
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
        private List<YjkEntityInfo> GetBeamPortLinkColumnOrWall(ModelBeamSeg modelBeamSeg, int jt)
        {
            List<YjkEntityInfo> yjkEntities = new List<YjkEntityInfo>();
            List<ModelColumnSeg> linkedColumns = GetBeamLinkedColumns(modelBeamSeg.StdFlrID, jt);
            if (linkedColumns.Count > 0)
            {
                return linkedColumns.Cast<YjkEntityInfo>().ToList();
            }
            List<ModelWallSeg> linkedWall = GetBeamLinkedWall(modelBeamSeg, jt);
            if (linkedWall.Count > 0)
            {
                return linkedWall.Cast<YjkEntityInfo>().ToList();
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
            return new YjkWallQuery(this.dtlModelPath).GetBeamLinkedWall(modelBeamSeg, jt);
        }
        private List<ModelBeamSeg> GetBeamLinkedBeam(ModelBeamSeg beamSeg, int jtID, bool isCollinear)
        {
            List<ModelBeamSeg> linkBeamSegs = new YjkBeamQuery(this.dtlModelPath).GetBeamLinkBeams(beamSeg, jtID);
            for (int i = 0; i < linkBeamSegs.Count; i++)
            {
                bool res = (beamSeg as ModelLineBeamSeg).IsCollinear(linkBeamSegs[i] as ModelLineBeamSeg);
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
            List<YjkEntityInfo> portLinkColumnOrWall = GetBeamPortLinkColumnOrWall(linkBeams[0], jtID);
            if(portLinkColumnOrWall.Count>0)
            {
                return jtID;
            }
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
            List<YjkEntityInfo> portLinkColumnOrWall = GetBeamPortLinkColumnOrWall(linkBeams[linkBeams.Count - 1], jtID);
            if (portLinkColumnOrWall.Count > 0)
            {
                return jtID;
            }
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
                return BackupFindBeam(linkBeams, findJt);
            }
            else if (linkBeamSegs.Count > 1)
            {
                throw new Exception("查找到了" + linkBeamSegs.Count + "个以上共线的梁");
            }
            return jtID;
        }
    }
}
