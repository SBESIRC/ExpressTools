using System;
using System.Collections.Generic;
using System.Linq;

namespace ThAreaFrameConfig.Model
{
    public class ThAOccupancyNullDepository : IThAOccupancyDepository
    {
        private List<ThAOccupancyStorey> storeys;

        public List<ThAOccupancyStorey> Storeys
        {
            get
            {
                return storeys;
            }
        }

        public ThAOccupancyNullDepository()
        {
            storeys = new List<ThAOccupancyStorey>();

        }

        public void AppendStorey(string identifier)
        {
            storeys.Add(DefaultAOccupancyStorey(identifier));
        }

        public List<ThAOccupancy> AOccupancies(string storey)
        {
            return storeys.Where(o => o.Identifier == storey).First().AOccupancies;
        }

        public List<ThAOccupancy> AOccupancies(ThAOccupancyStorey storey)
        {
            return AOccupancies(storey.Identifier);
        }

        private ThAOccupancy DefaultAOccupancy(Guid storeyId)
        {
            return new ThAOccupancy()
            {
                ID = Guid.NewGuid(),
                StoreyID = storeyId,
                Number = 1,
                Component = "主体",
                Category = "商业",
                Coefficient = 1.0,
                FARCoefficient = 1.0,
                Floors = null,
                Frame = (IntPtr)0
            };
        }

        private ThAOccupancyStorey DefaultAOccupancyStorey(string storey)
        {
            Guid guid = Guid.NewGuid();
            return new ThAOccupancyStorey()
            {
                ID = guid,
                Identifier = storey,
                AOccupancies = new List<ThAOccupancy>()
                {
                    DefaultAOccupancy(guid)
                }
            };
        }
    }
}
