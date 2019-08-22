﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
