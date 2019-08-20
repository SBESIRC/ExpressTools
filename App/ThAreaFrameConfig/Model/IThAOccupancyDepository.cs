using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public interface IThAOccupancyDepository
    {
        List<ThAOccupancyStorey> Storeys
        {
            get;
        }

        void AppendStorey(string identifier);

        List<ThAOccupancy> AOccupancies(string storey);
        List<ThAOccupancy> AOccupancies(ThAOccupancyStorey storey);
    
    }
}
