using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using TianHua.FanSelection.Model;

namespace TianHua.FanSelection.ExcelExport
{
    public class StaircaseNoAirExportWorker : BaseExportWorker
    {
        public override void ExportToExcel(ThFanVolumeModel fanmodel, Worksheet setsheet, Worksheet targetsheet, FanDataModel fandatamodel, ExcelFile excelfile)
        {
            StaircaseNoAirModel StaircaseNoAir = fanmodel as StaircaseNoAirModel;
            setsheet.SetCellValue("D2", fandatamodel.FanNum);
            setsheet.SetCellValue("D3", fandatamodel.Name);
            setsheet.SetCellValue("D4", Math.Max(StaircaseNoAir.QueryValue, (StaircaseNoAir.DoorOpeningVolume + StaircaseNoAir.LeakVolume)).ToString());
            setsheet.SetCellValue("D5", (StaircaseNoAir.DoorOpeningVolume + StaircaseNoAir.LeakVolume).ToString());
            setsheet.SetCellValue("D6", StaircaseNoAir.DoorOpeningVolume.ToString());
            setsheet.SetCellValue("D7", StaircaseNoAir.LeakVolume.ToString());
            setsheet.SetCellValue("D9", StaircaseNoAir.OverAk.ToString());
            setsheet.SetCellValue("D10", "1");
            setsheet.SetCellValue("D11", StaircaseNoAir.StairN1.ToString());
            setsheet.SetCellValue("D12", LeakArea(StaircaseNoAir).ToString());
            setsheet.SetCellValue("D13", "12");
            setsheet.SetCellValue("D14", GetN2Value(StaircaseNoAir).ToString());
            setsheet.SetCellValue("D15", StaircaseNoAir.QueryValue.ToString());
            setsheet.SetCellValue("D16", StaircaseNoAir.Count_Floor.ToString());
            setsheet.SetCellValue("D17", GetLoadRange(StaircaseNoAir.Load.ToString()));
            setsheet.SetCellValue("D18", GetStairLocation(StaircaseNoAir.Stair.ToString()));
            setsheet.SetCellValue("D19", GetStairSpaceState(StaircaseNoAir.Type_Area.ToString()));
            int rowNo = 20;
            foreach (var frontRoomDoor in StaircaseNoAir.FrontRoomDoors)
            {
                setsheet.SetCellValue("D" + rowNo, frontRoomDoor.Height_Door_Q.ToString());
                setsheet.SetCellValue("D" + (rowNo + 1), frontRoomDoor.Width_Door_Q.ToString());
                setsheet.SetCellValue("D" + (rowNo + 2), frontRoomDoor.Count_Door_Q.ToString());
                setsheet.SetCellValue("D" + (rowNo + 3), frontRoomDoor.Crack_Door_Q.ToString());
                setsheet.SetCellValue("D" + (rowNo + 4), frontRoomDoor.Type.ToString());
                rowNo += 5;
            }
            excelfile.CopyRangeToOtherSheet(setsheet, "A1:D34", targetsheet);
        }

        private double LeakArea(StaircaseNoAirModel StaircaseNoAir)
        {
            double doorarea = 0;
            foreach (var door in StaircaseNoAir.FrontRoomDoors)
            {
                if (door.Type.ToString() == "单扇")
                {
                    doorarea += (door.Width_Door_Q + door.Height_Door_Q) * 2 * door.Crack_Door_Q / 1000;
                }
                else
                {
                    doorarea += ((door.Width_Door_Q + door.Height_Door_Q) * 2 + door.Height_Door_Q) * door.Crack_Door_Q / 1000;
                }
            }
            return doorarea;
        }

        private double GetN2Value(StaircaseNoAirModel StaircaseNoAir)
        {
            return Math.Max((StaircaseNoAir.Count_Floor - StaircaseNoAir.StairN1) * StaircaseNoAir.FrontRoomDoors.Sum(q => q.Count_Door_Q),0);
        }
    }
}
