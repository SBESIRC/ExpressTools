using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThColumnInfo.Service;

namespace ThColumnInfo.Validate.Model
{
    public class ColumnDataModel
    {
        private ColumnTableRecordInfo ctri = null;
        public ColumnDataModel(ColumnTableRecordInfo columnTableRecordInfo)
        {
            this.ctri = columnTableRecordInfo;
            Init();
            Calculate();
        }

        private void Init()
        {
            if(this.ctri==null)
            {
                return;
            }
            Code = this.ctri.Code;
            List<double> sizeList = ThConverter.SplitSpec(ctri.Spec);
            if (sizeList.Count == 2)
            {
                B = sizeList[0];
                H = sizeList[1];
            }            
            List<double>  cornerIronSizeList = ThConverter.ReinforcementSpecToList(ctri.AngularReinforcement);
            List<double>  bMiddleIronSizeList = ThConverter.ReinforcementSpecToList(ctri.BEdgeSideMiddleReinforcement);
            List<double>  hMiddleIronSizeList = ThConverter.ReinforcementSpecToList(ctri.HEdgeSideMiddleReinforcement);
            if(cornerIronSizeList.Count==2)
            {
                IntCBarCount = (int)cornerIronSizeList[0];
                IntCBarDia = cornerIronSizeList[1];
            }
            if(bMiddleIronSizeList.Count==2)
            {
                IntXBarCount = (int)bMiddleIronSizeList[0];
                IntXBarDia = bMiddleIronSizeList[1];
            }
            if(hMiddleIronSizeList.Count==2)
            {
                IntYBarCount = (int)hMiddleIronSizeList[0];
                IntYBarDia = hMiddleIronSizeList[1];
            }

            if (!string.IsNullOrEmpty(ctri.HoopReinforcement))
            {
                HoopReinforcementAnalysis hoopReinforcementAnalysis = new HoopReinforcementAnalysis(ctri.HoopReinforcement);
                hoopReinforcementAnalysis.Analysis();
                IntStirrupDia = hoopReinforcementAnalysis.IntStirrupDia;
                IntStirrupSpacing = hoopReinforcementAnalysis.IntStirrupSpacing;
                IntStirrupSpacing0 = hoopReinforcementAnalysis.IntStirrupSpacing0;
            }
            if (!string.IsNullOrEmpty(ctri.HoopReinforcementTypeNumber))
            {
                int firsBracketIndex = ctri.HoopReinforcementTypeNumber.IndexOf('(');
                if (firsBracketIndex < 0)
                {
                    firsBracketIndex = ctri.HoopReinforcementTypeNumber.IndexOf('（');
                }
                if (firsBracketIndex >= 0)
                {
                    List<int> limbNums = ThColumnInfoUtils.GetDatas(ctri.HoopReinforcementTypeNumber.Substring(firsBracketIndex + 1));
                    if (limbNums.Count == 2)
                    {
                        IntYStirrupCount = limbNums[0]; //m
                        IntXStirrupCount = limbNums[1]; //n
                    }
                }
            }
            if(ctri.Remark.Contains("抗震"))
            {
                this.Antiseismic = ctri.Remark;
            }
        }
        public ColumnTableRecordInfo Ctri
        {
            get
            {
                return this.ctri;
            }
        }
        public string Code { get; set; } = "";
        /// <summary>
        /// 柱子长度
        /// </summary>
        public double B { get; set; }
        /// <summary>
        /// 柱子宽度
        /// </summary>
        public double H { get; set; }
        /// <summary>
        /// 角筋数量
        /// </summary>
        public int IntCBarCount { get; set; }
        /// <summary>
        /// 角筋直径
        /// </summary>
        public double IntCBarDia { get; set; }
        public int IntXBarCount { get; set; }
        public double IntXBarDia { get; set; }
        public int IntYBarCount { get; set; }
        public double IntYBarDia { get; set; }
        /// <summary>
        /// 箍筋直径
        /// </summary>
        public double IntStirrupDia { get; set; }
        /// <summary>
        /// 加密区间距
        /// </summary>
        public double IntStirrupSpacing { get; set; }
        /// <summary>
        /// 非加密区间距
        /// </summary>
        public double IntStirrupSpacing0 { get; set; }
        /// <summary>
        /// X方向肢数
        /// </summary>
        public int IntXStirrupCount { get; set; }
        /// <summary>
        /// X方向肢数
        /// </summary>
        public int IntYStirrupCount { get; set; }
        /// <summary>
        /// 抗震等级
        /// </summary>
        public string Antiseismic { get; set; } = "";

        public ColuJointCoreAnalysis ColuJointCore
        {
            get
            {
                return new ColuJointCoreAnalysis(this.ctri.ColuJoinCore);
            }
        }
        private double intCBarDiaArea = 0.0;
        private double intXBarDiaArea = 0.0;
        private double intYBarDiaArea = 0.0;
        /// <summary>
        /// 计算一些值
        /// </summary>
        private void Calculate()
        {
            if (ctri == null)
            {
                return;
            }
            //以下配筋率
            this.intCBarDiaArea = ThValidate.GetIronSectionArea((int)IntCBarDia);
            this.intXBarDiaArea = ThValidate.GetIronSectionArea((int)IntXBarDia);
            this.intYBarDiaArea = ThValidate.GetIronSectionArea((int)IntYBarDia);
            this.DblAs = IntCBarCount * intCBarDiaArea +
               2 * (IntXBarCount * intXBarDiaArea +
               IntYBarCount * intYBarDiaArea);
            //this.DblP = (dblAs * 10e4) / (B * H); //配筋率
            this.DblP = this.DblAs / (B * H);

            this.DblXAs = (IntCBarCount / 2) * intCBarDiaArea +
                IntXBarCount * intXBarDiaArea;
            this.DblYAs = (IntCBarCount / 2) * intCBarDiaArea +
                IntYBarCount * intYBarDiaArea;

            //单侧配筋率
            //this.DblXP = this.DblXAs * 10e4 / (B * H);
            //this.DblYP = this.DblYAs * 10e4 / (B * H);
            this.DblXP = this.DblXAs / (B * H);
            this.DblYP = this.DblYAs / (B * H);
        }
        public string GetDblAsCalculation()
        {
            string calculation = "dblAs=IntCBarCount[" + IntCBarCount + "] * intCBarDiaArea[" + intCBarDiaArea +
                "] + 2 * (IntXBarCount[" + IntXBarCount + "] *intXBarDiaArea[" + intXBarDiaArea + "] + IntYBarCount[" +
                IntYBarCount + "] *intYBarDiaArea[" + intYBarDiaArea + "]) = " + this.DblAs;
            return calculation;
        }
        public string GetDblXAsCalculation()
        {   
            string calculation="dblXAs=(IntCBarCount[" + IntCBarCount + "] / 2 * intCBarDiaArea[" + intCBarDiaArea +
                "] + IntXBarCount[" + IntXBarCount + "] *intXBarDiaArea[" + intXBarDiaArea + "] = " + this.DblXAs;
            return calculation;
        }
        public string GetDblYAsCalculation()
        {
            string calculation = "dblYAs=(IntCBarCount[" + IntCBarCount + "] / 2 * intCBarDiaArea[" + intCBarDiaArea +
                "] + IntYBarCount[" + IntYBarCount + "] *intYBarDiaArea[" + intYBarDiaArea + "] = "+this.DblYAs;
            return calculation;
        }
        public string GetDblpCalculation()
        {
            string calculation = "dblP=dblAs[" + this.DblAs + "] / (B[" + this.B + "] * H[" + this.H + "]) = " + this.DblAs / (B * H);
            return calculation;
        }
        public string GetDblxpCalculation()
        {
            string calculation = "dblXP=dblXAs[" + this.DblXAs + "] / (B[" + this.B + "] * H[" + this.H + "]) = " +this.DblXAs / (B * H);
            return calculation;
        }
        public string GetDblypCalculation()
        {
            string calculation = "dblYP=dblYAs[" + this.DblYAs + "] / (B[" + this.B + "] * H[" + this.H + "]) = " + this.DblYAs / (B * H);
            return calculation;
        }
        /// <summary>
        /// 配筋率
        /// </summary>
        public double DblP { get; set; }
        /// <summary>
        /// X单边配筋率
        /// </summary>
        public double DblXP { get; set; }
        /// <summary>
        /// Y单边配筋率
        /// </summary>
        public double DblYP { get; set; }

        public double DblXAs { get; set; }
        public double DblYAs { get; set; }
        public double DblAs { get; set; }
        public double IntStirrupDiaArea
        {
            get
            {
                return ThValidate.GetIronSectionArea((int)IntStirrupDia);
            }
        }
        /// <summary>
        /// 获取体积配箍率
        /// </summary>
        /// <param name="cover">保护层厚度</param>
        /// <returns></returns>
        public double GetVolumeStirrupRatio(double cover)
        { 
            double intStirrupDiaArea = this.IntStirrupDiaArea;
            double value1 = IntXStirrupCount * intStirrupDiaArea * (B - 2 * cover - IntStirrupDia);
            double value2 = IntYStirrupCount * intStirrupDiaArea * (H - 2 * cover - IntStirrupDia);
            double value3 = (B - 2 * cover - 2 * IntStirrupDia)  * (H - 2 * cover - 2 * IntStirrupDia) * IntStirrupSpacing;
            return (value1 + value2) / value3;
        }
        /// <summary>
        /// 获取体积配箍率
        /// </summary>
        /// <param name="cover">保护层厚度</param>
        /// <returns></returns>
        public double GetCoreVolumeStirrupRatio(ColuJointCoreAnalysis coluJointCore, double cover)
        {
            double intStirrupDiaArea = ThValidate.GetIronSectionArea((int)coluJointCore.Diameter);
            double value1 = IntXStirrupCount * intStirrupDiaArea * (B - 2 * cover - coluJointCore.Diameter);
            double value2 = IntYStirrupCount * intStirrupDiaArea * (H - 2 * cover - coluJointCore.Diameter);
            double value3 = (B - 2 * cover - 2 * coluJointCore.Diameter) * 
                (H - 2 * cover - 2 * coluJointCore.Diameter) * coluJointCore.Spacing;
            return (value1 + value2) / value3;
        }
        /// <summary>
        /// 获取核芯区配筋面积
        /// </summary>
        /// <returns></returns>
        public double GetCoreReinforcementArea(ColuJointCoreAnalysis coluJointCore)
        {
            //Todo 要和产品沟通具体计算公式
            double coreReinArea = 0.0;
            double diaArea = ThValidate.GetIronSectionArea((int)coluJointCore.Diameter);
            coreReinArea = IntCBarCount * diaArea + 2 * (IntXBarCount * diaArea +
               IntYBarCount * diaArea);
            return coreReinArea;
        }
        /// <summary>
        /// 获取体积配箍率计算过程
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>
        public string GetVolumeStirrupRatioCalculation(double cover)
        {
            string calculation = "体积配箍率= (intXStirrupCount[" + IntXStirrupCount +
                "]  *  intStirrupDiaArea[" + IntStirrupDiaArea + "] * (B[" + B + "] - 2 * cover[" + 
                cover + "] - intStirrupDia[" + IntStirrupDia + "]) + intYStirrupCount[" + IntYStirrupCount +
                "] * intStirrupDiaArea[" + IntStirrupDiaArea + "] * (H[" + H + "] - 2 * cover[" + cover + 
                "] - intStirrupDia[" + IntStirrupDia + "])) / ((B[" + B + "] - 2 * cover[" + cover +
                "] - 2 * IntStirrupDia[" + IntStirrupDia + "]) * " + "(H[" + H + "] - 2 * cover[" + cover +
                "] - 2 * IntStirrupDia[" + IntStirrupDia + "]) *" + "intStirrupSpacing[" + IntStirrupSpacing + "]) = ";
            return calculation;
        }
        /// <summary>
        /// 获取节点核芯区体积配箍率
        /// </summary>
        /// <param name="coluJointCore"></param>
        /// <param name="cover"></param>
        /// <returns></returns>
        public string GetCoreVolumeStirrupRatioCalculation(ColuJointCoreAnalysis coluJointCore, double cover)
        {
            double intStirrupDiaArea = ThValidate.GetIronSectionArea((int)coluJointCore.Diameter);
            double dia = coluJointCore.Diameter;
            string calculation = "节点核芯区体积配箍率= (intXStirrupCount[" + IntXStirrupCount +
                "]  *  intStirrupDiaArea[" + intStirrupDiaArea + "] * (B[" + B + "] - 2 * cover[" +
                cover + "] - intStirrupDia[" + dia + "]) + intYStirrupCount[" + IntYStirrupCount +
                "] * intStirrupDiaArea[" + intStirrupDiaArea + "] * (H[" + H + "] - 2 * cover[" + cover +
                "] - intStirrupDia[" + dia + "])) / ((B[" + B + "] - 2 * cover[" + cover +
                "] - 2 * IntStirrupDia[" + dia + "]) * " + "(H[" + H + "] - 2 * cover[" + cover +
                "] - 2 * IntStirrupDia[" + dia + "]) *" + "intStirrupSpacing[" + coluJointCore.Spacing + "]) = ";
            return calculation;
        }
        /// <summary>
        /// 获取节点核心区配筋面积
        /// </summary>
        /// <param name="coluJointCore"></param>
        /// <returns></returns>
        public string GetCoreReinAreaCalculation(ColuJointCoreAnalysis coluJointCore)
        {
            //ToDo,根据修改公式再调整
            double diaArea = ThValidate.GetIronSectionArea((int)coluJointCore.Diameter);
            double dia = coluJointCore.Diameter;
            string calculation = "节点核芯区配筋面积=IntCBarCount[" + IntCBarCount + "] * intCBarDiaArea[" + intCBarDiaArea +
                "] + 2 * (IntXBarCount[" + IntXBarCount + "] *intXBarDiaArea[" + intXBarDiaArea + "] + IntYBarCount[" +
                IntYBarCount + "] *intYBarDiaArea[" + intYBarDiaArea + "]) = " + this.DblAs;
            return calculation;
        }
    }
}
