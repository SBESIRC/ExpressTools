using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Service;

namespace ThColumnInfo.Validate.Model
{
    public abstract class ValidateModel
    {
        /// <summary>
        /// 柱号 KZ
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 子编号 KZ-1
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 抗震等级
        /// </summary>
        public string AntiSeismicGrade { get; set; }
        /// <summary>
        /// 核芯区 (%%132 12@100)
        /// </summary>
        public string JointCorehooping { get; set; }
        /// <summary>
        /// 柱表数据信息
        /// </summary>
        public ColumnDataModel Cdm { get; set; }
        public virtual bool ValidateProperty()
        {
            if(string.IsNullOrEmpty(this.Code))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 是否包含柱子代号(KZ、LZ、ZHZ...)
        /// </summary>
        /// <param name="columnCodes"></param>
        /// <returns></returns>
        protected bool IsContainsCodeSign(List<string> columnCodes)
        {
            bool contains = false;
            var res = columnCodes.Where(o => Code.ToUpper().Contains(o.ToUpper()));
            if (res.Count() > 0)
            {
                contains = true;
            }
            return contains;
        }
        /// <summary>
        /// 是不是非抗震等级
        /// </summary>
        public bool IsNonAntiseismic
        {
            get
            {
                if(!string.IsNullOrEmpty(this.AntiSeismicGrade) &&
                    this.AntiSeismicGrade.Contains("非"))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 强制性
        /// </summary>
        public string Mandatory { get; set; } = "";
        /// <summary>
        /// 核芯区数据
        /// </summary>
        public ColuJointCoreAnalysis ColuJoinCoreData
        {
            get
            {
                if(!string.IsNullOrEmpty(this.JointCorehooping))
                {
                    ColuJointCoreAnalysis coluJointCoreAnalysis = new ColuJointCoreAnalysis(this.JointCorehooping);
                    return coluJointCoreAnalysis;
                }
                else if(this.Cdm!=null)
                {
                    return this.Cdm.ColuJointCore;
                }
                return null;
            }
        }
        /// <summary>
        /// 是否转换柱
        /// </summary>
        /// <returns></returns>
        public bool IsZHZ()
        {
            if(!string.IsNullOrEmpty(this.Code))
            {
                int index = this.Code.ToUpper().IndexOf("ZHZ");
                if(index==0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
