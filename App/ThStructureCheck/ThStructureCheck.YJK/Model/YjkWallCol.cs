using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class CalcWallCol : YjkEntityInfo
    {
        public int TowNo { get; set; }
        public int FlrNo { get; set; }
        public int MdlFlr { get; set; }
        public string MdlNos { get; set; }
        public string MdlPos { get; set; }
        public int JtLB { get; set; }
        public int JtRB { get; set; }
        public int JtRT { get; set; }
        public int JtLT { get; set; }
        public string JtsT { get; set; }
        public string JtsB { get; set; }
    }
}
