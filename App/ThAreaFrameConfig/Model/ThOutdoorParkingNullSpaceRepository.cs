using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public class ThOutdoorParkingNullSpaceRepository : IThOutdoorParkingSpaceRepository
    {
        private List<ThOutdoorParkingSpace> spaces;

        public List<ThOutdoorParkingSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public ThOutdoorParkingNullSpaceRepository()
        {
            spaces = new List<ThOutdoorParkingSpace>();
            AppendDefaultOutdoorParkingSpace();
        }

        public void AppendDefaultOutdoorParkingSpace()
        {
            spaces.Add(new ThOutdoorParkingSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frame = (IntPtr)0,
                Storey = 0,
            });
        }
    }
}
