using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class FloorInfo
    {
        public int No { get; set; }
        /// <summary>
        /// 自然层名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 标准层ID
        /// </summary>
        public int StdFlrID { get; set; }
        /// <summary>
        /// 层高
        /// </summary>
        public double Height { get; set; }
        /// <summary>
        /// 标准层名称
        /// </summary>
        public string StdFlrName { get; set; } = "";
    }
}
