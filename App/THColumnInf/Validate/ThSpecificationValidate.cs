﻿using System.Collections.Generic;
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
            cdm = ThValidate.columnDataModels.Where(i => i.Code == columnInf.Code).Select(i=>i).First();
            //抗震等级 
            GetAntiSeismicGrade();
            GetProtectLayerThickness();
            GetConcreteStrength();
        }
        #region 需要从柱子识别、参数设置、构件属性定义来确定以下参数的值
        private string antiSeismicGrade = "";
        private double protectLayerThickness;
        private string concreteStrength = "";
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
            if (!string.IsNullOrEmpty(this.columnInf.AntiSeismicGrade))
            {
                this.antiSeismicGrade = this.columnInf.AntiSeismicGrade;
                return;
            }
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
                if (this.columnCustomData.ProtectLayerThickness > 0)
                {
                    this.protectLayerThickness = this.columnCustomData.ProtectLayerThickness;
                    return;
                }
            }
            this.protectLayerThickness = ThSpecificationValidate.paraSetInfo.ProtectLayerThickness;
        }
        /// <summary>
        /// 获取混凝土数值
        /// </summary>
        /// <returns></returns>
        private double GetConcreteStrengthValue()
        {
            double value = 0.0;
            if (!string.IsNullOrEmpty(this.concreteStrength))
            {
                List<double> values = ThColumnInfoUtils.GetDoubleValues(this.concreteStrength);
                if (values.Count > 0)
                {
                    if (values[0] <= 60)
                    {
                        value = 0.0;
                    }
                    else
                    {
                        value = 0.1;
                    }
                }
            }
            return value;
        }
        #endregion
        public void ValidateColumnInf()
        {         
            if (string.IsNullOrEmpty(columnInf.Code) || this.cdm == null)
            {
                return ;
            }
            validateRules.Add(BuildSectionTooSmallRule());
            validateRules.Add(BuildLongLessThanShortTripleRule());
            validateRules.Add(BuildAngularReinforcementNumRule());
            validateRules.Add(BuildVerDirForceIronRule());
            validateRules.Add(BuildAllVdIrBigThanFpmRule());
            validateRules.Add(BuildVerDirIronClearSpaceRule());
            validateRules.Add(BuildMinimumReinforceRatioARule());
            validateRules.Add(BuildMinimumReinforceRatioBRule());
            validateRules.Add(BuildStirrupLimbSpaceRule());
            validateRules.Add(new StirrupMinimumDiameterARule(this.cdm)); //箍筋最小直径A(箍筋)
            validateRules.Add(new StirrupMinimumDiameterBRule(this.cdm)); //箍筋最小直径B(箍筋)
            validateRules.Add(new StirrupMinimumDiameterCRule(this.cdm)); //箍筋最小直径C(箍筋)
            validateRules.Add(BuildStirrupMinimumDiameterDRule()); //箍筋最小直径D(箍筋)
            validateRules.Add(new StirrupMaximumSpacingARule(this.cdm)); //箍筋最大间距A(箍筋)
            validateRules.Add(new StirrupMaximumSpacingBRule(this.cdm)); //箍筋最大间距B(箍筋)
            validateRules.Add(new StirrupMaximumSpacingCRule(this.cdm)); //箍筋最大间距C(箍筋)
            validateRules.Add(new StirrupMaximumSpacingDRule(this.cdm)); //箍筋最大间距D(箍筋)
            validateRules.Add(new StirrupMaximumSpacingERule(this.cdm)); //箍筋最大间距E(箍筋)
            validateRules.Add(BuildStirrupMaximumSpaceFRule()); //箍筋最大间距F(箍筋)
            validateRules.Add(new StirrupMaximumSpacingHRule(this.cdm));//箍筋最大间距H(箍筋)
            validateRules.Add(BuildStirrupMaximumSpaceJRule()); //箍筋最大间距J(箍筋)
            validateRules.Add(new CompoundStirrupRule(this.cdm)); //复合箍筋(箍筋)

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
                AntiSeismicGrade = this.antiSeismicGrade,
                FloorTotalNums = ThSpecificationValidate.paraSetInfo.FloorCount,
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
                Cdm=this.cdm
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
                Cdm=this.cdm
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
                P1 = ThSpecificationValidate.paraSetInfo.GetLongitudinalReinforcementGrade(cdm.Ctri.BEdgeSideMiddleReinforcement),
                P2 = GetConcreteStrengthValue(),
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
            ColumnTableRecordInfo ctri = ThSpecificationValidate.dataSource.ColumnTableRecordInfos.
                Where(i => i.Code == this.columnInf.Code).Select(i => i).First();
            string columnType = "";
            if(this.columnInf.Code.ToUpper().Contains("KZ"))
            {
                columnType = "中柱";
            }
            else if(this.columnInf.Code.ToUpper().Contains("ZHZ"))
            {
                columnType = "框支柱";
            }
            double dblsespmin = ThValidate.GetIronMinimumReinforcementPercent(
                this.antiSeismicGrade, columnType, ThSpecificationValidate.paraSetInfo.StructureType);
            List<double> concreteValues = ThColumnInfoUtils.GetDoubleValues(this.concreteStrength);
            if(concreteValues.Count>0 && concreteValues[0]>=60)
            {
                dblsespmin += 0.1;
            }            
            MinimumReinforceRatioBModel mrrm = new MinimumReinforceRatioBModel
            {
                Code = this.columnInf.Code,
                Dblsespmin = dblsespmin,                
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
            double stirrupDiameterLimited = ThValidate.GetStirrupMinimumDiameter(this.antiSeismicGrade,
                ThSpecificationValidate.isGroundFloor);
            double shearSpanRatio = 2.5; //剪跨比(暂时设默认值)
            if(shearSpanRatio<2 && this.antiSeismicGrade.Contains("四级"))
            {
                stirrupDiameterLimited = 8.0;
            }
            StirrupMinimumDiameterDModel smdd = new StirrupMinimumDiameterDModel()
            {
                Code=this.columnInf.Code,
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
            bool isGroundFloor = ThSpecificationValidate.isGroundFloor;
            //抗震等级
            string antiSeismicGrade = "";
            if (this.columnInf.Code.ToUpper().Contains("ZHZ"))
            {
                antiSeismicGrade = "抗震一级";
            }
            else
            {
                antiSeismicGrade = this.antiSeismicGrade;
            }
            //箍筋间距限值
            double stirrupSpaceingLimited = ThValidate.GetStirrupMaximumDiameter(antiSeismicGrade, isGroundFloor, intBardiamin);
            //箍筋间距限值修正
            double dblXSpace = (this.cdm.B - 2 * this.protectLayerThickness) / (this.cdm.IntYStirrupCount - 1);
            double dblYSpace = (this.cdm.H - 2 * this.protectLayerThickness) / (this.cdm.IntXStirrupCount - 1);
            double dblStirrupSpace = Math.Max(dblXSpace,dblYSpace);

            if(antiSeismicGrade.Contains("一级") && !antiSeismicGrade.Contains("特"))
            {
                if(this.cdm.IntStirrupDia>12 && dblStirrupSpace<=150)
                {
                    stirrupSpaceingLimited = 150;
                }
            }
            else if(antiSeismicGrade.Contains("二级"))
            {
                if (this.cdm.IntStirrupDia >= 10 && dblStirrupSpace <= 150)
                {
                    stirrupSpaceingLimited = 150;
                }
            }
            StirrupMaximumSpacingFModel smsf = new StirrupMaximumSpacingFModel()
            {
                Code = this.columnInf.Code,
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
            IRule rule = null;
            StirrupMaximumSpacingJModel smsj = new StirrupMaximumSpacingJModel()
            {
                Code = this.columnInf.Code,
                Cdm=this.cdm,
                Antiseismic= this.antiSeismicGrade
            };
            rule = new StirrupMaximumSpacingJRule(smsj);
            return rule;
        }
    }
}