using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.ThBeamInfo.Service
{
    public class ThBeamColorManager
    {
        public static short ThColumnOutlineColorIndex = 2;
        public static short ThWallOutlineColorIndex = 5;

        public static short GetBeamColorIndex(BeamStatus bs)
        {
            short colorIndex = 0;
            switch(bs)
            {
                case BeamStatus.Primary:
                    colorIndex = 1;
                    break;
                case BeamStatus.Half:
                    colorIndex = 2;
                    break;
                case BeamStatus.Secondary:
                    colorIndex = 3;
                    break;
                case BeamStatus.Cantilever:
                    colorIndex = 4;
                    break;
            }
            return colorIndex;
        }
    }
}
