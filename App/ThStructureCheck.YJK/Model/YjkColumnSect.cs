using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    public class ModelColumnSect : YjkEntityInfo
    {
        public int No_ { get; set; }
        public string Name { get; set; }
        public int Mat { get; set; }
        public int Kind { get; set; }
        public string ShapeVal { get; set; }
        public string ShapeVal1 { get; set; }
        public string Spec
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ShapeVal))
                {
                    string[] strs = this.ShapeVal.Split(',');
                    if (this.Kind == 1)
                    {
                        //方形
                        if (strs.Length > 3)
                        {
                            return strs[1] + "x" + strs[2];
                        }
                    }
                    else if(this.Kind == 28)
                    {
                        //L型
                        if (strs.Length > 5)
                        {
                            return strs[1] + "x" + strs[2]+"x"+ strs[3]+"x"+ strs[4];
                        }
                    }
                }
                return "";
            }
        }

    }
}
