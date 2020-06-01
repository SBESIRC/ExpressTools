using System;
using System.Collections.Generic;
using System.Linq;
using AcHelper;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Linq2Acad;
using NFox.Cad.Collections;
using ThWss.View;
using ThWSS.Bussiness;
using ThWSS.Config;
using ThWSS.Config.Model;
using ThWSS.Engine;
using ThWSS.LayoutRule;
using ThWSS.Model;
using ThWSS.Utlis;
using TianHua.AutoCAD.Utility.ExtensionTools;

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
            ThSparyLayoutSet instance = new ThSparyLayoutSet();
            var result = instance.ShowDialog();
            if (result == false)
            {
                return;
            }

            var layoutModel = SetWindowValues(instance);
            Run(layoutModel);
        }

        /// <summary>
        /// 执行布置喷淋
        /// </summary>
        /// <returns></returns>
        private void Run(SparyLayoutModel layoutModel)
        {
            try
            {
                using (AcadDatabase acdb = AcadDatabase.Active())
                {
                    //防火分区
                    if (layoutModel.sparyLayoutWay == LayoutWay.fire)
                    {
                        ThSprayLayoutEngine.Instance.Layout(acdb.Database, null, layoutModel);
                    }
                    //来自框线
                    else if (layoutModel.sparyLayoutWay == LayoutWay.frame)
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
                            return;
                        }

                        // 执行操作
                        DBObjectCollection dBObjects = new DBObjectCollection();
                        foreach (ObjectId obj in entSelected.Value.GetObjectIds())
                        {
                            dBObjects.Add(acdb.Element<Entity>(obj, true));
                        }

                        List<Polyline> room = new List<Polyline>();
                        foreach (var item in dBObjects)
                        {
                            room.Add(item as Polyline);
                        }
                        var closedRoom = room.Select(x => { x.Closed = true; return x; }).ToList();
                        ThSprayLayoutEngine.Instance.Layout(closedRoom, layoutModel);
                    }
                    //自定义区域
                    else if (layoutModel.sparyLayoutWay == LayoutWay.customPart)
                    {
                        Polyline pline = new Polyline() { Closed = true };
                        using (PointCollector pc = new PointCollector(PointCollector.Shape.Polygon))
                        {
                            try
                            {
                                pc.Collect();
                            }
                            catch (Autodesk.AutoCAD.BoundaryRepresentation.Exception ex)
                            {
                                throw ex;
                            }
                            Point3dCollection winCorners = pc.CollectedPoints;
                            for (int i = 0; i < winCorners.Count; i++)
                            {
                                pline.AddVertexAt(i, new Point2d(winCorners[i].X, winCorners[i].Y), 0, 0, 0);
                            }
                        }

                        acdb.ModelSpace.Add(pline);
                        ThSprayLayoutEngine.Instance.Layout(new List<Polyline>() { pline }, layoutModel);
                    }
                }
            }
            catch (Autodesk.AutoCAD.BoundaryRepresentation.Exception ex)
            {
                //return false;
            }

        }

        private SparyLayoutModel SetWindowValues(ThSparyLayoutSet thSpary)
        {
            SparyLayoutModel layoutModel = new SparyLayoutModel();
            if (thSpary.fire.IsChecked == true)
            {
                layoutModel.sparyLayoutWay = LayoutWay.fire;
            }
            else if (thSpary.frame.IsChecked == true)
            {
                layoutModel.sparyLayoutWay = LayoutWay.frame;
            }
            else if (thSpary.customPart.IsChecked == true)
            {
                layoutModel.sparyLayoutWay = LayoutWay.customPart;
            }

            layoutModel.sparySSpcing = Convert.ToDouble(thSpary.customControl.sparySSpcing.Text);
            layoutModel.sparyESpcing = Convert.ToDouble(thSpary.customControl.sparyESpcing.Text);
            layoutModel.otherSSpcing = Convert.ToDouble(thSpary.customControl.otherSSpcing.Text);
            layoutModel.otherESpcing = Convert.ToDouble(thSpary.customControl.otherESpcing.Text);

            return layoutModel;
        }
    }
}
