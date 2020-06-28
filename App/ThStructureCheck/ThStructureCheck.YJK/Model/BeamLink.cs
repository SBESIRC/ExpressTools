using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.Common.Service;
using ThStructureCheck.YJK.Query;
using ThStructureCheck.YJK.Service;

namespace ThStructureCheck.YJK.Model
{
    public class BeamLink
    {
        public BeamStatus Status { get; set; }
        public List<YjkEntityInfo> Start { get; set; }
        public List<YjkEntityInfo> End { get; set; }
        public List<YjkEntityInfo> Beams { get; set; }
        ///// <summary>
        ///// 先默认用Yjk数据库连接，后续根据需要判断
        ///// 从左到右
        ///// </summary>
        //private bool Forward
        //{
        //    get
        //    {
        //        return forward;
        //    }
        //}
        //private bool forward = true;
        private string dtlCalcPath = "";
        public BeamCalculationIndex GenerateBeamCalculationIndex 
            (string dtlCalcPath)
        {
            BeamCalculationIndex beamCalculationIndex = new BeamCalculationIndex();
            this.dtlCalcPath = dtlCalcPath;
            List<BeamCalculationIndex> beamCalIndexes = new List<BeamCalculationIndex>();
            this.Beams.ForEach(i => beamCalIndexes.Add(new BeamCalculationIndex(i, dtlCalcPath)));
            double asv0=CalculateAsv0();
            return beamCalculationIndex;
        }
        /// <summary>
        /// 计算梁段非加密区数值
        /// </summary>
        private double CalculateAsv0()
        {
            double asv0 = 0.0;
            if(this.Status!= BeamStatus.Primary)
            {
                return asv0;
            }
            double startAsv0=CalculateStartAsv0();
            double endAsv0 = CalculateEndAsv0();
            return Math.Max(startAsv0, endAsv0);
        }
        private double CalculateStartAsv0()
        {
            double asv0 = 0.0;
            if(this.Start.Count == 0)
            {
                return asv0;
            }
            //目前都按直梁、直墙、方形柱处理
            ModelBeamSeg beamSeg = this.Beams[0] as ModelBeamSeg;
            Asv0Calculation asv0Calculation=null;
            ModelGrid beamModelGrid = beamSeg.Grid;
            ModelJoint beamStartJoint = new YjkJointQuery(beamSeg.DbPath).GetModelJoint(beamModelGrid.Jt1ID);
            ModelJoint beamEndJoint = new YjkJointQuery(beamSeg.DbPath).GetModelJoint(beamModelGrid.Jt2ID);
            Coordinate beamStartCoord = new Coordinate(beamStartJoint.X, beamStartJoint.Y);
            Coordinate beamEndCoord = new Coordinate(beamEndJoint.X, beamEndJoint.Y);       
            if (this.Start[0] is ModelColumnSeg modelColumnSeg)
            {
                asv0Calculation = new LineBeamRecColumnAsv0(beamSeg, this.Start[0] as ModelColumnSeg,this.dtlCalcPath);
            }
            else if (this.Start[0] is ModelWallSeg modelWallSeg)
            {
                ModelWallSeg currentWallSeg = modelWallSeg;
                foreach (YjkEntityInfo wallEnt in this.Start)
                {
                    if(wallEnt is ModelWallSeg wallSeg)
                    {
                        ModelGrid modelGrid = wallSeg.Grid;
                        ModelJoint startJoint= new YjkJointQuery(wallSeg.DbPath).GetModelJoint(modelGrid.Jt1ID);
                        ModelJoint endJoint = new YjkJointQuery(wallSeg.DbPath).GetModelJoint(modelGrid.Jt2ID);
                        Coordinate wallStartCoord = new Coordinate(startJoint.X, startJoint.Y);
                        Coordinate wallEndCoord = new Coordinate(endJoint.X, endJoint.Y);
                        LineRelation lineRelation = new LineRelation(beamStartCoord, beamEndCoord, wallStartCoord, wallEndCoord);
                        lineRelation.Relation();
                        if(lineRelation.Relationships.IndexOf(Relationship.Perpendicular)>=0 ||
                           lineRelation.Relationships.IndexOf(Relationship.UnRegular) >= 0)
                        {
                            currentWallSeg = wallSeg;
                            break;
                        }
                    }
                }
                asv0Calculation = new LineBeamLineWallAsv0(beamSeg, currentWallSeg, this.dtlCalcPath);
            }
            if(asv0Calculation!=null)
            {
                asv0Calculation.Calculate(this.Beams.Cast<ModelBeamSeg>().ToList());
                asv0 = asv0Calculation.Asv0;
            }
            return asv0;
        }
        private double CalculateEndAsv0()
        {
            double asv0 = 0.0;
            if(this.End.Count ==0)
            {
                return asv0;
            }
            //目前都按直梁、直墙、方形柱处理
            ModelBeamSeg beamSeg = this.Beams[this.Beams.Count-1] as ModelBeamSeg;
            Asv0Calculation asv0Calculation = null;
            ModelGrid beamModelGrid = beamSeg.Grid;
            ModelJoint beamStartJoint = new YjkJointQuery(beamSeg.DbPath).GetModelJoint(beamModelGrid.Jt1ID);
            ModelJoint beamEndJoint = new YjkJointQuery(beamSeg.DbPath).GetModelJoint(beamModelGrid.Jt2ID);
            Coordinate beamStartCoord = new Coordinate(beamStartJoint.X, beamStartJoint.Y);
            Coordinate beamEndCoord = new Coordinate(beamEndJoint.X, beamEndJoint.Y);
            if (this.End[0] is ModelColumnSeg modelColumnSeg)
            {
                asv0Calculation = new LineBeamRecColumnAsv0(beamSeg, modelColumnSeg, this.dtlCalcPath);
            }
            else if (this.End[0] is ModelWallSeg modelWallSeg)
            {
                ModelWallSeg currentWallSeg = modelWallSeg;
                foreach (YjkEntityInfo wallEnt in this.End)
                {
                    if (wallEnt is ModelWallSeg wallSeg)
                    {
                        ModelGrid modelGrid = wallSeg.Grid;
                        ModelJoint startJoint = new YjkJointQuery(wallSeg.DbPath).GetModelJoint(modelGrid.Jt1ID);
                        ModelJoint endJoint = new YjkJointQuery(wallSeg.DbPath).GetModelJoint(modelGrid.Jt2ID);
                        Coordinate wallStartCoord = new Coordinate(startJoint.X, startJoint.Y);
                        Coordinate wallEndCoord = new Coordinate(endJoint.X, endJoint.Y);
                        LineRelation lineRelation = new LineRelation(beamStartCoord, beamEndCoord, wallStartCoord, wallEndCoord);
                        lineRelation.Relation();
                        if (lineRelation.Relationships.IndexOf(Relationship.Perpendicular) >= 0 ||
                           lineRelation.Relationships.IndexOf(Relationship.UnRegular) >= 0)
                        {
                            currentWallSeg = wallSeg;
                            break;
                        }
                    }
                }
                asv0Calculation = new LineBeamLineWallAsv0(beamSeg, currentWallSeg, this.dtlCalcPath);
            }
            if (asv0Calculation != null)
            {
                asv0Calculation.Calculate(this.Beams.Cast<ModelBeamSeg>().ToList());
                asv0 = asv0Calculation.Asv0;
            }
            return asv0;
        }
    }
    public enum BeamStatus
    {
        Primary,
        Secondary,
        Unknown
    }
}
