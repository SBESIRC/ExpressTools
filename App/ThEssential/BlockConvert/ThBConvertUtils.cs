using System;

namespace ThEssential.BlockConvert
{
    /// <summary>
    /// 转换模式
    /// </summary>
    public enum ConvertMode
    {
        /// <summary>
        /// 弱电设备
        /// </summary>
        WEAKCURRENT = 0,
        /// <summary>
        /// 强电设备
        /// </summary>
        STRONGCURRENT = 1,
    }

    public static class ThBConvertUtils
    {
        /// <summary>
        /// 负载编号：“设备符号&-&楼层-编号”
        /// </summary>
        /// <param name="blockReference"></param>
        /// <returns></returns>
        public static string LoadSN(ThBConvertBlockReference blockReference)
        {
            try
            {
                var value = blockReference.StringValue(ThBConvertCommon.PROPERTY_EQUIPMENT_SYMBOL);
                if (string.IsNullOrEmpty(value))
                {
                    value = blockReference.StringValue(ThBConvertCommon.PROPERTY_FAN_TYPE);
                }
                return string.Format("{0}-{1}", value, 
                    blockReference.StringValue(ThBConvertCommon.PROPERTY_STOREY_AND_NUMBER));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 负载用途
        /// </summary>
        /// <param name="blockReference"></param>
        /// <returns></returns>
        public static string LoadUsage(ThBConvertBlockReference blockReference)
        {
            try
            {
                string quota = blockReference.StringValue(ThBConvertCommon.PROPERTY_QUOTA);
                string value = blockReference.StringValue(ThBConvertCommon.PROPERTY_FAN_USAGE);
                if (string.IsNullOrEmpty(value))
                {
                    value = blockReference.StringValue(ThBConvertCommon.PROPERTY_EQUIPMENT_NAME);
                }
                if (string.IsNullOrEmpty(value))
                {
                    value = blockReference.EffectiveName;
                }
                return string.Format("{0}({1})", value, quota);
            }
            catch
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// 获取属性（字符串）
        /// </summary>
        /// <param name="blockReference"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string StringValue(this ThBConvertBlockReference blockReference, string name)
        {
            try
            {
                return blockReference.Attributes[name] as string ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 是否为消防电源
        /// </summary>
        /// <param name="blockReference"></param>
        /// <returns></returns>
        public static bool IsFirePowerSupply(this ThBConvertBlockReference blockReference)
        {
            return blockReference.StringValue(ThBConvertCommon.PROPERTY_FIRE_POWER_SUPPLY) == "消防电源";
        }
        
        /// <summary>
        /// 块的转换比例
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static double Scale(this ThBlockConvertBlock block)
        {
            try
            {
                return Convert.ToDouble(block.Attributes[ThBConvertCommon.BLOCK_MAP_ATTRIBUTES_BLOCK_SCALE]);
            }
            catch
            {
                return 1.0;
            }
        }
    }
}
