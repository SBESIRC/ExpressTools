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
        private string dtlCalcPath = "";
        private BeamCalculationIndex beamCalIndex;
        public BeamCalculationIndex BeamCalIndex => beamCalIndex;
        public void GenerateBeamCalculationIndex 
            (string dtlCalcPath)
        {
            this.beamCalIndex = new BeamCalculationIndex();
            this.dtlCalcPath = dtlCalcPath;
            List<BeamCalculationIndex> beamCalIndexes = new List<BeamCalculationIndex>();
            this.Beams.ForEach(i => beamCalIndexes.Add(new BeamCalculationIndex(i, dtlCalcPath)));
            if(beamCalIndexes.Count==1)
            {
                this.beamCalIndex = beamCalIndexes[0];
            }
            else
            {
                foreach(var calculateIndex in beamCalIndexes)
                {
                    if(calculateIndex.Asv> this.beamCalIndex.Asv)
                    {
                        this.beamCalIndex.Asv = calculateIndex.Asv;
                    }
                    if(calculateIndex.LeftAsu> this.beamCalIndex.LeftAsu)
                    {
                        this.beamCalIndex.LeftAsu = calculateIndex.LeftAsu;
                    }
                    if(calculateIndex.RightAsu> this.beamCalIndex.RightAsu)
                    {
                        this.beamCalIndex.RightAsu = calculateIndex.RightAsu;
                    }
                    if(calculateIndex.Ast1> this.beamCalIndex.Ast1)
                    {
                        this.beamCalIndex.Ast1 = calculateIndex.Ast1;
                    }
                    if (calculateIndex.Ast > this.beamCalIndex.Ast)
                    {
                        this.beamCalIndex.Ast = calculateIndex.Ast;
                    }
                    if (calculateIndex.Asd > this.beamCalIndex.Asd)
                    {
                        this.beamCalIndex.Asd = calculateIndex.Asd;
                    }
                }
                this.beamCalIndex.Spec = beamCalIndexes[0].Spec;
            }
            this.beamCalIndex.Asv0= CalculateAsv0();
        }
        /// <summary>
        /// 计算梁段非加密区数值
        /// </summary>
        private double CalculateAsv0()
        {
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
                asv0Calculation = new LineBeamRecColumnAsv0(this.Beams.Cast<ModelBeamSeg>().ToList(), this.Start[0] as ModelRecColumnSeg, true, this.dtlCalcPath);
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
                asv0Calculation = new LineBeamLineWallAsv0(this.Beams.Cast<ModelBeamSeg>().ToList(), currentWallSeg as ModelLineWallSeg, true, this.dtlCalcPath);
            }
            if(asv0Calculation!=null)
            {
                asv0Calculation.Calculate();
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
                asv0Calculation = new LineBeamRecColumnAsv0(this.Beams.Cast<ModelBeamSeg>().ToList(), modelColumnSeg as ModelRecColumnSeg,false, this.dtlCalcPath);
            }
            else if (this.End[0] is ModelWallSeg modelWallSeg)
            {
                ModelWallSeg currentWallSeg = modelWallSeg;
                if(this.End.Count>1)
                {
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
                }
                asv0Calculation = new LineBeamLineWallAsv0(this.Beams.Cast<ModelBeamSeg>().ToList(), currentWallSeg as ModelLineWallSeg,false, this.dtlCalcPath);
            }
            if (asv0Calculation != null)
            {
                asv0Calculation.Calculate();
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
