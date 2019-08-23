using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    public interface IThResidentialRoomRepository
    {
        void AppendStorey(string identifier);
        void RemoveStorey(string identifier);

        IEnumerable<ThResidentialRoom> Rooms(string storey);

        IEnumerable<ThResidentialRoom> Rooms(ThResidentialStorey storey);

        IEnumerable<ThResidentialStorey> Storeys();
    }
}
