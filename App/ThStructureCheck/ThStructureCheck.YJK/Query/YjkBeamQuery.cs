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
            string sql = "select tblFloor.ID as tblFlrID , tblFloor.No_ as tblFlrNo, tblFloor.Name as tblFlrName," +
                "tblFloor.StdFlrID as tblFlrStdFlrID, tblFloor.LevelB,tblFloor.Height, tblBeamSeg.ID as tblBeamSegID , " +
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
                item.Floor.ID = Convert.ToInt32(dr["tblFlrID"].ToString());
                item.Floor.No_= Convert.ToInt32(dr["tblFlrNo"].ToString());
                item.Floor.Name = dr["tblFlrName"].ToString();
                item.Floor.StdFlrID= Convert.ToInt32(dr["tblFlrStdFlrID"].ToString());
                item.Floor.LevelB= Convert.ToDouble(dr["LevelB"].ToString());
                item.Floor.Height = Convert.ToDouble(dr["Height"].ToString());
                item.Floor.DbPath = this.dbPath;

                item.BeamSeg.ID = Convert.ToInt32(dr["tblBeamSegID"].ToString());
                item.BeamSeg.No_= Convert.ToInt32(dr["tblBeamSegNo"].ToString());
                item.BeamSeg.StdFlrID = Convert.ToInt32(dr["tblBeamSegStdFlrID"].ToString());
                item.BeamSeg.SectID= Convert.ToInt32(dr["SectID"].ToString());
                item.BeamSeg.GridID = Convert.ToInt32(dr["GridID"].ToString());
                item.BeamSeg.Ecc = Convert.ToInt32(dr["Ecc"].ToString());
                item.BeamSeg.HDiff1 = Convert.ToInt32(dr["HDiff1"].ToString());
                item.BeamSeg.HDiff2 = Convert.ToInt32(dr["HDiff2"].ToString());
                item.BeamSeg.Rotation = Convert.ToInt32(dr["Rotation"].ToString());
                item.BeamSeg.DbPath = this.dbPath;

                item.BeamSect.ID= Convert.ToInt32(dr["tblBeamSectID"].ToString());
                item.BeamSect.No_= Convert.ToInt32(dr["tblBeamSectNo"].ToString());
                item.BeamSect.Name = dr["tblBeamSectName"].ToString();
                item.BeamSect.Mat=Convert.ToInt32(dr["Mat"].ToString());
                item.BeamSect.Kind = Convert.ToInt32(dr["Kind"].ToString());
                item.BeamSect.ShapeVal= dr["ShapeVal"].ToString();
                item.BeamSect.ShapeVal1 = dr["ShapeVal1"].ToString();
                item.BeamSect.DbPath = this.dbPath;
                results.Add(item);
            }
            return results;
        }
        /// <summary>
        /// 获取模型库(dtlmodel)的表(TblFloor、TblBeamSeg)表中获取tblFloor.No值和tblBeamSeg.No_
        /// </summary>
        /// <param name="ID">模型库中的梁ID</param>
        /// <param name="tblFloorNo_">层编号</param>
        /// <param name="tblBeamSegNo_">梁编号</param>
        /// <returns></returns>
        public bool GetDtlmodelTblBeamSegFlrNoAndNo(int ID, out int tblFloorNo_, out int tblBeamSegNo_)
        {
            bool result = false;
            tblFloorNo_ = -1;
            tblBeamSegNo_ = -1;
            try
            {
                string sql = "select tblFloor.No_ as tblFloorNo,tblBeamSeg.No_ as tblBeamSegNo from tblFloor join tblBeamSeg" +
                    " on tblFloor.StdFlrID=tblBeamSeg.StdFlrID" +
                    " where  tblBeamSeg.ID=" + ID;
                DataTable dt = ExecuteDataTable(sql);
                string tblFloorNoStr = "";
                string tblBeamSegNoStr = "";
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["tblFloorNo"] != null)
                    {
                        tblFloorNoStr = dr["tblFloorNo"].ToString();
                    }
                    if (dr["tblBeamSegNo"] != null)
                    {
                        tblBeamSegNoStr = dr["tblBeamSegNo"].ToString();
                    }
                    break;
                }
                if (!string.IsNullOrEmpty(tblFloorNoStr) && !string.IsNullOrEmpty(tblBeamSegNoStr))
                {
                    if (int.TryParse(tblFloorNoStr, out tblFloorNo_) &&
                        int.TryParse(tblBeamSegNoStr, out tblBeamSegNo_)
                        )
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.WriteException(ex, "GetTblFloorTblBeamSegNoFromDtlmodel");
            }
            return result;
        }
        /// <summary>
        /// 从计算库(dtlCalc)的表(tblBeamSeg)中获取柱子ID
        /// </summary>
        /// <param name="tblFloor_No">模型库中tblFloor.No</param>
        /// <param name="tblBeamSegNo">模型库中tblBeamSeg.No_</param>
        /// <returns></returns>
        public int GetTblBeamSegIDFromDtlCalc(int tblFloor_No, int tblBeamSegNo)
        {
            int id = -1;
            try
            {
                string sql = "select ID from tblBeamSeg where MdlFlr = " + tblFloor_No + " and MdlNo = " + tblBeamSegNo;
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
                Utils.WriteException(ex, "GetTblBeamSegIDFromDtlCalc");
            }
            return id;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public CalcRCBeamDsn GetCalcRcBeamDsn(int ID)
        {
            CalcRCBeamDsn calcRCBeamDsn = new CalcRCBeamDsn();
            try
            {
                string sql = "select * from tblRCBeamDsn where ID = " + ID;
                DataTable dt = ExecuteDataTable(sql);
                string asTop = "";
                foreach (DataRow dr in dt.Rows)
                {
                    calcRCBeamDsn.ID = Convert.ToInt32(dr["ID"].ToString());
                    calcRCBeamDsn.AsTop= dr["AsTop"].ToString();
                    calcRCBeamDsn.AsBtm = dr["AsBtm"].ToString();
                    calcRCBeamDsn.Asv = dr["Asv"].ToString();
                    calcRCBeamDsn.Ast1 = dr["Ast1"].ToString();
                    calcRCBeamDsn.Astt = dr["Astt"].ToString();
                    calcRCBeamDsn.Asttc = dr["Asttc"].ToString();
                    calcRCBeamDsn.FrcAst = dr["FrcAst"].ToString();
                    calcRCBeamDsn.FrcAsb = dr["FrcAsb"].ToString();
                    calcRCBeamDsn.FrcAsv = dr["FrcAsv"].ToString();
                    break;
                }
            }
            catch (Exception ex)
            {
                Utils.WriteException(ex, "GetAsuFromDtlCalc");
            }
            return calcRCBeamDsn;
        }
        /// <summary>
        /// 获取计算库中的梁段
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
            calcBeamSeg.DbPath = this.dbPath;
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
                calcBeamSeg.DbPath = this.dbPath;
                results.Add(calcBeamSeg);
            }
            return results;
        }
        public List<ModelBeamSeg> GetFloorModelBeamSeg(int flrNo)
        {
            List<ModelBeamSeg> results = new List<ModelBeamSeg>();
            string sql = "select * from tblBeamSeg join tblFloor" +
                " on tblBeamSeg.StdFlrID=tblFloor.StdFlrID " +
                " where tblFloor.No_=" + flrNo;
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                ModelBeamSeg modelBeamSeg = new ModelBeamSeg();
                modelBeamSeg.ID = Convert.ToInt32(dr["ID"].ToString());
                modelBeamSeg.No_= Convert.ToInt32(dr["No_"].ToString());
                modelBeamSeg.StdFlrID = Convert.ToInt32(dr["StdFlrID"].ToString());
                modelBeamSeg.SectID = Convert.ToInt32(dr["SectID"].ToString());
                modelBeamSeg.GridID= Convert.ToInt32(dr["GridID"].ToString());
                modelBeamSeg.Ecc = Convert.ToInt32(dr["Ecc"].ToString());
                modelBeamSeg.HDiff1 = Convert.ToInt32(dr["HDiff1"].ToString());
                modelBeamSeg.HDiff2 = Convert.ToInt32(dr["HDiff2"].ToString());
                modelBeamSeg.Rotation = Convert.ToDouble(dr["Rotation"].ToString());
                modelBeamSeg.DbPath = this.dbPath;
                results.Add(modelBeamSeg);
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
                preLinkBeam.FlrNo + " and ID!="+preLinkBeam.ID + " and (Jt1="+jt+ " or Jt2="+jt+")";
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
                calcBeamSeg.DbPath = this.dbPath;
                results.Add(calcBeamSeg);
            }
            return results;
        }
        public List<ModelBeamSeg> GetBeamLinkBeams(ModelBeamSeg preLinkBeam, int jt)
        {
            List<ModelBeamSeg> results = new List<ModelBeamSeg>();
            string sql = "select * from tblBeamSeg join tblGrid" +
                " on tblBeamSeg.GridID = tblGrid.ID and tblBeamSeg.StdFlrID=tblGrid.StdFlrID" +
                " where tblBeamSeg.ID !="+preLinkBeam.ID +" and tblBeamSeg.StdFlrID=" + preLinkBeam.StdFlrID+
                " and (tblGrid.Jt1ID = " + jt + " or tblGrid.Jt2ID =" + jt+")" ;
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                ModelBeamSeg modelBeamSeg = new ModelBeamSeg();
                modelBeamSeg.ID = Convert.ToInt32(dr["ID"].ToString());
                modelBeamSeg.No_ = Convert.ToInt32(dr["No_"].ToString());
                modelBeamSeg.StdFlrID = Convert.ToInt32(dr["StdFlrID"].ToString());
                modelBeamSeg.SectID = Convert.ToInt32(dr["SectID"].ToString());
                modelBeamSeg.GridID = Convert.ToInt32(dr["GridID"].ToString());
                modelBeamSeg.Ecc = Convert.ToInt32(dr["Ecc"].ToString());
                modelBeamSeg.HDiff1 = Convert.ToInt32(dr["HDiff1"].ToString());
                modelBeamSeg.HDiff2 = Convert.ToInt32(dr["HDiff2"].ToString());
                modelBeamSeg.Rotation = Convert.ToDouble(dr["Rotation"].ToString());
                modelBeamSeg.DbPath = this.dbPath;
                results.Add(modelBeamSeg);
            }
            return results;
        }

    }
}
