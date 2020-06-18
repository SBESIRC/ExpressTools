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
using ThWSS.Engine;
using ThWSS.Model;
using ThWSS.Beam;
using ThStructure.BeamInfo.Command;
using ThStructure.BeamInfo.Business;
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

        [CommandMethod("TIANHUACAD", "-THCalOBB", CommandFlags.Modal)]
        public void ThDistinguishBeamCLI()
        {
            SprayLayoutModel layoutModel = new SprayLayoutModel()
            {
                sparyLayoutWay = LayoutWay.frame,
            };
            Run(layoutModel);
        }

        [CommandMethod("TIANHUACAD", "THGETBEAMINFO", CommandFlags.Modal)]
        public void THGETBEAMINFO()
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            using (ThBeamDbManager beamManager = new ThBeamDbManager(acdb.Database))
            {
                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                // 获取所有构成梁的曲线（线，多段线，圆弧）
                var beamCurves = ThBeamGeometryService.Instance.BeamCurves(beamManager);
                // 考虑到多段线的情况，需要将多段线“炸”成线来处理
                thDisBeamCommand.CalBeamStruc(ThBeamGeometryPreprocessor.ExplodeCurves(beamCurves));
            }
        }


        [CommandMethod("TIANHUACAD", "THMERGEBEAMCURVES", CommandFlags.Modal)]
        public void ThMergeBeamCurves()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[]
                {
                    RXClass.GetClass(typeof(Arc)).DxfName,
                    RXClass.GetClass(typeof(Line)).DxfName,
                    RXClass.GetClass(typeof(Polyline)).DxfName,
                }));
                var result = Active.Editor.GetSelection(options, filterlist);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new DBObjectCollection();
                foreach (var objId in result.Value.GetObjectIds())
                {
                    var obj = acadDatabase.Element<Entity>(objId, true);
                    objs.Add(obj.GetTransformedCopy(Matrix3d.Identity));
                }
                var results = ThBeamGeometryPreprocessor.MergeCurves(objs);
                if (results.Count == 0)
                {
                    return;
                }

                // 将合并后的图元添加到图纸中
                // 未参与合并的图元会被重新添加到图纸中
                foreach (Entity obj in results)
                {
                    acadDatabase.ModelSpace.Add(obj);
                }
                // 将原图元从图纸中删除
                foreach (var objId in result.Value.GetObjectIds())
                {
                    acadDatabase.Element<Entity>(objId, true).Erase();
                }
            }
        }

        [CommandMethod("TIANHUACAD", "THGETBEAMINFO2", CommandFlags.Modal)]
        public void THGETBEAMINFO2()
        {
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                // 选择对象
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == "ARC,LINE,LWPOLYLINE" & o.Dxf((int)DxfCode.LayerName) == "S_BEAM");
                var entSelected = Active.Editor.GetSelection(options, filterlist);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                };

                // 执行操作
                DBObjectCollection dBObjects = new DBObjectCollection();
                foreach (ObjectId obj in entSelected.Value.GetObjectIds())
                {
                    var entity = acdb.Element<Entity>(obj);
                    dBObjects.Add(entity.GetTransformedCopy(Matrix3d.Identity));
                }

                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                thDisBeamCommand.CalBeamStruc(dBObjects);
            }
        }

        /// <summary>
        /// 执行布置喷淋
        /// </summary>
        /// <returns></returns>
        private void Run(SprayLayoutModel layoutModel)
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

        private SprayLayoutModel SetWindowValues(ThSparyLayoutSet thSpary)
        {
            SprayLayoutModel layoutModel = new SprayLayoutModel();
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
