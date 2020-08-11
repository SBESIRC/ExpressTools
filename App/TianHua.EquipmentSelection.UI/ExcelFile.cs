using System;
using Microsoft.Office.Interop.Excel;

namespace TianHua.FanSelection.UI
{
    public class ExcelFile
    {
        public Application ExcelApp { get; set; }

        public int currentrows { get; set; }
        public int currentcolumns { get; set; }
        public int lastrowno { get; set; }

        public ExcelFile()
        {
            ExcelApp = new Application
            {
                DisplayAlerts = false,
                Visible = false,
                ScreenUpdating = false
            };

            currentrows = 0;
            currentcolumns = 0;
            lastrowno = 0;
        }

        public void Close()
        {
            ExcelApp.Quit();
        }

        public Workbook OpenWorkBook(string path)
        {
            return ExcelApp.Workbooks.Open(path, System.Type.Missing, System.Type.Missing, System.Type.Missing,
              System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing,
            System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);
        }

        public void SaveWorkbook(Workbook workbook, string savepath)
        {
            workbook.SaveAs(savepath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange,
                   Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
        }

        public void CopyRangeToOtherSheet(Worksheet sourcesheet, string sourcerangestr, Worksheet targetsheet, string targetrangestr)
        {
            Range sourcerange = sourcesheet.Range[sourcerangestr];
            Range targetrange = targetsheet.Cells[currentrows+1,currentcolumns+1];
            targetrange.Insert(XlInsertShiftDirection.xlShiftToRight);
            sourcerange.Copy(targetrange);

            if (currentcolumns <=5)
            {
                currentcolumns += 6;
                lastrowno = Math.Max(lastrowno,currentrows+ sourcerange.Rows.Count);
            }
            else
            {
                currentcolumns = 0;
                lastrowno = Math.Max(lastrowno, currentrows + sourcerange.Rows.Count);
                currentrows = lastrowno + 1;
            }
        }
    }
}
