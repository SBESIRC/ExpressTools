using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using TianHua.FanSelection.Model;

namespace TianHua.FanSelection.ExcelExport
{
    public class RefugeFontRoomExportWorker : BaseExportWorker
    {
        public override void ExportToExcel(ThFanVolumeModel fanmodel, Worksheet setsheet, Worksheet targetsheet, FanDataModel fandatamodel, ExcelFile excelfile)
        {
            RefugeFontRoomModel refugeFontRoomModel = fanmodel as RefugeFontRoomModel;
            setsheet.SetCellValue("D2", fandatamodel.FanNum);
            setsheet.SetCellValue("D3", fandatamodel.Name);
            setsheet.SetCellValue("D4", refugeFontRoomModel.DoorOpeningVolume.ToString());
            setsheet.SetCellValue("D5", GetAkValue(refugeFontRoomModel).ToString());
            setsheet.SetCellValue("D6", "1");
            int rowNo = 7;
            foreach (var frontRoomDoor in refugeFontRoomModel.FrontRoomDoors)
            {
                setsheet.SetCellValue("D" + rowNo, frontRoomDoor.Height_Door_Q.ToString());
                setsheet.SetCellValue("D" + (rowNo + 1), frontRoomDoor.Width_Door_Q.ToString());
                setsheet.SetCellValue("D" + (rowNo + 2), frontRoomDoor.Count_Door_Q.ToString());
                rowNo += 3;
            }
            excelfile.CopyRangeToOtherSheet(setsheet, "A1:D15", targetsheet);
        }

        public double GetAkValue(RefugeFontRoomModel model)
        {
            double ak = 0;
            model.FrontRoomDoors.ForEach(d=> ak += d.Width_Door_Q * d.Height_Door_Q * d.Count_Door_Q);
            return ak;
        }
    }
}
