using System;
using System.Linq;

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
            if (style.Contains(ThFanSelectionCommon.HTFC_TYPE_NAME))
            {
                return ThFanSelectionCommon.HTFC_BLOCK_NAME;
            }
            else if (style.Contains(ThFanSelectionCommon.AXIAL_TYPE_NAME))
            {
                return ThFanSelectionCommon.AXIAL_BLOCK_NAME;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 属性“设备符号”值
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="installSpace"></param>
        /// <returns></returns>
        public static string Symbol(string scenario, string installSpace)
        {
            return string.Format("{0}-{1}", ScenarioPrefix(scenario), installSpace);
        }

        /// <summary>
        /// 属性“楼层-编号”值
        /// </summary>
        public static string StoreyNumber(string storey, string number)
        {
            return string.Format("{0}-{1}", storey, number);
        }

        /// <summary>
        /// 属性“变频”值
        /// </summary>
        /// <param name="control"></param>
        /// <param name="fre"></param>
        /// <returns></returns>
        public static string FixedFrequency(string control, bool fre)
        {
            if (fre)
            {
                if (control == ThFanSelectionCommon.BLOCK_ATTRIBUTE_VALUE_SINGLE_SPEED)
                {
                    return ThFanSelectionCommon.BLOCK_ATTRIBUTE_VALUE_VARIABLE_FREQUENCY;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                if (control == ThFanSelectionCommon.BLOCK_ATTRIBUTE_VALUE_SINGLE_SPEED)
                {
                    return ThFanSelectionCommon.BLOCK_ATTRIBUTE_VALUE_FIXED_FREQUENCY;
                }
                else
                {
                    return ThFanSelectionCommon.BLOCK_ATTRIBUTE_VALUE_DOUBLE_SPEED;
                }
            }
        }

        /// <summary>
        /// 属性“消防电源”值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FirePower(string type)
        {
            if (type == "消防")
            {
                return ThFanSelectionCommon.BLOCK_ATTRIBUTE_VALUE_FIRE_POWER;
            }
            else
            {
                return ThFanSelectionCommon.BLOCK_ATTRIBUTE_VALUE_NON_FIRE_POWER;
            }
        }

        /// <summary>
        /// 设备符号前缀
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public static string ScenarioPrefix(string scenario)
        {
            return PubVar.g_ListFanPrefixDict.Where(o => o.FanUse == scenario).First().Prefix;
        }

        /// <summary>
        /// 匹配模型名称和可见性名称
        /// </summary>
        /// <param name="model"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public static bool MatchModelName(string model, string visibility)
        {
            // 规则1：去掉模型名称最后面的字母
            return visibility == model.Substring(0, model.Length - 1);
        }
    }
}