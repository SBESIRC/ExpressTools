using System.Collections.Generic;

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
