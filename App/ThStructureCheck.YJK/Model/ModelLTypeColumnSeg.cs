using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.Common.Model.Column;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Model
{
    public class ModelLTypeColumnSeg : ModelColumnSeg, IEntityInf
    {
        public double Length => 0.0; //暂时不计算

        public IEntity BuildGeometry()
        {
            //Todo 后续要增加弧梁
            return LTypeColumnGeo();
        }
        public LTypeColumnGeometry LTypeColumnGeo()
        {
            //ToDo
            return null;
        }
    }
}
