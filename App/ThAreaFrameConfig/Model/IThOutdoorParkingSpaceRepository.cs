using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    public interface IThOutdoorParkingSpaceRepository
    {
        List<ThOutdoorParkingSpace> Spaces
        {
            get;
        }

        void AppendDefaultOutdoorParkingSpace();
    }
}
