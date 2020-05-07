using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThSitePlan
{
    public interface IConfigManage
    {

        List<ColorGeneralDataModel> m_ListColorGeneral { get; set; }

        List<LayerDataModel> m_ListLayer { get; set; } 

        //List<string> m_ListLayer { get; set; }

        List<string> m_ListScript { get; set; }

    }
}
