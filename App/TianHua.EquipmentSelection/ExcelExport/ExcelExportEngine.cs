using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianHua.FanSelection.Model;

namespace TianHua.FanSelection.ExcelExport
{
    public class ExcelExportEngine
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ExcelExportEngine instance = new ExcelExportEngine();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ExcelExportEngine() { }
        internal ExcelExportEngine() { }
        public static ExcelExportEngine Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public Workbook Sourcebook { get; set; }
        public Worksheet Targetsheet { get; set; }
        public FanDataModel Model { get; set; }
        public ExcelFile File { get; set; }

        public void Run()
        {
            var worker = BaseExportWorker.Create(Model.FanVolumeModel);
            if (worker != null)
            {
                var sourcesheet = Sourcebook.GetSheetFromSheetName(Model.FanVolumeModel.FireScenario);
                worker.ExportToExcel(Model.FanVolumeModel, sourcesheet, Targetsheet, Model, File);
            }
        }
    }
}
