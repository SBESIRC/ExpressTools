using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.Common.Model.Wall;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Model
{
    public class ModelArcWallSeg : ModelWallSeg, IEntityInf
    {
        public IEntity BuildGeometry()
        {
            return BuildArcWallGeometry();
        }
        public double Length => GetBeamLength(); //ToDo
        private ArcWallGeometry BuildArcWallGeometry()
        {
            //Todo
            return null;
        }
        private double GetBeamLength()
        {
            //ToDo
            return 0.0;
        }
    }
}
