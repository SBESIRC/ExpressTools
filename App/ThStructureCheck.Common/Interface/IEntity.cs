using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.Common.Interface
{
    public interface IEntity
    {
        Entity Draw();
    }
}
