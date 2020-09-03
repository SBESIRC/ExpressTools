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
        public string GetLoadRange(string load)
        {
            switch (load)
            {
                case "LoadHeightLow" :
                    return "h<=24m";
                case "LoadHeightMiddle" :
                    return "24m<h<=50m";
                case "LoadHeightHigh" :
                    return "50m<h<100m";
                default:
                    break;
            }
            return string.Empty;
        }
        public string GetStairLocation(string location)
        {
            switch (location)
            {
                case "OnGround":
                    return "地上";
                case "UnderGound":
                    return "地下";
                default:
                    break;
            }
            return string.Empty;
        }
        public string GetStairSpaceState(string state)
        {
            switch (state)
            {
                case "Residence":
                    return "住宅";
                case "Business":
                    return "商业";
                default:
                    break;
            }
            return string.Empty;
        }

        public abstract void ExportToExcel(ThFanVolumeModel fanmodel, Worksheet setsheet, Worksheet targetsheet, FanDataModel fandatamodel, ExcelFile excelfile);
    }
}
