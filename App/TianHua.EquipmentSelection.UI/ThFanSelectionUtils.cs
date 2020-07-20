using System;

namespace TianHua.FanSelection.UI
{
    public class ThFanSelectionUtils
    {
        /// <summary>
        /// 从风机形式获取图块名称
        /// </summary>
        /// <param name="ventStyle"></param>
        public static string BlockName(string style)
        {
            if (style.Contains("离心"))
            {
                return ThFanSelectionCommon.HTFC_BLOCK_NAME;
            }
            else if (style.Contains("轴流"))
            {
                return ThFanSelectionCommon.AXIAL_BLOCK_NAME;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
