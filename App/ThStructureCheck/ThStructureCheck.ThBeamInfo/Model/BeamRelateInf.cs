using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.ThBeamInfo.Interface;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.ThBeamInfo.Model
{
    /// <summary>
    /// 记录Yjk中的梁和Cad图纸中的梁关联信息
    /// </summary>
    public class BeamRelateInf
    {
        public YjkEntityInfo DbBeamInf
        {
            get;
            set;
        }
        /// <summary>
        /// DataBase中的柱子在Cad图纸的位置
        /// </summary>
        public List<Point3d> InModelPts
        {
            get;
            set;
        }
        /// <summary>
        /// 关联到的模型中实际的梁
        /// </summary>
        public List<IBeamInfo> ModelBeamInfs
        {
            get;
            set;
        }
        /// <summary>
        /// 自定义数据
        /// </summary>
        public BeamCustomData CustomData
        {
            get;
            set;
        }
        /// <summary>
        /// 埋入从计算库中获取的数据
        /// </summary>
        public YjkBeamCalculateInfo BeamCalculateInfo
        {
            get;
            set;
        }
    }
}
