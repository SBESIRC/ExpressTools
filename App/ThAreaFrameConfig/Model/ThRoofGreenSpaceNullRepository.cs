using System;
using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    internal class ThRoofGreenSpaceNullRepository : IThRoofGreenSpaceRepository
    {
        private List<ThRoofGreenSpace> spaces;

        public List<ThRoofGreenSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public ThRoofGreenSpaceNullRepository()
        {
            spaces = new List<ThRoofGreenSpace>();
            AppendRoofGreenSpace();
        }

        public void AppendRoofGreenSpace()
        {
            spaces.Add(new ThRoofGreenSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frame = (IntPtr)0,
                Coefficient = 1.0
            });
        }
    }
}
