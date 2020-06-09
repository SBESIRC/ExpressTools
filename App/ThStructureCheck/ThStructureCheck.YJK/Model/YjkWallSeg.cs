using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Model
{
    public class ModelWallSeg : YjkEntityInfo
    {
        public int No_ { get; set; }
        public int StdFlrID { get; set; }
        public int SectID { get; set; }
        public int GridID { get; set; }
        public int Ecc { get; set; }
        public int HDiff1 { get; set; }
        public int HDiff2 { get; set; }
        public int HDiffB { get; set; }
        public int sloping { get; set; }
        public int EccDown { get; set; }
        public int offset1 { get; set; }
        public int offset2 { get; set; }
        public ModelGrid Grid
        {
            get
            {
                return new YjkGridQuery(this.DbPath).GetModelGrid(this.GridID);
            }
        }
    }
}
