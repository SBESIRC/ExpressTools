using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK.Query
{
    public class YjkJointQuery : YjkQuery
    {
        public YjkJointQuery(string dbPath) : base(dbPath)
        {
        }
        public CalcJoint GetCalcJoint(int jtID)
        {
            CalcJoint calcJoint = new CalcJoint();
            string sql = "select * from tblJoint where ID=" + jtID;
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                calcJoint.ID = Convert.ToInt32(dr["ID"].ToString());
                calcJoint.TowNo = Convert.ToInt32(dr["TowNo"].ToString());
                calcJoint.FlrNo = Convert.ToInt32(dr["FlrNo"].ToString());
                calcJoint.MdlFlr = Convert.ToInt32(dr["MdlFlr"].ToString());
                calcJoint.MdlNo = Convert.ToInt32(dr["MdlNo"].ToString());
                calcJoint.Coord = dr["Coord"].ToString();
            }
            return calcJoint;
        }
    }
}
