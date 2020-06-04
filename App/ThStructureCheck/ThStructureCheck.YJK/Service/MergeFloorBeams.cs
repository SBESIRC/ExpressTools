using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK.Service
{
    class MergeFloorBeams
    {
        private int floorNo;
        public static string dtlModelPath = "";
        public static string dtlCalcPath = "";
        private List<ModelColumnSegCompose> modelColumnSegs = new List<ModelColumnSegCompose>();
        private List<ModelBeamSegCompose> modelBeamSegs = new List<ModelBeamSegCompose>();

        private List<CalcColumnSeg> calcColumnSegs = new List<CalcColumnSeg>();
        private List<CalcBeamSeg> calcBeamSegs = new List<CalcBeamSeg>();

        public MergeFloorBeams(string modelPath,string calcPath,int floorNo)
        {
            this.floorNo = floorNo;
            dtlModelPath = modelPath;
            dtlCalcPath = dtlModelPath;
        }

        public void Run()
        {
            GetAllColumnBeamSeg();
            GetAllCalcColumnSeg();
            GetAllCalcBeamSeg();

            FindColumnBeamLink();
        }
        /// <summary>
        /// 查找所有与柱子相连接的主梁
        /// </summary>
        private void FindColumnBeamLink()
        {
            while(this.calcColumnSegs.Count>0)
            {
                CalcColumnSeg originColumn = this.calcColumnSegs[0];
                this.calcColumnSegs.RemoveAt(0);
                BuildColumnBeamLink primaryBeamLink = new BuildColumnBeamLink(originColumn, this.calcColumnSegs, this.calcBeamSegs);
                primaryBeamLink.Find();
            }
        }
        /// <summary>
        /// 获取模型库中当前层中所有的柱和梁
        /// </summary>
        private void GetAllColumnBeamSeg()
        {
            this.modelColumnSegs = new YjkColumnQuery(dtlModelPath).GetModelColumnSegComposes(this.floorNo);
            this.modelBeamSegs = new YjkBeamQuery(dtlModelPath).GetModelBeamSegComposes(this.floorNo);
        }
        /// <summary>
        /// 从dtlCalc库中的tblColumnSeg表中获取当前层所有记录
        /// </summary>
        private void GetAllCalcColumnSeg()
        {
            YjkColumnQuery yjkColumnQuery = new YjkColumnQuery(dtlCalcPath);
            this.modelColumnSegs.ForEach(i =>
            {
                int calcColumnId = yjkColumnQuery.GetTblColSegIDFromDtlCalc(i.Floor.No_, i.ColumnSeg.No_);
                this.calcColumnSegs.Add(yjkColumnQuery.GetCalcColumnSeg(calcColumnId));
            });
        }
        /// <summary>
        /// 从dtlCalc库中的tblBeamSeg表中获取当前层所有记录
        /// </summary>
        private void GetAllCalcBeamSeg()
        {
            YjkBeamQuery yjkBeamQuery = new YjkBeamQuery(dtlCalcPath);
            this.modelBeamSegs.ForEach(i =>
            {
                int calcBeamId = yjkBeamQuery.GetTblColSegIDFromDtlCalc(i.Floor.No_, i.BeamSeg.No_);
                this.calcBeamSegs.Add(yjkBeamQuery.GetCalcBeamSeg(calcBeamId));
            });
        }
    }
}
