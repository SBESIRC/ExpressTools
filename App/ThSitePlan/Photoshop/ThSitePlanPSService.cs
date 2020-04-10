using System;
using PsApplication = Photoshop.Application;
using Photoshop;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSService : IDisposable
    {
        public PsApplication Application { get; set; }

        public ThSitePlanPSService()
        {
            Application = new PsApplication();
        }

        public void Dispose()
        {
            //
        }

        // 创建一个空白图纸
        public void NewEmptyDocument(string DocName)
        {
            var StartUnits = Application.Preferences.RulerUnits;
            Application.Preferences.RulerUnits = PsUnits.psCM;
            Application.Documents.Add(
                ThSitePlanCommon.PsDocOpenPropertity["DocWidth"],
                ThSitePlanCommon.PsDocOpenPropertity["DocHight"],
                ThSitePlanCommon.PsDocOpenPropertity["PPI"],
                DocName);
            Application.ActiveDocument.ArtLayers[1].IsBackgroundLayer = true;
            Application.Preferences.RulerUnits = StartUnits;
        }

        public void ExportToFile(string path)
        {
            Application.ActiveDocument.SaveAs(path);
        }
    }
}