using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Model
{
    public class ModelColumnSeg : YjkEntityInfo,IEntityInf
    {
        public int No_ { get; set; }
        public int StdFlrID { get; set; }
        public int SectID { get; set; }
        public int JtID { get; set; }
        public int EccX { get; set; }
        public int EccY { get; set; }
        public double Rotation { get; set; }
        public int HDiffB { get; set; }
        public int ColcapId { get; set; }
        public string Cut_Col { get; set; }
        public string Cut_Cap { get; set; }
        public string Cut_Slab { get; set; }

        public ModelColumnSect ColumnSect
        {
            get
            {
                return new YjkColumnQuery(this.DbPath).GetModelColumnSect(this.SectID);
            }
        }
        public IEntity BuildGeometry()
        {
            //Todo 后续要增加弧梁
            return RectangleColumnGeo();
        }
        public RectangleColumnGeometry RectangleColumnGeo()
        {
            YjkJointQuery yjkJointQuery = new YjkJointQuery(this.DbPath);
            ModelJoint columnJoint = yjkJointQuery.GetModelJoint(this.JtID);
            List<double> datas = Utils.GetDoubleDatas(this.ColumnSect.Spec);
            if (datas.Count == 0)
            {
                datas.Add(0.0);
                datas.Add(0.0);
            }
            else if (datas.Count == 1)
            {
                datas.Add(0.0);
            }
            return new RectangleColumnGeometry()
            {
                Origin = new Coordinate(columnJoint.X, columnJoint.Y),
                EccX = this.EccX,
                EccY = this.EccY,
                Rotation = this.Rotation,
                B = datas[0],
                H = datas[1]
            };
        }
    }
    public class CalcColumnSeg : YjkEntityInfo
    {
        public int FlrNo { get; set; }
        public int TowNo { get; set; }
        public int MdlFlr { get; set; }
        public int MdlNo { get; set; }
        public int Jt1 { get; set; }
        public int Jt2 { get; set; }
    }
}
