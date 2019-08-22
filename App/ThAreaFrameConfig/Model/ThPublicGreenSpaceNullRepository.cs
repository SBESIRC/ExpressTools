using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Model
{
    public class ThPublicGreenSpaceNullRepository : IThPublicGreenSpaceRepository
    {
        private List<ThPublicGreenSpace> spaces;

        public List<ThPublicGreenSpace> Spaces
        {
            get
            {
                return spaces;
            }
        }

        public ThPublicGreenSpaceNullRepository()
        {
            spaces = new List<ThPublicGreenSpace>();
            AppendDefaultPublicGreenSpace();
        }

        public void AppendDefaultPublicGreenSpace()
        {
            spaces.Add(new ThPublicGreenSpace()
            {
                ID = Guid.NewGuid(),
                Number = spaces.Count + 1,
                Frame = (IntPtr)0,
            });
        }
    }
}
