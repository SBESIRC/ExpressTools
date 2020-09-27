using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo.Service
{
    /// <summary>
    /// %%132 10@100/250
    /// 表示箍筋为HPB300级钢筋，直径 10
    /// 加密区间距为100 非加密区间距为250
    /// </summary>
    public class HoopReinforcementAnalysis
    {  
        private string hoopReinforcement = "";
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
        public HoopReinforcementAnalysis(string hoopReinforcement)
        {
            this.hoopReinforcement = hoopReinforcement;
        }
        public HoopReinforcementAnalysis()
        {
        }
        public void Analysis()
        {
            if(string.IsNullOrEmpty(hoopReinforcement))
            {
                return;
            }
            if(!new ColumnTableRecordInfo().ValidateHoopReinforcement(hoopReinforcement))
            {
                return;
            }
            List<double> stirupDatas = ThColumnInfoUtils.GetDoubleValues(hoopReinforcement);
            if (stirupDatas.Count > 0)
            {
                IntStirrupDia = stirupDatas[0];
                if (stirupDatas.Count > 1)
                {
                    IntStirrupSpacing = stirupDatas[1];
                }
                if (stirupDatas.Count > 2)
                {
                    IntStirrupSpacing0 = stirupDatas[2];
                }
                else
                {
                    IntStirrupSpacing0 = IntStirrupSpacing;
                }
            }
        }
    }
}
