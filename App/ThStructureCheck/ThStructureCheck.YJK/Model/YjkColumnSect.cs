using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class ModelColumnSect : YjkTableInfo
    {
        public int No_ { get; set; }
        public string Name { get; set; }
        public int Mat { get; set; }
        public int Kind { get; set; }
        public double ShapeVal { get; set; }
        public double ShapeVal1 { get; set; }
    }
}
