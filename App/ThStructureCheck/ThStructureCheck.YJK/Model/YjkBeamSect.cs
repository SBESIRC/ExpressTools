using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class ModelBeamSect: YjkEntityInfo
    {
        public int No_ { get; set; }
        public string Name { get; set; }
        public int Mat { get; set; }
        public int Kind { get; set; }
        public string ShapeVal { get; set; }
        public string ShapeVal1 { get; set; }
    }
}
