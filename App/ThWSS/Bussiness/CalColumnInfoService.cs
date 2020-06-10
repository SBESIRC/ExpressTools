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
using ThWSS.Utlis;

namespace ThWSS.Bussiness
{
    public class CalColumnInfoService
    {
        public List<Polyline> GetColumnStruc()
        {
            List<Polyline> column = new List<Polyline>();
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "ARC,LINE,LWPOLYLINE" & o.Dxf((int)DxfCode.LayerName) == "S_COLU");
                var entSelected = Active.Editor.SelectAll(filterlist);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return column;
                }

                // 执行操作
                DBObjectCollection dBObjects = new DBObjectCollection();
                foreach (ObjectId obj in entSelected.Value.GetObjectIds())
                {
                    dBObjects.Add(acdb.Element<Entity>(obj));
                }

                foreach (var item in dBObjects)
                {
                    column.Add(OrientedBoundingBox.Calculate(item as Polyline));
                }
            }
            return column;
        }
    }
}
