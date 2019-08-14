using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public interface IThResidentialRoomRepository
    {
        IEnumerable<ThResidentialStorey> Storeys();
        IEnumerable<ThResidentialRoom> Rooms(string storey);
        IEnumerable<ThResidentialRoom> Rooms(ThResidentialStorey storey);
    }
}
