using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    class YjkWallCalculateInfo:ICalculateInfo
    {
        private string dtlModelPath = "";
        private string dtlCalcPath = "";
        public YjkWallCalculateInfo(string dtlModelPath, string dtlCalcPath)
        {
            this.dtlModelPath = dtlModelPath;
            this.dtlCalcPath = dtlCalcPath;
        }
        public ICalculateInfo GetCalculateInfo(IEntityInf entInf)
        {
            if (!(entInf is YjkWall))
            {
                return null;
            }
            return this;
        }
    }
}
