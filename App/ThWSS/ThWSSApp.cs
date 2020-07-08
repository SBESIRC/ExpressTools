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
using System.Linq;
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

        /// <summary>
        /// 自动识别图纸中的面积框线
        /// </summary>
        [CommandMethod("TIANHUACAD", "THAUTOAREAOUTLINES", CommandFlags.Modal)]
        public void ThAutoAreaOutlines()
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

            // 先获取现有的面积框线
            var objs = new ObjectIdCollection();
            filterlist = OpFilter.Bulid(o =>
                o.Dxf((int)DxfCode.LayerName) == ThWSSCommon.AreaOutlineLayer &
                o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName);
            var result = Active.Editor.SelectByWindow(
                pline.GeometricExtents.MinPoint,
                pline.GeometricExtents.MaxPoint,
                PolygonSelectionMode.Window,
                filterlist);
            if (result.Status == PromptStatus.OK)
            {
                foreach (ObjectId obj in result.Value.GetObjectIds())
                {
                    objs.Add(obj);
                }
            }

            // 获取房间轮廓线
            void handler(object s, ObjectEventArgs e)
            {
                if (e.DBObject is Polyline polyline)
                {
                    if (polyline.Layer == ThWSSCommon.AreaOutlineLayer)
                    {
                        objs.Add(e.DBObject.ObjectId);
                    }
                }
            }
            Active.Database.ObjectAppended += handler;
            using (var roomManager = new ThRoomDbManager(Active.Database))
            using (var engine = new ThRoomRecognitionEngine())
            {
                foreach(ObjectId frame in entSelected.Value.GetObjectIds())
                {
                    engine.Acquire(Active.Database, pline, frame);
                }
            }
            Active.Database.ObjectAppended -= handler;

            //// 剔除重合的面积框线
            //if (objs.Count > 0)
            //{
            //    Active.Editor.OverkillCmd(objs);
            //}
        }

        /// <summary>
        /// 用户自绘面积框线
        /// </summary>
        [CommandMethod("TIANHUACAD", "THCUSTOMAREAOUTLINES", CommandFlags.Modal)]
        public void ThCustomAreaOutlines()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                // 选择自定义区域
                var frame = CreatePolygonArea();
                if (frame == null || frame.NumberOfVertices == 0)
                {
                    return;
                }

                // 设置到指定图层
                frame.LayerId = acadDatabase.Database.CreateAreaOutlineLayer();

                // 添加到当前图纸
                acadDatabase.ModelSpace.Add(frame);
            }
        }

        /// <summary>
        /// 识别已绘制的面积框线
        /// </summary>
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

            // 先获取现有的面积框线
            var objs = new ObjectIdCollection();
            var filterlist = OpFilter.Bulid(o =>
                o.Dxf((int)DxfCode.LayerName) == ThWSSCommon.AreaOutlineLayer &
                o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName);
            var result = Active.Editor.SelectByWindow(
                pline.GeometricExtents.MinPoint,
                pline.GeometricExtents.MaxPoint,
                PolygonSelectionMode.Window,
                filterlist);
            if (result.Status == PromptStatus.OK)
            {
                foreach (ObjectId obj in result.Value.GetObjectIds())
                {
                    objs.Add(obj);
                }
            }

            // 通过“炸”外参获取的房间框线被创建在由外参引入的临时图层上
            // 当外参“卸载”后，这个临时外参图层将失效
            // 为了避免这个情况，将置于临时外参图层的房间框线复制到指定图层中
            // https://www.keanw.com/2009/05/importing-autocad-layers-from-xrefs-using-net.html
            using (var outlines = new ThAreaOutlineDbManager(Active.Database))
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach(ObjectId obj in outlines.Geometries)
                {
                    var outline = acadDatabase.Database.AreaOutline(obj);
                    if (pline.Contains(outline))
                    {
                        objs.Add(acadDatabase.ModelSpace.Add(outline));
                        outline.LayerId = acadDatabase.Database.CreateAreaOutlineLayer();
                    }
                }
            }

            //// 剔除重合的面积框线
            //if (objs.Count > 0)
            //{
            //    Active.Editor.OverkillCmd(objs);
            //}
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

                // 考虑到性能问题，暂时只支持选择一个防火分区
                PromptSelectionOptions options = new PromptSelectionOptions()
                {
                    SingleOnly = true,
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


                // 获取防火分区内的房间框线
                var objs = new ObjectIdCollection();
                filterlist = OpFilter.Bulid(o =>
                    o.Dxf((int)DxfCode.LayerName) == ThWSSCommon.AreaOutlineLayer &
                    o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName);
                entSelected = Active.Editor.SelectByPolyline(entSelected.Value.GetObjectIds()[0],
                    PolygonSelectionMode.Crossing, filterlist);
                if (entSelected.Status != PromptStatus.OK)
                {
                    return;
                }

                // 执行操作
                var frames = new ObjectIdCollection(entSelected.Value.GetObjectIds());
                ThSprayLayoutEngine.Instance.Layout(Active.Database, pline, frames, layoutModel);
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
                    layoutModel.sparySSpcing = 300;
                    layoutModel.sparyESpcing = 4400;
                    layoutModel.otherSSpcing = 300;
                    layoutModel.otherESpcing = 2200;
                    radius = 20000000;
                }
                else if (thSpary.standControl.danLevel.SelectedIndex == 1)
                {
                    layoutModel.sparySSpcing = 300;
                    layoutModel.sparyESpcing = 3600;
                    layoutModel.otherSSpcing = 300;
                    layoutModel.otherESpcing = 1800;
                    radius = 12500000;
                }
                else if (thSpary.standControl.danLevel.SelectedIndex == 2)
                {
                    layoutModel.sparySSpcing = 300;
                    layoutModel.sparyESpcing = 3400;
                    layoutModel.otherSSpcing = 300;
                    layoutModel.otherESpcing = 1700;
                    radius = 11500000;
                }
                else if (thSpary.standControl.danLevel.SelectedIndex == 3)
                {
                    layoutModel.sparySSpcing = 300;
                    layoutModel.sparyESpcing = 3000;
                    layoutModel.otherSSpcing = 300;
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
