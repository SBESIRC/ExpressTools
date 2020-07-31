using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Service
{
    /// <summary>
    /// %%132 12@100
    /// 表示框架节点核芯区箍筋为HPB300级钢筋，直径 12，间距 100
    /// </summary>
    public class ColuJointCoreAnalysis
    {
        private string coluJointCore = "";
        private double diameter = 0.0;
        private double spacing = 0.0;
        /// <summary>
        /// 直径 10
        /// </summary>
        public double Diameter => diameter;
        /// <summary>
        /// 间距 100
        /// </summary>
        public double Spacing => spacing;
        public ColuJointCoreAnalysis(string coluJointCore)
        {
            this.coluJointCore = coluJointCore;
            Analysis();
        }
        private void Analysis()
        {
            if(string.IsNullOrEmpty(this.coluJointCore))
            {
                return;
            }
            if (!new ColumnTableRecordInfo().ValidateHoopReinforcement(this.coluJointCore))
            {
                return;
            }
            Handle();
            List<double> datas = ThColumnInfoUtils.GetDoubleValues(this.coluJointCore);
            if (datas.Count == 2)
            {
                diameter = datas[0];
                spacing =  datas[1];
            }
        }
        private void Handle()
        {
            if(this.coluJointCore.IndexOf("(")>=0)
            {
                this.coluJointCore=this.coluJointCore.Replace("(", "");
            }
            if (this.coluJointCore.IndexOf(")") >= 0)
            {
                this.coluJointCore = this.coluJointCore.Replace(")", "");
            }
            if (this.coluJointCore.IndexOf("（") >= 0)
            {
                this.coluJointCore = this.coluJointCore.Replace("（", "");
            }
            if (this.coluJointCore.IndexOf("）") >= 0)
            {
                this.coluJointCore = this.coluJointCore.Replace("）", "");
            }
            this.coluJointCore = this.coluJointCore.Trim();
        }
    }
}
