using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Model;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Service
{
    /// <summary>
    /// 创建一段梁
    /// </summary>
    public class BuildBeamLink
    {
        //主梁一定有一端是连在柱子上
        private BeamLink beamLink=new BeamLink();
        private string dtlModelPath;
        private YjkGridQuery yjkGridQuery;
        private List<ModelBeamSeg> modelBeamSegs = new List<ModelBeamSeg>();

        public BuildBeamLink(List<YjkEntityInfo> yjkEntities)
        {
            if(yjkEntities.Count>0)
            {
                this.dtlModelPath = yjkEntities[0].DbPath;
                yjkGridQuery = new YjkGridQuery(this.dtlModelPath);
            }
            this.modelBeamSegs = yjkEntities.Cast<ModelBeamSeg>().ToList();
        }
        public BeamLink BL => this.beamLink;
        public void Build()
        {
            if(modelBeamSegs.Count==0)
            {
                return;
            }
            this.beamLink = new BeamLink();
            List<ModelBeamSeg> linkBeams = new List<ModelBeamSeg> { this.modelBeamSegs[0] };
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
            bool allExits = false;
            if(beamLink.Beams.Count== this.modelBeamSegs.Count)
            {
                foreach(var item in this.modelBeamSegs)
                {
                    var findRes = beamLink.Beams.Where(i => i.ID == item.ID).Select(i => i);
                    if (findRes.Count() > 0)
                    {
                        allExits=true;
                    }
                    else
                    {
                        allExits = false;
                        break;
                    }
                }
            }
            if(!allExits)
            {
                this.beamLink.Beams = new List<YjkEntityInfo>();
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
