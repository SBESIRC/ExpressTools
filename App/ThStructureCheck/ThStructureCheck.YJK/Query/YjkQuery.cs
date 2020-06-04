using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK
{
    public class YjkQuery
    {
        private string connectionString = string.Empty;
        public YjkQuery(string dbPath)
        {
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
    }
}
