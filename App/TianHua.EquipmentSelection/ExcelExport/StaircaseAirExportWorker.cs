using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using TianHua.FanSelection.Model;

namespace TianHua.FanSelection.ExcelExport
{
    public class StaircaseAirExportWorker : BaseExportWorker
    {
        public override void ExportToExcel(ThFanVolumeModel fanmodel, Worksheet setsheet, Worksheet targetsheet, FanDataModel fandatamodel, ExcelFile excelfile)
        {
            StaircaseAirModel StaircaseAir = fanmodel as StaircaseAirModel;
            setsheet.SetCellValue("D2", fandatamodel.GetFanNum());
            setsheet.SetCellValue("D3", fandatamodel.Name);
            setsheet.SetCellValue("D4", Math.Max(StaircaseAir.QueryValue, (StaircaseAir.DoorOpeningVolume + StaircaseAir.LeakVolume)).ToString());
            setsheet.SetCellValue("D5", (StaircaseAir.DoorOpeningVolume + StaircaseAir.LeakVolume).ToString());
            setsheet.SetCellValue("D6", StaircaseAir.DoorOpeningVolume.ToString());
            setsheet.SetCellValue("D7", StaircaseAir.LeakVolume.ToString());
            setsheet.SetCellValue("D9", StaircaseAir.OverAk.ToString());
            setsheet.SetCellValue("D10", "1");
            setsheet.SetCellValue("D11", StaircaseAir.StairN1.ToString());
            setsheet.SetCellValue("D12", LeakArea(StaircaseAir).ToString());
            setsheet.SetCellValue("D13", "12");
            setsheet.SetCellValue("D14", GetN2Value(StaircaseAir).ToString());
            setsheet.SetCellValue("D15", StaircaseAir.QueryValue.ToString());
            setsheet.SetCellValue("D16", StaircaseAir.Count_Floor.ToString());
            setsheet.SetCellValue("D17", StaircaseAir.Load.ToString());
            setsheet.SetCellValue("D18", StaircaseAir.Stair.ToString());
            setsheet.SetCellValue("D19", StaircaseAir.Type_Area.ToString());
            int rowNo = 20;
            foreach (var frontRoomDoor in StaircaseAir.FrontRoomDoors)
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

        private double LeakArea(StaircaseAirModel StaircaseAir)
        {
            double doorarea = 0;
            foreach (var door in StaircaseAir.FrontRoomDoors)
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

        private double GetN2Value(StaircaseAirModel StaircaseAir)
        {
            return Math.Max((StaircaseAir.Count_Floor - StaircaseAir.StairN1) * StaircaseAir.FrontRoomDoors.Sum(q => q.Count_Door_Q), 0);
        }
    }
}
