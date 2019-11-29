using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Model;
namespace ThColumnInfo.Validate
{
    public class ThNoCalculationValidate
    {
        public static IDataSource dataSource;
        public static ParameterSetInfo paraSetInfo;

        private Dictionary<ColumnInf, List<ValidateResult>> columnValidResultDic = new Dictionary<ColumnInf, List<ValidateResult>>();
        public Dictionary<ColumnInf, List<ValidateResult>> ColumnValidResultDic
        {
            get
            {
                return columnValidResultDic;
            }
        }
        public ThNoCalculationValidate(IDataSource ds, ParameterSetInfo paraSetInf)
        {
            dataSource = ds;
            paraSetInfo = paraSetInf;
        }
        public void Validate()
        {
            if (dataSource == null || dataSource.ColumnInfs.Count == 0)
            {
                return;
            }
            for (int i = 0; i < dataSource.ColumnInfs.Count; i++)
            {
                NoCalculationValidate ncv = new NoCalculationValidate(dataSource.ColumnInfs[i]);
                ncv.ValidateColumnInf();
                columnValidResultDic.Add(dataSource.ColumnInfs[i], ncv.ValidResults);
            }
        }        
    }
    public class NoCalculationValidate 
    {
        private ColumnInf columnInf;
        private ColumnTableRecordInfo ctri;
        private double b;
        private double h;
        private int antiSeismicGradeIndex = -1;
        private int angularReinforceNum;

        private List<double> cornerIronSizeList=new List<double>();
        private List<double> bMiddleIronSizeList=new List<double>();
        private List<double> hMiddleIronSizeList=new List<double>();
        private double stirrupDia;

        private List<IRule> validateRules = new List<IRule>();

        private List<ValidateResult> validResults = new List<ValidateResult>();
        /// <summary>
        /// 验证结果
        /// </summary>
        public List<ValidateResult> ValidResults
        {
            get
            {
                return this.validResults;
            }
        }
        public NoCalculationValidate(ColumnInf columnInf)
        {
            this.columnInf = columnInf;
            Init();
        }
        private void Init()
        {
            if (ThNoCalculationValidate.dataSource == null || 
                ThNoCalculationValidate.dataSource.ColumnTableRecordInfos.Count==0)
            {
                return;
            }
            var ctriRes= ThNoCalculationValidate.dataSource.ColumnTableRecordInfos.Where(i => i.Code == columnInf.Code);
            if(ctriRes!=null && ctriRes.Count()>0)
            {
                ctri = ctriRes.First();
            }
            List<double> sizeList = ThConverter.SplitSpec(ctri.Spec);
            if (sizeList.Count == 2)
            {
                b = sizeList[0];
                h = sizeList[1];
            }
            angularReinforceNum = ThConverter.AngularReinforcementSpecToInt(ctri.AngularReinforcement);
            stirrupDia = ThConverter.StirrUpSpecToDouble(ctri.HoopReinforcement);
            string antiSeismicGrade = "";
            //抗震等级 
            if (string.IsNullOrEmpty(columnInf.AntiSeismicGrade))
            {
                if (!string.IsNullOrEmpty(ctri.Remark) && ctri.Remark.Contains("抗震"))
                {
                    antiSeismicGrade = ctri.Remark;
                }
                else if (!string.IsNullOrEmpty(ThNoCalculationValidate.paraSetInfo.AntiSeismicGrade))
                {
                    antiSeismicGrade = ThNoCalculationValidate.paraSetInfo.AntiSeismicGrade;
                }
            }
            else
            {
                antiSeismicGrade = columnInf.AntiSeismicGrade;
            }
            antiSeismicGradeIndex = ThConverter.AntiSeismicGradeStringToInt(antiSeismicGrade);
            cornerIronSizeList = ThConverter.ReinforcementSpecToList(ctri.AngularReinforcement);
            bMiddleIronSizeList = ThConverter.ReinforcementSpecToList(ctri.BEdgeSideMiddleReinforcement);
            hMiddleIronSizeList = ThConverter.ReinforcementSpecToList(ctri.HEdgeSideMiddleReinforcement);
        }
        public void ValidateColumnInf()
        {         
            if (string.IsNullOrEmpty(columnInf.Code) || this.ctri==null)
            {
                return ;
            }
            validateRules.Add(BuildColumnSectionRule());
            validateRules.Add(BuildAngularReinforcementNumRule());
            if (cornerIronSizeList.Count == 2 && bMiddleIronSizeList.Count == 2 && hMiddleIronSizeList.Count == 2)
            {
                validateRules.Add(BuildVerDirForceIronModel());
                validateRules.Add(BuildAllVdIrBigThanFpm());
                validateRules.Add(BuildVerDirIronClearSpaceModel());
            }
            for (int i = 0; i < this.validateRules.Count; i++)
            {
                if (this.validateRules[i] == null)
                {
                    continue;
                }
                this.validateRules[i].Validate();
                this.validResults.AddRange(this.validateRules[i].ValidateResults);
            }
        }
        private IRule BuildAngularReinforcementNumRule()
        {
            AngularReinforcementNumModel arnm = new AngularReinforcementNumModel
            {
                AngularReinforcementNum = angularReinforceNum
            };
            IRule rule = new AngularReinforcementNumRule(arnm);
            return rule;
        }
        private IRule BuildColumnSectionRule()
        {
            ColumnSectionModel columnSectionModel= new ColumnSectionModel
            {
                AntiSeismicGrade = antiSeismicGradeIndex,
                FloorTotalNums = ThNoCalculationValidate.paraSetInfo.FloorCount,
                B = b,
                H = h
            };
            IRule columnSectionRule = new ColumnSectionRule(columnSectionModel);
            return columnSectionRule;
        }
        private IRule BuildVerDirForceIronModel()
        {
            VerDirForceIronModel verDirForceIronModel= new VerDirForceIronModel
            {
                IntCBarDia = cornerIronSizeList[1],
                IntXBarDia = bMiddleIronSizeList[1],
                IntYBarDia = hMiddleIronSizeList[1]
            };
            IRule rule = new VerDirForceIronDiaRule(verDirForceIronModel);
            return rule;
        }
        private IRule BuildAllVdIrBigThanFpm()
        {
            AllVerDirIronReinRatioBigThanFivePerModel avdirFPM= new AllVerDirIronReinRatioBigThanFivePerModel
            {
                IntCBarCount = (int)cornerIronSizeList[0],
                IntCBarDia = cornerIronSizeList[1],
                IntXBarCount = (int)bMiddleIronSizeList[0],
                IntXBarDia = bMiddleIronSizeList[1],
                IntYBarCount = (int)hMiddleIronSizeList[0],
                IntYBarDia = hMiddleIronSizeList[1]
            };
            IRule rule = new AllVerDirIronReinRatioBigThanFivePerRule(avdirFPM);
            return rule;
        }
        private IRule BuildVerDirIronClearSpaceModel()
        {
            VerDirIronClearSpaceModel vdiCSM= new VerDirIronClearSpaceModel
            {
                ProtectLayerThickness = ThNoCalculationValidate.paraSetInfo.ProtectLayerThickness,
                IntStirrupDia = stirrupDia,
                B = b,
                H = h,
                IntCBarDia = cornerIronSizeList[1],
                IntXBarCount = (int)bMiddleIronSizeList[0],
                IntXBarDia = bMiddleIronSizeList[1],
                IntYBarCount = (int)hMiddleIronSizeList[0],
                IntYBarDia = hMiddleIronSizeList[1]
            };
            IRule rule = new VerDirIronClearSpaceRule(vdiCSM);
            return rule;
        }
    }
}
