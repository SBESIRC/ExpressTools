using System;
using TianHua.Publics.BaseCode;
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

                // 风机功能
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_USAGE] = model.Name,

                // 风量
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_VOLUME] = Convert.ToString(model.AirVolume),

                // 全压
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_PRESSURE] = model.FanModelPa,

                // 电量
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_CHARGE] = model.FanModelMotorPower,

                // 定频
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FIXED_FREQUENCY] = ThFanSelectionUtils.FixedFrequency(model.Control, model.IsFre),

                // 消防电源
                [ThFanSelectionCommon.BLOCK_ATTRIBUTE_FIRE_POWER_SUPPLY] = ThFanSelectionUtils.FirePower(model.PowerType),
            };
            return attributes;
        }

        public static bool IsModified(this FanDataModel model, Dictionary<string, string> attributes)
        {
            // 设备符号
            if (attributes.ContainsKey(ThFanSelectionCommon.BLOCK_ATTRIBUTE_EQUIPMENT_SYMBOL))
            {
                if (attributes[ThFanSelectionCommon.BLOCK_ATTRIBUTE_EQUIPMENT_SYMBOL] 
                    != ThFanSelectionUtils.Symbol(model.Scenario, model.InstallSpace))
                {
                    return true;
                }
            }
            else
            {
                throw new ArgumentException();
            }

            // 楼层-编号
            if (attributes.ContainsKey(ThFanSelectionCommon.BLOCK_ATTRIBUTE_STOREY_AND_NUMBER))
            {
                if (attributes[ThFanSelectionCommon.BLOCK_ATTRIBUTE_STOREY_AND_NUMBER] !=
                    ThFanSelectionUtils.StoreyNumber(model.InstallFloor, model.VentNum))
                {
                    return true;
                }
            }
            else
            {
                throw new ArgumentException();
            }

            // 风机功能
            if (attributes.ContainsKey(ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_USAGE))
            {
                if(attributes[ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_USAGE] != model.Name)
                {
                    return true;
                }
            }
            else
            {
                throw new ArgumentException();
            }

            // 风量
            if (attributes.ContainsKey(ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_VOLUME))
            {
                if (FuncStr.NullToInt(attributes[ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_VOLUME]) 
                    != model.AirVolume)
                {
                    return true;
                }
            }
            else
            {
                throw new ArgumentException();
            }

            // 全压
            if (attributes.ContainsKey(ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_PRESSURE))
            {
                if (FuncStr.NullToInt(attributes[ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_PRESSURE]) 
                    != FuncStr.NullToInt(model.FanModelPa))
                {
                    return true;
                }
            }
            else
            {
                throw new ArgumentException();
            }

            // 电量
            if (attributes.ContainsKey(ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_CHARGE))
            {
                
                if (FuncStr.NullToDouble(attributes[ThFanSelectionCommon.BLOCK_ATTRIBUTE_FAN_CHARGE]) 
                    != FuncStr.NullToDouble(model.FanModelMotorPower))
                {
                    return true;
                }
            }
            else
            {
                throw new ArgumentException();
            }

            // 定频
            if (attributes.ContainsKey(ThFanSelectionCommon.BLOCK_ATTRIBUTE_FIXED_FREQUENCY))
            {
                if (attributes[ThFanSelectionCommon.BLOCK_ATTRIBUTE_FIXED_FREQUENCY] 
                    != ThFanSelectionUtils.FixedFrequency(model.Control, model.IsFre))
                {
                    return true;
                }
            }
            else
            {
                throw new ArgumentException();
            }

            // 消防电源
            if (attributes.ContainsKey(ThFanSelectionCommon.BLOCK_ATTRIBUTE_FIRE_POWER_SUPPLY))
            {
                if (attributes[ThFanSelectionCommon.BLOCK_ATTRIBUTE_FIRE_POWER_SUPPLY]
                    != ThFanSelectionUtils.FirePower(model.PowerType))
                {
                    return true;
                }
            }
            else
            {
                throw new ArgumentException();
            }

            return false;
        }
    }
}
