using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;

namespace ThStructureCheck.YJK.Interface
{
    public interface IEntityInf
    {
        double Length { get;}
        IEntity BuildGeometry();
    }
}
