using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianHua.Publics.BaseCode;

namespace TianHua.FanSelection
{
    public static class FanDataModelExtension
    {
        //风机编号格式为：风机缩写-子项编号-楼层编号-风机序号:  ESF-3-F1-1
        public static string GetFanNum(this FanDataModel model)
        {
            string _FanNum = string.Empty;
            if (model.PID != "0") { return _FanNum; }
            var _PrefixDict = PubVar.g_ListFanPrefixDict.Find(p => p.FanUse == model.Scenario);
            if (_PrefixDict != null)
                _FanNum = _PrefixDict.Prefix;
            else
                _FanNum = " ";

            if (FuncStr.NullToStr(model.InstallSpace) != string.Empty && FuncStr.NullToStr(model.InstallSpace) != "未指定子项")
                _FanNum += "-" + model.InstallSpace;
            else
                _FanNum += "- ";

            if (FuncStr.NullToStr(model.InstallFloor) != string.Empty && FuncStr.NullToStr(model.InstallFloor) != "未指定楼层")
                _FanNum += "-" + model.InstallFloor;
            else
                _FanNum += "- ";

            if (FuncStr.NullToStr(model.VentNum) != string.Empty)
                _FanNum += "-" + model.VentNum;
            else
                _FanNum += "- ";

            return _FanNum;
        }
    }
}
