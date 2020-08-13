using System;
using System.IO;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Core;

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

            currentrows = 1;
            currentcolumns = 1;
            lastrowno = 1;
        }

        public void Close()
        {
            ExcelApp.Quit();
        }

        public Workbook OpenWorkBook(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }
            return ExcelApp.Workbooks.Open(path, System.Type.Missing, System.Type.Missing, System.Type.Missing,
              System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing,
            System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);
        }

        public void SaveWorkbook(Workbook workbook, string savepath)
        {
            workbook.SaveAs(savepath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange,
                   Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
        }

        public void CopyRangeToOtherSheet(Worksheet sourcesheet, string sourcerangestr, Worksheet targetsheet)
        {
            Range sourcerange = sourcesheet.Range[sourcerangestr];
            Range targetrange = targetsheet.Cells[currentrows,currentcolumns];
            targetrange.Insert();
            sourcerange.Copy(targetrange);

            if (currentcolumns <5)
            {
                currentcolumns += 5;
                lastrowno = Math.Max(lastrowno,currentrows+ sourcerange.Rows.Count);
            }
            else
            {
                currentcolumns = 1;
                lastrowno = Math.Max(lastrowno, currentrows + sourcerange.Rows.Count);
                currentrows = lastrowno + 1;
            }
        }
    }
}
