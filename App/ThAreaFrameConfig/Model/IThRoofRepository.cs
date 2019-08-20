using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
