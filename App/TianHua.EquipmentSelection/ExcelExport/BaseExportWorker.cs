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
            if (model is FireFrontModel fm)
            {
                fm.FrontRoomDoors = fm.FrontRoomDoors.Where(d => d.Count_Door_Q * d.Height_Door_Q * d.Width_Door_Q != 0).ToList();
                return new FireFrontExportWorker();
            }
            else if (model is FontroomNaturalModel fn)
            {
                fn.FrontRoomDoors = fn.FrontRoomDoors.Where(d => d.Count_Door_Q * d.Height_Door_Q * d.Width_Door_Q != 0).ToList();
                fn.StairCaseDoors = fn.StairCaseDoors.Where(d => d.Count_Door_Q * d.Height_Door_Q * d.Width_Door_Q != 0).ToList();
                return new FontroomNaturalExportWorker();
            }
            else if (model is FontroomWindModel ft)
            {
                ft.FrontRoomDoors = ft.FrontRoomDoors.Where(d => d.Count_Door_Q * d.Height_Door_Q * d.Width_Door_Q != 0).ToList();
                return new FontroomWindExportWorker();
            }
            else if (model is StaircaseNoAirModel sn)
            {
                sn.FrontRoomDoors = sn.FrontRoomDoors.Where(d => d.Count_Door_Q * d.Height_Door_Q * d.Width_Door_Q != 0).ToList();
                return new StaircaseNoAirExportWorker();
            }
            else if (model is StaircaseAirModel sa)
            {
                sa.FrontRoomDoors = sa.FrontRoomDoors.Where(d => d.Count_Door_Q * d.Height_Door_Q * d.Width_Door_Q != 0).ToList();
                return new StaircaseAirExportWorker();
            }
            else if (model is RefugeRoomAndCorridorModel)
            {
                return new RefugeCorridorExportWorker();
            }
            else if (model is RefugeFontRoomModel rf)
            {
                rf.FrontRoomDoors = rf.FrontRoomDoors.Where(d => d.Count_Door_Q * d.Height_Door_Q * d.Width_Door_Q != 0).ToList();
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
