using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common;
using ThStructureCheck.YJK.Interface;

namespace ThStructureCheck.YJK.Query
{
    public class YjkQuery
    {
        private string connectionString = string.Empty;
        protected string dbPath= string.Empty;
        public YjkQuery(string dbPath)
        {
            this.dbPath = dbPath;
            this.connectionString= "Data Source=" + dbPath;
        }
        public DataTable ExecuteDataTable(string sql)
        {
            using (SQLiteConnection conn = new SQLiteConnection(this.connectionString))
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
        public virtual IList<IEntityInf> Extract(int floorNo)
        {
            return null;
        }
        /// <summary>
        /// 从模型库(dtlModel)中的表tblColSegPara中获取抗震等级 获取抗震等级在
        /// </summary>
        /// <returns></returns>
        public List<double> GetAntiSeismicGradeInModel()
        {
            List<double> res = new List<double>();
            double paraVal = 0.0;
            double adjustVal = 0.0;
            try
            {
                string sql = "select ID,ParaVal from tblProjectPara where ID=701 or ID=704";
                DataTable dt = ExecuteDataTable(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ID"] != null)
                    {
                        if (Convert.ToDouble(dr["ID"]) == 701)
                        {
                            if (dr["ParaVal"] != null)
                            {
                                paraVal = Convert.ToDouble(dr["ParaVal"]);
                            }
                        }
                        if (Convert.ToDouble(dr["ID"]) == 704)
                        {
                            if (dr["ParaVal"] != null)
                            {
                                adjustVal = Convert.ToDouble(dr["ParaVal"]);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Utils.WriteException(ex, "GetAntiSeismicGradeInModel");
            }
            res.Add(paraVal);
            res.Add(adjustVal);
            return res;
        }
    }
}
