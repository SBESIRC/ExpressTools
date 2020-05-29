using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThColumnInfo.Validate
{
    public class ThSpecificationValidate :ThValidate
    {
        public static ParameterSetInfo paraSetInfo;
        private string filePath = "";
        private Dictionary<ColumnInf, List<string>> columnValidResultDic = new Dictionary<ColumnInf, List<string>>();
        private Dictionary<ColumnInf, List<string>> calculationStepDic = new Dictionary<ColumnInf, List<string>>();
        public static bool isGroundFloor = false;
        public readonly static double shearSpanRatio = 2.5; //剪跨比
        public Dictionary<ColumnInf, List<string>> ColumnValidResultDic
        {
            get
            {
                return columnValidResultDic;
            }
        }
        public ThSpecificationValidate(IDataSource ds, ParameterSetInfo paraSetInf,string innerFrameName=""):base(ds)
        {
            paraSetInfo = paraSetInf;
            var doc = ThColumnInfoUtils.GetMdiActiveDocument();
            FileInfo fileInfo =new FileInfo(doc.Name);
            if (fileInfo.Exists)
            {

                this.filePath = fileInfo.Directory.FullName + "\\" + DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss") + ".txt";
            }
            else
            {
                this.filePath = Environment.CurrentDirectory + "\\" + DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss") + ".txt";
            }
            isGroundFloor = false;
            if (!string.IsNullOrEmpty(innerFrameName) && !string.IsNullOrEmpty(paraSetInf.BottomFloorLayer))
            {
                if(innerFrameName== paraSetInf.BottomFloorLayer)
                {
                    isGroundFloor = true;
                }
            }
        }
        public void Validate(List<ColumnInf> columnInfs = null)
        {
            if (dataSource == null || dataSource.ColumnInfs.Count == 0)
            {
                if(columnInfs==null)
                {
                    return;
                }
            }
            List<ColumnInf> validColumns = new List<ColumnInf>();
            if (columnInfs != null && columnInfs.Count>0)
            {
                validColumns = columnInfs;
                validColumns.Sort(new ColumnInfCompare());
            }
            else
            {
                validColumns = dataSource.ColumnInfs.Where(i =>
             ThColumnInfoUtils.GetDatas(i.Code).Count > 0 && i.Error == ErrorMsg.OK).Select(i => i).ToList();
                validColumns.Sort(new ColumnInfCompare());
            }
            for (int i = 0; i < validColumns.Count; i++)
            {
                NoCalculationValidate ncv = new NoCalculationValidate(validColumns[i]);
                ncv.ValidateColumnInf();
                List<string> totalStrings = new List<string>();
                totalStrings.AddRange(ncv.ErrorResults);
                totalStrings.Add("XXXXXX");
                totalStrings.AddRange(ncv.CorrectResults);
                columnValidResultDic.Add(validColumns[i], totalStrings);
                calculationStepDic.Add(validColumns[i], ncv.CalculationSteps);
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
                foreach(var item in this.calculationStepDic)
                {
                    sw.WriteLine("柱号：" + item.Key.Code);
                    sw.WriteLine("坐标：");
                    for (int i=0;i<item.Key.Points.Count;i++)
                    {
                        sw.WriteLine("      "+ item.Key.Points[i].ToString());
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
    public class NoCalculationValidate 
    {
        private ColumnInf columnInf;
        private ColumnDataModel cdm = null;
        private List<IRule> validateRules = new List<IRule>();
        private List<string> errorResults = new List<string>();
        private List<string> correctResults = new List<string>();

        private List<string> calculationSteps = new List<string>();
        private ColumnCustomData columnCustomData;

        /// <summary>
        /// 错误结果
        /// </summary>
        public List<string> ErrorResults
        {
            get
            {
                return this.errorResults;
            }
        }
        /// <summary>
        /// 正确结果
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
        public NoCalculationValidate(ColumnInf columnInf)
        {
            this.columnInf = columnInf;
            Init();
        }
        private void Init()
        {
            if (ThSpecificationValidate.dataSource == null ||
                ThSpecificationValidate.dataSource.ColumnTableRecordInfos.Count==0)
            {
                return;
            }
            PlantCalDataToDraw plantCalDataToDraw = new PlantCalDataToDraw(false);
            ObjectId columnId = plantCalDataToDraw.GetUnVisibleColumn(columnInf.Points);
            bool isSuccess = false;
            this.columnCustomData = plantCalDataToDraw.ReadEmbededColumnCustomData(columnId, out isSuccess);
            var cdmRes = ThValidate.columnDataModels.Where(i => i.Code == columnInf.Code).Select(i => i);
            if (cdmRes != null && cdmRes.Count() > 0)
            {
                cdm = cdmRes.First();
            }
            //抗震等级 
            GetAntiSeismicGrade();
            GetProtectLayerThickness();
            GetConcreteStrength();
            GetStructureType();
            GetCornerColumn();
        }
        #region 需要从柱子识别、参数设置、构件属性定义来确定以下参数的值
        private string antiSeismicGrade = "";
        private double protectLayerThickness;
        private string concreteStrength = "";
        private string structureType = "";
        private bool cornerColumn = false;
        /// <summary>
        /// 获取抗震等级
        /// </summary>
        /// <returns></returns>
        private void GetAntiSeismicGrade()
        {
            if (this.columnCustomData != null)
            {
                if (!string.IsNullOrEmpty(this.columnCustomData.AntiSeismicGrade))
                {
                    this.antiSeismicGrade = this.columnCustomData.AntiSeismicGrade;
                    return;
                }
            }
            //暂时不考虑
            //if (!string.IsNullOrEmpty(this.columnInf.AntiSeismicGrade))
            //{
            //    this.antiSeismicGrade = this.columnInf.AntiSeismicGrade;
            //    return;
            //}
            this.antiSeismicGrade = ThSpecificationValidate.paraSetInfo.AntiSeismicGrade;
        }
        /// <summary>
        /// 获取混凝土强度
        /// </summary>
        private void GetConcreteStrength()
        {
            //构件属性定义
            if (this.columnCustomData != null)
            {
                if (!string.IsNullOrEmpty(this.columnCustomData.ConcreteStrength))
                {
                    this.concreteStrength = this.columnCustomData.ConcreteStrength;
                    return;
                }
            }
            //柱识别            
            //参数设置
            this.concreteStrength = ThSpecificationValidate.paraSetInfo.ConcreteStrength;
        }
        /// <summary>
        /// 获取保护层厚度
        /// </summary>
        private void GetProtectLayerThickness()
        {
            if (this.columnCustomData != null)
            {
                if(!string.IsNullOrEmpty(this.columnCustomData.ProtectLayerThickness))
                {
                    double value = 0.0;
                    if(double.TryParse(this.columnCustomData.ProtectLayerThickness,out value))
                    {
                        if(value>0.0)
                        {
                            this.protectLayerThickness = value;
                            return;
                        }
                    }
                }
            }
            this.protectLayerThickness = ThSpecificationValidate.paraSetInfo.ProtectLayerThickness;
        }
        /// <summary>
        /// 获取结果类型
        /// </summary>
        private void GetStructureType()
        {
            //参数设置
            this.structureType = ThSpecificationValidate.paraSetInfo.StructureType;
        }
        /// <summary>
        /// 获取角柱
        /// </summary>
        private void GetCornerColumn()
        {
            if (this.columnCustomData != null)
            {
                if (!string.IsNullOrEmpty(this.columnCustomData.CornerColumn))
                {
                    if (this.columnCustomData.CornerColumn == "是")
                    {
                        this.cornerColumn = true;
                        return;
                    }
                }
            }
        }
        #endregion
        public void ValidateColumnInf()
        {         
            if (string.IsNullOrEmpty(columnInf.Code) || this.cdm == null)
            {
                return ;
            }
            validateRules.Add(BuildSectionTooSmallRule());                // 最小截面(截面)
            validateRules.Add(BuildLongLessThanShortTripleRule());        // 长短边比值(截面)
            validateRules.Add(BuildAngularReinforcementNumRule());        // 角筋根数(角筋)
            validateRules.Add(BuildVerDirForceIronRule());                // 纵向钢筋直径最小值(侧面纵筋)
            validateRules.Add(BuildAllVdIrBigThanFpmRule());              // 最大配筋率(侧面纵筋)
            validateRules.Add(BuildVerDirIronClearSpaceRule());           // 纵筋净间距(侧面纵筋)
            validateRules.Add(BuildMinimumReinforceRatioARule());         // 最小配筋率A(侧面纵筋)
            validateRules.Add(BuildMinimumReinforceRatioBRule());         // 最小配筋率B(侧面纵筋)
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
            validateRules.Add(BuildStirrupFullHeightEncryptionRule());    // 箍筋全高加密(箍筋)
            validateRules.Add(BuildStirrupMaximumSpaceFRule());           // 箍筋最大间距F(箍筋)
            validateRules.Add(BuildStirrupMaximumSpacingHRule());         // 箍筋最大间距H(箍筋)
            validateRules.Add(BuildStirrupMaximumSpaceJRule());           // 箍筋最大间距J(箍筋)
            validateRules.Add(BuildVolumeReinforceRatioARule());          // 体积配箍率A(箍筋)
            for (int i = 0; i < this.validateRules.Count; i++)
            {
                if (this.validateRules[i] == null)
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
        /// 角筋根数(角筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildAngularReinforcementNumRule()
        {
            AngularReinforcementNumModel arnm = new AngularReinforcementNumModel
            {
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                AngularReinforcementNum = cdm.IntCBarCount
            };
            IRule rule = new AngularReinforcementNumRule(arnm);
            return rule;
        }
        /// <summary>
        /// 最小截面(截面)
        /// </summary>
        /// <returns></returns>
        private IRule BuildSectionTooSmallRule()
        {
            ColumnSectionModel columnSectionModel = new ColumnSectionModel
            {
                Code = this.columnInf.Code,
                Text=this.columnInf.Text,
                AntiSeismicGrade = this.antiSeismicGrade,
                FloorTotalNums = ThSpecificationValidate.paraSetInfo.FloorCount,
                Cdm=cdm
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                AntiSeismicGrade = this.antiSeismicGrade,                
                Cdm = cdm
            };
            IRule columnSectionRule = new LongShortEdgeRatioRule(columnSectionModel);
            return columnSectionRule;
        }
        /// <summary>
        /// 纵向钢筋直径最小值(侧面纵筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildVerDirForceIronRule()
        {
            VerDirForceIronModel verDirForceIronModel= new VerDirForceIronModel
            {
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                Cdm =this.cdm
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
            MaximumReinforcementRatioModel mrrm= new MaximumReinforcementRatioModel
            {
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                Cdm =this.cdm
            };
            IRule rule = new MaximumReinforcementRatioRule(mrrm);
            return rule;
        }
        /// <summary>
        /// 纵筋净间距(侧面纵筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildVerDirIronClearSpaceRule()
        {
            VerDirIronClearSpaceModel vdiCSM= new VerDirIronClearSpaceModel
            {
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                ProtectLayerThickness = this.protectLayerThickness,
                Cdm=this.cdm
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                ConcreteStrength = this.concreteStrength,               
                Cdm=cdm
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                AntiSeismicGrade =this.antiSeismicGrade,
                ConcreteStrength=this.concreteStrength,
                StructureType=this.structureType,
                IsCornerColumn=this.cornerColumn,
                Cdm = cdm,                
                IsFourClassHigherArchitecture= ThSpecificationValidate.paraSetInfo.IsFourClassHigherArchitecture
            };
            IRule rule = new MinimumReinforcementRatioBRule(mrrm);
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                Cdm = cdm,
            };
            rule = new CompoundStirrupRule(csm);
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                Cdm = cdm,
            };
            rule = new StirrupMaximumSpacingERule(smse);
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                Cdm = cdm,
            };
            rule = new StirrupMinimumDiameterCRule(smdc);
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
                Code=this.columnInf.Code,
                Text = this.columnInf.Text,
                AntiSeismicGrade =this.antiSeismicGrade,
                IsFirstFloor= ThSpecificationValidate.isGroundFloor,
                Jkb= ThSpecificationValidate.shearSpanRatio,
                IntStirrupDia = cdm.IntStirrupDia
            };
            IRule rule = new StirrupMinimumDiameterDRule(smdd);
            return rule;
        }
        /// <summary>
        /// 箍筋全高加密(箍筋)
        /// </summary>
        /// <returns></returns>
        private IRule BuildStirrupFullHeightEncryptionRule()
        {
            StirrupFullHeightEncryptionModel sfhem = new StirrupFullHeightEncryptionModel()
            {
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                Jkb  = ThSpecificationValidate.shearSpanRatio,
                Cdm = cdm
            };
            IRule rule = new StirrupFullHeightEncryptionRule(sfhem);
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                Cdm = this.cdm,
                IsFirstFloor= ThSpecificationValidate.isGroundFloor,
                AntiSeismicGrade=this.antiSeismicGrade,
                ProtectThickness=this.protectLayerThickness
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
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
            IRule rule = null;
            StirrupMaximumSpacingJModel smsj = new StirrupMaximumSpacingJModel()
            {
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                Cdm =this.cdm,
                Antiseismic= this.antiSeismicGrade
            };
            rule = new StirrupMaximumSpacingJRule(smsj);
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
                Code = this.columnInf.Code,
                Text = this.columnInf.Text,
                Cdm = this.cdm,
                AntiSeismicGrade = this.antiSeismicGrade,
                ProtectLayerThickness = this.protectLayerThickness
            };
            IRule rule = new VolumeReinforceRatioARule(vrra);
            return rule;
        }
    }
}
