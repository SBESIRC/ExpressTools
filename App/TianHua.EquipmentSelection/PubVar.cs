using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.FanSelection
{
    public static class PubVar
    {
        public static List<FanPrefixDictDataModel> g_ListFanPrefixDict = new List<FanPrefixDictDataModel>()
        {
            new FanPrefixDictDataModel(){ No = 1, FanUse = "平时排风", Prefix ="EF", Explain = "包含燃烧和散热" },
            new FanPrefixDictDataModel(){ No = 2, FanUse = "平时送风", Prefix ="SF", Explain = "包含燃烧和散热" },
            new FanPrefixDictDataModel(){ No = 3, FanUse = "厨房排油烟", Prefix ="EKF", Explain = "" },

            new FanPrefixDictDataModel(){ No = 4, FanUse = "厨房排油烟补风", Prefix ="SF", Explain = "平时风机,自动备注" },
            new FanPrefixDictDataModel(){ No = 5, FanUse = "消防排烟", Prefix ="ESF", Explain = "" },
            new FanPrefixDictDataModel(){ No = 6, FanUse = "消防补风", Prefix ="SSF", Explain = "平时风机,自动备注" },
            new FanPrefixDictDataModel(){ No = 7, FanUse = "消防加压送风", Prefix ="SPF", Explain = "" },

            new FanPrefixDictDataModel(){ No = 8, FanUse = "事故排风", Prefix ="EF", Explain = "平时风机,自动备注" },
            new FanPrefixDictDataModel(){ No = 9, FanUse = "事故补风", Prefix ="SF", Explain = "平时风机,自动备注" },

            new FanPrefixDictDataModel(){ No = 10, FanUse = "平时排风兼消防排烟", Prefix ="E(S)F", Explain = "包含燃烧和散热" },
            new FanPrefixDictDataModel(){ No = 11, FanUse = "平时送风兼消防补风", Prefix ="S(S)F", Explain = "包含燃烧和散热" },

            new FanPrefixDictDataModel(){ No = 12, FanUse = "平时排风兼事故排风", Prefix ="EF", Explain = "平时风机,自动备注" },
            new FanPrefixDictDataModel(){ No = 13, FanUse = "平时送风兼事故补风", Prefix ="SF", Explain = "平时风机,自动备注" }
        };


        public static List<MotorEfficiency> g_ListMotorEfficiency = new List<MotorEfficiency>()
        {
            new MotorEfficiency(){ Key ="直连",Value =0.99 },

            new MotorEfficiency(){ Key ="皮带",Value =0.85 }
        };

    }
}
