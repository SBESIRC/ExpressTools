using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public interface IDataSource
    {
        List<ColumnInf> ColumnInfs { get; set; }
        List<ColumnTableRecordInfo> ColumnTableRecordInfos { get; set; }
        List<ColumnTableRecordInfo> InvalidCtris  { get; set; } //记录有问题的柱表信息
        void Extract(bool importCal=false);
    }
}
