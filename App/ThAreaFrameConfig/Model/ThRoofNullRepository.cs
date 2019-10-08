using System;
using System.Collections.Generic;

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
                Category = "住宅",
                Frame = (IntPtr)0,
                Coefficient = 1.0,
                FARCoefficient = 1.0
            });
        }
    }
}
