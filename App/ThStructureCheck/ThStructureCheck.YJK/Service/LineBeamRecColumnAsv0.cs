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
        private ModelBeamSeg modelBeamSeg;
        private ModelColumnSeg modelColumnSeg;
        private YjkBeamQuery calcBeamQuery;
        private YjkBeamQuery modelBeamQuery;
        private double b;
        private double h;
        public LineBeamRecColumnAsv0(ModelBeamSeg modelBeamSeg, ModelColumnSeg modelColumnSeg,string dtlCalcPath)
            :base(dtlCalcPath)
        {
            this.modelBeamSeg = modelBeamSeg;
            this.modelColumnSeg = modelColumnSeg;
            this.calcBeamQuery = new YjkBeamQuery(dtlCalcPath);
            this.modelBeamQuery = new YjkBeamQuery(modelBeamSeg.DbPath);

            List<double> specDatas = Utils.GetDoubleDatas(modelBeamSeg.BeamSect.Spec);
            if(specDatas.Count>=2)
            {
                this.b = specDatas[0];
                this.h = specDatas[1];
            }
        }
        private List<ModelBeamSeg> modelBeamSegs = new List<ModelBeamSeg>();

        public override void Calculate(List<ModelBeamSeg> modelBeamSegs)
        {
            this.modelBeamSegs = modelBeamSegs;
            IEntity beamGeo = this.modelBeamSeg.BuildGeometry();
            IEntity columnGeo = this.modelColumnSeg.BuildGeometry();
            //箍筋插入到柱子或墙中的深度
            double dis = GeometricCalculation.GetInsertBeamDis(columnGeo, beamGeo);
            int floorNo;
            int beamSegNo;          
            bool res = this.modelBeamQuery.GetDtlmodelTblBeamSegFlrNoAndNo(modelBeamSeg.ID, out floorNo, out beamSegNo);
            int calcBeamId = this.calcBeamQuery.GetTblBeamSegIDFromDtlCalc(floorNo, beamSegNo);
            string antiSeismicGrade = GetAntiSeismicGrade(calcBeamId); //抗震等级
            BeamPortEncryptStirrup beamPortEncryptStirrup = new BeamPortEncryptStirrup(antiSeismicGrade);
            double asvLength = beamPortEncryptStirrup.GetAsvLength(this.h);
            asvLength += dis;
            CalcRCBeamDsn calcRCBeamDsn = calcBeamQuery.GetCalcRcBeamDsn(calcBeamId);
            List<double> asves = calcRCBeamDsn.AsvCollection;
            double maxAsv = asves.OrderByDescending(i => i).First();
            int index = asves.IndexOf(maxAsv);
            if (asvLength < this.modelBeamSeg.Length)
            {
                if (index==0)
                {
                    CalculateForward(asves, asvLength, this.modelBeamSeg.Length / 8.0);
                }
                else if(index == asves.Count-1)
                {
                    CalculateOpposite(asves, asvLength, this.modelBeamSeg.Length / 8.0);
                }
            }
            else if (modelBeamSegs.Count > 1)
            {
                if(index == 0)
                {
                    double totalLength = this.modelBeamSeg.Length;
                    for (int i=1;i< modelBeamSegs.Count;i++)
                    {
                        totalLength += modelBeamSegs[i].Length;
                        if(asvLength<= totalLength)
                        {
                            double preLength = 0.0;
                            for(int j=0;j< i;j++)
                            {
                                preLength += modelBeamSegs[j].Length;
                            }
                            int middlefloorNo;
                            int middlebeamSegNo;
                            bool middleRes = this.modelBeamQuery.GetDtlmodelTblBeamSegFlrNoAndNo(modelBeamSegs[i].ID, out middlefloorNo, out middlebeamSegNo);
                            int middleCalcBeamId = this.calcBeamQuery.GetTblBeamSegIDFromDtlCalc(middlefloorNo, middlebeamSegNo);
                            CalcRCBeamDsn middleCalcRCBeamDsn = calcBeamQuery.GetCalcRcBeamDsn(middleCalcBeamId);
                            List<double> middleAsves = middleCalcRCBeamDsn.AsvCollection;
                            CalculateForward(middleAsves, asvLength - preLength, modelBeamSegs[i].Length / 8.0);
                            break;
                        }
                    }
                }
                else if (index == asves.Count - 1)
                {
                    double totalLength = 0.0;
                    for (int i = modelBeamSegs.Count-1; i >=0 ; i--)
                    {
                        totalLength += modelBeamSegs[i].Length;
                        if (asvLength <= totalLength)
                        {
                            double preLength = 0.0;
                            for (int j = modelBeamSegs.Count - 1; j >i; j--)
                            {
                                preLength += modelBeamSegs[j].Length;
                            }
                            int middlefloorNo;
                            int middlebeamSegNo;
                            bool middleRes = this.modelBeamQuery.GetDtlmodelTblBeamSegFlrNoAndNo(modelBeamSegs[i].ID, out middlefloorNo, out middlebeamSegNo);
                            int middleCalcBeamId = this.calcBeamQuery.GetTblBeamSegIDFromDtlCalc(middlefloorNo, middlebeamSegNo);
                            CalcRCBeamDsn middleCalcRCBeamDsn = calcBeamQuery.GetCalcRcBeamDsn(middleCalcBeamId);
                            List<double> middleAsves = middleCalcRCBeamDsn.AsvCollection;
                            CalculateOpposite(middleAsves, asvLength - preLength, modelBeamSegs[i].Length / 8.0);
                            break;
                        }
                    }
                }
            }
        }
        private void CalculateForward(List<double> asves, double asvLength, double splitLength)
        {
            //正向
            for (int i = 1; i <= 8; i++)
            {
                if (asvLength == i * splitLength)
                {
                    this.Asv0 = asves[i];
                    break;
                }
                if (asvLength < i * splitLength)
                {
                    double startV = asves[i - 1];
                    double endV = asves[i];
                    double startL = (i - 1) * splitLength;
                    double endL = (i) * splitLength;
                    double diffL = asvLength - startL;
                    this.Asv0 = Utils.DifferenceMethod(startL, endL, diffL, startV, endV);
                    break;
                }
            }
        }
        private void CalculateOpposite(List<double> asves, double asvLength, double splitLength)
        {
            //负向
            for (int i = 1; i <= 8; i++)
            {
                if (asvLength == i * splitLength)
                {
                    this.Asv0 = asves[asves.Count - 1 - i];
                    break;
                }
                if (asvLength < i * splitLength)
                {
                    double startV = asves[asves.Count - i];
                    double endV = asves[asves.Count - 1 - i];
                    double startL = (i - 1) * splitLength;
                    double endL = (i) * splitLength;
                    double diffL = asvLength - startL;
                    this.Asv0 = Utils.DifferenceMethod(startL, endL, diffL, startV, endV);
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
