using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructure.BeamInfo.Model;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.ThBeamInfo.Service
{
    /// <summary>
    /// 匹配Yjk中的梁断和图纸中的梁断
    /// </summary>
    public class ThBeamMatch
    {
        private List<Tuple<YjkEntityInfo, Polyline>> yjkBeams =new List<Tuple<YjkEntityInfo, Polyline>>();
        private List<Beam> dwgBeams = new List<Beam>();
        public ThBeamMatch(List<Tuple<YjkEntityInfo, Polyline>> yjkBeams, List<Beam> dwgBeams)
        {
            this.yjkBeams = yjkBeams;
            this.dwgBeams = dwgBeams;
        }
        public void Match()
        {
            if(this.yjkBeams.Count==0 || this.dwgBeams.Count==0)
            {
                return;
            }
        }
    }
}
