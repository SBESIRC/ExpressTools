using System;
using AcHelper;
using Linq2Acad;
using DotNetARX;
using ThWss.View;
using ThWSS.Beam;
using ThWSS.Model;
using ThWSS.Utlis;
using ThWSS.Engine;
using ThCADCore.NTS;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThStructure.BeamInfo.Command;
using ThStructure.BeamInfo.Business;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThWSS
{
    public class ThWSSApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
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
            PromptKeywordOptions keywordOptions = new PromptKeywordOptions("\n请指定布置区域：")
            {
                AllowNone = true
            };
            keywordOptions.Keywords.Add("Firecompartment", "Firecompartment", "防火分区(F)");
            keywordOptions.Keywords.Add("frameLine", "frameLine", "来自框线(L)");
            keywordOptions.Keywords.Add("Custom", "Custom", "自定义区域(C)");
            keywordOptions.Keywords.Default = "Firecompartment";
            PromptResult result = Active.Editor.GetKeywords(keywordOptions);
            if (result.Status != PromptStatus.OK)
            {
                return;
            }

            var layoutModel = SprayLayoutModel.Create();
            if (result.StringResult == "Firecompartment")
            {
                layoutModel.sparyLayoutWay = LayoutWay.fire;
            }
            else if (result.StringResult == "frameLine")
            {
                layoutModel.sparyLayoutWay = LayoutWay.frame;
            }
            else if (result.StringResult == "Custom")
            {
                layoutModel.sparyLayoutWay = LayoutWay.customPart;
            }
            Run(layoutModel);
        }

        [CommandMethod("TIANHUACAD", "THGETBEAMINFO", CommandFlags.Modal)]
        public void THGETBEAMINFO()
        {
            // 选择楼层区域
            // 暂时只支持矩形区域
            var pline = CreateWindowArea();
            if (pline == null)
            {
                return;
            }

            using (ThBeamDbManager beamManager = new ThBeamDbManager(Active.Database))
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                ThDisBeamCommand thDisBeamCommand = new ThDisBeamCommand();
                var beamCurves = ThBeamGeometryService.Instance.BeamCurves(acdb.Database, pline);
                var beams = thDisBeamCommand.CalBeamStruc(beamCurves);
                foreach(var beam in beams)
                {
                    acdb.ModelSpace.Add(beam.BeamBoundary);
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

                // 梁线的图元类型
                // 暂时不支持弧梁
                var dxfNames = new string[]
                {
                    RXClass.GetClass(typeof(Line)).DxfName,
                    RXClass.GetClass(typeof(Polyline)).DxfName,
                };
                // 梁线的图元图层
                var layers = ThBeamLayerManager.GeometryLayers(acdb.Database);
                var filterlist = OpFilter.Bulid(o => 
                    o.Dxf((int)DxfCode.Start) == string.Join(",", dxfNames) & 
                    o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray()));
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

        [CommandMethod("TIANHUACAD", "THGETROOMINFO", CommandFlags.Modal)]
        public void ThGetRoomInfo()
        {
            // 选择楼层区域
            // 暂时只支持矩形区域
            var pline = CreateWindowArea();
            if (pline == null)
            {
                return;
            }

            // 选择防火分区
            PromptSelectionOptions options = new PromptSelectionOptions()
            {
                AllowDuplicates = false,
                RejectObjectsOnLockedLayers = true,
            };
            var filterlist = OpFilter.Bulid(o =>
                o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName);
            var entSelected = Active.Editor.GetSelection(options, filterlist);
            if (entSelected.Status != PromptStatus.OK)
            {
                return;
            }

            // 获取房间轮廓线
            using (var roomManager = new ThRoomDbManager(Active.Database))
            using (var engine = new ThRoomRecognitionEngine())
            {
                foreach(ObjectId frame in entSelected.Value.GetObjectIds())
                {
                    engine.Acquire(Active.Database, pline, frame);
                }
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

        [CommandMethod("TIANHUACAD", "THAREAOUTLINES", CommandFlags.Modal)]
        public void ThAreaOutlines()
        {
            // 选择楼层区域
            // 暂时只支持矩形区域
            var pline = CreateWindowArea();
            if (pline == null)
            {
                return;
            }

            // 通过“炸”外参获取的房间框线被创建在由外参引入的临时图层上
            // 当外参“卸载”后，这个临时外参图层将失效
            // 为了避免这个情况，将置于临时外参图层的房间框线复制到指定图层中
            // https://www.keanw.com/2009/05/importing-autocad-layers-from-xrefs-using-net.html
            using (var outlines = new ThRoomLineDbManager(Active.Database))
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach(ObjectId obj in outlines.Geometries)
                {
                    var outline = acadDatabase.Database.AreaOutline(obj);
                    if (pline.Contains(outline))
                    {
                        acadDatabase.ModelSpace.Add(outline);
                        outline.LayerId = acadDatabase.Database.CreateAreaOutlineLayer();
                    }
                }
            }
        }

        /// <summary>
        /// 执行布置喷淋
        /// </summary>
        /// <returns></returns>
        private void Run(SprayLayoutModel layoutModel)
        {
            //清空数据
            ThSprayLayoutEngine.Instance.RoomEngine = new ThRoomRecognitionEngine();

            if (layoutModel.sparyLayoutWay == LayoutWay.fire)
            {
                // 选择楼层区域
                // 暂时只支持矩形区域
                var pline = CreateWindowArea();
                if (pline == null)
                {
                    return;
                }

                // 选择防火分区
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o =>
                    o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName);
                var entSelected = Active.Editor.GetSelection(options, filterlist);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                }

                // 执行操作
                foreach (var obj in entSelected.Value.GetObjectIds())
                {
                    ThSprayLayoutEngine.Instance.Layout(Active.Database, pline, obj, layoutModel);
                }
            }
            else if (layoutModel.sparyLayoutWay == LayoutWay.frame)
            {
                // 选择楼层区域
                // 暂时只支持矩形区域
                var pline = CreateWindowArea();
                if (pline == null)
                {
                    return;
                }

                // 选取框线
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    AllowDuplicates = false,
                    RejectObjectsOnLockedLayers = true,
                };
                var filterlist = OpFilter.Bulid(o =>
                    o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName);
                var entSelected = Active.Editor.GetSelection(options, filterlist);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                }

                // 执行操作
                var frames = new ObjectIdCollection(entSelected.Value.GetObjectIds());
                ThSprayLayoutEngine.Instance.Layout(Active.Database, pline, frames, layoutModel);
            }
            else if (layoutModel.sparyLayoutWay == LayoutWay.customPart)
            {
                // 选择楼层区域
                // 暂时只支持矩形区域
                var pline = CreateWindowArea();
                if (pline == null)
                {
                    return;
                }

                // 选择自定义区域
                var frame = CreatePolygonArea();
                if (frame == null || frame.NumberOfVertices == 0)
                {
                    return;
                }

                // 执行操作
                var frames = new DBObjectCollection()
                {
                    frame
                };
                ThSprayLayoutEngine.Instance.Layout(Active.Database, pline, frames, layoutModel);
            }
        }

        private Polyline CreateWindowArea()
        {
            Polyline pline = new Polyline()
            {
                Closed = true
            };
            using (PointCollector pc = new PointCollector(PointCollector.Shape.Window))
            {
                try
                {
                    pc.Collect();
                    pline.CreateRectangle(
                        pc.CollectedPoints[0].ToPoint2D(),
                        pc.CollectedPoints[1].ToPoint2D());
                    return pline;
                }
                catch
                {
                    return null;
                }
            }
        }

        private Polyline CreatePolygonArea()
        {
            Polyline pline = new Polyline()
            {
                Closed = true
            };
            using (PointCollector pc = new PointCollector(PointCollector.Shape.Polygon))
            {
                try
                {
                    pc.Collect();
                    pline.CreatePolyline(pc.CollectedPoints);
                }
                catch
                {
                    return null;
                }
            }
            return pline;
        }

        private SprayLayoutModel SetWindowValues(ThSparyLayoutSet thSpary)
        {
            SprayLayoutModel layoutModel = SprayLayoutModel.Create();
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

            if (!string.IsNullOrEmpty(thSpary.radius.Text))
            {
                layoutModel.protectRadius = Convert.ToDouble(thSpary.radius.Text);
            }

            if (thSpary.custom.IsChecked == true)
            {
                layoutModel.sparySSpcing = Convert.ToDouble(thSpary.customControl.sparySSpcing.Text);
                layoutModel.sparyESpcing = Convert.ToDouble(thSpary.customControl.sparyESpcing.Text);
                layoutModel.otherSSpcing = Convert.ToDouble(thSpary.customControl.otherSSpcing.Text);
                layoutModel.otherESpcing = Convert.ToDouble(thSpary.customControl.otherESpcing.Text);

                if (string.IsNullOrEmpty(thSpary.radius.Text))
                {
                    layoutModel.protectRadius = Math.Sqrt((layoutModel.otherESpcing * layoutModel.otherESpcing * 4) / Math.PI);
                }
            }
            else if (thSpary.standard.IsChecked == true)
            {
                double radius = 0;
                if (thSpary.standControl.danLevel.SelectedIndex == 0)
                {
                    layoutModel.sparySSpcing = 100;
                    layoutModel.sparyESpcing = 4400;
                    layoutModel.otherSSpcing = 100;
                    layoutModel.otherESpcing = 2200;
                    radius = 20000000;
                }
                else if (thSpary.standControl.danLevel.SelectedIndex == 1)
                {
                    layoutModel.sparySSpcing = 100;
                    layoutModel.sparyESpcing = 3600;
                    layoutModel.otherSSpcing = 100;
                    layoutModel.otherESpcing = 1800;
                    radius = 12500000;
                }
                else if (thSpary.standControl.danLevel.SelectedIndex == 2)
                {
                    layoutModel.sparySSpcing = 100;
                    layoutModel.sparyESpcing = 3400;
                    layoutModel.otherSSpcing = 100;
                    layoutModel.otherESpcing = 1700;
                    radius = 11500000;
                }
                else if (thSpary.standControl.danLevel.SelectedIndex == 3)
                {
                    layoutModel.sparySSpcing = 100;
                    layoutModel.sparyESpcing = 3000;
                    layoutModel.otherSSpcing = 100;
                    layoutModel.otherESpcing = 1500;
                    radius = 9000000;
                }

                if (string.IsNullOrEmpty(thSpary.radius.Text))
                {
                    layoutModel.protectRadius = Math.Sqrt(radius / Math.PI);
                }
            }

            if (thSpary.upSpary.IsChecked == true)
            {
                layoutModel.sprayType = 0;
            }
            else if (thSpary.downSpary.IsChecked == true)
            {
                layoutModel.sprayType = 1;
            }
            
            layoutModel.UseBeam = thSpary.HasBeam.IsChecked == true;
            if (layoutModel.UseBeam)
            {
                layoutModel.beamHeight = Convert.ToDouble(thSpary.beamHeight.Text);
                layoutModel.floorThcik = Convert.ToDouble(thSpary.plateThick.Text);
            }

            return layoutModel;
        }
    }
}
