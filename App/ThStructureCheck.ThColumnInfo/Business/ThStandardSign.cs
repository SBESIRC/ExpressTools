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
using TianHua.AutoCAD.Utility.ExtensionTools;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace ThColumnInfo
{
    public class ThStandardSign:ICloneable
    {
        private BlockReference br;
        private string pattern = @"(A){1}\d{1}(L)?(\d)?";
        private string innerFrameName = "";
        private bool isStandardThSign = false;
        private string propertyName = "内框名称";
        private string documentName = "";
        private string docFullPath = "";
        public string DocName
        {
            get
            {
                return documentName;
            }
        }

        public string DocPath
        {
            get
            {
                return docFullPath;
            }
        }
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
        private List<ColumnTableRecordInfo> columnTableRecordInfos { get; set; } = new List<ColumnTableRecordInfo>();
        public List<ColumnTableRecordInfo> ColumnTableRecordInfos { get => columnTableRecordInfos; }
        public bool IsValid { get; set; } = false;
        public ThStandardSign(BlockReference br,string documentName="",string docFullPath = "")
        {
            this.br = br;
            this.documentName = documentName;
            this.docFullPath = docFullPath;
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
                if (columnInfs.Count>0)
                {
                    List<string> correctCodes = columnInfs.Select(i => i.Text).ToList();
                    columnRelateInfs = columnRelateInfs.Where(i => i.ModelColumnInfs.Count == 1 &&
                    correctCodes.IndexOf(i.ModelColumnInfs[0].Text) >= 0).Select(i => i).ToList();
                }
                ThCalculationValidate tcv = new ThCalculationValidate(this.SignExtractColumnInfo, columnRelateInfs, this.SignPlantCalData.CalInfo);
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
        public void ExtractColumnTableData()
        {
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            ViewTableRecord view = doc.Editor.GetCurrentView();
            try
            {
                Point3d leftDownPt = Point3d.Origin;
                Point3d rightUpPt = Point3d.Origin;
                var pt1Res =  doc.Editor.GetPoint("\n请选择柱表的左下角点");
                if (pt1Res.Status == PromptStatus.OK)
                {
                    leftDownPt = pt1Res.Value;
                }
                else
                {
                    return;
                }
                PromptCornerOptions pco = new PromptCornerOptions("\n请选择柱表的右上角点", leftDownPt);
                //pco.UseDashedLine = true;
                var pt2Res = doc.Editor.GetCorner(pco);
                if (pt2Res.Status == PromptStatus.OK)
                {
                    rightUpPt = pt2Res.Value;
                }
                else
                {
                    return;
                }
                ThProgressBar.Start("提取柱表......");
                using (Transaction trans = doc.TransactionManager.StartTransaction())
                {
                    ParameterSetVM parameterSetVM = new ParameterSetVM();
                    COMTool.ZoomWindow(ThColumnInfoUtils.TransPtFromUcsToWcs(leftDownPt)
                        , ThColumnInfoUtils.TransPtFromUcsToWcs(rightUpPt));
                    //提取柱表
                    ExtractColumnTable extractColumnTable = new ExtractColumnTable(
                        leftDownPt, rightUpPt, parameterSetVM.ParaSetInfo); //如果不是原位图纸，提取一下柱表信息
                    extractColumnTable.Extract();
                    this.columnTableRecordInfos = extractColumnTable.ColumnTableRecordInfos;
                    trans.Commit();
                }
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "ExtractColumnTableData");
            }
            finally
            {
                ThProgressBar.Stop();
                doc.Editor.SetCurrentView(view);
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
