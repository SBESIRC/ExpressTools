using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Linq2Acad;
using NFox.Cad.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThWSS.Engine;
using ThWSS.Utlis;

namespace ThWSS.Bussiness
{
    public class CalColumnInfoService
    {
        public List<Polyline> GetColumnStruc()
        {
            List<Polyline> column = new List<Polyline>();
            var thcolumn = ThSprayLayoutEngine.RoomEngine.Elements.Where(x => x is ThColumn).ToList();
            if (thcolumn.Count > 0)
            {
                column = thcolumn.SelectMany(x => x.Properties.Values).Cast<Polyline>().ToList();
            }
            return column;
        }
    }
}
