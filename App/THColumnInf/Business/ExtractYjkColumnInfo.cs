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
            string sql = "select tblColSeg.StdFlrID,tblColSeg.JtID,tblColSeg.EccX,tblColSeg.EccY,tblColSeg.Rotation," +
                "tblColSect.ShapeVal,tblJoint.X,tblJoint.Y from tblStdFlr join tblColSeg join tblColSect join tblJoint" +
                " where tblStdFlr.No_=" +floorNo+ " and tblStdFlr.ID = tblColSeg.StdFlrID and tblColSeg.SectID = tblColSect.ID and" +
                " tblColSeg.StdFlrID=tblJoint.StdFlrID and tblColSeg.JtID=tblJoint.ID";
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                DrawColumnInf yjkColumnInf = new DrawColumnInf();
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
    }
}
