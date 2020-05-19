
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThColumnInfo.ViewModel;

namespace ThColumnInfo.Validate
{
    public class ThCalculationValidate : ThValidate
    {
        private string filePath = "";
        private Dictionary<ColumnRelateInf, List<string>> columnValidateResultDic
            = new Dictionary<ColumnRelateInf, List<string>>();
        private List<ColumnRelateInf> columns = new List<ColumnRelateInf>();
        private Dictionary<ColumnRelateInf, List<string>> calculationStepDic = new Dictionary<ColumnRelateInf, List<string>>();
        private CalculationInfo calculateInfo;

        /// <summary>
        /// 自然层总数
        /// </summary>
        public static int FloorCount=0;

        /// <summary>
        /// 错误
        /// </summary>
        public Dictionary<ColumnRelateInf, List<string>> ColumnValidateResultDic
        {
            get
            {
                return columnValidateResultDic;
            }
            set
            {
                columnValidateResultDic = value;
            }
        }
        public ThCalculationValidate(IDataSource ds, List<ColumnRelateInf> relatedColumns,CalculationInfo calInf):
            base(ds)
        { 
            this.columns = relatedColumns;
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            FileInfo fileInfo = new FileInfo(doc.Name);
            if (fileInfo.Exists)
            {
                this.filePath = fileInfo.Directory.FullName + "\\" + DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss") + ".txt";
            }
            else
            {
                this.filePath = Environment.CurrentDirectory + "\\" + DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss") + ".txt";
            }
            this.calculateInfo = calInf;
            GetNaturalFloorCount();
        }
        /// <summary>
        /// 获取自然层总层数
        /// </summary>
        private void GetNaturalFloorCount()
        {
            string dtModelPath = this.calculateInfo.GetDtlmodelFullPath();
            if (string.IsNullOrEmpty(dtModelPath))
            {
                return;
            }
            FileInfo fi = new FileInfo(dtModelPath);
            if (!fi.Exists)
            {
                return;
            }
            //获取自然层及对应的标准层
            ExtractYjkColumnInfo extractYjkColumnInfo = new ExtractYjkColumnInfo(dtModelPath);
            FloorCount = extractYjkColumnInfo.GetNaturalFloorCount();
        }
        public void Validate()
        {
            if (columns == null || columns.Count == 0 || dataSource == null)
            {
                return;
            }
            List<ColumnRelateInf> columnRelateInfs= columns.Where(i => i.ModelColumnInfs.Count == 1).Select(i => i).ToList();
            columnRelateInfs=columnRelateInfs.Where(i => ThColumnInfoUtils.GetDatas(
                i.ModelColumnInfs[0].Code).Count > 0).Select(i => i).ToList();
            columnRelateInfs.Sort(new ColumnRelateInfCompare());
            for (int i=0;i< columnRelateInfs.Count;i++)
            {
                if(columns[i].ModelColumnInfs.Count==0)
                {
                    continue;
                }
                CalculationValidate cv = new CalculationValidate(columnRelateInfs[i]);
                cv.ValidateColumnInf();
                List<string> totalStrings = new List<string>();
                totalStrings.AddRange(cv.ErrorResults);
                totalStrings.Add("XXXXXX");
                totalStrings.AddRange(cv.CorrectResults);
                columnValidateResultDic.Add(columnRelateInfs[i], totalStrings);
                calculationStepDic.Add(columnRelateInfs[i], cv.CalculationSteps);
                ThProgressBar.MeterProgress();
            }
        }
        public void PrintCalculation()
        {
            try
            {
                FileStream fs = new FileStream(this.filePath, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                foreach (var item in this.calculationStepDic)
                {
                    if(item.Key.ModelColumnInfs.Count==1)
                    {
                        sw.WriteLine("关联柱号：" + item.Key.ModelColumnInfs[0].Text);
                    }
                    sw.WriteLine("JtID：" + item.Key.DbColumnInf.JtID);
                    sw.WriteLine("FloorID：" + item.Key.DbColumnInf.FloorID);
                    sw.WriteLine("StdFlrID：" + item.Key.DbColumnInf.StdFlrID);
                    sw.WriteLine("坐标：");
                    for (int i = 0; i < item.Key.InModelPts.Count; i++)
                    {
                        sw.WriteLine("      " + item.Key.InModelPts[i].ToString());
                    }
                    item.Value.ForEach(i => sw.WriteLine(i));
                }
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "ThSpecificationValidate.PrintCalculation");
            }
        }
    }
    public class CalculationValidate
    {
        private delegate void GetParameter();
        private ColumnDataModel cdm=null;
        private ColumnRelateInf columnRelateInf;
        private List<IRule> validateRules = new List<IRule>();
        private List<string> errorResults = new List<string>();
        private List<string> correctResults = new List<string>();
        private List<string> calculationSteps = new List<string>();

        /// <summary>
        /// 错误验证结果
        /// </summary>
        public List<string> ErrorResults
        {
            get
            {
                return this.errorResults;
            }
        }
        /// <summary>
        /// 正确验证结果
        /// </summary>
        public List<string> CorrectResults
        {
            get
            {
                return this.correctResults;
            }
        }
        public List<string> CalculationSteps
        {
            get
            {
                return this.calculationSteps;
            }
        }
        public CalculationValidate(ColumnRelateInf columnRelateInf)
        {
            this.columnRelateInf = columnRelateInf;
            var cdmRes = ThValidate.columnDataModels.Where(i => i.Code == columnRelateInf.ModelColumnInfs[0].Code).Select(i => i);
            if(cdmRes!=null && cdmRes.Count()>0)
            {
                cdm = cdmRes.First();
            }
            GetParameter getParameter = GetAntiSeismicGrade; //抗震等级
            getParameter += GetProtectLayerThickness;  //保护层厚度
            getParameter += GetConcreteStrength;      //混凝土强度
            getParameter += GetCornerColumn; //角柱
            getParameter += GetHoopReinforceEnlargeTimes; //箍筋放大倍数
            getParameter += GetLongitudinalReinforceEnlargeTimes; //纵筋放大倍数
            getParameter += GetStructureType; // 结构类型
            getParameter();
        }
        //优先级：构件属性定义->计算书->参数设置
        #region 需要从柱子识别、YJK、参数设置、构件属性定义来确定以下参数的值
        private string antiSeismicGrade = ""; //抗震等级
        private double protectLayerThickness; //保护层厚度
        private string concreteStrength = ""; //混凝土强度
        private bool cornerColumn;  //角柱

        private double hoopReinforceEnlargeTimes; //箍筋放大倍数
        private double longitudinalReinforceEnlargeTimes; //纵筋放大倍数
        private string structureType = ""; //结构类型

        /// <summary>
        /// 箍筋全高度加密
        /// </summary>
        private bool ReinforceFullHeightEncryption {
            get
            {
                if (this.columnRelateInf != null)
                {
                    //构件属性定义
                    if (this.columnRelateInf.CustomData != null)
                    {
                        if (!string.IsNullOrEmpty(this.columnRelateInf.CustomData.HoopReinforceFullHeightEncryption))
                        {
                            if (this.columnRelateInf.CustomData.HoopReinforceFullHeightEncryption == "是")
                            {
                                return true;
                            }
                            else if (this.columnRelateInf.CustomData.HoopReinforceFullHeightEncryption == "否")
                            {
                                return false;
                            }
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 获取抗震等级
        /// </summary>
        /// <returns></returns>
        private void GetAntiSeismicGrade()
        {
            if (this.columnRelateInf != null)
            {
                //构件属性定义
                if (this.columnRelateInf.CustomData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.CustomData.AntiSeismicGrade))
                    {
                        this.antiSeismicGrade = this.columnRelateInf.CustomData.AntiSeismicGrade;
                        return;
                    }
                }
                //YJK
                if (this.columnRelateInf.YjkColumnData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.YjkColumnData.AntiSeismicGrade))
                    {
                        this.antiSeismicGrade = this.columnRelateInf.YjkColumnData.AntiSeismicGrade;
                        return;
                    }
                }
                //柱识别
                if (!string.IsNullOrEmpty(this.columnRelateInf.ModelColumnInfs[0].AntiSeismicGrade))
                {
                    this.antiSeismicGrade = this.columnRelateInf.ModelColumnInfs[0].AntiSeismicGrade;
                    return;
                }
            }
            //参数设置
            this.antiSeismicGrade = ThSpecificationValidate.paraSetInfo.AntiSeismicGrade;
        }
        /// <summary>
        /// 获取保护层厚度
        /// </summary>
        private void GetProtectLayerThickness()
        {
            if (this.columnRelateInf != null)
            {
                //构件属性定义
                if (this.columnRelateInf.CustomData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.CustomData.ProtectLayerThickness))
                    {
                        double value = 0.0;
                        if (double.TryParse(this.columnRelateInf.CustomData.ProtectLayerThickness, out value))
                        {
                            if (value > 0.0)
                            {
                                this.protectLayerThickness = value;
                                return;
                            }
                        }
                    }
                }
                //YJK
                if (this.columnRelateInf.YjkColumnData != null)
                {
                    if (this.columnRelateInf.YjkColumnData.ProtectThickness > 0)
                    {
                        this.protectLayerThickness = this.columnRelateInf.YjkColumnData.ProtectThickness;
                        return;
                    }
                }
                //柱识别
            }
            //参数设置
            this.protectLayerThickness = ThSpecificationValidate.paraSetInfo.ProtectLayerThickness;
        }
        /// <summary>
        /// 获取混凝土强度
        /// </summary>
        private void GetConcreteStrength()
        {
            if (this.columnRelateInf != null)
            {
                //构件属性定义
                if (this.columnRelateInf.CustomData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.CustomData.ConcreteStrength))
                    {
                        this.concreteStrength = this.columnRelateInf.CustomData.ConcreteStrength;
                        return;
                    }
                }
                //YJK
                //柱识别
            }
            //参数设置
            this.concreteStrength = ThSpecificationValidate.paraSetInfo.ConcreteStrength;
        }
        /// <summary>
        /// 是否角柱
        /// </summary>
        private void GetCornerColumn()
        {
            if (this.columnRelateInf != null)
            {
                //构件属性定义
                if (this.columnRelateInf.CustomData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.CustomData.CornerColumn))
                    {
                        if (this.columnRelateInf.CustomData.CornerColumn == "是")
                        {
                            this.cornerColumn = true;
                            return;
                        }
                        else if (this.columnRelateInf.CustomData.CornerColumn == "否")
                        {
                            this.cornerColumn = false;
                            return;
                        }
                    }
                }
                //YJK
                this.cornerColumn = this.columnRelateInf.YjkColumnData.IsCorner;
                //柱识别
            }
        }
        /// <summary>
        /// 箍筋放大倍数
        /// </summary>
        private void GetHoopReinforceEnlargeTimes()
        {
            if (this.columnRelateInf != null)
            {
                //构件属性定义
                if (this.columnRelateInf.CustomData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.CustomData.HoopReinforcementEnlargeTimes))
                    {
                        double value = 0.0;
                        if (double.TryParse(this.columnRelateInf.CustomData.HoopReinforcementEnlargeTimes, out value))
                        {
                            if (value > 0.0)
                            {
                                this.hoopReinforceEnlargeTimes = value;
                                return;
                            }
                        }
                    }
                }
                //YJK
                //柱识别
            }
        }        
        /// <summary>
        /// 纵筋放大倍数
        /// </summary>
        private void GetLongitudinalReinforceEnlargeTimes()
        {
            if (this.columnRelateInf != null)
            {
                //构件属性定义
                if (this.columnRelateInf.CustomData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.CustomData.LongitudinalReinforceEnlargeTimes))
                    {
                        double value = 0.0;
                        if (double.TryParse(this.columnRelateInf.CustomData.LongitudinalReinforceEnlargeTimes, out value))
                        {
                            if (value > 0.0)
                            {
                                this.longitudinalReinforceEnlargeTimes = value;
                                return;
                            }
                        }
                    }
                }
                //YJK
                //柱识别
            }
        }        

        /// <summary>
        /// 获取结果类型
        /// </summary>
        private void GetStructureType()
        {
            if (this.columnRelateInf != null)
            {
                //构件属性定义
                if (this.columnRelateInf.YjkColumnData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.YjkColumnData.StructureType))
                    {
                        this.structureType = this.columnRelateInf.YjkColumnData.StructureType;
                        return;
                    }
                }
                //YJK
                //柱识别
            }
            //参数设置
            this.structureType = ThSpecificationValidate.paraSetInfo.StructureType;
        }
        #endregion

        public void ValidateColumnInf()
        {
            validateRules.Add(BuildSectionTooSmallRule());                // 最小截面
            validateRules.Add(BuildLongLessThanShortTripleRule());        // 长短边比值
            validateRules.Add(BuildShearSpanRatioRule());                 // 剪跨比(截面)
            validateRules.Add(BuildAxialCompressionRatioRule());          // 轴压比(轴压比)
            validateRules.Add(BuildAngularReinforcementNumRule());        // 角筋根数
            validateRules.Add(BuildAngularReinforcementDiaRule());        // 配筋面积(实配钢筋应满足计算值)
            validateRules.Add(BuildVerDirForceIronRule());                // 纵向钢筋直径最小值(侧面纵筋)
            validateRules.Add(BuildAllVdIrBigThanFpmRule());              // 最大配筋率(侧面纵筋)
            validateRules.Add(BuildVerDirIronClearSpaceRule());           // 纵筋净间距
            validateRules.Add(BuildMinimumReinforceRatioARule());         // 最小配筋率A(侧面纵筋)
            validateRules.Add(BuildMinimumReinforceRatioBRule());         // 最小配筋率B(侧面纵筋)
            validateRules.Add(BuildReinforcementAreaRule());              // 配筋面积(侧面纵筋)
            validateRules.Add(BuildStirrupLimbSpaceRule());               // 箍筋肢距(箍筋)
            validateRules.Add(BuildStirrupMinimumDiameterARule());        // 箍筋最小直径A(箍筋)
            validateRules.Add(BuildStirrupMinimumDiameterBRule());        // 箍筋最小直径B(箍筋)
            validateRules.Add(BuildStirrupMaximumSpacingARule());         // 箍筋最大间距A(箍筋)
            validateRules.Add(BuildStirrupMaximumSpacingBRule());         // 箍筋最大间距B(箍筋)
            validateRules.Add(BuildStirrupMaximumSpacingCRule());         // 箍筋最大间距C(箍筋)
            validateRules.Add(BuildCompoundStirrupRule());                // 复合箍筋(箍筋)
            validateRules.Add(BuildStirrupMinimumDiameterCRule());        // 箍筋最小直径C(箍筋)
            validateRules.Add(BuildStirrupMaximumSpacingDRule());         // 箍筋最大间距D(箍筋)
            validateRules.Add(BuildStirrupMaximumSpacingERule());         // 箍筋最大间距E(箍筋)
            validateRules.Add(BuildStirrupMinimumDiameterDRule());        // 箍筋最小直径D(箍筋)
            validateRules.Add(BuildStirrupMaximumSpaceFRule());           // 箍筋最大间距F(箍筋)
            validateRules.Add(BuildStirrupMaximumSpacingHRule());         // 箍筋最大间距H(箍筋)
            validateRules.Add(BuildStirrupMaximumSpaceJRule());           // 箍筋最大间距J(箍筋)
            validateRules.Add(BuildVolumeReinforceRatioARule());          // 体积配箍率A(箍筋)
            validateRules.Add(BuildVolumeReinforceRatioBRule());          // 体积配箍率B(箍筋)
            validateRules.Add(BuildVolumeReinforceRatioCRule());          // 体积配箍率C(箍筋)
            validateRules.Add(BuildStirrupReinforcementAreaRule());       // 配筋面积(箍筋)
            for (int i = 0; i < this.validateRules.Count; i++)
            {
                if (this.validateRules[i]==null)
                {
                    continue;
                }
                this.validateRules[i].Validate();
                this.errorResults.AddRange(this.validateRules[i].ValidateResults);
                this.correctResults.AddRange(this.validateRules[i].CorrectResults);
                this.calculationSteps.AddRange(this.validateRules[i].GetCalculationSteps());
            }
        }
        /// <summary>
        /// 最小截面(截面)
        /// </summary>
        /// <returns></returns>
        private IRule BuildSectionTooSmallRule()
        {
            ColumnSectionModel columnSectionModel = new ColumnSectionModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                AntiSeismicGrade = this.antiSeismicGrade,
                FloorTotalNums = ThCalculationValidate.FloorCount,
                Cdm = cdm
            };
            IRule columnSectionRule = new SectionTooSmallRule(columnSectionModel);
            return columnSectionRule;
        }
        /// <summary>
        /// 长短边比值(截面)
        /// </summary>
        /// <returns></returns>
        private IRule BuildLongLessThanShortTripleRule()
        {
            ColumnSectionModel columnSectionModel = new ColumnSectionModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                AntiSeismicGrade = this.antiSeismicGrade,
                Cdm = cdm
            };
            IRule columnSectionRule = new LongShortEdgeRatioRule(columnSectionModel);
            return columnSectionRule;
        }
        /// <summary>
        /// 剪跨比(截面)
        /// </summary>
        /// <returns></returns>
        private IRule BuildShearSpanRatioRule()
        {
            ShearSpanRatioModel ssrm = new ShearSpanRatioModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                ShearSpanRatio = this.columnRelateInf.YjkColumnData.Jkb
            };
            IRule rule = new ShearSpanRatioRule(ssrm);
            return rule;
        }

        /// <summary>
        /// 轴压比(轴压比)
        /// </summary>
        /// <returns></returns>
        private IRule BuildAxialCompressionRatioRule()
        {
            AxialCompressionRatioModel acrm = new AxialCompressionRatioModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                AxialCompressionRatio = this.columnRelateInf.YjkColumnData.AxialCompressionRatio,
                AxialCompressionRatioLimited = this.columnRelateInf.YjkColumnData.AxialCompressionRatioLimited
            };
            IRule rule = new AxialCompressionRatioRule(acrm);
            return rule;
        }
        /// <summary>
        /// 角筋根数(角筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildAngularReinforcementNumRule()
        {
            AngularReinforcementNumModel arnm = new AngularReinforcementNumModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                AngularReinforcementNum = cdm.IntCBarCount
            };
            IRule rule = new AngularReinforcementNumRule(arnm);
            return rule;
        }
        /// <summary>
        /// 配筋面积,实配钢筋应满足计算值
        /// </summary>
        /// <returns></returns>
        private IRule BuildAngularReinforcementDiaRule()
        {
            AngularReinforcementDiaModel ardm = new AngularReinforcementDiaModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                IsCornerColumn = this.cornerColumn,
                AngularReinforcementDia = cdm.IntCBarDia,
                AngularReinforcementDiaLimited = this.columnRelateInf.YjkColumnData.ArDiaLimited
            };
            IRule rule = new AngularReinforcementDiaRule(ardm);
            return rule;
        }
        /// <summary>
        /// 纵向钢筋直径最小值(侧面纵筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildVerDirForceIronRule()
        {
            VerDirForceIronModel verDirForceIronModel = new VerDirForceIronModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = this.cdm
            };
            IRule rule = new VerDirForceIronDiaRule(verDirForceIronModel);
            return rule;
        }
        /// <summary>
        /// 最大配筋率(侧面纵筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildAllVdIrBigThanFpmRule()
        {
            MaximumReinforcementRatioModel mrrm = new MaximumReinforcementRatioModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = this.cdm
            };
            IRule rule = new MaximumReinforcementRatioRule(mrrm);
            return rule;
        }
        /// <summary>
        /// 纵筋净间距
        /// </summary>
        /// <returns></returns>
        private IRule BuildVerDirIronClearSpaceRule()
        {
            VerDirIronClearSpaceModel vdiCSM = new VerDirIronClearSpaceModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                ProtectLayerThickness = this.protectLayerThickness,
                Cdm = this.cdm
            };
            IRule rule = new VerDirIronClearSpaceRule(vdiCSM);
            return rule;
        }
        /// <summary>
        /// 最小配筋率A(侧面纵筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildMinimumReinforceRatioARule()
        {
            MinimumReinforceRatioAModel mrrm = new MinimumReinforceRatioAModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                ConcreteStrength= this.concreteStrength,                
                Cdm = cdm
            };
            IRule rule = new MinimumReinforcementRatioARule(mrrm);
            return rule;
        }
        /// <summary>
        /// 最小配筋率B(侧面纵筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildMinimumReinforceRatioBRule()
        {
            MinimumReinforceRatioBModel mrrm = new MinimumReinforceRatioBModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                IsCornerColumn = this.cornerColumn,
                AntiSeismicGrade = this.antiSeismicGrade,
                ConcreteStrength = this.concreteStrength,
                StructureType = this.structureType,
                Cdm = cdm,
                IsFourClassHigherArchitecture = ThSpecificationValidate.paraSetInfo.IsFourClassHigherArchitecture, //后期再调整
            };
            IRule rule = new MinimumReinforcementRatioBRule(mrrm);
            return rule;
        }
        /// <summary>
        /// 配筋面积(侧面纵筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildReinforcementAreaRule()
        {
            //X向限值、Y向限值
            ReinforcementAreaModel ram = new ReinforcementAreaModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm =cdm,
                DblXAsCal= this.columnRelateInf.YjkColumnData.DblXAsCal,
                DblYAsCal= this.columnRelateInf.YjkColumnData.DblYAsCal
            };
            IRule rule = new ReinforcementAreaRule(ram);
            return rule;
        }
        /// <summary>
        /// 箍筋肢距(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupLimbSpaceRule()
        {
            IRule rule = null;
            StirrupLimbSpaceModel slsm = new StirrupLimbSpaceModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                AntiSeismicGrade = this.antiSeismicGrade,
                Cdm = cdm,
                ProtectLayerThickness = this.protectLayerThickness
            };
            rule = new StirrupLimbSpaceRule(slsm);
            return rule;
        }
        /// <summary>
        /// 箍筋最小直径A(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMinimumDiameterARule()
        {
            IRule rule = null;
            StirrupMinimumDiameterAModel smda = new StirrupMinimumDiameterAModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = cdm,
            };
            rule = new StirrupMinimumDiameterARule(smda); 
            return rule;
        }
        /// <summary>
        /// 箍筋最小直径B(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMinimumDiameterBRule()
        {
            IRule rule = null;
            StirrupMinimumDiameterBModel smdb = new StirrupMinimumDiameterBModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = cdm,
            };
            rule = new StirrupMinimumDiameterBRule(smdb);
            return rule;
        }
        /// <summary>
        /// 箍筋最大间距A(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMaximumSpacingARule()
        {
            IRule rule = null;
            StirrupMaximumSpacingAModel smsa = new StirrupMaximumSpacingAModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = cdm,
            };
            rule = new StirrupMaximumSpacingARule(smsa);
            return rule;
        }
        /// <summary>
        /// 箍筋最大间距B(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMaximumSpacingBRule()
        {
            IRule rule = null;
            StirrupMaximumSpacingBModel smsb = new StirrupMaximumSpacingBModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = cdm,
            };
            rule = new StirrupMaximumSpacingBRule(smsb);
            return rule;
        }
        /// <summary>
        /// 箍筋最大间距C(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMaximumSpacingCRule()
        {
            IRule rule = null;
            StirrupMaximumSpacingCModel smsc = new StirrupMaximumSpacingCModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = cdm,
            };
            rule = new StirrupMaximumSpacingCRule(smsc);
            return rule;
        }
        /// <summary>
        /// 复合箍筋(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildCompoundStirrupRule()
        {
            IRule rule = null;
            CompoundStirrupModel csm = new CompoundStirrupModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = cdm,
            };
            rule = new CompoundStirrupRule(csm);
            return rule;
        }
        /// <summary>
        /// 箍筋最小直径C(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMinimumDiameterCRule()
        {
            IRule rule = null;
            StirrupMinimumDiameterCModel smdc = new StirrupMinimumDiameterCModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = cdm,
            };
            rule = new StirrupMinimumDiameterCRule(smdc);
            return rule;
        }
        /// <summary>
        /// 箍筋最大间距D(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMaximumSpacingDRule()
        {
            IRule rule = null;
            StirrupMaximumSpacingDModel smsd = new StirrupMaximumSpacingDModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = cdm,
            };
            rule = new StirrupMaximumSpacingDRule(smsd);
            return rule;
        }
        /// <summary>
        /// 箍筋最大间距E(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMaximumSpacingERule()
        {
            IRule rule = null;
            StirrupMaximumSpacingEModel smse = new StirrupMaximumSpacingEModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = cdm,
            };
            rule = new StirrupMaximumSpacingERule(smse);
            return rule;
        }
        /// <summary>
        /// 箍筋最小直径D(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMinimumDiameterDRule()
        {
            StirrupMinimumDiameterDModel smdd = new StirrupMinimumDiameterDModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                AntiSeismicGrade =this.antiSeismicGrade,
                IsFirstFloor=this.columnRelateInf.YjkColumnData.IsGroundFloor,
                Jkb= this.columnRelateInf.YjkColumnData.Jkb,
                IntStirrupDia = cdm.IntStirrupDia
            };
            IRule rule = new StirrupMinimumDiameterDRule(smdd);
            return rule;
        }
        /// <summary>
        /// 箍筋最大间距F(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMaximumSpaceFRule()
        {
            IRule rule = null;
            StirrupMaximumSpacingFModel smsf = new StirrupMaximumSpacingFModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                AntiSeismicGrade =this.antiSeismicGrade,
                IsFirstFloor=this.columnRelateInf.YjkColumnData.IsGroundFloor,
                ProtectThickness=this.protectLayerThickness,
                Cdm = this.cdm,
                Jkb= this.columnRelateInf.YjkColumnData.Jkb
             };
            rule = new StirrupMaximumSpacingFRule(smsf);
            return rule;
        }
        /// <summary>
        /// 箍筋最大间距H(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMaximumSpacingHRule()
        {
            IRule rule = null;
            StirrupMaximumSpacingHModel smsh = new StirrupMaximumSpacingHModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = cdm,
            };
            rule = new StirrupMaximumSpacingHRule(smsh);
            return rule;
        }
        /// <summary>
        /// 箍筋最大间距J(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMaximumSpaceJRule()
        {
            StirrupMaximumSpacingJModel smsj = new StirrupMaximumSpacingJModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm =this.cdm,
                Antiseismic = this.antiSeismicGrade
            };
            IRule rule = new StirrupMaximumSpacingJRule(smsj);
            return rule;
        }
        /// <summary>
        /// 体积配箍率A(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildVolumeReinforceRatioARule()
        {
            VolumeReinforceRatioAModel vrra = new VolumeReinforceRatioAModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text= this.columnRelateInf.ModelColumnInfs[0].Text,
                AntiSeismicGrade =this.antiSeismicGrade,                
                Cdm = this.cdm,
                ProtectLayerThickness = this.protectLayerThickness
            };
            IRule rule = new VolumeReinforceRatioARule(vrra);
            return rule;
        }
        /// <summary>
        /// 体积配箍率B(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildVolumeReinforceRatioBRule()
        {
            VolumeReinforceRatioBModel vrrb = new VolumeReinforceRatioBModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                ShearSpanRatio = this.columnRelateInf.YjkColumnData.Jkb,
                Cdm = this.cdm,
                FortificationIntensity= this.columnRelateInf.YjkColumnData.FortiCation,
                ProtectLayerThickness = this.protectLayerThickness,
                AntiSeismicGrade = this.antiSeismicGrade
            };
            IRule rule = new VolumeReinforceRatioBRule(vrrb);
            return rule;
        }
        /// <summary>
        /// 体积配箍率C(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildVolumeReinforceRatioCRule()
        {
            VolumeReinforceRatioCModel vrrc = new VolumeReinforceRatioCModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                VolumnReinforceRatioLimited = this.columnRelateInf.YjkColumnData.VolumeReinforceLimitedValue,
                Cdm = this.cdm,
                ProtectLayerThickness = this.protectLayerThickness
            };
            IRule rule = new VolumeReinforceRatioCRule(vrrc);
            return rule;
        }
        /// <summary>
        /// 配筋面积(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupReinforcementAreaRule()
        {
            StirrupReinforcementAreaModel sram = new StirrupReinforcementAreaModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Text = this.columnRelateInf.ModelColumnInfs[0].Text,
                Cdm = this.cdm,
                IntStirrupSpacingCal = this.columnRelateInf.YjkColumnData.IntStirrupSpacingCal,
                DblStirrupAsCal = this.columnRelateInf.YjkColumnData.DblStirrupAsCal,
                DblStirrupAsCal0 = this.columnRelateInf.YjkColumnData.DblStirrupAsCal0
            };
            IRule rule = new StirrupReinforcementAreaRule(sram);
            return rule;
        }
    }
}
