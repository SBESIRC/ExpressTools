using System.IO;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrameConfig.Model
{
    public static class ThFireCompartmentStandardHelper
    {
        public static readonly string TextLayerName = "AD-NUMB";
        public static readonly string FillLayerName = "AE-PATN-MATE";
        public static readonly string TextStyleName = "TH-STYLE1";
        public static readonly string TemplateName = "THArchitecture.dwg";

        public static ObjectId CreateFCNoteTextStyle(this Database database)
        {
            string dwg = Path.Combine(ThCADCommon.StandardStylePath(), TemplateName);
            using (var sourceDb = AcadDatabase.Open(dwg, DwgOpenMode.ReadOnly))
            { 
                using(var activeDb = AcadDatabase.Active())
                {
                    if (!activeDb.TextStyles.Contains(TextStyleName))
                    {
                        activeDb.TextStyles.Add(sourceDb.TextStyles.Element(TextStyleName));
                    }

                    return activeDb.TextStyles.Element(TextStyleName).ObjectId;
                }
            }
        }

        public static ObjectId CreateFCNoteTextLayer(this Database database)
        {
            string dwg = Path.Combine(ThCADCommon.StandardStylePath(), TemplateName);
            using (var sourceDb = AcadDatabase.Open(dwg, DwgOpenMode.ReadOnly))
            {
                using (var activeDb = AcadDatabase.Active())
                {
                    if (!activeDb.Layers.Contains(TextLayerName))
                    {
                        activeDb.Layers.Add(sourceDb.Layers.Element(TextLayerName));
                    }

                    return activeDb.Layers.Element(TextLayerName).ObjectId;
                }
            }
        }

        public static ObjectId CreateFCFillLayer(this Database database)
        {
            string dwg = Path.Combine(ThCADCommon.StandardStylePath(), TemplateName);
            using (var sourceDb = AcadDatabase.Open(dwg, DwgOpenMode.ReadOnly))
            {
                using (var activeDb = AcadDatabase.Active())
                {
                    if (!activeDb.Layers.Contains(FillLayerName))
                    {
                        activeDb.Layers.Add(sourceDb.Layers.Element(FillLayerName));
                    }

                    return activeDb.Layers.Element(FillLayerName).ObjectId;
                }
            }
        }
    }
}
