using System;
using Linq2Acad;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThAOccupancy
    {
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 楼层ID
        [Display(AutoGenerateField = false)]
        public Guid StoreyID { get; set; }

        // 编号
        public int Number { get; set; }

        // 构件
        public string Component { get; set; }

        // 类型
        public string Category { get; set; }

        // 计算系数
        public double Coefficient { get; set; }

        // 计容系数
        public double FARCoefficient { get; set; }

        // 车位层数
        public UInt16 Floors { get; set; }

        // 面积
        public double Area
        {
            get
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    ObjectId objId = new ObjectId(Frame);
                    Polyline frameLine = acadDatabase.ElementOrDefault<Polyline>(objId);
                    if (frameLine == null)
                        return 0.0;

                    return frameLine.Area * (1.0 / 1000000.0);
                }
            }
        }

        // 面积框线
        public IntPtr Frame { get; set; }
    }
}
