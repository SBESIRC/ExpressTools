using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public interface IDatabaseDataSource
    {
        List<DrawColumnInf> ColumnInfs { get; set; }
        void Extract(int floorNo);
    }
}
