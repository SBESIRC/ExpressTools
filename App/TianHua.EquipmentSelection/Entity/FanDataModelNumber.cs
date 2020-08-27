﻿using TianHua.Publics.BaseCode;

namespace TianHua.FanSelection
{
    public partial class FanDataModel
    {
        /// <summary>
        /// 风机编号
        /// </summary>
        public string FanNum
        {
            get
            {
                string _FanNum = string.Empty;
                if (PID != "0") { return _FanNum; }
                var _PrefixDict = PubVar.g_ListFanPrefixDict.Find(p => p.FanUse == Scenario);
                if (_PrefixDict != null)
                    _FanNum = _PrefixDict.Prefix;
                else
                    _FanNum = " ";

                if (FuncStr.NullToStr(InstallSpace) != string.Empty && FuncStr.NullToStr(InstallSpace) != "未指定子项")
                    _FanNum += "-" + InstallSpace;
                else
                    _FanNum += "- ";

                if (FuncStr.NullToStr(InstallFloor) != string.Empty && FuncStr.NullToStr(InstallFloor) != "未指定楼层")
                    _FanNum += "-" + InstallFloor;
                else
                    _FanNum += "- ";

                if (FuncStr.NullToStr(VentNum) != string.Empty)
                    _FanNum += "-" + VentNum;
                else
                    _FanNum += "- ";

                return _FanNum;
            }
        }
    }
}