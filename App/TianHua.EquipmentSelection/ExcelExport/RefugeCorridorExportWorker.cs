using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using TianHua.FanSelection.Model;

namespace TianHua.FanSelection.ExcelExport
{
    public class RefugeCorridorExportWorker : BaseExportWorker
    {
        public override void ExportToExcel(ThFanVolumeModel fanmodel, Worksheet setsheet, Worksheet targetsheet, FanDataModel fandatamodel, ExcelFile excelfile)
        {
            RefugeRoomAndCorridorModel refugeCorridorModel = fanmodel as RefugeRoomAndCorridorModel;
            setsheet.SetCellValue("D2", fandatamodel.GetFanNum());
            setsheet.SetCellValue("D3", fandatamodel.Name);
            setsheet.SetCellValue("D4", refugeCorridorModel.WindVolume.ToString());
            setsheet.SetCellValue("D5", refugeCorridorModel.Area_Net.ToString());
            setsheet.SetCellValue("D6", refugeCorridorModel.AirVol_Spec.ToString());
            excelfile.CopyRangeToOtherSheet(setsheet, "A1:D6", targetsheet);
        }
    }
}
