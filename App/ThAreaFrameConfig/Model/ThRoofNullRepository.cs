using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    internal class ThRoofNullRepository : IThRoofRepository
    {
        private List<ThRoof> roofs;

        public List<ThRoof> Roofs
        {
            get
            {
                return roofs;
            }
        }

        public ThRoofNullRepository()
        {
            roofs = new List<ThRoof>();
            AppendDefaultRoof();
        }

        public void AppendDefaultRoof()
        {
            roofs.Add(new ThRoof()
            {
                ID = Guid.NewGuid(),
                Number = roofs.Count + 1,
                Frame = ObjectId.Null.OldIdPtr,
                Coefficient = 1.0,
                FARCoefficient = 1.0
            });
        }
    }
}
