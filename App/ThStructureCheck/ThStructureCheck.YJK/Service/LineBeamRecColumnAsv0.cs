using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Service;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Model;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Service
{
    /// <summary>
    /// 直梁和方型柱
    /// </summary>
    class LineBeamRecColumnAsv0 : Asv0Calculation
    {
        private ModelRecColumnSeg modelRecColumnSeg;
        private double b;
        private double h;
        private IEntity beamGeo;
        private IEntity columnGeo;
       
        public LineBeamRecColumnAsv0(List<ModelBeamSeg> modelBeamSegs, ModelRecColumnSeg modelRecColumnSeg, bool start, string dtlCalcPath)
            :base(modelBeamSegs, modelRecColumnSeg, start,dtlCalcPath)
        {
            this.modelRecColumnSeg = modelRecColumnSeg;           
            columnGeo = this.modelRecColumnSeg.BuildGeometry();
            Init();
        }
        public LineBeamRecColumnAsv0(List<ModelBeamSeg> modelBeamSegs, IEntity recGeo, bool start, string dtlCalcPath)
            :base(modelBeamSegs,null, start,dtlCalcPath)
        {
            columnGeo = recGeo;
            Init();
        }
        private void Init()
        {            
            List<double> specDatas = Utils.GetDoubleDatas(modelBeamSeg.BeamSect.Spec);
            if (specDatas.Count >= 2)
            {
                this.b = specDatas[0];
                this.h = specDatas[1];
            }
            beamGeo = (base.modelBeamSeg as ModelLineBeamSeg).BuildGeometry();
            InsertDepthCalculate insertDepthCalculate = new InsertDepthCalculate(beamGeo, columnGeo);
            insertDepthCalculate.Calculate();
            base.insertDepth = insertDepthCalculate.InsertDepth;
            int floorNo;
            int beamSegNo;
            bool res = this.modelBeamQuery.GetDtlmodelTblBeamSegFlrNoAndNo(modelBeamSeg.ID, out floorNo, out beamSegNo);
            int calcBeamId = this.calcBeamQuery.GetTblBeamSegIDFromDtlCalc(floorNo, beamSegNo);
            string antiSeismicGrade = GetAntiSeismicGrade(calcBeamId); //抗震等级
            BeamPortEncryptStirrup beamPortEncryptStirrup = new BeamPortEncryptStirrup(antiSeismicGrade);
            this.asvLength = beamPortEncryptStirrup.GetAsvLength(this.h);
            this.asvLength += base.insertDepth;
        }
        public override void Calculate()
        {
            if(base.linkEnty==null || base.modelBeamSeg==null)
            {
                return;
            }
            if(base.start)
            {
                CalculateStart();
            }
            else
            {
                CalculateEnd();
            }
        }
        private void CalculateStart()
        {
            double totalLength = 0.0;
            double preLength = 0.0;
            for(int i=0;i< base.beamSegs.Count;i++)
            {
                ModelLineBeamSeg modelLineBeamSeg= base.beamSegs[i] as ModelLineBeamSeg;
                totalLength += modelLineBeamSeg.Length;
                if (asvLength <= modelLineBeamSeg.Length)
                {
                    int floorNo;
                    int beamSegNo;
                    bool res = this.modelBeamQuery.GetDtlmodelTblBeamSegFlrNoAndNo(beamSegs[i].ID, out floorNo, out beamSegNo);
                    int calcBeamId = this.calcBeamQuery.GetTblBeamSegIDFromDtlCalc(floorNo, beamSegNo);
                    CalcRCBeamDsn calcRCBeamDsn = calcBeamQuery.GetCalcRcBeamDsn(calcBeamId);
                    List<double> asves = calcRCBeamDsn.AsvCollection;
                    CalculateForward(asves, asvLength - preLength, modelLineBeamSeg.Length / 8.0);
                    break;
                }
                preLength += modelLineBeamSeg.Length;
            }
        }
        private void CalculateEnd()
        {
            double totalLength = 0.0;
            double preLength = 0.0;
            for (int i = base.beamSegs.Count -1; i >= 0; i--)
            {
                ModelLineBeamSeg modelLineBeamSeg = base.beamSegs[i] as ModelLineBeamSeg;
                totalLength += modelLineBeamSeg.Length;
                if (asvLength <= modelLineBeamSeg.Length)
                {
                    int floorNo;
                    int beamSegNo;
                    bool res = this.modelBeamQuery.GetDtlmodelTblBeamSegFlrNoAndNo(beamSegs[i].ID, out floorNo, out beamSegNo);
                    int calcBeamId = this.calcBeamQuery.GetTblBeamSegIDFromDtlCalc(floorNo, beamSegNo);
                    CalcRCBeamDsn calcRCBeamDsn = calcBeamQuery.GetCalcRcBeamDsn(calcBeamId);
                    List<double> asves = calcRCBeamDsn.AsvCollection;
                    CalculateOpposite(asves, asvLength - preLength, modelLineBeamSeg.Length / 8.0);
                    break;
                }
                preLength += modelLineBeamSeg.Length;
            }
        }
        private void CalculateForward(List<double> asves, double diffL, double splitLength)
        {
            //正向
            for (int i = 1; i <= 8; i++)
            {
                if (asvLength == i * splitLength)
                {
                    base.asv0 = asves[i];
                    break;
                }
                if (asvLength < i * splitLength)
                {
                    double startV = asves[i - 1];
                    double endV = asves[i];
                    double startL = (i - 1) * splitLength;
                    double endL = (i) * splitLength;
                    base.asv0 = Utils.DifferenceMethod(startL, endL, diffL, startV, endV);
                    break;
                }
            }
        }
        private void CalculateOpposite(List<double> asves, double diffL, double splitLength)
        {
            //负向
            for (int i = 1; i <= 8; i++)
            {
                if (asvLength == i * splitLength)
                {
                    base.asv0 = asves[asves.Count - 1 - i];
                    break;
                }
                if (asvLength < i * splitLength)
                {
                    double startV = asves[asves.Count - i];
                    double endV = asves[asves.Count - 1 - i];
                    double startL = (i - 1) * splitLength;
                    double endL = (i) * splitLength;
                    base.asv0 = Utils.DifferenceMethod(startL, endL, diffL, startV, endV);
                    break;
                }
            }
        }
        public string GetAntiSeismicGrade(int calcBeamId)
        {
            string antiSeismicGrade = "";
            //获取抗震等级(dtlCalc)
            double antiSeismicGradeParaValue = this.calcBeamQuery.GetAntiSeismicGradeInCalculation(calcBeamId);
            if (antiSeismicGradeParaValue != 0.0)
            {
                antiSeismicGrade = ThYjkDatbaseExtension.GetAntiSeismicGrade(antiSeismicGradeParaValue);
            }
            else
            {
                List<double> antiSeismicGradeValues = this.modelBeamQuery.GetAntiSeismicGradeInModel();
                antiSeismicGrade = ThYjkDatbaseExtension.GetAntiSeismicGrade(antiSeismicGradeValues[0], antiSeismicGradeValues[1]);
            }
            return antiSeismicGrade;
        }
    }
}
