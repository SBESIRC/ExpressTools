using System.Linq;
using System.Collections.Generic;

namespace ThAreaFrameConfig.Model
{
    public class ThResidentialRoomNullRepository : IThResidentialRoomRepository
    {
        private List<ThResidentialStorey> storeys;

        public ThResidentialRoomNullRepository()
        {
            storeys = new List<ThResidentialStorey>();
            AppendStorey("c1");
        }

        public void AppendStorey(string identifier)
        {
            storeys.Add(ThResidentialRoomUtil.DefaultResidentialStorey(identifier));
        }

        public void RemoveStorey(string identifier)
        {
            var storey = storeys.Where(o => o.Identifier == identifier).FirstOrDefault();
            if (storey != null)
            {
                storeys.Remove(storey);
            }
        }

        public IEnumerable<ThResidentialRoom> Rooms(string storey)
        {
            return storeys.Where(o => o.Identifier == storey).First().Rooms;
        }

        public IEnumerable<ThResidentialRoom> Rooms(ThResidentialStorey storey)
        {
            return Rooms(storey.Identifier);
        }

        public IEnumerable<ThResidentialStorey> Storeys()
        {
            return storeys;
        }
    }
}
