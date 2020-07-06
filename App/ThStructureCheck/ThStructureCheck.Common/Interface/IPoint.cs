using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.Common.Interface
{
    public interface IPoint
    {
        double X { get; set; }
        double Y { get; set; }
        double Z { get; set; }
        void ResetZ();
        void Set(double x, double y, double z);
    }
}
