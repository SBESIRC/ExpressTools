using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class ModelColumnSegCompose
    {
        public ModelColumnSegCompose()
        {
            this.Floor = new YjkFloor();
            this.ColumnSeg = new ModelColumnSeg();
            this.ColumnSect = new ModelColumnSect();
            this.Joint = new ModelJoint();
        }
        public YjkFloor Floor { get; set; }
        public ModelColumnSeg ColumnSeg { get; set; }
        public ModelColumnSect ColumnSect { get; set; }

        public ModelJoint Joint { get; set; }
    }
} 
