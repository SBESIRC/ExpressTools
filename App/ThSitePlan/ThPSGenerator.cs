using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThSitePlan
{
    public abstract class ThPSGenerator
    {
        public abstract List<string> InputPDFPth { get; set; }
        public abstract string OutputPDSPth { get; set; }

        public abstract void GeneratorInPS();
    }
}