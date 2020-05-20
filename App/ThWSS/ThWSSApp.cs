using System;
using System.Collections.Generic;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Linq2Acad;
using NFox.Cad.Collections;
using ThWss.View;
using ThWSS.Bussiness;
using ThWSS.Config;
using ThWSS.Config.Model;
using ThWSS.LayoutRule;
using ThWSS.Utlis;

namespace ThWSS
{
    public class ThWSSApp : IExtensionApplication
    {
        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Terminate()
        {
            //throw new NotImplementedException();
        }

        [CommandMethod("TIANHUACAD", "THCalOBB", CommandFlags.Modal)]
        public void ThDistinguishBeam()
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                //ThSparyLayoutSet thSparyLayoutSet = new ThSparyLayoutSet();
                //thSparyLayoutSet.ShowDialog();
                //return;
                // 选择对象
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "POLYLINE,LWPOLYLINE");
                var entSelected = Active.Editor.GetSelection(options, filterlist);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                };

                // 执行操作
                DBObjectCollection dBObjects = new DBObjectCollection();
                foreach (ObjectId obj in entSelected.Value.GetObjectIds())
                {
                    dBObjects.Add(acdb.Element<Entity>(obj));
                }

                List<Polyline> room = new List<Polyline>();
                foreach (var item in dBObjects)
                {
                    room.Add(item as Polyline);
                }
                SprayLayoutService sprayLayoutService = new SprayLayoutService();
                sprayLayoutService.LayoutSpray(room);
            }
        }
    }
}
