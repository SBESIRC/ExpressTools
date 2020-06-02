using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class ModelColumnSegSectJoint
    {
        public YjkFloor Floor { get; set; }
        public ModelColumnSeg ColumnSeg { get; set; }
        public ModelColumnSect ColumnSect { get; set; }

        public YjkJoint Joint { get; set; }
    }
} 
