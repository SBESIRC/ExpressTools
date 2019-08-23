using System;
using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    public class ThPlotSpaceNullRepository : IThPlotSpaceRepository
    {
        private List<ThPlotSpace> spaces;

        public List<ThPlotSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public ThPlotSpaceNullRepository()
        {
            spaces = new List<ThPlotSpace>();
            AppendDefaultPlotSpace();
        }

        public void AppendDefaultPlotSpace()
        {
            spaces.Add(new ThPlotSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frame = (IntPtr)0,
                HouseHold = 0
            });
        }
    }
}
