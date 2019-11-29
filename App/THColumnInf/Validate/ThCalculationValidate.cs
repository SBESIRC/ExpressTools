
using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.Model;

namespace ThColumnInfo.Validate
{
    public class ThCalculationValidate
    {
        public static IDataSource dataSource;
        public static CalculationInfo calculationInfo;

        private Dictionary<ColumnRelateInf, List<ValidateResult>> columnValidResultDic
            = new Dictionary<ColumnRelateInf, List<ValidateResult>>();
   
        public ThCalculationValidate(IDataSource ds, CalculationInfo calInfo)
        {
            dataSource = ds;
            calculationInfo = calInfo;
        }
        public void Validate(List<ColumnRelateInf> columns)
        {
            if (columns == null || columns.Count == 0 || dataSource == null
                || calculationInfo == null)
            {
                return;
            }
            for(int i=0;i< columns.Count;i++)
            {
                CalculationValidate cv = new CalculationValidate(columns[i]);
                cv.ValidateColumnInf();
                columnValidResultDic.Add(columns[i], cv.ValidResults);
            }
        }
    }
    public class CalculationValidate
    {
        private List<double> cornerIronSizeList=new List<double>();
        private List<double> bMiddleIronSizeList=new List<double>();
        private List<double> hMiddleIronSizeList=new List<double>();
        private ColumnTableRecordInfo ctri;
        private double b;
        private double h;
        private double stirrupDia;

        private ColumnRelateInf columnRelateInf;
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
        public CalculationValidate(ColumnRelateInf columnRelateInf)
        {
            this.columnRelateInf = columnRelateInf;
            Init();
        }
        private void Init()
        {
            if (columnRelateInf.ModelColumnInfs.Count > 0)
            {
                return;
            }
            if (ThNoCalculationValidate.dataSource == null ||
    ThNoCalculationValidate.dataSource.ColumnTableRecordInfos.Count == 0)
            {
                return;
            }
            var ctriRes = ThNoCalculationValidate.dataSource.ColumnTableRecordInfos.Where
                (i => i.Code == columnRelateInf.ModelColumnInfs[0].Code).Select(i => i);
            if (ctriRes != null && ctriRes.Count() > 0)
            {
                ctri = ctriRes.First();
            }
            List<double> sizeList = ThConverter.SplitSpec(ctri.Spec);
            if (sizeList.Count == 2)
            {
                b = sizeList[0];
                h = sizeList[1];
            }
            stirrupDia = ThConverter.StirrUpSpecToDouble(ctri.HoopReinforcement);
            cornerIronSizeList = ThConverter.ReinforcementSpecToList(ctri.AngularReinforcement);
            bMiddleIronSizeList = ThConverter.ReinforcementSpecToList(ctri.BEdgeSideMiddleReinforcement);
            hMiddleIronSizeList = ThConverter.ReinforcementSpecToList(ctri.HEdgeSideMiddleReinforcement);
        }
        public void ValidateColumnInf()
        {
            validateRules.Add(BuildAxialCompressionRatioRule());
            validateRules.Add(BuildAngularReinforcementDiaRule());
            validateRules.Add(BuildVerDirIronClearSpaceRule());
            for (int i = 0; i < this.validateRules.Count; i++)
            {
                if (this.validateRules[i]==null)
                {
                    continue;
                }
                this.validateRules[i].Validate();
                this.validResults.AddRange(this.validateRules[i].ValidateResults);
            }
        }
        private IRule BuildAxialCompressionRatioRule()
        {
            ExtractYjkColumnInfo extractYjkColumnInfo = new ExtractYjkColumnInfo(ThCalculationValidate.calculationInfo.GetDtlCalcFullPath());
            double axialCompressionRatio;
            double axialCompressionRatioLimited;
            bool res=extractYjkColumnInfo.GetAxialCompressionRatio(columnRelateInf.DbColumnInf.JtID,
                out axialCompressionRatio,out axialCompressionRatioLimited);
            IRule rule=null;
            if(res)
            {
                AxialCompressionRatioModel acrm = new AxialCompressionRatioModel
                {
                    AxialCompressionRatio = axialCompressionRatio,
                    AxialCompressionRatioLimited = axialCompressionRatioLimited
                };
                rule = new AxialCompressionRatioRule(acrm);
            }
            return rule;
        }
        private IRule BuildAngularReinforcementDiaRule()
        {
            IRule rule = null;
            ExtractYjkColumnInfo extractYjkColumnInfo = new ExtractYjkColumnInfo(ThCalculationValidate.calculationInfo.GetDtlCalcFullPath());
            double arDiaLimited = 0.0;
            bool res=extractYjkColumnInfo.GetAngularReinforcementDiaLimited(columnRelateInf.DbColumnInf.JtID, out arDiaLimited);
            if (res)
            {
                if(cornerIronSizeList.Count==2 && arDiaLimited>0.0)
                {
                    AngularReinforcementDiaModel ardm = new AngularReinforcementDiaModel
                    {
                        AngularReinforcementDia = cornerIronSizeList[1],
                        AngularReinforcementDiaLimited = arDiaLimited
                    };
                    rule = new AngularReinforcementDiaRule(ardm);
                }
            }
            return rule;
        }
        private IRule BuildVerDirIronClearSpaceRule()
        {
            IRule rule = null;
            ExtractYjkColumnInfo extractCalcDb = new ExtractYjkColumnInfo(ThCalculationValidate.calculationInfo.GetDtlCalcFullPath());
            double protectThickNess = 0.0;
            bool findRes=extractCalcDb.GetProtectLayerThickInTblColSegPara(columnRelateInf.DbColumnInf.JtID, out protectThickNess);
            if(!findRes)
            {
                ExtractYjkColumnInfo extractModelDb = new ExtractYjkColumnInfo(ThCalculationValidate.calculationInfo.GetDtlmodelFullPath());
                findRes=extractModelDb.GetProtectLayerThickInTblStdFlrPara(columnRelateInf.DbColumnInf.StdFlrID, out protectThickNess);
                if(!findRes)
                {
                    findRes = extractModelDb.GetProtectLayerThickInTblProjectPara(out protectThickNess);
                }
            }
            if(findRes && protectThickNess>0.0)
            {
                VerDirIronClearSpaceModel vdiCSM = new VerDirIronClearSpaceModel
                {
                    ProtectLayerThickness = protectThickNess,
                    IntStirrupDia = stirrupDia,
                    B = b,
                    H = h,
                    IntCBarDia = cornerIronSizeList[1],
                    IntXBarCount = (int)bMiddleIronSizeList[0],
                    IntXBarDia = bMiddleIronSizeList[1],
                    IntYBarCount = (int)hMiddleIronSizeList[0],
                    IntYBarDia = hMiddleIronSizeList[1]
                };
                rule = new VerDirIronClearSpaceRule(vdiCSM);
            }
            return rule;
        }
    }
}
