﻿
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
            List<FloorInfo> floorInfs = new List<FloorInfo>();
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
            floorInfs = extractYjkColumnInfo.GetNaturalFloorInfs();
            FloorCount = floorInfs.Count;
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
                CalculationValidate cv = new CalculationValidate(columns[i]);
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
                        sw.WriteLine("关联柱号：" + item.Key.ModelColumnInfs[0].Code);
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
            cdm = ThValidate.columnDataModels.Where(i => i.Code == columnRelateInf.ModelColumnInfs[0].Code).Select(i => i).First();
            GetParameter getParameter = GetAntiSeismicGrade; //抗震等级
            getParameter += GetProtectLayerThickness;  //保护层厚度
            getParameter += GetConcreteStrength;      //混凝土强度
            getParameter += GetCornerColumn; //角柱
            getParameter += GetHoopReinforceEnlargeTimes; //箍筋放大倍数
            getParameter += GetLongitudinalReinforceEnlargeTimes; //纵筋放大倍数
            getParameter += GetHoopReinforceFullHeightEncryption; //全高度加密
            getParameter += GetStructureType; // 结构类型
            getParameter();
        }
        #region 需要从柱子识别、YJK、参数设置、构件属性定义来确定以下参数的值
        private string antiSeismicGrade = ""; //抗震等级
        private double protectLayerThickness; //保护层厚度
        private string concreteStrength = ""; //混凝土强度
        private bool cornerColumn;  //角柱

        private double hoopReinforceEnlargeTimes; //箍筋放大倍数
        private double longitudinalReinforceEnlargeTimes; //纵筋放大倍数
        private bool hoopReinforceFullHeightEncryption; //箍筋全高度加密
        private string structureType = ""; //结构类型
        /// <summary>
        /// 获取抗震等级
        /// </summary>
        /// <returns></returns>
        private void GetAntiSeismicGrade()
        {
            if(this.columnRelateInf==null)
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
            if (this.columnRelateInf == null)
            {
                //构件属性定义
                if (this.columnRelateInf.CustomData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.CustomData.ProtectLayerThickness))
                    {
                        double value = 0.0;
                        if (double.TryParse(this.columnRelateInf.CustomData.ProtectLayerThickness, out value))
                        {
                            if(value>0.0)
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
                    if (this.columnRelateInf.YjkColumnData.ProtectThickness>0)
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
            if (this.columnRelateInf == null)
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
            if (this.columnRelateInf == null)
            {
                //构件属性定义
                if (this.columnRelateInf.CustomData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.CustomData.CornerColumn))
                    {
                        if(this.columnRelateInf.CustomData.CornerColumn=="是")
                        {
                            this.cornerColumn = true;
                            return;
                        }
                        else if(this.columnRelateInf.CustomData.CornerColumn == "否" )
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
            if (this.columnRelateInf == null)
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
            if (this.columnRelateInf == null)
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
        private void GetHoopReinforceFullHeightEncryption()
        {
            if (this.columnRelateInf == null)
            {
                //构件属性定义
                if (this.columnRelateInf.CustomData != null)
                {
                    if (!string.IsNullOrEmpty(this.columnRelateInf.CustomData.HoopReinforceFullHeightEncryption))
                    {
                        if (this.columnRelateInf.CustomData.HoopReinforceFullHeightEncryption == "是")
                        {
                            this.hoopReinforceFullHeightEncryption = true;
                            return;
                        }
                        else if (this.columnRelateInf.CustomData.HoopReinforceFullHeightEncryption == "否")
                        {
                            this.hoopReinforceFullHeightEncryption = false;
                            return;
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
            if (this.columnRelateInf == null)
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
            validateRules.Add(new StirrupMinimumDiameterARule(this.cdm)); // 箍筋最小直径A(箍筋)
            validateRules.Add(new StirrupMinimumDiameterBRule(this.cdm)); // 箍筋最小直径B(箍筋)
            validateRules.Add(new StirrupMaximumSpacingARule(this.cdm));  // 箍筋最大间距A(箍筋)
            validateRules.Add(new StirrupMaximumSpacingBRule(this.cdm));  // 箍筋最大间距B(箍筋)
            validateRules.Add(new StirrupMaximumSpacingCRule(this.cdm));  // 箍筋最大间距C(箍筋)
            validateRules.Add(new CompoundStirrupRule(this.cdm));         // 复合箍筋(箍筋)
            validateRules.Add(new StirrupMinimumDiameterCRule(this.cdm)); // 箍筋最小直径C(箍筋)
            validateRules.Add(new StirrupMaximumSpacingDRule(this.cdm));  // 箍筋最大间距D(箍筋)
            validateRules.Add(new StirrupMaximumSpacingERule(this.cdm));  // 箍筋最大间距E(箍筋)
            validateRules.Add(BuildStirrupMinimumDiameterDRule());        // 箍筋最小直径D(箍筋)
            validateRules.Add(BuildStirrupMaximumSpaceFRule());           // 箍筋最大间距F(箍筋)
            validateRules.Add(new StirrupMaximumSpacingHRule(this.cdm));  // 箍筋最大间距H(箍筋)
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
                P1 = ThSpecificationValidate.paraSetInfo.GetLongitudinalReinforcementGrade(cdm.Ctri.BEdgeSideMiddleReinforcement),
                P2 = ThValidate.GetConcreteStrengthValue(this.concreteStrength),
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
            //柱子类型
            string columnType = "";           
            if(this.cornerColumn)
            {
                columnType = "角柱";
            }
            else if(this.columnRelateInf.ModelColumnInfs[0].Code.ToUpper().Contains("ZHZ"))
            {
                columnType = "框支柱";
            }
            else
            {
                columnType = "中柱"; //不是角柱或框支柱的都认为是中柱和边柱
            }
            double dblsespmin = ThValidate.GetIronMinimumReinforcementPercent(
                this.antiSeismicGrade, columnType, this.structureType);

            //混凝土强度大于等于C60,加上0.1
            List<double> concreteValues = ThColumnInfoUtils.GetDoubleValues(this.columnRelateInf.CustomData.ConcreteStrength);
            if (concreteValues.Count > 0 && concreteValues[0] >= 60)
            {
                dblsespmin += 0.1;
            }
            MinimumReinforceRatioBModel mrrm = new MinimumReinforceRatioBModel
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                Dblsespmin = dblsespmin,
                Cdm = cdm,
                IsFourClassHigherArchitecture = ThSpecificationValidate.paraSetInfo.IsFourClassHigherArchitecture, //后期再调整
                ConcreteStrength = this.columnRelateInf.CustomData.ConcreteStrength
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
                Cdm=cdm,
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
                AntiSeismicGrade = this.antiSeismicGrade,
                Cdm = cdm,
                ProtectLayerThickness = this.protectLayerThickness
            };
            rule = new StirrupLimbSpaceRule(slsm);
            return rule;
        }
        /// <summary>
        /// 箍筋最小直径D(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupMinimumDiameterDRule()
        {
            double stirrupDiameterLimited = ThValidate.GetStirrupMinimumDiameter(
                this.antiSeismicGrade,
                this.columnRelateInf.YjkColumnData.IsGroundFloor);
            if (this.columnRelateInf.YjkColumnData.Jkb < 2 &&
                this.antiSeismicGrade.Contains("四级"))
            {
                stirrupDiameterLimited = 8.0;
            }
            StirrupMinimumDiameterDModel smdd = new StirrupMinimumDiameterDModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                IntStirrupDia = cdm.IntStirrupDia,
                IntStirrupDiaLimited = stirrupDiameterLimited
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
            //纵向钢筋直径最小值
            double intBardiamin = Math.Min(this.cdm.IntXBarDia, this.cdm.IntYBarDia);
            intBardiamin = Math.Min(intBardiamin, this.cdm.IntCBarDia);
           
            //箍筋间距限值
            double stirrupSpaceingLimited = ThValidate.GetStirrupMaximumDiameter(
                this.antiSeismicGrade,
                this.columnRelateInf.YjkColumnData.IsGroundFloor, intBardiamin);

            //箍筋间距限值修正
            double dblXSpace = (this.cdm.B - 2 * this.columnRelateInf.YjkColumnData.ProtectThickness)
                / (this.cdm.IntYStirrupCount - 1);
            double dblYSpace = (this.cdm.H - 2 * this.columnRelateInf.YjkColumnData.ProtectThickness)
                / (this.cdm.IntXStirrupCount - 1);
            double dblStirrupSpace = Math.Max(dblXSpace, dblYSpace);

            if (this.antiSeismicGrade.Contains("一级") && 
                !this.antiSeismicGrade.Contains("特"))
            {
                if (this.cdm.IntStirrupDia > 12 && dblStirrupSpace <= 150)
                {
                    stirrupSpaceingLimited = 150;
                }
            }
            else if (this.antiSeismicGrade.Contains("二级"))
            {
                if (this.cdm.IntStirrupDia >= 10 && dblStirrupSpace <= 150)
                {
                    stirrupSpaceingLimited = 150;
                }
            }
            StirrupMaximumSpacingFModel smsf = new StirrupMaximumSpacingFModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                IntStirrupSpacing = this.cdm.IntStirrupSpacing,
                IntStirrupSpacingLimited = stirrupSpaceingLimited
            };
            rule = new StirrupMaximumSpacingFRule(smsf);
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
                Cdm=this.cdm,
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
            double limitedValue = ThValidate.GetVolumeReinforcementRatioLimited(
                this.columnRelateInf.ModelColumnInfs[0].Code, this.antiSeismicGrade);
            VolumeReinforceRatioAModel vrra = new VolumeReinforceRatioAModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
                VolumnReinforceRatioLimited = limitedValue,
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
                ShearSpanRatio = this.columnRelateInf.YjkColumnData.Jkb,
                Cdm = this.cdm,
                FortificationIntensity= this.columnRelateInf.YjkColumnData.FortiCation,
                ProtectLayerThickness = this.protectLayerThickness,
                Antiseismic= this.antiSeismicGrade
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
                VolumnReinforceRatioLimited = this.columnRelateInf.YjkColumnData.VolumeReinforceLimitedValue,
                Cdm = this.cdm,
                ProtectLayerThickness = this.protectLayerThickness
            };
            IRule rule = new VolumeReinforceRatioCRule(vrrc);
            return rule;
        }
        /// <summary>
        /// 配镜面积(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupReinforcementAreaRule()
        {
            StirrupReinforcementAreaModel sram = new StirrupReinforcementAreaModel()
            {
                Code = this.columnRelateInf.ModelColumnInfs[0].Code,
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
