using System;
using Linq2Acad;
using System.ComponentModel.DataAnnotations;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThResidentialAreaFrame
    {
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 组件ID
        [Display(AutoGenerateField = false)]
        public Guid ComponentID { get; set; }

        // 户型ID
        [Display(AutoGenerateField = false)]
        public Guid RoomID { get; set; }

        // 编号
        public int Number { get; set; }

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

        // 计算系数
        public double Coefficient { get; set; }

        // 计容系数
        public double FARCoefficient { get; set; }

        // 面积框线
        public IntPtr Frame { get; set; }
    }
}
