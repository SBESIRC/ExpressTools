using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK.Query
{
    public class YjkGridQuery:YjkQuery
    {
        public YjkGridQuery(string dbPath):base(dbPath)
        {
        }
        public ModelGrid GetModelGrid(int gridID)
        {
            ModelGrid modelGrid = new ModelGrid();
            string sql = "select * from tblGrid where ID=" + gridID;
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                modelGrid.ID = Convert.ToInt32(dr["ID"].ToString());
                modelGrid.No_ = Convert.ToInt32(dr["No_"].ToString());
                modelGrid.StdFlrID = Convert.ToInt32(dr["StdFlrID"].ToString());
                modelGrid.Jt1ID = Convert.ToInt32(dr["Jt1ID"].ToString());
                modelGrid.Jt2ID = Convert.ToInt32(dr["Jt2ID"].ToString());
                modelGrid.AxisID = Convert.ToInt32(dr["AxisID"].ToString());
                modelGrid.DbPath = this.dbPath;
            }
            return modelGrid;
        }
    }
}
