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
        void Extract();
    }
}
