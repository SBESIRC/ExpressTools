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
        /// <summary>
        /// 获取模型库(dtlmodel)的表(TblFloor、TblColSeg)表中获取No值
        /// </summary>
        public bool GetTblFloorTblColSegNoFromDtlmodel(int JtID,out int tblFloorNo_,out int tblColSegNo_)
        {
            bool result = false;
            tblFloorNo_ = -1;
            tblColSegNo_ = -1;
            try
            {
                string sql = "select tblFloor.No_ as tblFloorNo,tblColSeg.No_ as tblColSegNo from tblFloor join tblColSeg" +
                    " where tblFloor.StdFlrID=tblColSeg.StdFlrID and tblColSeg.JtID=" + JtID;
                DataTable dt = ExecuteDataTable(sql);
                string tblFloorNoStr = "";
                string tblColSegNoStr = "";
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["tblFloorNo"] != null)
                    {
                        tblFloorNoStr = dr["tblFloorNo"].ToString();
                    }
                    if (dr["tblColSegNo"] != null)
                    {
                        tblColSegNoStr = dr["tblColSegNo"].ToString();
                    }
                    break;
                }
                if (!string.IsNullOrEmpty(tblFloorNoStr) && !string.IsNullOrEmpty(tblColSegNoStr))
                {
                    if(int.TryParse(tblFloorNoStr,out tblFloorNo_) && 
                        int.TryParse(tblColSegNoStr, out tblColSegNo_)
                        )
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetTblFloorTblColSegNoFromDtlmodel");
            }
            return result;
        }
        /// <summary>
        /// 从计算库(dtlCalc)的表(tblColSeg)中获取柱子ID
        /// </summary>
        /// <param name="tblFloor_No"></param>
        /// <param name="tblColSegNo"></param>
        /// <returns></returns>
        public int GetTblColSegIDFromDtlCalc(int tblFloor_No,int tblColSegNo)
        {
            int id=-1;
                try
            {
                string sql = "select ID from tblColSeg where MdlFlr = " + tblFloor_No + " and MdlNo = " + tblColSegNo;
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
                ThColumnInfoUtils.WriteException(ex, "GetTblColSegIDFromDtlCalc");
            }
            return id;
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
        /// <summary>
        /// 从计算库(dtlCalc)的表(tblRCColDsn)中获取轴压比
        /// </summary>
        /// <param name="columnID"></param>
        /// <param name="axialCompressionRatio"></param>
        /// <param name="axialCompressionRatioLimited"></param>
        /// <returns></returns>
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
                        if (BaseFunction.IsNumeric(values[0]))
                        {
                            axialCompressionRatio = Convert.ToDouble(values[0]);
                        }
                        else
                        {
                            res = false;
                        }
                        if (res && BaseFunction.IsNumeric(values[1]))
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
        /// <summary>
        /// 从计算库(dtlCalc)的表(tblRCColDsn)中获取剪跨比
        /// </summary>
        /// <param name="columnID"></param>
        /// <param name="shearSpanRatio"></param>
        /// <returns></returns>
        public bool GetShearSpanRatio(int columnID,out double shearSpanRatio)
        {
            bool res = true;
            shearSpanRatio = 0.0;
            try
            {
                string sql = "select JKB from tblRCColDsn where ID =" + columnID;
                DataTable dt = ExecuteDataTable(sql);
                string jkb = "";
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["JKB"] != null)
                    {
                        jkb = dr["JKB"].ToString();
                    }
                    break;
                }
                if (!string.IsNullOrEmpty(jkb))
                {
                    res = double.TryParse(jkb, out shearSpanRatio);
                }
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetShearSpanRatio");
                res = false;
            }
            return res;
        }
        /// <summary>
        /// 从模型库(dtlModel)的表(tblProjectPara)中获取假定箍筋间距
        /// </summary>
        /// <param name="intStirrupSpacingCal"></param>
        /// <returns></returns>
        public bool GetIntStirrupSpacingCal(out double intStirrupSpacingCal)
        {
            bool res = false;
            intStirrupSpacingCal = 0.0;
            try
            {
                string sql = "select * from tblProjectPara where ID =" + 817;
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ParaVal"] != null)
                    {
                        intStirrupSpacingCal = (double)dr["ParaVal"];
                        res = true;
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetIntStirrupSpacingCal");
                res = false;
            }
            return res;
        }
        /// <summary>
        /// 从模型库(dtlModel)的表(tblProjectPara)中获取设防烈度
        /// </summary>
        /// <param name="columnID"></param>
        /// <param name="fortificationIntensity"></param>
        /// <returns></returns>
        public bool GetFortificationIntensity(out double fortificationIntensity)
        {
            bool res = true;
            fortificationIntensity = 0.0;
            try
            {
                string sql = "select * from tblProjectPara where ID =" + 302;
                DataTable dt = ExecuteDataTable(sql);
                string fortification = "";
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ParaVal"] != null)
                    {
                        fortification = dr["ParaVal"].ToString();
                    }
                    break;
                }
                if (!string.IsNullOrEmpty(fortification))
                {
                    res = double.TryParse(fortification, out fortificationIntensity);
                }
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetFortificationIntensity");
                res = false;
            }
            return res;
        }
        /// <summary>
        /// 从计算库(dtlCalc)的表(tblRCColDsn)中获取角筋直径限值
        /// </summary>
        /// <param name="columnID"></param>
        /// <param name="arDiaLimited"></param>
        /// <returns></returns>
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
                        if (BaseFunction.IsNumeric(values[0]))
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
                        if (BaseFunction.IsNumeric(values[0]))
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
        /// <summary>
        /// 在计算库(dtlCalc)的表(tblRCColDsn)中判断是否为角柱
        /// </summary>
        /// <param name="columnID"></param>
        /// <returns></returns>
        public bool CheckCornerColumn(int columnID)
        {
            bool isCorner = false;
            try
            {
                string sql = "select AsBiAxialT,AsBiAxialB from tblRCColDsn where ID =" + columnID;
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["AsBiAxialT"] != null)
                    {
                        isCorner = true;
                        break;
                    }
                    if (dr["AsBiAxialB"] != null)
                    {
                        isCorner = true;
                        break;
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "CheckCornerColumn");
            }
            return isCorner;
        }
        /// <summary>
        /// 从计算库(dtlCalc)的表(tblColSegPara)中获取保护层厚度
        /// </summary>
        /// <param name="columnID"></param>
        /// <param name="protectLayerThickness"></param>
        /// <returns></returns>
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
                if(!string.IsNullOrEmpty(protectThickStr) && BaseFunction.IsNumeric(protectThickStr))
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
        /// <summary>
        /// 从计算库(dtlCalc)的表(tblStdFlrPara)中获取保护层厚度
        /// </summary>
        /// <param name="strFlrID"></param>
        /// <param name="protectLayerThickness"></param>
        /// <returns></returns>
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
                if (!string.IsNullOrEmpty(protectThickStr) && BaseFunction.IsNumeric(protectThickStr))
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
        /// <summary>
        /// 从模型库(dtlModel)的表(tblProjectPara)中保护层厚度
        /// </summary>
        /// <param name="protectLayerThickness"></param>
        /// <returns></returns>
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
                if (!string.IsNullOrEmpty(protectThickStr) && BaseFunction.IsNumeric(protectThickStr))
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
        /// <summary>
        /// 获取自然层数
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 从计算库(dtlCalc)中的表tblColSegPara中获取抗震等级
        /// </summary>
        /// <param name="columnID"></param>
        /// <returns></returns>
        public double GetAntiSeismicGradeInCalculation(int columnID)
        {
            double paraVal = 0.0;
            try
            {                
                string sql = "select ParaVal from tblColSegPara where ID =" + columnID + " and Kind=3";
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ParaVal"] != null)
                    {
                        paraVal = Convert.ToDouble(dr["ParaVal"]);
                    }
                    break;
                }                
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetAntiSeismicGradeInCalculation");
            }
            return paraVal;
        }
        /// <summary>
        /// 从模型库中获取工程信息表中的抗震等级
        /// </summary>
        /// <returns></returns>
        public List<double> GetAntiSeismicGradeInModel()
        {
            double firstValue = 0.0;
            double secondValue = 0.0;
            try
            {
                string sql = "select * from tblProjectPara where ID =701 or ID=704";
                DataTable dt = ExecuteDataTable(sql);
                
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ID"] != null)
                    {
                        if(dr["ID"].ToString()== "701")
                        {
                            firstValue = Convert.ToDouble(dr["ParaVal"]);
                        }
                        else if(dr["ID"].ToString() == "704")
                        {
                            secondValue= Convert.ToDouble(dr["ParaVal"]);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetAntiSeismicGradeInCalculation");
            }
            return new List<double> { firstValue , secondValue };
        }
        /// <summary>
        /// 从模型库中获取工程信息表中的结构类型
        /// </summary>
        /// <returns></returns>
        public double GetStructureTypeInModel()
        {
            double paraValue = 0.0;
            try
            {
                string sql = "select * from tblProjectPara where ID =101";
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ParaVal"] != null)
                    {
                        paraValue= Convert.ToDouble(dr["ParaVal"]);
                    }
                    break;
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetAntiSeismicGradeInCalculation");
            }
            return paraValue;
        }
        /// <summary>
        /// 从计算库(dtlCalc)中的表(tblRCColDsn)获取配筋面积限值
        /// </summary>
        /// <param name="columnID"></param>
        /// <returns></returns>
        public List<double> GetDblXYAsCal(int columnID)
        {
            List<double> xyValues = new List<double>() {0.0,0.0};
            try
            {
                string sql = "select AsDsnT,AsDsnB from tblRCColDsn where ID =" + columnID;
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["AsDsnT"] != null && dr["AsDsnB"] != null)
                    {
                        string asDsnT = dr["AsDsnT"].ToString();
                        string asDsnB = dr["AsDsnB"].ToString();
                        string[] asDsnTValues = asDsnT.Split(',');
                        string[] asDsnBValues = asDsnB.Split(',');
                        double xAsMax = Math.Max(Convert.ToDouble(asDsnTValues[1]), Convert.ToDouble(asDsnBValues[1]));
                        double yAsMax = Math.Max(Convert.ToDouble(asDsnTValues[2]), Convert.ToDouble(asDsnBValues[2]));
                        xyValues.Add(xAsMax);
                        xyValues.Add(yAsMax);
                    }
                    break;
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetDblXYAsCal");
            }
            return xyValues;
        }
        /// <summary>
        /// 从计算库(dtlCalc)中的表(tblRCColDsn)获取配筋面积限值
        /// </summary>
        /// <param name="columnID"></param>
        /// <returns></returns>
        public double GetDblStirrupAsCalLimited(int columnID)
        {
            double limitedValue = 0.0;
            try
            {
                string sql = "select AsDsnT,AsDsnB from tblRCColDsn where ID =" + columnID;
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["AsDsnT"] != null && dr["AsDsnB"] != null)
                    {
                        string asDsnT = dr["AsDsnT"].ToString();
                        string asDsnB = dr["AsDsnB"].ToString();
                        string[] asDsnTValues = asDsnT.Split(',');
                        string[] asDsnBValues = asDsnB.Split(',');
                        if(asDsnTValues.Length>4 && asDsnBValues.Length>4)
                        {
                            List<double> values = new List<double>();
                            values.Add(Convert.ToDouble(asDsnTValues[3]));
                            values.Add(Convert.ToDouble(asDsnTValues[4]));
                            values.Add(Convert.ToDouble(asDsnBValues[3]));
                            values.Add(Convert.ToDouble(asDsnBValues[4]));
                            limitedValue = values.OrderByDescending(i => i).First();
                        }
                    }
                    break;
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetDblStirrupAsCalLimited");
            }
            return limitedValue;
        }
        /// <summary>
        /// 从计算库(dtlCalc)中的表(tblRCColDsn)获取配筋面积限值
        /// </summary>
        /// <param name="columnID"></param>
        /// <returns></returns>
        public double GetDblStirrupAsCal0Limited(int columnID)
        {
            double limitedValue = 0.0;
            try
            {
                string sql = "select AsCalcT,AsCalcB from tblRCColDsn where ID =" + columnID;
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["AsCalcT"] != null && dr["AsCalcB"] != null)
                    {
                        string asCalcT = dr["AsCalcT"].ToString();
                        string asCalcB = dr["AsCalcB"].ToString();
                        string[] asCalcTValues = asCalcT.Split(',');
                        string[] asCalcBValues = asCalcB.Split(',');
                        if (asCalcTValues.Length > 4 && asCalcBValues.Length > 4)
                        {
                            List<double> values = new List<double>();
                            values.Add(Convert.ToDouble(asCalcTValues[3]));
                            values.Add(Convert.ToDouble(asCalcTValues[4]));
                            values.Add(Convert.ToDouble(asCalcBValues[3]));
                            values.Add(Convert.ToDouble(asCalcBValues[4]));
                            limitedValue = values.OrderByDescending(i => i).First();
                        }
                    }
                    break;
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetDblStirrupAsCal0Limited");
            }
            return limitedValue;
        }
        /// <summary>
        /// 从计算库(dtlCalc)中的表(tblRCColDsn)体积配筋率限值
        /// </summary>
        /// <param name="columnID"></param>
        /// <returns></returns>
        public bool GetVolumeReinforceLimitedValue(int columnID,out double volumeReinforceLimitedValue)
        {
            bool res = false;
            volumeReinforceLimitedValue = 0.0;
            try
            {
                string sql = "select AsRatio from tblRCColDsn where ID =" + columnID;
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["AsRatio"] != null)
                    {
                        string asRatio = dr["AsRatio"].ToString();
                        string[] asRatioValues = asRatio.Split(',');
                        if(asRatioValues.Count()>2)
                        {
                            res= double.TryParse(asRatioValues[2], out volumeReinforceLimitedValue);
                        }
                    }
                    break;
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetVolumeReinforceLimitedValue");
            }
            return res;
        }
    }
}
