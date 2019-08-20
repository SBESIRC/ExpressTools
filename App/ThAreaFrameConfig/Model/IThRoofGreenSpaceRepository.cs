﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public interface IThRoofGreenSpaceRepository
    {
        List<ThRoofGreenSpace> Spaces
        {
            get;
        }

        void AppendRoofGreenSpace();
    }
}
