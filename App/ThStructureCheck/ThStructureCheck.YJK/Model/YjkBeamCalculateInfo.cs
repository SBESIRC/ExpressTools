using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Interface;

namespace ThStructureCheck.YJK.Model
{
    public class YjkBeamCalculateInfo : ICalculateInfo
    {
        private string dtlModelPath = "";
        private string dtlCalcPath = "";
        public YjkBeamCalculateInfo(string dtlModelPath, string dtlCalcPath)
        {
            this.dtlModelPath = dtlModelPath;
            this.dtlCalcPath = dtlCalcPath;
        }
        public ICalculateInfo GetCalculateInfo(IEntityInf entInf)
        {
            return this;
        }
    }
}
