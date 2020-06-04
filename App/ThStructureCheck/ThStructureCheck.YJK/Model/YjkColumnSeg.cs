using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class ModelColumnSeg : YjkEntityInfo
    {
        public int No_ { get; set; }
        public int StdFlrID { get; set; }
        public int SectID { get; set; }
        public int JtID { get; set; }
        public int EccX { get; set; }
        public int EccY { get; set; }
        public double Rotation { get; set; }
        public int HDiffB { get; set; }
        public int ColcapId { get; set; }
        public string Cut_Col { get; set; }
        public string Cut_Cap { get; set; }
        public string Cut_Slab { get; set; }
    }
    public class CalcColumnSeg : YjkEntityInfo
    {
        public int FlrNo { get; set; }
        public int TowNo { get; set; }
        public int MdlFlr { get; set; }
        public int MdlNo { get; set; }
        public int Jt1 { get; set; }
        public int Jt2 { get; set; }
    }
}
