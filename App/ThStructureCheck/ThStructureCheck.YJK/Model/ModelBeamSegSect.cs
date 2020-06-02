using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class ModelBeamSegSect
    {
        public YjkFloor Floor { get; set; }
        public ModelBeamSeg BeamSeg { get; set; }
        public ModelBeamSect BeamSect { get; set; }
    }
}
