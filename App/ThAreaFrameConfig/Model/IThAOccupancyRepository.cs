using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    public interface IThAOccupancyRepository
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
