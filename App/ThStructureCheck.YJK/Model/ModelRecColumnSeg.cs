using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.Common.Model;
using ThStructureCheck.Common.Model.Column;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Query;

namespace ThStructureCheck.YJK.Model
{
    public class ModelRecColumnSeg: ModelColumnSeg, IEntityInf
    {
        public double Length => 0.0; //暂时不计算

        public IEntity BuildGeometry()
        {
            //Todo 后续要增加弧梁
            return RectangleColumnGeo();
        }
        public RecColumnGeometry RectangleColumnGeo()
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
            return new RecColumnGeometry()
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
}
