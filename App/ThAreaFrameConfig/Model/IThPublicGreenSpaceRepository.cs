using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public interface IThPublicGreenSpaceRepository
    {
        List<ThPublicGreenSpace> Spaces
        {
            get;
        }

        void AppendDefaultPublicGreenSpace();
    }
}
