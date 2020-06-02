using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class YjkJoint : YjkTableInfo
    {
        public int No_ { get; set; }
        public int StdFlrID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int HDiff { get; set; }
    }
}
