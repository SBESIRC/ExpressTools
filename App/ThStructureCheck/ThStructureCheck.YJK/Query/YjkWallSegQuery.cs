using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK.Query
{
    public class YjkWallSegQuery : YjkQuery
    {
        public YjkWallSegQuery(string dbPath):base(dbPath)
        {
        }
        public List<ModelWallSeg> GetBeamLinkedWall(ModelBeamSeg modelBeam,int jt)
        {
            List<ModelWallSeg> results = new List<ModelWallSeg>();
            string sql = "select * from tblWallSeg join tblGrid " +
                "on tblWallSeg.GridID=tblGrid.ID and tblWallSeg.StdFlrID=tblGrid.StdFlrID " +
                "where tblWallSeg.StdFlrID="+ modelBeam.StdFlrID+ "and (tblGrid.Jt1ID="+jt+ " or tblGrid.Jt2ID="+jt+")";
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                ModelWallSeg modelWallSeg = new ModelWallSeg();
                modelWallSeg.ID = Convert.ToInt32(dr["ID"].ToString());
                modelWallSeg.No_ = Convert.ToInt32(dr["No_"].ToString());
                modelWallSeg.StdFlrID = Convert.ToInt32(dr["StdFlrID"].ToString());
                modelWallSeg.SectID = Convert.ToInt32(dr["SectID"].ToString());
                modelWallSeg.GridID = Convert.ToInt32(dr["GridID"].ToString());
                modelWallSeg.Ecc = Convert.ToInt32(dr["Ecc"].ToString());
                modelWallSeg.HDiff1 = Convert.ToInt32(dr["HDiff1"].ToString());
                modelWallSeg.HDiff2 = Convert.ToInt32(dr["HDiff2"].ToString());
                modelWallSeg.HDiffB = Convert.ToInt32(dr["HDiffB"].ToString());
                modelWallSeg.sloping = Convert.ToInt32(dr["sloping"].ToString());
                modelWallSeg.EccDown = Convert.ToInt32(dr["EccDown"].ToString());
                modelWallSeg.offset1 = Convert.ToInt32(dr["offset1"].ToString());
                modelWallSeg.offset2 = Convert.ToInt32(dr["offset2"].ToString());
                modelWallSeg.DbPath = this.dbPath;
                results.Add(modelWallSeg);
            }
            return results;
        }
    }
}
