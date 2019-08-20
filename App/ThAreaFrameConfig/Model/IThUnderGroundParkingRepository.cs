using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
