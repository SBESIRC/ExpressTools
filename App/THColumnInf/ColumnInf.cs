using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace THColumnInfo
{
    public enum ErrorMsg
    {
        /// <summary>
        /// 柱编号缺失
        /// </summary>
        CodeEmpty,
        /// <summary>
        ///  参数识别不全
        /// </summary>
        InfNotCompleted,
        /// <summary>
        /// 数据正确
        /// </summary>
        OK
    }
    public class ColumnInf
    {
        /// <summary>
        /// 柱子编号
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// 柱子规格
        /// </summary>
        public string Spec { get; set; } = "";
        /// <summary>
        /// 如果柱子边没有柱原位标注，则所有钢筋规格一样
        /// </summary>
        public string IronSpec { get; set; } = "";
        /// <summary>
        /// 四角钢筋规格
        /// </summary>
        public string CornerIronSpec { get; set; } = "";
        /// <summary>
        /// X方向钢筋规格
        /// </summary>
        public string XIronSpec { get; set; } = "";
        /// <summary>
        /// Y方向钢筋规格
        /// </summary>
        public string YIronSpec { get; set; } = "";
        /// <summary>
        /// X方向钢筋数量
        /// </summary>
        public int XIronNum { get; set; } = 0;
        /// <summary>
        /// Y方向钢筋数量
        /// </summary>
        public int YIronNum { get; set; } = 0;
        /// <summary>
        /// 相邻两个箍筋在垂直方向高度
        /// </summary>
        public string NeiborGuJinHeightSpec { get; set; } = "";
        /// <summary>
        /// 用于记录正确柱子的数量(在TreeView填充数据，整理信息的时候设置)
        /// </summary>
        public int Num { get; set; } = 0; 
        /// <summary>
        /// 柱子对角点（左下角和右上角）
        /// </summary>
        public List<Point3d> Points { get; set; } = new List<Point3d>();
        /// <summary>
        /// 当前物体句柄
        /// </summary>
        public string CurrentHandle { get; set; } = "";
        /// <summary>
        /// 与正确柱子具有相同编号的物体句柄
        /// </summary>
        public List<string> Handles { get; set; } = new List<string>();
        /// <summary>
        /// 除了编号，其它都没赋值
        /// </summary>
        /// <returns></returns>
        public bool OnlyCodeSetValue()
        {
            bool res = false;
            if (!string.IsNullOrEmpty(this.Code) && this.Spec == "" && this.IronSpec == "" && this.CornerIronSpec == "" && this.XIronSpec == ""
                && this.YIronSpec == "" && this.NeiborGuJinHeightSpec == "")
            {
                return true;
            }
            return res;
        }
        /// <summary>
        /// 获取柱子信息状态
        /// </summary>
        /// <returns></returns>
        public ErrorMsg GetColumnInfStatus()
        {
            if(string.IsNullOrEmpty(this.Code))
            {
                return ErrorMsg.CodeEmpty;
            }
            if(OnlyCodeSetValue()) //仅仅编号有值
            {
                return ErrorMsg.OK;
            }
            if(string.IsNullOrEmpty(this.NeiborGuJinHeightSpec))
            {
                return ErrorMsg.InfNotCompleted;
            }
            if(string.IsNullOrEmpty(this.IronSpec) && (string.IsNullOrEmpty(this.CornerIronSpec) &&
                string.IsNullOrEmpty(this.XIronSpec) && string.IsNullOrEmpty(this.YIronSpec)))
            {
                return ErrorMsg.InfNotCompleted;
            }
            if(string.IsNullOrEmpty(this.Spec))
            {
                return ErrorMsg.InfNotCompleted;
            }
            if(this.XIronNum<=0 || this.XIronNum <= 0)
            {
                return ErrorMsg.InfNotCompleted;
            }
            if(string.IsNullOrEmpty(this.NeiborGuJinHeightSpec))
            {
                return ErrorMsg.InfNotCompleted;
            }
            return ErrorMsg.OK;
        }
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
