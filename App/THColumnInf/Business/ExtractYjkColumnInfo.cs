using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ThColumnInfo
{
    public class ExtractYjkColumnInfo : IDatabaseDataSource
    {
        private string connectionString = string.Empty;
        public List<DrawColumnInf> ColumnInfs { get; set; } = new List<DrawColumnInf>();
        public ExtractYjkColumnInfo(string dbPath)
        {
            this.connectionString = "Data Source=" + dbPath;
        }
        public void Extract(int floorNo)
        {
            string sql = "select tblFloor.ID, tblColSeg.StdFlrID,tblColSeg.JtID,tblColSeg.EccX,tblColSeg.EccY,tblColSeg.Rotation," +
                "tblColSect.ShapeVal,tblJoint.X,tblJoint.Y from tblFloor join tblColSeg join tblColSect join tblJoint" +
                " where tblFloor.No_ =" + floorNo + " and tblFloor.StdFlrID = tblColSeg.StdFlrID and tblColSeg.SectID = tblColSect.ID and" +
                " tblColSeg.StdFlrID = tblJoint.StdFlrID and tblColSeg.JtID = tblJoint.ID";
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                DrawColumnInf yjkColumnInf = new DrawColumnInf();
                yjkColumnInf.FloorID = Convert.ToInt32(dr["ID"].ToString());
                yjkColumnInf.StdFlrID= Convert.ToInt32(dr["StdFlrID"].ToString());
                yjkColumnInf.JtID= Convert.ToInt32(dr["JtID"].ToString());
                yjkColumnInf.EccX=Convert.ToDouble(dr["EccX"].ToString());
                yjkColumnInf.EccY = Convert.ToDouble(dr["EccY"].ToString());
                yjkColumnInf.Rotation = Convert.ToDouble(dr["Rotation"].ToString());
                yjkColumnInf.ShapeVal =dr["ShapeVal"].ToString();
                yjkColumnInf.X= Convert.ToDouble(dr["X"].ToString());
                yjkColumnInf.Y = Convert.ToDouble(dr["Y"].ToString());
                this.ColumnInfs.Add(yjkColumnInf);
            }
        }
        public DataTable ExecuteDataTable(string sql)
        {
            using (SQLiteConnection conn=new SQLiteConnection(this.connectionString))
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    DataTable data = new DataTable();
                    adapter.Fill(data);
                    return data;
                }
            }
        }
        public bool GetAxialCompressionRatio(int columnID,out double axialCompressionRatio,out double axialCompressionRatioLimited)
        {
            bool res = true;
            axialCompressionRatio = 0.0;
            axialCompressionRatioLimited = 0.0;
            try
            {
                string sql = "select ZYB from tblRCColDsn where ID =" + columnID;
                DataTable dt = ExecuteDataTable(sql);
                string zyb = "";
                foreach (DataRow dr in dt.Rows)
                {
                    if(dr["ZYB"]!=null)
                    {
                        zyb = dr["ZYB"].ToString();
                    }
                    break;
                }
                if (!string.IsNullOrEmpty(zyb))
                {
                    string[] values = zyb.Split(',');
                    if(values!=null && values.Length>2)
                    {
                        if (ThColumnInfoUtils.IsNumeric(values[0]))
                        {
                            axialCompressionRatio = Convert.ToDouble(values[0]);
                        }
                        else
                        {
                            res = false;
                        }
                        if (res && ThColumnInfoUtils.IsNumeric(values[1]))
                        {
                            axialCompressionRatioLimited = Convert.ToDouble(values[1]);
                        }
                        else
                        {
                            res = false;
                        }
                    }
                    else
                    {
                        res = false;
                    }
                }
            }
            catch(Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetAxialCompressionRatio");
                res = false;
            }
            return res;
        }
        public bool GetAngularReinforcementDiaLimited(int columnID,out double arDiaLimited)
        {
            bool res = true;
            arDiaLimited = 0.0;
            double asBiAxialT = 0.0;
            double asBiAxialB = 0.0;
            try
            {
                string sql = "select AsBiAxialT,AsBiAxialB from tblRCColDsn where ID =" + columnID;
                DataTable dt = ExecuteDataTable(sql);
                string asBiAxialTStr = "";
                string asBiAxialBStr = "";
                foreach (DataRow dr in dt.Rows)
                {
                    if(dr["AsBiAxialT"]!=null)
                    {
                        asBiAxialTStr = dr["AsBiAxialT"].ToString();
                    }
                    if(dr["AsBiAxialB"]!=null)
                    {
                        asBiAxialBStr = dr["AsBiAxialB"].ToString();
                    }
                    break;
                }
                if(string.IsNullOrEmpty(asBiAxialTStr) && string.IsNullOrEmpty(asBiAxialBStr))
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(asBiAxialTStr))
                {
                    string[] values = asBiAxialTStr.Split(',');
                    if (values != null && values.Length > 1)
                    {
                        if (ThColumnInfoUtils.IsNumeric(values[0]))
                        {
                            asBiAxialT = Convert.ToDouble(values[0]);
                        }                       
                    }
                   
                }
                if (!string.IsNullOrEmpty(asBiAxialBStr))
                {
                    string[] values = asBiAxialBStr.Split(',');
                    if (values != null && values.Length > 1)
                    {
                        if (ThColumnInfoUtils.IsNumeric(values[0]))
                        {
                            asBiAxialT = Convert.ToDouble(values[0]);
                        }
                    }
                }
                arDiaLimited = Math.Max(asBiAxialT, asBiAxialB);
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetAngularReinforcementDiaLimited");
                res = false;
            }
            return res;
        }
        public bool GetProtectLayerThickInTblColSegPara(int columnID, out double protectLayerThickness)
        {
            bool res = false;
            protectLayerThickness = 0.0;
            try
            {
                string sql = "select ParaVal from tblColSegPara where ID =" + columnID +" and Kind=4";
                DataTable dt = ExecuteDataTable(sql);
                string protectThickStr = "";
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ParaVal"] != null)
                    {
                        protectThickStr = dr["ParaVal"].ToString();
                    }
                    break;
                }
                if(!string.IsNullOrEmpty(protectThickStr) && ThColumnInfoUtils.IsNumeric(protectThickStr))
                {
                    protectLayerThickness = Convert.ToDouble(protectThickStr);
                }
                if(protectLayerThickness>0.0)
                {
                    return true;
                }                
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetProtectLayerThickInTblColSegPara");
            }
            return res;
        }
        public bool GetProtectLayerThickInTblStdFlrPara(int strFlrID, out double protectLayerThickness)
        {
            bool res = false;
            protectLayerThickness = 0.0;
            try
            {
                string sql = "select ParaVal from tblStdFlrPara where StdFlrID =" + strFlrID + " and Kind=14";
                DataTable dt = ExecuteDataTable(sql);
                string protectThickStr = "";
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ParaVal"] != null)
                    {
                        protectThickStr = dr["ParaVal"].ToString();
                    }
                    break;
                }
                if (!string.IsNullOrEmpty(protectThickStr) && ThColumnInfoUtils.IsNumeric(protectThickStr))
                {
                    protectLayerThickness = Convert.ToDouble(protectThickStr);
                }
                if (protectLayerThickness > 0.0)
                {
                    if (protectLayerThickness == 9999)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetProtectLayerThickInTblStdFlrPara");
            }
            return res;
        }
        public bool GetProtectLayerThickInTblProjectPara(out double protectLayerThickness)
        {
            bool res = false;
            protectLayerThickness = 0.0;
            try
            {
                string sql = "select ParaVal from tblProjectPara where ID=710";
                DataTable dt = ExecuteDataTable(sql);
                string protectThickStr = "";
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ParaVal"] != null)
                    {
                        protectThickStr = dr["ParaVal"].ToString();
                    }
                    break;
                }
                if (!string.IsNullOrEmpty(protectThickStr) && ThColumnInfoUtils.IsNumeric(protectThickStr))
                {
                    protectLayerThickness = Convert.ToDouble(protectThickStr);
                }
                if (protectLayerThickness > 0.0)
                {
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetProtectLayerThickInTblProjectPara");
            }
            return res;
        }
        public int GetUndergroundFloor()
        {
            int floorNum = 0;
            try
            {
                string sql = "select * from tblProjectPara where ID=106";
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ParaVal"] != null)
                    {
                        floorNum = Convert.ToInt32(dr["ParaVal"]);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetUndergroundFloor");
            }
            return floorNum;
        }
        public List<FloorInfo> GetNaturalFloorInfs()
        {
            List<FloorInfo> flrInfs = new List<FloorInfo>();
            try
            {
                string sql = "select No_,StdFlrID,Height from tblFloor";
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["No_"] != null && dr["StdFlrID"] != null)
                    {
                        FloorInfo floorInfo = new FloorInfo();
                        floorInfo.No = Convert.ToInt32(dr["No_"]);
                        floorInfo.StdFlrID = Convert.ToInt32(dr["StdFlrID"]);
                        floorInfo.Height = Convert.ToInt32(dr["Height"]);
                        flrInfs.Add(floorInfo);
                    }
                }
                flrInfs=flrInfs.OrderBy(i => i.No).ToList();
                int undergoundFloorNum = GetUndergroundFloor();
                for(int i=0;i< undergoundFloorNum;i++)
                {
                    flrInfs[i].Name = -1 * (undergoundFloorNum - i) + "F";
                }
                for (int i = undergoundFloorNum; i < flrInfs.Count; i++)
                {
                    flrInfs[i].Name = (i-undergoundFloorNum)+1 + "F";
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetNaturalFloors");
            }
            return flrInfs;
        }
    }
}
