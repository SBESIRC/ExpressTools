using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Interface;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK.Query
{
    public class YjkWallBeamQuery:YjkQuery
    {
        public YjkWallBeamQuery(string dbPath) : base(dbPath)
        {
        }
        public override IList<IEntityInf> Extract(int floorNo)
        {
            return base.Extract(floorNo);
        }
        /// <summary>
        /// 提取dtlModel中所有的柱子信息
        /// </summary>
        /// <param name="floorNo"></param>
        /// <returns></returns>
        public List<CalcWallBeam> GetBeamLinkWalls(int flrNo,int jt)
        {
            List<CalcWallBeam> results = new List<CalcWallBeam>();
            string sql = "selct * from tblWallBeam where FlrNo =" + flrNo +
                " and Jt1=" + jt + " or Jt2=" + jt;
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                CalcWallBeam item = new CalcWallBeam();
                item.ID = Convert.ToInt32(dr["ID"].ToString());
                item.TowNo= Convert.ToInt32(dr["TowNo"].ToString());
                item.FlrNo = Convert.ToInt32(dr["FlrNo"].ToString());
                item.MdlFlr = Convert.ToInt32(dr["MdlFlr"].ToString());
                item.MdlNo = Convert.ToInt32(dr["MdlNo"].ToString());
                item.B = Convert.ToDouble(dr["B"].ToString());
                item.H = Convert.ToDouble(dr["H"].ToString());
                item.Jt1 = Convert.ToInt32(dr["Jt1"].ToString());
                item.Jt2 = Convert.ToInt32(dr["Jt2"].ToString());
                item.Jts = dr["Jts"].ToString();
                results.Add(item);
            }
            return results;
        }
    }
}
