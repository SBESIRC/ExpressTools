using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Service;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Model;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Service
{
    public abstract class Asv0Calculation
    {
        protected string dtlCalcPath = "";
        protected double asv0 = 0.0;
        protected List<ModelBeamSeg> beamSegs = new List<ModelBeamSeg>();
        protected bool start;
        protected IEntityInf linkEnty;
        protected ModelBeamSeg modelBeamSeg; //起始梁段
        protected double insertDepth = 0.0;  //梁插入柱或墙的深度
        protected YjkBeamQuery calcBeamQuery;
        protected YjkBeamQuery modelBeamQuery;
        protected double asvLength; //加密区长度
        /// <summary>
        /// 非加密箍筋
        /// </summary>
        public double Asv0 => this.asv0;

        protected ModelBeamSeg asv0BeamSeg;
        /// <summary>
        /// 非加密箍筋所在的梁索引
        /// </summary>
        public ModelBeamSeg Asv0BeamSeg => asv0BeamSeg;
        /// <summary>
        /// 非加密区箍筋
        /// </summary>
        /// <param name="beamEntity"></param>
        /// <param name="linkEntity"></param>
        public Asv0Calculation(List<ModelBeamSeg> beamSegs,IEntityInf linkEnty, bool start,string dtlCalcPath)
        {
            this.dtlCalcPath = dtlCalcPath;
            this.beamSegs = beamSegs;
            this.linkEnty = linkEnty;
            this.start = start;
            if(beamSegs.Count>0)
            {
                if (start)
                {
                    modelBeamSeg = beamSegs[0];
                }
                else
                {
                    modelBeamSeg = beamSegs[beamSegs.Count-1];
                }
                this.calcBeamQuery = new YjkBeamQuery(dtlCalcPath);
                this.modelBeamQuery = new YjkBeamQuery(modelBeamSeg.DbPath);
            }
        }
        public abstract void Calculate();
    }
}
