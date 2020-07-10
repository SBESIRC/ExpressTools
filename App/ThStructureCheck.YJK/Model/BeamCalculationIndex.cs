using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Model;
using ThStructureCheck.Common.Service;
using ThStructureCheck.YJK.Query;
using ThStructureCheck.YJK.Service;

namespace ThStructureCheck.YJK.Model
{
    /// <summary>
    /// 梁计算书指标
    /// </summary>
    public class BeamCalculationIndex
    {
        private int keepPointNum = 1;
        #region---------Property----------
        /// <summary>
        /// 加密区箍筋
        /// </summary>
        public double Asv { get; set; }
        /// <summary>
        /// 非加密区箍筋
        /// </summary>
        public double Asv0 { get; set; }
        /// <summary>
        /// 左侧梁顶纵筋
        /// </summary>
        public double LeftAsu { get; set; }
        /// <summary>
        /// 右侧梁顶纵筋
        /// </summary>
        public double RightAsu { get; set; }
        /// <summary>
        /// 梁侧抗扭箍筋单肢面积
        /// </summary>
        public double Ast1 { get; set; }
        /// <summary>
        /// 梁侧面纵筋
        /// </summary>
        public double Ast { get; set; }
        /// <summary>
        /// 下部通长钢筋
        /// </summary>
        public double Asd { get; set; }
        /// <summary>
        /// 规格
        /// </summary>
        public string Spec { get; set; }
        public string AntiSeismicGrade { get; set; }
        /// <summary>
        /// 获取 左侧梁顶纵筋 - 右侧梁顶纵筋
        /// </summary>
        /// <returns></returns>
        public string AsuFormat
        {
            get
            {
                return Math.Round(LeftAsu, keepPointNum) + "-" +
                    Math.Round(RightAsu, keepPointNum);
            }  
        }
        /// <summary>
        /// 获取 G 加密区箍筋 - 非加密区箍筋
        /// </summary>
        /// <returns></returns>
        public string GFormat
        {
            get
            {
                return "G" + Math.Round(this.Asv, keepPointNum)  + "-" +
                    Math.Round(this.Asv0, keepPointNum);
            }
        }
        /// <summary>
        /// 获取 VT 梁侧面纵筋 - 单肢箍筋面积
        /// </summary>
        public string VtFormat
        {
            get
            {
                return "VT" + Math.Round(this.Ast, keepPointNum) + "-" +
                    Math.Round(this.Ast1, keepPointNum);
            }
        }
        #endregion
        private string dtlCalcPath = "";
        private YjkEntityInfo beam;
        public BeamCalculationIndex(YjkEntityInfo beam, string dtlCalcPath)
        {
            this.beam = beam;
            this.dtlCalcPath = dtlCalcPath;
            Build();
        }
        public BeamCalculationIndex()
        {
        }
        private void Build()
        {
            if (beam is ModelBeamSeg modelBeamSeg)
            {
                YjkBeamQuery modelQuery = new YjkBeamQuery(modelBeamSeg.DbPath);
                YjkBeamQuery calcQuery = new YjkBeamQuery(this.dtlCalcPath);
                int floorNo;
                int beamNo;
                bool findRes = modelQuery.GetDtlmodelTblBeamSegFlrNoAndNo(modelBeamSeg.ID, out floorNo, out beamNo);
                if (!findRes)
                {
                    return;
                }
                int calcBeamID = calcQuery.GetTblBeamSegIDFromDtlCalc(floorNo, beamNo);
                CalcRCBeamDsn calcRCBeamDsn = calcQuery.GetCalcRcBeamDsn(calcBeamID);
                //加密区箍筋
                this.Asv = calcRCBeamDsn.EncryptStirrupAsv;
                //单肢箍筋面积
                this.Ast1 = calcRCBeamDsn.ResistTwistStirrupAst1;
                //梁侧面纵筋
                this.Ast = calcRCBeamDsn.BeamSideLongiReinforceAst;
                //梁截面
                this.Spec = modelBeamSeg.BeamSect.Spec;
                //左侧梁顶纵筋
                this.LeftAsu = calcRCBeamDsn.LeftAsu;
                //右侧梁顶纵筋
                this.RightAsu = calcRCBeamDsn.RightAsu;
                //梁底纵筋Asd (下部通长钢筋)
                this.Asd = calcRCBeamDsn.BeamBottomLongiReinAsd;

                //获取抗震等级(dtlCalc)
                double antiSeismicGradeParaValue = calcQuery.GetAntiSeismicGradeInCalculation(calcBeamID);
                if (antiSeismicGradeParaValue != 0.0)
                {
                    this.AntiSeismicGrade = ThYjkDatbaseExtension.GetAntiSeismicGrade(antiSeismicGradeParaValue);
                }
                else
                {
                    List<double> antiSeismicGradeValues = modelQuery.GetAntiSeismicGradeInModel();
                    this.AntiSeismicGrade = ThYjkDatbaseExtension.GetAntiSeismicGrade(antiSeismicGradeValues[0], 0.0); //目前不修正，打开 0.0->antiSeismicGradeValues[1]
                }
                this.Asv0 = CalculateAsv0();
            }
        }
        /// <summary>
        /// 计算梁段非加密区数值
        /// </summary>
        private double CalculateAsv0()
        {
            double startAsv0 = CalculateStartAsv0();
            double endAsv0 = CalculateEndAsv0();
            return Math.Max(startAsv0, endAsv0);
        }
        private double CalculateStartAsv0()
        {
            double asv0 = 0.0;
            //目前都按直梁、直墙、方形柱处理
            ModelBeamSeg beamSeg = this.beam as ModelBeamSeg;
            Asv0Calculation asv0Calculation = null;
            ModelGrid beamModelGrid = beamSeg.Grid;
            ModelJoint beamStartJoint = new YjkJointQuery(beamSeg.DbPath).GetModelJoint(beamModelGrid.Jt1ID);
            ModelJoint beamEndJoint = new YjkJointQuery(beamSeg.DbPath).GetModelJoint(beamModelGrid.Jt2ID);
            Coordinate beamStartCoord = new Coordinate(beamStartJoint.X, beamStartJoint.Y);
            Coordinate beamEndCoord = new Coordinate(beamEndJoint.X, beamEndJoint.Y);
            bool isPrimary = true;
            BuildFlrBeamLink buildFlrBeamLink = new BuildFlrBeamLink(beamSeg.DbPath);
            List<YjkEntityInfo> startLinks = buildFlrBeamLink.GetEndPortLinks(beamSeg, beamModelGrid.Jt1ID,out isPrimary);
            if(startLinks.Count==0)
            {
                return asv0;
            }
            if (startLinks[0] is ModelColumnSeg modelColumnSeg)
            {
                asv0Calculation = new LineBeamRecColumnAsv0(new List<ModelBeamSeg>() { beamSeg }, startLinks[0] as ModelRecColumnSeg, true, this.dtlCalcPath);
            }
            else if (startLinks[0] is ModelWallSeg modelWallSeg)
            {
                ModelWallSeg currentWallSeg = modelWallSeg;
                foreach (YjkEntityInfo wallEnt in startLinks)
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
                asv0Calculation = new LineBeamLineWallAsv0(new List<ModelBeamSeg>() { beamSeg }, currentWallSeg as ModelLineWallSeg, true, this.dtlCalcPath);
            }
            if (asv0Calculation != null)
            {
                asv0Calculation.Calculate();
                asv0 = asv0Calculation.Asv0;
            }
            return asv0;
        }
        private double CalculateEndAsv0()
        {
            double asv0 = 0.0;
            //目前都按直梁、直墙、方形柱处理
            ModelBeamSeg beamSeg = this.beam as ModelBeamSeg;
            Asv0Calculation asv0Calculation = null;
            ModelGrid beamModelGrid = beamSeg.Grid;
            ModelJoint beamStartJoint = new YjkJointQuery(beamSeg.DbPath).GetModelJoint(beamModelGrid.Jt1ID);
            ModelJoint beamEndJoint = new YjkJointQuery(beamSeg.DbPath).GetModelJoint(beamModelGrid.Jt2ID);
            Coordinate beamStartCoord = new Coordinate(beamStartJoint.X, beamStartJoint.Y);
            Coordinate beamEndCoord = new Coordinate(beamEndJoint.X, beamEndJoint.Y);
            bool isPrimary = true;
            BuildFlrBeamLink buildFlrBeamLink = new BuildFlrBeamLink(beamSeg.DbPath);
            List<YjkEntityInfo> endLinks = buildFlrBeamLink.GetEndPortLinks(beamSeg, beamModelGrid.Jt2ID, out isPrimary);
            if (endLinks.Count == 0)
            {
                return asv0;
            }
            if (endLinks[0] is ModelColumnSeg modelColumnSeg)
            {
                asv0Calculation = new LineBeamRecColumnAsv0(new List<ModelBeamSeg>() { beamSeg }, modelColumnSeg as ModelRecColumnSeg, false, this.dtlCalcPath);
            }
            else if (endLinks[0] is ModelWallSeg modelWallSeg)
            {
                ModelWallSeg currentWallSeg = modelWallSeg;
                foreach (YjkEntityInfo wallEnt in endLinks)
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
                asv0Calculation = new LineBeamLineWallAsv0(new List<ModelBeamSeg>() { beamSeg }, currentWallSeg as ModelLineWallSeg, false, this.dtlCalcPath);
            }
            if (asv0Calculation != null)
            {
                asv0Calculation.Calculate();
                asv0 = asv0Calculation.Asv0;
            }
            return asv0;
        }
    }
}
