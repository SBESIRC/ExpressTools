using System;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThSitePlan
{
    public class ThSitePlanHatchOverride : IDisposable
    {
        private string HPName { get; set; }
        private string HPColor { get; set; }

        public ThSitePlanHatchOverride()
        {
            HPName = Application.GetSystemVariable("HPNAME") as string;
            HPColor = Application.GetSystemVariable("HPCOLOR") as string;
            Application.SetSystemVariable("HPNAME", ThSitePlanCommon.hatch_pattern);
            Application.SetSystemVariable("HPCOLOR", ThSitePlanCommon.hatch_color_index.ToString());
        }

        public void Dispose()
        {
            Application.SetSystemVariable("HPNAME", HPName);
            Application.SetSystemVariable("HPCOLOR", HPColor);
        }
    }
}
