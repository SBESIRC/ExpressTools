using System;
using System.Collections.Generic;

namespace TianHua.FanSelection.UI
{
    public static class FanDataModelExtension
    {
        public static Dictionary<string, string> Attributes(this FanDataModel model)
        {
            var attributes = new Dictionary<string, string>
            {
                // 设备符号
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_EQUIPMENT_SYMBOL] = ThFanSelectionUtils.Symbol(model.Scenario, model.InstallSpace),

                // 楼层-编号
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_STOREY_AND_NUMBER] = ThFanSelectionUtils.StoreyNumber(model.InstallFloor, model.VentNum),

                // 风机功能
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_USAGE] = model.Name,

                // 风量
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_VOLUME] = Convert.ToString(model.AirVolume),

                // 全压
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_PRESSURE] = Convert.ToString(model.FanModelPa),

                // 电量
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_CHARGE] = string.Empty,

                // 定频
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FIXED_FREQUENCY] = ThFanSelectionUtils.FixedFrequency(model.Control, model.IsFre),

                // 消防电源
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FIRE_POWER_SUPPLY] = ThFanSelectionUtils.FirePower(model.PowerType),
            };
            return attributes;
        }
    }
}
