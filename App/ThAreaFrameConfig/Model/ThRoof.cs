using System;
using Linq2Acad;
using System.ComponentModel.DataAnnotations;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThRoof
    {
        // ID
        [Display(AutoGenerateField = false)]
        public Guid ID { get; set; }

        // 编号
        public int Number { get; set; }

        // 面积
        public double Area
        {
            get
            {
                if (Frame == (IntPtr)0)
                    return 0.0;

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

        
        // 状态
        public bool IsDefined
        {
            get
            {
                return Frame != ObjectId.Null.OldIdPtr;
            }
        }
    }
}
