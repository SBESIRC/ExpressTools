using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    /// <summary>
    /// 银建科计算书信息，用于计算书校对
    /// </summary>
    class YjkColumnCalculateInfo: ICalculateInfo
    {
        private string dtlModelPath = "";
        private string dtlCalcPath = "";
        public YjkColumnCalculateInfo(string dtlModelPath,string dtlCalcPath)
        {
            this.dtlModelPath = dtlModelPath;
            this.dtlCalcPath = dtlCalcPath;
        }
        private int writeNumber = 0;
        private string antiSeismicGrade = ""; //抗震等级

        private double protectThickness; //保护层厚度

        private double jkb; //剪跨比

        private double axialCompressionRatio; //轴压比

        private double axialCompressionRatioLimited; //轴压比限值

        private double arDiaLimited; //角筋直径

        private bool isCorner ; //（角柱，框架柱）

        private string structureType = ""; //结构类型

        private double dblXAsCal; //配筋面积限值(X向限值)

        private double dblYAsCal; //配筋面积限值(Y向限值)

        private double fortiCation = 0.0; //设防烈度

        private double volumeReinforceLimitedValue = 0.0; //体积配筋率限值

        private double dblStirrupAsCal=0.0; //配筋面积限值

        private double dblStirrupAsCal0 = 0.0; //配筋面积限值

        private double intStirrupSpacingCal = 0.0; //假定箍筋间距

        private bool isGroundFloor = false; //是否底层
        /// <summary>
        /// 抗震等级
        /// </summary>
        public string AntiSeismicGrade { get => antiSeismicGrade; set => antiSeismicGrade = value; }
        /// <summary>
        /// 保护层厚度
        /// </summary>
        public double ProtectThickness { get => protectThickness; set => protectThickness = value; }
        /// <summary>
        /// 剪跨比
        /// </summary>
        public double Jkb { get => jkb; set => jkb = value; }
        /// <summary>
        /// 轴压比
        /// </summary>
        public double AxialCompressionRatio { get => axialCompressionRatio; set => axialCompressionRatio = value; }
        /// <summary>
        /// 轴压比限值
        /// </summary>
        public double AxialCompressionRatioLimited { get => axialCompressionRatioLimited; set => axialCompressionRatioLimited = value; }
        /// <summary>
        /// 角筋直径
        /// </summary>
        public double ArDiaLimited { get => arDiaLimited; set => arDiaLimited = value; }
        /// <summary>
        /// 是否角柱
        /// </summary>
        public bool IsCorner { get => isCorner; set => isCorner = value; }
        /// <summary>
        /// 结构类型
        /// </summary>
        public string StructureType { get => structureType; set => structureType = value; }
        /// <summary>
        /// 配筋面积限值(X向限值)
        /// </summary>
        public double DblXAsCal { get => dblXAsCal; set => dblXAsCal = value; }
        /// <summary>
        /// 配筋面积限值(Y向限值)
        /// </summary>
        public double DblYAsCal { get => dblYAsCal; set => dblYAsCal = value; }
        /// <summary>
        /// 设防烈度
        /// </summary>
        public double FortiCation { get => fortiCation; set => fortiCation = value; }
        /// <summary>
        /// 体积配筋率限值
        /// </summary>
        public double VolumeReinforceLimitedValue { get => volumeReinforceLimitedValue; set => volumeReinforceLimitedValue = value; }
        /// <summary>
        /// 配筋面积限值
        /// </summary>
        public double DblStirrupAsCal { get => dblStirrupAsCal; set => dblStirrupAsCal = value; }
        /// <summary>
        /// 配筋面积限值
        /// </summary>
        public double DblStirrupAsCal0 { get => dblStirrupAsCal0; set => dblStirrupAsCal0 = value; }
        /// <summary>
        /// 假定箍筋间距
        /// </summary>
        public double IntStirrupSpacingCal { get => intStirrupSpacingCal; set => intStirrupSpacingCal = value; }
        /// <summary>
        /// 写入次数
        /// </summary>
        public int WriteNumber { get => writeNumber; set => writeNumber = value; }
        /// <summary>
        /// 是否底层
        /// </summary>
        public bool IsGroundFloor { get => isGroundFloor; set => isGroundFloor = value; }

        public ICalculateInfo GetCalculateInfo(IEntityInf entInf)
        {
            if(!(entInf is ModelColumnSeg))
            {
                return null;
            }
            ModelColumnSeg modelColumnSeg = entInf as ModelColumnSeg;
            YjkColumnQuery dtlModelQuery = new YjkColumnQuery(this.dtlModelPath);
            YjkColumnQuery dtlCalcQuery = new YjkColumnQuery(this.dtlCalcPath);
            //获取jtId对应的自然层编号和柱编号（dtlModel）
            int tblFloorNo, tblColSegNo;
            dtlModelQuery.GetTblFloorTblColSegNoFromDtlmodel(modelColumnSeg.JtID, out tblFloorNo, out tblColSegNo);

            //获取dtlCalc库中的柱子ID
            int columnId = dtlCalcQuery.GetTblColSegIDFromDtlCalc(tblFloorNo, tblColSegNo);

            //获取剪跨比(dtlCalc)
            bool resJkb = dtlCalcQuery.GetShearSpanRatio(columnId, out jkb);
            this.Jkb = jkb;

            //获取轴压比和轴压比限值(dtlCalc)
            double axialCompressionRatio;
            double axialCompressionRatioLimited;
            bool resCompressionRatio = dtlCalcQuery.GetAxialCompressionRatio(columnId,
                out axialCompressionRatio, out axialCompressionRatioLimited);
            this.AxialCompressionRatio = axialCompressionRatio;
            this.AxialCompressionRatioLimited = axialCompressionRatioLimited;

            //获取角筋直径限值(dtlCalc)
            double arDiaLimited = 0.0;
            bool resArDiaLimited = dtlCalcQuery.GetAngularReinforcementDiaLimited(columnId, out arDiaLimited);
            this.ArDiaLimited = arDiaLimited;

            //获取抗震等级(dtlCalc)
            double antiSeismicGradeParaValue = dtlCalcQuery.GetAntiSeismicGradeInCalculation(columnId);
            if (antiSeismicGradeParaValue != 0.0)
            {
                this.AntiSeismicGrade = ThYjkDatbaseExtension.GetAntiSeismicGrade(antiSeismicGradeParaValue);
            }
            else
            {
                List<double> antiSeismicGradeValues = dtlModelQuery.GetAntiSeismicGradeInModel();
                this.AntiSeismicGrade = ThYjkDatbaseExtension.GetAntiSeismicGrade(antiSeismicGradeValues[0], antiSeismicGradeValues[1]);
            }
            //获取保护层厚度
            double protectThickness = 0.0;
            bool findRes = dtlCalcQuery.GetProtectLayerThickInTblColSegPara(
                columnId, out protectThickness);
            if (!findRes)
            {
                findRes = dtlModelQuery.GetProtectLayerThickInTblStdFlrPara(
                    modelColumnSeg.StdFlrID, out protectThickness);
                if (!findRes)
                {
                    findRes = dtlModelQuery.GetProtectLayerThickInTblProjectPara(out protectThickness);
                }
            }
            this.ProtectThickness = protectThickness;

            //判断是否为角柱
            this.IsCorner = dtlModelQuery.CheckColumnIsCorner(modelColumnSeg.JtID);

            double structureParaValue = dtlModelQuery.GetStructureTypeInModel();
            this.StructureType = ThYjkDatbaseExtension.GetStructureType(structureParaValue);

            //获取配筋面积限值
            List<double> values = dtlCalcQuery.GetDblXYAsCal(columnId);
            if (values.Count == 2)
            {
                this.DblXAsCal = values[0];
                this.DblYAsCal = values[1];
            }
            //是否底层
            this.IsGroundFloor = dtlModelQuery.CheckIsGroundFloor(modelColumnSeg.StdFlrID);

            //获取设防烈度
            double fortiCation = 0.0;
            if (dtlModelQuery.GetFortificationIntensity(out fortiCation))
            {
                double fortiCationRealValue = 0.0;
                if (ThYjkDatbaseExtension.GetFortiCation(fortiCation, out fortiCationRealValue))
                {
                    this.FortiCation = fortiCationRealValue;
                }
            }

            //获取体积配筋率限值
            double volumeReinforceLimitedValue = 0.0;
            bool resVRLV = dtlCalcQuery.GetVolumeReinforceLimitedValue(
                columnId, out volumeReinforceLimitedValue);
            this.VolumeReinforceLimitedValue = volumeReinforceLimitedValue;

            //获取配筋面积限值
            double dblStirrupAsCal = dtlCalcQuery.GetDblStirrupAsCalLimited(columnId);
            double dblStirrupAsCal0 = dtlCalcQuery.GetDblStirrupAsCal0Limited(columnId);

            this.DblStirrupAsCal = dblStirrupAsCal;
            this.DblStirrupAsCal0 = dblStirrupAsCal0;

            //假定箍筋间距
            double intStirrupSpacingCal = 0.0;
            bool resIntStirrupSpacingCal = dtlModelQuery.GetIntStirrupSpacingCal(out intStirrupSpacingCal);
            this.IntStirrupSpacingCal = intStirrupSpacingCal;
            return this;
        }
    }
}
