using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;

namespace ThStructureCheck.Common.Model.Beam
{
    public class BrokenLineBeamGeometry : BeamGeometry, IEntity
    {
        public Entity Draw()
        {
            throw new NotImplementedException();
        }

        public override Polyline DrawCenterLine()
        {
            throw new NotImplementedException();
        }
    }
}
