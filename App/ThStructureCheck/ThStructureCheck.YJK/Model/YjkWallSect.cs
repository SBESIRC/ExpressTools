using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class ModelWallSect : YjkEntityInfo
    {
        public int No_ { get; set; }
        public int Mat { get; set; }
        public int Kind { get; set; }
        public int B { get; set; }
        public int H { get; set; }
        public int T2 { get; set; }
        public double Dis { get; set; }
        public string Colsect1 { get; set; }
        public string ColShapeVal1 { get; set; }
        public string Colsect2 { get; set; }
        public string ColShapeVal2 { get; set; }
    }
}
