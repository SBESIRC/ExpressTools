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
        /// <summary>
        /// 提取dtlModel中所有的柱子信息
        /// </summary>
        /// <param name="floorNo"></param>
        /// <returns></returns>
        public List<ModelWallSegCompose> GetModelWallSegComposes(int floorNo)
        {
            List<ModelWallSegCompose> results = new List<ModelWallSegCompose>();
            string sql = "select tblFloor.ID as tblFlrID,tblFloor.No_ as tblFlrNo,tblFloor.Name as tblFlrName," +
                         "tblFloor.StdFlrID as tblFlrStdFlrID, tblFloor.LevelB,tblFloor.Height, " +
                         "tblWallSeg.ID as tblWallSegID,tblWallSeg.No_ as tblWallSegNo,tblWallSeg.StdFlrID as tblWallSegStdFlrID ," +
                         "tblWallSeg.SectID , tblWallSeg.GridID,tblWallSeg.Ecc,tblWallSeg.HDiff1,tblWallSeg.HDiff2,tblWallSeg.HDiffB," +
                         "tblWallSeg.sloping,tblWallSeg.EccDown,tblWallSeg.offset1,tblWallSeg.offset2," +
                         "tblWallSect.ID as tblWallSectID,tblWallSect.No_ as tblWallSectNo,tblWallSect.Mat,tblWallSect.Kind,tblWallSect.B,tblWallSect.H," +
                         "tblWallSect.T2 ,tblWallSect.Dis,tblWallSect.colsect1,tblWallSect.colShapeVal1,tblWallSect.colsect2,tblWallSect.colShapeVal2" +
                         " from tblFloor join tblWallSeg join tblWallSect " +
                         " on tblFloor.StdFlrID = tblWallSeg.StdFlrID and tblWallSeg.SectID = tblWallSect.ID" +
                         " where tblFloor.No_ = " + floorNo;

            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                ModelWallSegCompose item = new ModelWallSegCompose();
                item.Floor.ID = Convert.ToInt32(dr["tblFlrID"].ToString());
                item.Floor.No_ = Convert.ToInt32(dr["tblFlrNo"].ToString());
                item.Floor.Name = dr["tblFlrName"].ToString();
                item.Floor.StdFlrID = Convert.ToInt32(dr["tblFlrStdFlrID"].ToString());
                item.Floor.LevelB = Convert.ToDouble(dr["LevelB"].ToString());
                item.Floor.Height = Convert.ToDouble(dr["Height"].ToString());
                item.Floor.DbPath = this.dbPath;

                item.WallSeg.ID = Convert.ToInt32(dr["tblWallSegID"].ToString());
                item.WallSeg.No_ = Convert.ToInt32(dr["tblWallSegNo"].ToString());
                item.WallSeg.StdFlrID = Convert.ToInt32(dr["tblWallSegStdFlrID"].ToString());
                item.WallSeg.SectID = Convert.ToInt32(dr["SectID"].ToString());
                item.WallSeg.GridID = Convert.ToInt32(dr["GridID"].ToString());
                item.WallSeg.Ecc = Convert.ToInt32(dr["Ecc"].ToString());
                item.WallSeg.HDiff1 = Convert.ToInt32(dr["HDiff1"].ToString());
                item.WallSeg.HDiff2 = Convert.ToInt32(dr["HDiff2"].ToString());
                item.WallSeg.HDiffB = Convert.ToInt32(dr["HDiffB"].ToString());
                item.WallSeg.Sloping = Convert.ToInt32(dr["sloping"].ToString());
                item.WallSeg.EccDown = Convert.ToInt32(dr["EccDown"].ToString());
                item.WallSeg.Offset1 = Convert.ToInt32(dr["offset1"].ToString());
                item.WallSeg.Offset2 = Convert.ToInt32(dr["offset2"].ToString());
                item.WallSeg.DbPath = this.dbPath;

                item.WallSect.ID = Convert.ToInt32(dr["tblWallSectID"].ToString());
                item.WallSect.No_ = Convert.ToInt32(dr["tblWallSectNo"].ToString());
                item.WallSect.Mat = Convert.ToInt32(dr["Mat"].ToString());
                item.WallSect.Kind = Convert.ToInt32(dr["Kind"].ToString());
                item.WallSect.B = Convert.ToInt32(dr["B"].ToString());
                item.WallSect.H = Convert.ToInt32(dr["H"].ToString());
                item.WallSect.T2 = Convert.ToInt32(dr["T2"].ToString());
                item.WallSect.Dis = Convert.ToDouble(dr["Dis"].ToString());
                item.WallSect.Colsect1 = dr["colsect1"].ToString();
                item.WallSect.ColShapeVal1 = dr["colShapeVal1"].ToString();
                item.WallSect.Colsect2 = dr["colsect2"].ToString();
                item.WallSect.ColShapeVal2 = dr["colShapeVal2"].ToString();
                item.WallSect.DbPath = this.dbPath;
                results.Add(item);
            }
            return results;
        }
        public List<ModelWallSeg> GetBeamLinkedWall(ModelBeamSeg modelBeam,int jt)
        {
            List<ModelWallSeg> results = new List<ModelWallSeg>();
            string sql = "select * from tblWallSeg join tblGrid " +
                "on tblWallSeg.GridID=tblGrid.ID and tblWallSeg.StdFlrID=tblGrid.StdFlrID " +
                "where tblWallSeg.StdFlrID="+ modelBeam.StdFlrID+ " and (tblGrid.Jt1ID="+jt+ " or tblGrid.Jt2ID="+jt+")";
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
                modelWallSeg.Sloping = Convert.ToInt32(dr["sloping"].ToString());
                modelWallSeg.EccDown = Convert.ToInt32(dr["EccDown"].ToString());
                modelWallSeg.Offset1 = Convert.ToInt32(dr["offset1"].ToString());
                modelWallSeg.Offset2 = Convert.ToInt32(dr["offset2"].ToString());
                modelWallSeg.DbPath = this.dbPath;
                results.Add(modelWallSeg);
            }
            return results;
        }
    }
}
