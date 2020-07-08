using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.YJK.Interface;

namespace ThStructureCheck.YJK.Query
{
    public class YjkBoardQuery:YjkQuery
    {
        public YjkBoardQuery(string dbPath) : base(dbPath)
        {
        }
        public override IList<IEntityInf> Extract(int floorNo)
        {
            return base.Extract(floorNo);
        }
    }
}
