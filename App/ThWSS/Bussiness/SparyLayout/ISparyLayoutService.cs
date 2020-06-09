using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThWSS.Model;

namespace ThWSS.Bussiness.SparyLayout
{
    public interface ISparyLayoutService
    {
        void LayoutSpray(List<Polyline> roomsLine, SparyLayoutModel layoutModel);
    }
}
