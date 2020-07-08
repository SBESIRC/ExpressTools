using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    /// <summary>
    /// 墙梁
    /// </summary>
    public class CalcWallBeam : YjkEntityInfo
    {
        public int TowNo { get; set; }
        public int FlrNo { get; set; }
        public int MdlFlr { get; set; }
        public int MdlNo { get; set; }
        public double B { get; set; }
        public double H { get; set; }
        public int Jt1 { get; set; }
        public int Jt2 { get; set; }
        public string Jts { get; set; }
    }
}
