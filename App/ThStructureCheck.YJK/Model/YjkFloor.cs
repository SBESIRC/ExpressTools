using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    /// <summary>
    /// tblFloor
    /// </summary>
    public class YjkFloor: YjkEntityInfo
    {
        public int No_ { get; set; }
        public string Name { get; set; }
        public int StdFlrID { get; set; }
        public double LevelB { get; set; }
        public double Height { get; set; }
    }
}
