using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK
{
    class MergeFloorBeams
    {
        private int floorNo;
        private string dtlModelPath = "";
        private string dtlCalcPath = "";
        private List<ModelColumnSeg> modelColumnSegs = new List<ModelColumnSeg>();
        private List<ModelBeamSeg> modelBeamSegs = new List<ModelBeamSeg>();

        public MergeFloorBeams(string dtlModelPath,string dtlCalcPath,int floorNo)
        {
            this.floorNo = floorNo;
            this.dtlModelPath = dtlModelPath;
            this.dtlCalcPath = dtlModelPath;
            Init();
        }
        private void Init()
        {
            //this.modelBeamSegs = new YjkBeamQuery(this.dtlModelPath).;
        }
        public void Run()
        {

        }

    }
}
