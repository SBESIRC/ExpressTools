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
        private CalcColumnSeg start;
        private List<CalcColumnSeg> calcColumnSegs;
        private List<CalcBeamSeg> calcBeamSegs;
        public BuildBeamLink(CalcColumnSeg start,List<CalcColumnSeg> calcColumnSegs,List<CalcBeamSeg> calcBeamSegs)
        {
            this.start = start;
            this.calcColumnSegs = calcColumnSegs;
            this.calcBeamSegs = calcBeamSegs;
        }

        public List<BeamLink> BeamLinks => this.beamLinks;

        public void Find()
        {
            List<CalcBeamSeg> linkBeamSegs = this.calcBeamSegs.Where(
                i => i.Jt1 == this.start.Jt1 || i.Jt2 == this.start.Jt1).Select(i => i).ToList();
            foreach (CalcBeamSeg calcBeamSeg in linkBeamSegs)
            {
                BeamLink beamLink = new BeamLink();
                beamLink.Start = new List<YjkEntityInfo> { this.start };
                List<CalcBeamSeg> linkBeams = new List<CalcBeamSeg> { calcBeamSeg };
                int jt = calcBeamSeg.Jt2;
                if (calcBeamSeg.Jt2 == this.start.Jt1)
                {
                    jt = calcBeamSeg.Jt1;
                }
                int lastFindJt=FindBeam(linkBeams, jt);
                beamLink.Beams = linkBeams;
                beamLink.IsPrimary = true;
                List<CalcColumnSeg> linkColumns = this.calcColumnSegs.Where(i => i.Jt1 == linkBeams[linkBeams.Count - 1].Jt1 ||
                i.Jt1 == linkBeams[linkBeams.Count - 1].Jt2).Select(i => i).ToList();
                if(linkColumns!=null && linkColumns.Count==1)
                {
                    //末端连接是柱子
                    beamLink.End = linkColumns.Cast<YjkEntityInfo>().ToList();
                }   
                if (beamLink.End.Count==0)
                {
                    List<CalcWallBeam> linkWallBeams = new YjkWallBeamQuery(MergeFloorBeams.dtlCalcPath)
                        .GetBeamLinkWalls(linkBeams[linkBeams.Count - 1]);
                    if (linkWallBeams.Count > 0)
                    {
                        //末端连接是墙
                        beamLink.End = linkWallBeams.Cast<YjkEntityInfo>().ToList();
                    }
                }
                if(beamLink.End.Count == 0)
                {
                    List<CalcWallCol> linkWallCols = new YjkWallColQuery(MergeFloorBeams.dtlCalcPath)
                        .GetBeamLinkWalls(linkBeams[linkBeams.Count - 1]);
                    if (linkWallCols.Count > 0)
                    {
                        //末端连接是墙
                        beamLink.End = linkWallCols.Cast<YjkEntityInfo>().ToList();
                    }
                }
                if(beamLink.End.Count == 0)
                {
                    List<CalcBeamSeg> linkVertialBeams = GetBeamLinkedVertialBeam(linkBeams, lastFindJt);
                    beamLink.End = linkVertialBeams.Cast<YjkEntityInfo>().ToList();
                }

                this.beamLinks.Add(beamLink);
            }
        }
        private List<CalcBeamSeg> GetBeamLinkedVertialBeam(List<CalcBeamSeg> linkedBeams,int jtID)
        {
            List<CalcBeamSeg> linkBeamSegs = this.calcBeamSegs.Where(
               i => i.Jt1 == jtID || i.Jt2 == jtID).Select(i => i).ToList();
            for (int i = 0; i < linkBeamSegs.Count; i++)
            {
                int count = linkedBeams.Where(j => j.ID == linkBeamSegs[i].ID).Select(j => j).Count();
                if (count > 0)
                {
                    linkBeamSegs.RemoveAt(i);
                    i = i - 1;
                }
                else
                {
                    bool isCollinaear = linkedBeams[linkedBeams.Count-1].IsCollinear(linkBeamSegs[i]);
                    if (isCollinaear)
                    {
                        linkBeamSegs.RemoveAt(i);
                        i = i - 1;
                    }
                }
            }
            return linkBeamSegs;
        }
        /// <summary>
        /// 获取梁链条最后一个连接的物体
        /// </summary>
        /// <param name="linkedBeams">梁链条</param>
        /// <param name="jt">梁链条最后一个物体要查找的jtID</param>
        /// <returns></returns>
        private List<CalcBeamSeg> GetBeamLinkedBeam(List<CalcBeamSeg> linkedBeams,int jt)
        {
            List<CalcBeamSeg> linkBeamSegs = this.calcBeamSegs.Where(
               i => i.Jt1 == jt || i.Jt2 == jt ).Select(i => i).ToList();
            for (int i = 0; i < linkBeamSegs.Count; i++)
            {
                int count = linkedBeams.Where(j => j.ID == linkBeamSegs[i].ID).Select(j => j).Count();
                if (count > 0)
                {
                    linkBeamSegs.RemoveAt(i);
                    i = i - 1;
                }
                else
                {
                    bool isCollinaear = linkedBeams[linkedBeams.Count-1].IsCollinear(linkBeamSegs[i]);
                    if (!isCollinaear)
                    {
                        linkBeamSegs.RemoveAt(i);
                        i = i - 1;
                    }
                }
            }
            return linkBeamSegs;
        }
        private int FindBeam(List<CalcBeamSeg> linkBeams,int jtID)
        {
            List<CalcBeamSeg> linkBeamSegs = GetBeamLinkedBeam(linkBeams, jtID);
            if (linkBeamSegs.Count==1)
            {
                linkBeams.Add(linkBeamSegs[0]);
                int findJt = linkBeamSegs[0].Jt2;
                if (linkBeamSegs[0].Jt2== jtID)
                {
                    findJt = linkBeamSegs[0].Jt1;
                }
                FindBeam(linkBeams, findJt);
            }
            else if(linkBeamSegs.Count > 1)
            {
                throw new Exception("查找到了"+ linkBeamSegs.Count + "个以上共线的梁");
            }
            return jtID;
        }
    }
    /// <summary>
    /// 从柱子出发寻找主梁
    /// </summary>
    public class BuildColumnBeamLink : IBeamLink
    {
        //主梁一定有一端是连在柱子上
        private List<BeamLink> beamLinks = new List<BeamLink>();
        private CalcColumnSeg start;
        private List<CalcColumnSeg> calcColumnSegs;
        private List<CalcBeamSeg> calcBeamSegs;
        public BuildColumnBeamLink(CalcColumnSeg start, List<CalcColumnSeg> calcColumnSegs, List<CalcBeamSeg> calcBeamSegs)
        {
            this.start = start;
            this.calcColumnSegs = calcColumnSegs;
            this.calcBeamSegs = calcBeamSegs;
        }

        public List<BeamLink> BeamLinks => this.beamLinks;

        public void Find()
        {
            List<CalcBeamSeg> linkBeamSegs = this.calcBeamSegs.Where(
                i => i.Jt1 == this.start.Jt1 || i.Jt2 == this.start.Jt1).Select(i => i).ToList();
            foreach (CalcBeamSeg calcBeamSeg in linkBeamSegs)
            {
                BeamLink beamLink = new BeamLink();
                beamLink.Start = new List<YjkEntityInfo> { this.start };
                List<CalcBeamSeg> linkBeams = new List<CalcBeamSeg> { calcBeamSeg };
                int jt = calcBeamSeg.Jt2;
                if (calcBeamSeg.Jt2 == this.start.Jt1)
                {
                    jt = calcBeamSeg.Jt1;
                }
                int lastFindJt = FindBeam(linkBeams, jt);
                beamLink.Beams = linkBeams;
                beamLink.IsPrimary = true;
                List<CalcColumnSeg> linkColumns = this.calcColumnSegs.Where(i => i.Jt1 == linkBeams[linkBeams.Count - 1].Jt1 ||
                i.Jt1 == linkBeams[linkBeams.Count - 1].Jt2).Select(i => i).ToList();
                if (linkColumns != null && linkColumns.Count == 1)
                {
                    //末端连接是柱子
                    beamLink.End = linkColumns.Cast<YjkEntityInfo>().ToList();
                }
                if (beamLink.End.Count == 0)
                {
                    List<CalcWallBeam> linkWallBeams = new YjkWallBeamQuery(MergeFloorBeams.dtlCalcPath)
                        .GetBeamLinkWalls(linkBeams[linkBeams.Count - 1]);
                    if (linkWallBeams.Count > 0)
                    {
                        //末端连接是墙
                        beamLink.End = linkWallBeams.Cast<YjkEntityInfo>().ToList();
                    }
                }
                if (beamLink.End.Count == 0)
                {
                    List<CalcWallCol> linkWallCols = new YjkWallColQuery(MergeFloorBeams.dtlCalcPath)
                        .GetBeamLinkWalls(linkBeams[linkBeams.Count - 1]);
                    if (linkWallCols.Count > 0)
                    {
                        //末端连接是墙
                        beamLink.End = linkWallCols.Cast<YjkEntityInfo>().ToList();
                    }
                }
                if (beamLink.End.Count == 0)
                {
                    List<CalcBeamSeg> linkVertialBeams = GetBeamLinkedVertialBeam(linkBeams, lastFindJt);
                    beamLink.End = linkVertialBeams.Cast<YjkEntityInfo>().ToList();
                }

                this.beamLinks.Add(beamLink);
            }
        }
        private List<CalcBeamSeg> GetBeamLinkedVertialBeam(List<CalcBeamSeg> linkedBeams, int jtID)
        {
            List<CalcBeamSeg> linkBeamSegs = this.calcBeamSegs.Where(
               i => i.Jt1 == jtID || i.Jt2 == jtID).Select(i => i).ToList();
            for (int i = 0; i < linkBeamSegs.Count; i++)
            {
                int count = linkedBeams.Where(j => j.ID == linkBeamSegs[i].ID).Select(j => j).Count();
                if (count > 0)
                {
                    linkBeamSegs.RemoveAt(i);
                    i = i - 1;
                }
                else
                {
                    bool isCollinaear = linkedBeams[linkedBeams.Count - 1].IsCollinear(linkBeamSegs[i]);
                    if (isCollinaear)
                    {
                        linkBeamSegs.RemoveAt(i);
                        i = i - 1;
                    }
                }
            }
            return linkBeamSegs;
        }
        /// <summary>
        /// 获取梁链条最后一个连接的物体
        /// </summary>
        /// <param name="linkedBeams">梁链条</param>
        /// <param name="jt">梁链条最后一个物体要查找的jtID</param>
        /// <returns></returns>
        private List<CalcBeamSeg> GetBeamLinkedBeam(List<CalcBeamSeg> linkedBeams, int jt)
        {
            List<CalcBeamSeg> linkBeamSegs = this.calcBeamSegs.Where(
               i => i.Jt1 == jt || i.Jt2 == jt).Select(i => i).ToList();
            for (int i = 0; i < linkBeamSegs.Count; i++)
            {
                int count = linkedBeams.Where(j => j.ID == linkBeamSegs[i].ID).Select(j => j).Count();
                if (count > 0)
                {
                    linkBeamSegs.RemoveAt(i);
                    i = i - 1;
                }
                else
                {
                    bool isCollinaear = linkedBeams[linkedBeams.Count - 1].IsCollinear(linkBeamSegs[i]);
                    if (!isCollinaear)
                    {
                        linkBeamSegs.RemoveAt(i);
                        i = i - 1;
                    }
                }
            }
            return linkBeamSegs;
        }
        private int FindBeam(List<CalcBeamSeg> linkBeams, int jtID)
        {
            List<CalcBeamSeg> linkBeamSegs = GetBeamLinkedBeam(linkBeams, jtID);
            if (linkBeamSegs.Count == 1)
            {
                linkBeams.Add(linkBeamSegs[0]);
                int findJt = linkBeamSegs[0].Jt2;
                if (linkBeamSegs[0].Jt2 == jtID)
                {
                    findJt = linkBeamSegs[0].Jt1;
                }
                FindBeam(linkBeams, findJt);
            }
            else if (linkBeamSegs.Count > 1)
            {
                throw new Exception("查找到了" + linkBeamSegs.Count + "个以上共线的梁");
            }
            return jtID;
        }
    }
}
