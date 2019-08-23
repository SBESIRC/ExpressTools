using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    public interface IThUnderGroundParkingRepository
    {
        List<ThUnderGroundParking> Parkings
        {
            get;
        }

        void AppendDefaultUnderGroundParking();
    }
}
