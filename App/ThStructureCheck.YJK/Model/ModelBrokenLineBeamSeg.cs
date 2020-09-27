using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.Common.Model.Beam;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Model
{
    public class ModelBrokenLineBeamSeg : ModelBeamSeg, IEntityInf
    {
        public IEntity BuildGeometry()
        {
            return BuildBrokenLineBeamGeo();
        }
        public double Length => GetBeamLength();
        private double GetBeamLength()
        {
            //ToDo
            return 0.0;
        }
        private BrokenLineBeamGeometry BuildBrokenLineBeamGeo()
        {
            //ToDo
            return null;
        }
    }
}
