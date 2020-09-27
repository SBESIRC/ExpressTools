﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.FanSelection
{
    public interface IFanSelection
    {

        List<FanDataModel> m_ListFan { get; set; }

        List<string> m_ListScenario { get; set; }

        List<string> m_ListVentStyle { get; set; }

        List<string> m_ListVentConnect { get; set; }

        List<string> m_ListVentLev { get; set; }

        List<string> m_ListEleLev { get; set; }

        List<int> m_ListMotorTempo { get; set; }

        List<string> m_ListMountType { get; set; }

        List<FanDesignDataModel> m_ListFanDesign { get; set; }

        FanDesignDataModel m_FanDesign { get; set; }
    }
}
