using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianHua.FanSelection.Model;

namespace TianHua.FanSelection.ExcelExport
{
    public abstract class BaseExportWorker
    {
        public static BaseExportWorker Create(ThFanVolumeModel model)
        {
            if (model is FireFrontModel)
            {
                return new FireFrontExportWorker();
            }
            else if (model is FontroomNaturalModel)
            {
                return new FontroomNaturalExportWorker();
            }
            else if (model is FontroomWindModel)
            {
                return new FontroomWindExportWorker();
            }
            else if (model is StaircaseNoAirModel)
            {
                return new StaircaseNoAirExportWorker();
            }
            else if (model is StaircaseAirModel)
            {
                return new StaircaseAirExportWorker();
            }
            else if (model is RefugeRoomAndCorridorModel)
            {
                return new RefugeCorridorExportWorker();
            }
            else if (model is RefugeFontRoomModel)
            {
                return new RefugeFontRoomExportWorker();
            }
            return null;
        }
        public abstract void ExportToExcel(ThFanVolumeModel fanmodel, Worksheet setsheet, Worksheet targetsheet, FanDataModel fandatamodel, ExcelFile excelfile);
    }
}
