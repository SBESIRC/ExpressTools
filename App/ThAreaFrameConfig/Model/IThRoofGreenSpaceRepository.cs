using System.Collections.Generic;

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
