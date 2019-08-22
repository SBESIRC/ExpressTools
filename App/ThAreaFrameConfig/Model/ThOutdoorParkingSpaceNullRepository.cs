using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public class ThOutdoorParkingSpaceNullRepository : IThOutdoorParkingSpaceRepository
    {
        private List<ThOutdoorParkingSpace> spaces;

        public List<ThOutdoorParkingSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public ThOutdoorParkingSpaceNullRepository()
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
