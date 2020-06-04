using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK.Query
{
    public class YjkBeamQuery:YjkQuery
    {
        public YjkBeamQuery(string dbPath):base(dbPath)
        {
        }
        /// <summary>
        /// 提取dtlModel中所有的柱子信息
        /// </summary>
        /// <param name="floorNo"></param>
        /// <returns></returns>
        public List<ModelBeamSegCompose> GetModelBeamSegComposes(int floorNo)
        {
            List<ModelBeamSegCompose> results = new List<ModelBeamSegCompose>();
            string sql = "select tblFloor.ID as tblFloorID , tblFloor.No_ as tblFloorNo, tblFloor.Name as tblFloorName," +
                "tblFloor.StdFlrID as tblFloorStdFlrID, tblFloor.LevelB,tblFloor.Height, tblBeamSeg.ID as tblBeamSegID , " +
                "tblBeamSeg.No_ as tblBeamSegNo, tblBeamSeg.StdFlrID as tblBeamSegStdFlrID,tblBeamSeg.SectID,tblBeamSeg.GridID," +
                "tblBeamSeg.Ecc,tblBeamSeg.HDiff1,tblBeamSeg.HDiff2,tblBeamSeg.Rotation , tblBeamSect.ID as tblBeamSectID," +
                " tblBeamSect.No_ as tblBeamSectNo , tblBeamSect.Name as tblBeamSectName, tblBeamSect.Mat,tblBeamSect.Kind," +
                "tblBeamSect.ShapeVal, tblBeamSect.ShapeVal1 " +
                "from tblFloor join tblBeamSeg join tblBeamSect on tblFloor.StdFlrID = tblBeamSeg.StdFlrID and tblBeamSeg.SectID = tblBeamSect.ID" +
                " where tblFloor.No_ =" + floorNo;
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                ModelBeamSegCompose item = new ModelBeamSegCompose();
                item.Floor.ID = Convert.ToInt32(dr["tblFloorID"].ToString());
                item.Floor.No_= Convert.ToInt32(dr["tblFloorNo"].ToString());
                item.Floor.Name = dr["tblFloorName"].ToString();
                item.Floor.StdFlrID= Convert.ToInt32(dr["tblFloorStdFlrID"].ToString());
                item.Floor.LevelB= Convert.ToDouble(dr["LevelB"].ToString());
                item.Floor.Height = Convert.ToDouble(dr["Height"].ToString());

                item.BeamSeg.ID = Convert.ToInt32(dr["tblBeamSegID"].ToString());
                item.BeamSeg.No_= Convert.ToInt32(dr["tblBeamSegNo"].ToString());
                item.BeamSeg.StdFlrID = Convert.ToInt32(dr["tblBeamSegStdFlrID"].ToString());
                item.BeamSeg.SectID= Convert.ToInt32(dr["SectID"].ToString());
                item.BeamSeg.GridID = Convert.ToInt32(dr["GridID"].ToString());
                item.BeamSeg.Ecc = Convert.ToInt32(dr["Ecc"].ToString());
                item.BeamSeg.HDiff1 = Convert.ToInt32(dr["HDiff1"].ToString());
                item.BeamSeg.HDiff2 = Convert.ToInt32(dr["HDiff2"].ToString());
                item.BeamSeg.Rotation = Convert.ToInt32(dr["Rotation"].ToString());

                item.BeamSect.ID= Convert.ToInt32(dr["tblBeamSectID"].ToString());
                item.BeamSect.No_= Convert.ToInt32(dr["tblBeamSectNo"].ToString());
                item.BeamSect.Name = dr["tblBeamSectName"].ToString();
                item.BeamSect.Mat=Convert.ToInt32(dr["Mat"].ToString());
                item.BeamSect.Kind = Convert.ToInt32(dr["Kind"].ToString());
                item.BeamSect.ShapeVal= dr["ShapeVal"].ToString();
                item.BeamSect.ShapeVal1 = dr["ShapeVal1"].ToString();

                results.Add(item);
            }
            return results;
        }

        /// <summary>
        /// 从计算库(dtlCalc)的表(tblBeamSeg)中获取柱子ID
        /// </summary>
        /// <param name="tblFloor_No"></param>
        /// <param name="tblColSegNo"></param>
        /// <returns></returns>
        public int GetTblColSegIDFromDtlCalc(int tblFloor_No, int tblBeamSegNo)
        {
            int id = -1;
            try
            {
                string sql = "select ID from tblBeamSeg where FlrNo = " + tblFloor_No + " and MdlNo = " + tblBeamSegNo;
                DataTable dt = ExecuteDataTable(sql);
                string idStr = "";
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ID"] != null)
                    {
                        idStr = dr["ID"].ToString();
                    }
                    break;
                }
                if (!string.IsNullOrEmpty(idStr))
                {
                    if (int.TryParse(idStr, out id))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.WriteException(ex, "GetTblColSegIDFromDtlCalc");
            }
            return id;
        }
        public CalcBeamSeg GetCalcBeamSeg(int id)
        {
            CalcBeamSeg calcBeamSeg = new CalcBeamSeg();
            string sql = "select * from tblBeamSeg where ID=" + id;
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                calcBeamSeg.ID = Convert.ToInt32(dr["ID"].ToString());
                calcBeamSeg.TowNo= Convert.ToInt32(dr["TowNo"].ToString());
                calcBeamSeg.FlrNo = Convert.ToInt32(dr["FlrNo"].ToString());
                calcBeamSeg.MdlFlr= Convert.ToInt32(dr["MdlFlr"].ToString());
                calcBeamSeg.MdlNo = Convert.ToInt32(dr["MdlNo"].ToString());
                calcBeamSeg.Jt1 = Convert.ToInt32(dr["Jt1"].ToString());
                calcBeamSeg.Jt2 = Convert.ToInt32(dr["Jt2"].ToString());
            }
            return calcBeamSeg;
        }
        public List<CalcBeamSeg> GetFloorCalcBeamSeg(int flrNo)
        {
            List<CalcBeamSeg> results = new List<CalcBeamSeg>();            
            string sql = "select * from tblBeamSeg where FlrNo=" + flrNo;
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                CalcBeamSeg calcBeamSeg = new CalcBeamSeg();
                calcBeamSeg.ID = Convert.ToInt32(dr["ID"].ToString());
                calcBeamSeg.TowNo = Convert.ToInt32(dr["TowNo"].ToString());
                calcBeamSeg.FlrNo = Convert.ToInt32(dr["FlrNo"].ToString());
                calcBeamSeg.MdlFlr = Convert.ToInt32(dr["MdlFlr"].ToString());
                calcBeamSeg.MdlNo = Convert.ToInt32(dr["MdlNo"].ToString());
                calcBeamSeg.Jt1 = Convert.ToInt32(dr["Jt1"].ToString());
                calcBeamSeg.Jt2 = Convert.ToInt32(dr["Jt2"].ToString());
                results.Add(calcBeamSeg);
            }
            return results;
        }
        /// <summary>
        /// 获取梁末端节点连接的梁
        /// </summary>
        /// <param name="preLinkBeam"></param>
        /// <param name="jt"></param>
        /// <returns></returns>
        public List<CalcBeamSeg> GetBeamLinkBeams(CalcBeamSeg preLinkBeam,int jt)
        {
            List<CalcBeamSeg> results = new List<CalcBeamSeg>();
            string sql = "select * from tblBeamSeg where FlrNo=" + 
                preLinkBeam.FlrNo + "and ID!="+preLinkBeam.ID + "or Jt1="+jt+ "or Jt2="+jt;
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                CalcBeamSeg calcBeamSeg = new CalcBeamSeg();
                calcBeamSeg.ID = Convert.ToInt32(dr["ID"].ToString());
                calcBeamSeg.TowNo = Convert.ToInt32(dr["TowNo"].ToString());
                calcBeamSeg.FlrNo = Convert.ToInt32(dr["FlrNo"].ToString());
                calcBeamSeg.MdlFlr = Convert.ToInt32(dr["MdlFlr"].ToString());
                calcBeamSeg.MdlNo = Convert.ToInt32(dr["MdlNo"].ToString());
                calcBeamSeg.Jt1 = Convert.ToInt32(dr["Jt1"].ToString());
                calcBeamSeg.Jt2 = Convert.ToInt32(dr["Jt2"].ToString());
                results.Add(calcBeamSeg);
            }
            return results;
        }
    }
}
