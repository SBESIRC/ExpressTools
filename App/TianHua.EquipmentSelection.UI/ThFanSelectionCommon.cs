using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.FanSelection.UI
{
    public class ThFanSelectionCommon
    {
        public static readonly string HTFC_BLOCK_NAME = "HTFC(DF)柜式离心风机";
        public static readonly string AXIAL_BLOCK_NAME = "消防高温排烟专用风机";
        public static readonly string BLOCK_FAN_LAYER = "H-EQUP-FANS";
        public static readonly string BLOCK_FAN_FILE = "暖通.选型.风机.dwg";
        public static readonly string HTFC_Selection = "离心风机选型.json";
        public static readonly string HTFC_Parameters = "离心风机参数.json";
        public static readonly string AXIAL_Selection = "轴流风机选型.json";
        public static readonly string AXIAL_Parameters = "轴流风机参数.json";
        public static readonly string RegAppName_FanSelection = "THCAD_FAN_SELECTION";

        public static readonly string BLOCK_ATTRIBUTE_EQUIPMENT_SYMBOL = "设备符号";
        public static readonly string BLOCK_ATTRIBUTE_STOREY_AND_NUMBER = "楼层-编号";
        public static readonly string BLOCK_ATTRIBUTE_FAN_USAGE = "风机功能";
        public static readonly string BLOCK_ATTRIBUTE_FAN_VOLUME = "风量";
        public static readonly string BLOCK_ATTRIBUTE_FAN_PRESSURE = "全压";
        public static readonly string BLOCK_ATTRIBUTE_FAN_CHARGE = "电量";
        public static readonly string BLOCK_ATTRIBUTE_FIXED_FREQUENCY = "定频";
        public static readonly string BLOCK_ATTRIBUTE_FIRE_POWER_SUPPLY = "消防电源";
        public static readonly string BLOCK_DYNAMIC_PROPERTY_VISIBILITY = "可见性";
        public static readonly string BLOCK_DYNAMIC_PROPERTY_VISIBILITY2 = "风机箱型号可见性";

        public static readonly string BLOCK_ATTRIBUTE_VALUE_FIXED_FREQUENCY = "定频";
        public static readonly string BLOCK_ATTRIBUTE_VALUE_VARIABLE_FREQUENCY = "变频";
        public static readonly string BLOCK_ATTRIBUTE_VALUE_DUAL_FREQUENCY = "双频";
        public static readonly string BLOCK_ATTRIBUTE_VALUE_SINGLE_SPEED = "单速";
        public static readonly string BLOCK_ATTRIBUTE_VALUE_DOUBLE_SPEED = "双速";
        public static readonly string BLOCK_ATTRIBUTE_VALUE_FIRE_POWER = "消防电源";
        public static readonly string BLOCK_ATTRIBUTE_VALUE_NON_FIRE_POWER = "非消防电源";
    }
}
