﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                return "G" + Math.Round(this.Asv, keepPointNum)  + " - " +
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
            }
        }
    }
}
