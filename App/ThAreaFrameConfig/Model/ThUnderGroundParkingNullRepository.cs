using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    class ThUnderGroundParkingNullRepository : IThUnderGroundParkingRepository
    {
        private List<ThUnderGroundParking> parkings;

        public List<ThUnderGroundParking> Parkings
        {
            get
            {
                return parkings;
            }
        }

        public ThUnderGroundParkingNullRepository()
        {
            parkings = new List<ThUnderGroundParking>();
            AppendDefaultUnderGroundParking();
        }

        public void AppendDefaultUnderGroundParking()
        {
            parkings.Add(new ThUnderGroundParking()
            {
                ID = Guid.NewGuid(),
                Number = parkings.Count + 1,
                Floors = 1,
                Frames = new List<IntPtr>(),
            });
        }
    }
}
