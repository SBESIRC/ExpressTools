using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;

namespace ThStructureCheck.Common.Model.Beam
{
    /// <summary>
    /// 弧梁几何形状
    /// </summary>
    public class ArcBeamGeometry : BeamGeometry, IEntity
    {
        public Coordinate CenterPoint { get; set; }
        public Coordinate TopPoint { get; set; }
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
