using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using NFox.Cad.Collections;
using ThWSS.Model;
using ThWSS.Utlis;

namespace ThWSS.Bussiness.SparyLayout
{
    class SparyLayoutByBeamService : ISparyLayoutService
    {
        public void LayoutSpray(List<Polyline> roomsLine, SparyLayoutModel layoutModel)
        {
            foreach (var room in roomsLine)
            {
                //*******预处理房间*********
                //1.处理小的凹边
                var rommBounding = GeUtils.CreateConvexPolygon(room, 1500);

                //2.去掉线上多余的点
                rommBounding = GeUtils.ReovePointOnLine(new List<Polyline>() { rommBounding }, new Tolerance(0.001, 0.001)).First();

                List<Polyline> beamPolys = GetBeamStruc();

                RegionDivisionByBeamUtils regionDivision = new RegionDivisionByBeamUtils();
                var respolys = regionDivision.DivisionRegion(rommBounding, beamPolys);
                using (AcadDatabase acdb = AcadDatabase.Active())
                {
                    foreach (var poly in respolys)
                    {
                        acdb.ModelSpace.Add(poly);
                    }
                }
            }
        }

        private List<Polyline> GetBeamStruc()
        {
            List<Polyline> beam = new List<Polyline>();
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "POLYLINE,LWPOLYLINE");
                var entSelected = Active.Editor.GetSelection(options, filterlist);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return beam;
                }

                // 执行操作
                DBObjectCollection dBObjects = new DBObjectCollection();
                foreach (ObjectId obj in entSelected.Value.GetObjectIds())
                {
                    dBObjects.Add(acdb.Element<Entity>(obj, true));
                }
                
                foreach (var item in dBObjects)
                {
                    beam.Add(item as Polyline);
                }
            }
            return beam;
        }
    }
}
