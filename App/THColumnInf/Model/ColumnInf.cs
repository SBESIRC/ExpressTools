using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ThColumnInfo
{
    public class ColumnInf
    {
        /// <summary>
        /// 柱子编号
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// 柱子对角点（左下角和右上角）
        /// </summary>
        public List<Point3d> Points { get; set; } = new List<Point3d>();
    }
    class ColumnInfCompare : IComparer<ColumnInf>
    {
        public int Compare(ColumnInf x, ColumnInf y)
        {
            List<string> xCodeStrs = SplitCode(x.Code);
            List<string> yCodeStrs = SplitCode(y.Code);
            int copareIndex = 0;

            if(xCodeStrs.Count==2 && yCodeStrs.Count==2)
            {
                copareIndex = xCodeStrs[0].CompareTo(yCodeStrs[0]);
                if (copareIndex==0)
                {
                    if(Convert.ToDouble(xCodeStrs[1])< Convert.ToDouble(yCodeStrs[1]))
                    {
                        copareIndex = -1;
                    }
                    else if(Convert.ToDouble(xCodeStrs[1]) > Convert.ToDouble(yCodeStrs[1]))
                    {
                        copareIndex = 1;
                    }
                }
            }
            else if(xCodeStrs.Count == 2)
            {
                copareIndex = -1;
            }
            else if(yCodeStrs.Count == 2)
            {
                copareIndex = 1;
            }
            else
            {
                copareIndex = x.Code.CompareTo(y.Code);
            }
            //小于0 x.Code 在y.Code前； 等于0  x.Code ，y.Code位置相同 ；大于0 x.Code 在y.Code后
            return copareIndex;
        }
        private List<string> SplitCode(string code)
        {
            List<string> strs = new List<string>();
            string str = "";
            string num = "";
            byte[] arr = System.Text.Encoding.ASCII.GetBytes(code);
            int startIndex = 0;
            for(int i=0;i< arr.Length;i++)
            {
                if(!((int)(arr[i])>=48 && (int)(arr[i]) <= 57))
                {
                    str += (char)arr[i];
                }
                else
                {
                    startIndex = i;
                    break;
                }
            }
            for (int i = startIndex; i < arr.Length; i++)
            {
                if ((int)(arr[i]) >= 48 && (int)(arr[i]) <= 57)
                {
                    num += (char)arr[i];
                }
                else
                {
                    break;
                }
            }
            if(str!="" && num!="")
            {
                strs.Add(str);
                strs.Add(num);
            }
            return strs;
        }
    }
}
