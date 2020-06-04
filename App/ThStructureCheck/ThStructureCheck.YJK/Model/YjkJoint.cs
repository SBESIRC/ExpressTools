using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    /// <summary>
    /// 模型库节点
    /// </summary>
    public class ModelJoint : YjkEntityInfo
    {
        public int No_ { get; set; }
        public int StdFlrID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int HDiff { get; set; }
    }
    /// <summary>
    /// 计算书节点
    /// </summary>
    public class CalcJoint : YjkEntityInfo
    {
        public int TowNo { get; set; }
        public int FlrNo { get; set; }
        public int MdlFlr { get; set; }
        public int MdlNo { get; set; }
        public string Coord { get; set; }
        public Point GetCoordinate()
        {
            Point xyz=null;
            if(string.IsNullOrEmpty(this.Coord))
            {
                return xyz;
            }
            string[] values = this.Coord.Split(',');
            if(values.Length==2)
            {
                xyz.X = Convert.ToDouble(values[0]);
                xyz.Y = Convert.ToDouble(values[1]);
            }
            else if (values.Length == 3)
            {
                xyz.X = Convert.ToDouble(values[0]);
                xyz.Y = Convert.ToDouble(values[1]);
                xyz.Z = Convert.ToDouble(values[2]);
            }
            return xyz;
        }
    }
}
