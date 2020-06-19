using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class ModelWallSegCompose
    {
        public ModelWallSegCompose()
        {
            this.Floor = new YjkFloor();
            this.WallSeg = new ModelWallSeg();
            this.WallSect = new ModelWallSect();
        }
        public YjkFloor Floor { get; set; }
        public ModelWallSeg WallSeg { get; set; }
        public ModelWallSect WallSect { get; set; }
    }
}
