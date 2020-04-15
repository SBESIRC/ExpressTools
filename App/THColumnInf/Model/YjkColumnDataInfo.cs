using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class YjkColumnDataInfo
    {
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
    }
}
