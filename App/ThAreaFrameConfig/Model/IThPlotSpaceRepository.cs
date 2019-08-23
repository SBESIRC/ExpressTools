using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    public interface IThPlotSpaceRepository
    {
        List<ThPlotSpace> Spaces
        {
            get;
        }

        void AppendDefaultPlotSpace();
    }
}
