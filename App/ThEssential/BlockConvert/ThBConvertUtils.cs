using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThEssential.BlockConvert
{
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
                return string.Format("{0}-{1}",
                    blockReference.Attributes[ThBConvertCommon.PROPERTY_EQUIPMENT_SYMBOL],
                    blockReference.Attributes[ThBConvertCommon.PROPERTY_STOREY_AND_NUMBER]);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 电量
        /// </summary>
        /// <param name="blockReference"></param>
        /// <returns></returns>
        public static string PowerQuantity(ThBConvertBlockReference blockReference)
        {
            try
            {
                return blockReference.Attributes[ThBConvertCommon.PROPERTY_POWER_QUANTITY];
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
                return blockReference.Attributes[ThBConvertCommon.PROPERTY_LOAD_USAGE];
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
