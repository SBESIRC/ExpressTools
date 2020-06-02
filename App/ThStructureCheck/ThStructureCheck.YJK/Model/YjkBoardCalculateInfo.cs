using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.YJK.Model
{
    class YjkBoardCalculateInfo:ICalculateInfo
    {
        private string dtlModelPath = "";
        private string dtlCalcPath = "";
        public YjkBoardCalculateInfo(string dtlModelPath, string dtlCalcPath)
        {
            this.dtlModelPath = dtlModelPath;
            this.dtlCalcPath = dtlCalcPath;
        }
        public ICalculateInfo GetCalculateInfo(IEntityInf entInf)
        {
            if (!(entInf is YjkBoard))
            {
                return null;
            }           
            return this;
        }
    }
}
