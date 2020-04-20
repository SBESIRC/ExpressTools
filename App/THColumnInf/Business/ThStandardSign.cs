using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ThColumnInfo.Validate;
using ThColumnInfo.ViewModel;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThColumnInfo
{
    public class ThStandardSign:ICloneable
    {
        private BlockReference br;
        private string pattern = @"(A){1}\d{1}(L)?(\d)?";
        private string innerFrameName = "";
        private bool isStandardThSign = false;
        private string propertyName = "内框名称";
        public BlockReference Br
        {
            get { return br; }
        }
        public bool IsStandardThSign
        {
            get
            {
                return isStandardThSign;
            }
            private set
            {
                isStandardThSign = value;
            }
        }
        public string InnerFrameName
        {
            get
            {
                return innerFrameName;
            }
            private set
            {
                innerFrameName = value;
            }
        }

        /// <summary>
        /// 当前图框内提取的柱子信息
        /// </summary>
        public ExtractColumnPosition SignExtractColumnInfo{ get; set; }
        /// <summary>
        /// 当前图框埋入的计算书
        /// </summary>
        public PlantCalDataToDraw SignPlantCalData { get; set; }
        /// <summary>
        /// 规范校对
        /// </summary>
        public ThSpecificationValidate ThSpecificValidate { get; set; }
        /// <summary>
        /// 计算书校对
        /// </summary>
        public ThCalculationValidate ThCalculateValidate { get; set; }
        public bool IsValid { get; set; } = false;
        public ThStandardSign(BlockReference br)
        {
            this.br = br;
            CheckBlockReferenceIsThSign();
        }
        private void CheckBlockReferenceIsThSign()
        {
            if(this.br==null || string.IsNullOrEmpty(this.br.Name))
            {
                return;
            }
            GetBlockNameIsStandardSignName();
            GetBlockInnerNameHasValue();
            if(this.isStandardThSign && !string.IsNullOrEmpty(this.innerFrameName))
            {
                this.IsValid = true;
            }
        }
        private void GetBlockNameIsStandardSignName()
        {
            Regex rg = new Regex(this.pattern);
            if(rg.IsMatch(this.br.Name.ToUpper()) && this.br.Name.ToUpper().Contains("THAPE") &&
                this.br.Name.ToUpper().Contains("INNER"))
            {
                this.isStandardThSign = true;
            }
        }
        private void GetBlockInnerNameHasValue()
        {            
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in this.br.AttributeCollection)
                {
                    AttributeReference ar = trans.GetObject(id, OpenMode.ForRead) as AttributeReference;
                    if (ar.Tag == this.propertyName)
                    {
                        if (!string.IsNullOrEmpty(ar.TextString))
                        {
                            this.innerFrameName = ar.TextString;
                            this.innerFrameName.Trim(' ');
                        }
                        break;
                    }
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="onlyValidateSpecification"></param>
        /// <param name="columnInfs">如果有，是从面板数据正确节点传入</param>
        public void Validate(bool onlyValidateSpecification=false,List<ColumnInf> columnInfs=null)
        {
            try
            {
                if (this.SignExtractColumnInfo == null)
                {
                    return;
                }
                //参数设置
                ParameterSetVM psVM = new ParameterSetVM();
                ParameterSetInfo paraSetInfo = psVM.ParaSetInfo;
                //验证规范
                ThSpecificationValidate tsv = new ThSpecificationValidate(this.SignExtractColumnInfo, paraSetInfo,this.innerFrameName);
                tsv.Validate(columnInfs);
                tsv.PrintCalculation();
                ThProgressBar.MeterProgress();
                this.ThSpecificValidate = tsv;

                if(onlyValidateSpecification)
                {
                    return;
                }
               
                //计算书校核
                List<ColumnRelateInf> columnRelateInfs = this.SignPlantCalData.GetColumnRelateInfs(this);//从图纸中获取
                ThCalculationValidate tcv = new ThCalculationValidate(this.SignExtractColumnInfo, columnRelateInfs);
                tcv.Validate();
                tcv.PrintCalculation();
                ThProgressBar.MeterProgress();
                this.ThCalculateValidate = tcv;
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "ThColumnInfoCheckWindow");
            }            
        }
        public void RelateColumnFrameId()
        {
            for (int i=0;i<this.SignExtractColumnInfo.ColumnInfs.Count;i++)
            {
                if(this.SignExtractColumnInfo.ColumnInfs[i].Error!=ErrorMsg.OK)
                {
                    continue;
                }
                this.SignPlantCalData.RelateCalulationColumn(this.SignExtractColumnInfo.ColumnInfs[i]);
                ThProgressBar.MeterProgress();
            }
        }

        public object Clone()
        {
            ThStandardSign thStandardSign = new ThStandardSign(this.br);
            thStandardSign.innerFrameName = this.innerFrameName;
            thStandardSign.isStandardThSign = this.isStandardThSign;
            thStandardSign.SignPlantCalData = null;
            thStandardSign.SignExtractColumnInfo = null;
            thStandardSign.ThCalculateValidate = null;
            thStandardSign.ThSpecificValidate = null;
            return thStandardSign;
        }
    }
    public class InnerFrameNameDesc : IComparer<ThStandardSign>
    {
        public int Compare(ThStandardSign x, ThStandardSign y)
        {
            List<double> yDatas = ThColumnInfoUtils.GetDoubleValues(y.InnerFrameName);
            List<double> xDatas = ThColumnInfoUtils.GetDoubleValues(x.InnerFrameName);
            if(yDatas.Count==0 && xDatas.Count==0)
            {
                return 0;
            }
            else if(yDatas.Count == 0)
            {
                return -1;
            }
            else if(xDatas.Count == 0)
            {
                return 1;
            }
            double yMaxValue =yDatas.OrderByDescending(i => i).First();
            double xMaxValue = xDatas.OrderByDescending(i => i).First();          
            return yMaxValue.CompareTo(xMaxValue);
        }
    }
}
