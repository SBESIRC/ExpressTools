using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class ModelBeamSeg : YjkTableInfo
    {
        public int No_ { get; set; }
        public int StdFlrID { get; set; }
        public int SectID { get; set; }
        public int GridID { get; set; }
        public int Ecc { get; set; }
        public int HDiff1 { get; set; }
        public int HDiff2 { get; set; }
        public double Rotation { get; set; }
    }
    public class CalcBeamSeg : YjkTableInfo
    {
        public int TowNo { get; set; }
        public int FlrNo { get; set; }
        public int MdlFlr { get; set; }
        public int MdlNo { get; set; }
        public int Jt1 { get; set; }
        public int Jt2 { get; set; }
    }
}
