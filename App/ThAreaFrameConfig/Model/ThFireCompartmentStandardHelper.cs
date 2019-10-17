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
            return database.ImportSymbolTableRecord<TextStyleTable>(dwg, TextStyleName);
        }

        public static ObjectId CreateFCNoteTextLayer(this Database database)
        {
            string dwg = Path.Combine(ThCADCommon.StandardStylePath(), TemplateName);
            return database.ImportSymbolTableRecord<LayerTable>(dwg, TextLayerName);
        }

        public static ObjectId CreateFCFillLayer(this Database database)
        {
            string dwg = Path.Combine(ThCADCommon.StandardStylePath(), TemplateName);
            return database.ImportSymbolTableRecord<LayerTable>(dwg, FillLayerName);
        }
    }
}
