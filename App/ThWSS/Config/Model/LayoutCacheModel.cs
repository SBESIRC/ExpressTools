using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThWSS.Config.Model
{
    public class LayoutCacheModel
    {
        public string tanName { get; set; }

        public Constraint constraint { get; set; }

        public string nozzleType { get; set; }

        public string protectRadius { get; set; }

        public HasBeam hasBeam { get; set; }

        public string layoutType { get; set; }
    }

    public class Constraint
    {
        public string constraintType { get; set; }

        public Standard standard { get; set; }

        public Custom custom { get; set; }
    }

    public class Standard
    {
        public string hazardLevel { get; set; }

        public string range { get; set; }
    }

    public class Custom
    {
        public string sparySpacing { get; set; }

        public string otherSpacing { get; set; }
    }

    public class HasBeam
    {
        public string considerBeam { get; set; }

        public string beamHeight { get; set; }

        public string plateThick { get; set; }
    }
}
