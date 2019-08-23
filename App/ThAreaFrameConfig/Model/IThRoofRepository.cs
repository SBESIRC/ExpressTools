using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    public interface IThRoofRepository
    {
        List<ThRoof> Roofs
        {
            get;
        }

        void AppendDefaultRoof();
    }
}
