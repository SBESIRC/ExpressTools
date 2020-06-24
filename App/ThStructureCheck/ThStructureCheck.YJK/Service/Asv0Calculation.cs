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
    public class Asv0Calculation
    {
        private double asv0;        
        private YjkEntityInfo beamEntity;
        private YjkEntityInfo linkEntity;
        private string dtlCalcPath = "";
        public double Asv0 => asv0;
        /// <summary>
        /// 非加密区箍筋
        /// </summary>
        /// <param name="beamEntity"></param>
        /// <param name="linkEntity"></param>
        public Asv0Calculation(YjkEntityInfo beamEntity, YjkEntityInfo linkEntity,string dtlCalcPath)
        {
            this.beamEntity = beamEntity;
            this.linkEntity = linkEntity;
            this.dtlCalcPath = dtlCalcPath;
        }
        public void Calculate()
        {
            if(this.beamEntity==null)
            {
                return;
            }
            if(this.beamEntity is ModelBeamSeg modelBeamSeg)
            {
                if(this.linkEntity is ModelColumnSeg modelColumnSeg)
                {
                    CalculateAsv0(modelBeamSeg, modelColumnSeg);
                }
                else if(this.linkEntity is ModelWallSeg modelWallSeg)
                {

                }
            }
        }
        #region----------计算梁与柱的非加密区箍筋-----------
        private void CalculateAsv0(ModelBeamSeg modelBeamSeg,ModelColumnSeg modelColumnSeg)
        {
            IAsv0Factory asv0Factory = null;
            //后续根据判断来选择执行方法
            //CalculateLineBeamRecColumn;
            //calculateAsv0Delegate(modelBeamSeg, modelColumnSeg);
        }
        private void CalculateLineBeamRecColumn(IEntityInf modelBeamSegEnt, IEntityInf modelColumnSegEnt)
        {
            IEntity beamGeo = modelBeamSegEnt.BuildGeometry();
            IEntity columnGeo = modelColumnSegEnt.BuildGeometry();
            double dis = GeometricCalculation.GetInsertBeamDis(columnGeo, beamGeo);
            int floorNo;
            int beamSegNo;
            ModelBeamSeg modelBeamSeg = modelBeamSegEnt as ModelBeamSeg;
            ModelColumnSeg modelColumnSeg = modelColumnSegEnt as ModelColumnSeg;
            bool res = new YjkBeamQuery(modelBeamSeg.DbPath).GetDtlmodelTblBeamSegFlrNoAndNo(modelBeamSeg.ID, out floorNo, out beamSegNo);
            int beamCalcId = new YjkBeamQuery(this.dtlCalcPath).GetTblBeamSegIDFromDtlCalc(floorNo, beamSegNo);
        }
        private void CalculateArcBeamRecColumn(IEntityInf modelBeamSegEnt, IEntityInf modelColumnSegEnt)
        {
           //ToDo
        }
        private void CalculateLineBeamLTypeColumn(IEntityInf modelBeamSegEnt, IEntityInf modelColumnSegEnt)
        {
            //ToDo
        }
        private void CalculateArcBeamLTypeColumn(IEntityInf modelBeamSegEnt, IEntityInf modelColumnSegEnt)
        {
            //ToDo
        }
        #endregion
        #region----------计算梁与墙的非加密区箍筋-----------
        private void CalculateAsv0(ModelBeamSeg modelBeamSeg, ModelWallSeg modelWallSeg)
        {
            IEntity wallGeo = modelWallSeg.BuildGeometry();
            IEntity beamGeo = modelBeamSeg.BuildGeometry();
            double dis = GeometricCalculation.GetInsertBeamDis(wallGeo, beamGeo);
        }
        #endregion
    }
}
