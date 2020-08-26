using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using TianHua.FanSelection.Model;

namespace TianHua.FanSelection.ExcelExport
{
    public class FontroomNaturalExportWorker : BaseExportWorker
    {
        public override void ExportToExcel(ThFanVolumeModel fanmodel, Worksheet setsheet, Worksheet targetsheet, FanDataModel fandatamodel, ExcelFile excelfile)
        {
            FontroomNaturalModel fontroomNaturalModel = fanmodel as FontroomNaturalModel;
            setsheet.SetCellValue("D2", fandatamodel.GetFanNum());
            setsheet.SetCellValue("D3", fandatamodel.Name);
            setsheet.SetCellValue("D4", Math.Max(fontroomNaturalModel.QueryValue, (fontroomNaturalModel.DoorOpeningVolume + fontroomNaturalModel.LeakVolume)).ToString());
            setsheet.SetCellValue("D5", (fontroomNaturalModel.DoorOpeningVolume + fontroomNaturalModel.LeakVolume).ToString());
            setsheet.SetCellValue("D6", fontroomNaturalModel.DoorOpeningVolume.ToString());
            setsheet.SetCellValue("D8", fontroomNaturalModel.LeakVolume.ToString());
            setsheet.SetCellValue("D9", fontroomNaturalModel.OverAk.ToString());

            double ak = 0.0, ai = 0.0, ag = 0.0;
            fontroomNaturalModel.FrontRoomDoors.ForEach(o => ak += o.Width_Door_Q * o.Height_Door_Q * o.Count_Door_Q);
            fontroomNaturalModel.StairCaseDoors.ForEach(o => ai += o.Width_Door_Q * o.Height_Door_Q * o.Count_Door_Q);
            fontroomNaturalModel.StairCaseDoors.ForEach(o => ag += o.Width_Door_Q * o.Height_Door_Q * o.Count_Door_Q);
            double v = 0.6 * ai / (ag + 1);
            setsheet.SetCellValue("D10", v.ToString());
            setsheet.SetCellValue("D11", Math.Min(fontroomNaturalModel.Count_Floor, 3).ToString());
            setsheet.SetCellValue("D12", (fontroomNaturalModel.Length_Valve * fontroomNaturalModel.Width_Valve).ToString());
            setsheet.SetCellValue("D13", (fontroomNaturalModel.Count_Floor < 3 ? 0 : fontroomNaturalModel.Count_Floor-3).ToString());
            setsheet.SetCellValue("D14", ai.ToString());
            setsheet.SetCellValue("D15", ag.ToString());
            setsheet.SetCellValue("D16", fontroomNaturalModel.QueryValue.ToString());
            setsheet.SetCellValue("D17", fontroomNaturalModel.Count_Floor.ToString());
            setsheet.SetCellValue("D18", fontroomNaturalModel.Load.ToString());
            setsheet.SetCellValue("D19", fontroomNaturalModel.Length_Valve.ToString());
            setsheet.SetCellValue("D20", fontroomNaturalModel.Width_Valve.ToString());
            int rowNo = 21;
            foreach (var frontRoomDoor in fontroomNaturalModel.FrontRoomDoors)
            {
                setsheet.SetCellValue("D" + rowNo, frontRoomDoor.Height_Door_Q.ToString());
                setsheet.SetCellValue("D" + (rowNo + 1), frontRoomDoor.Width_Door_Q.ToString());
                setsheet.SetCellValue("D" + (rowNo + 2), frontRoomDoor.Count_Door_Q.ToString());
                rowNo += 3;
            }
            rowNo = 30;
            foreach (var stairCaseDoor in fontroomNaturalModel.StairCaseDoors)
            {
                setsheet.SetCellValue("D" + rowNo, stairCaseDoor.Height_Door_Q.ToString());
                setsheet.SetCellValue("D" + (rowNo + 1), stairCaseDoor.Width_Door_Q.ToString());
                setsheet.SetCellValue("D" + (rowNo + 2), stairCaseDoor.Count_Door_Q.ToString());
                rowNo += 3;
            }
            excelfile.CopyRangeToOtherSheet(setsheet, "A1:D38", targetsheet);
        }
    }
}
