﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Model;

namespace ThStructureCheck.YJK.Query
{
    public class YjkWallColQuery:YjkQuery
    {
        public YjkWallColQuery(string dbPath) : base(dbPath)
        {
        }
        /// <summary>
        /// 提取dtlModel中所有的柱子信息
        /// </summary>
        /// <param name="floorNo"></param>
        /// <returns></returns>
        public List<CalcWallCol> GetBeamLinkWalls(int flrNo,int jt)
        {
            List<CalcWallCol> results = new List<CalcWallCol>();
            string sql = "select * from tblWallCol where FlrNo ="
                + flrNo + " and (JtRT=" + jt + " or JtLT=" + jt+")";
            DataTable dt = ExecuteDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                CalcWallCol item = new CalcWallCol();
                item.ID = Convert.ToInt32(dr["ID"].ToString());
                item.TowNo = Convert.ToInt32(dr["TowNo"].ToString());
                item.FlrNo = Convert.ToInt32(dr["FlrNo"].ToString());
                item.MdlFlr = Convert.ToInt32(dr["MdlFlr"].ToString());
                item.MdlNos = dr["MdlNos"].ToString();
                item.MdlPos= dr["MdlPos"].ToString();
                item.JtLB = Convert.ToInt32(dr["JtLB"].ToString());
                item.JtRB = Convert.ToInt32(dr["JtRB"].ToString());
                item.JtRT = Convert.ToInt32(dr["JtRT"].ToString());
                item.JtLT = Convert.ToInt32(dr["JtLT"].ToString());
                item.JtsT = dr["JtsT"].ToString();
                item.JtsB = dr["JtsB"].ToString();
                item.DbPath = this.dbPath;
                results.Add(item);
            }
            return results;
        }
    }
}
