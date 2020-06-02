using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    class MergeBeams
    {
        public bool IsPrimary { get; set; }
        public CalcBeamSeg StartLink { get; set; }
        public CalcBeamSeg EndLink { get; set; }
        public List<CalcBeamSeg> Merges { get; set; }
    }
}
