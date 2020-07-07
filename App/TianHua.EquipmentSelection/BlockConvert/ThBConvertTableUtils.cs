using System;
using Linq2Acad;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace TianHua.AutoCAD.BlockConvert
{
    public static class ThBConvertTableUtils
    {
        public static string Text(this Cell cell)
        {
            if (cell.Value == null)
            {
                return string.Empty;
            }
            using (var mText = new MText()
            {
                Contents = cell.Value.ToString(),
            })
            {
                return mText.Text;
            }
        }
    }
}
