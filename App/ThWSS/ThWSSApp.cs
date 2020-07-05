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
using TianHua.AutoCAD.Utility.ExtensionTools;
using Autodesk.Windows;
using System.Runtime.InteropServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThWSS
{
    public class MyData
    {
        public int counter = 0;
        public bool delay = true;

        // since the callback data can be reused, be sure
        // to reset it before invoking the task dialog
        public void Reset()
        {
            counter = 0;
            delay = true;
        }
    }

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

        /// <summary>
        /// 喷淋布置命令（界面）
        /// </summary>
        [CommandMethod("TIANHUACAD", "THPT", CommandFlags.Modal)]
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
        /// 喷淋布置命令（命令行）
        /// </summary>
        [CommandMethod("TIANHUACAD", "-THPT", CommandFlags.Modal)]
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

        /// <summary>
        /// 自动识别图纸中的面积框线
        /// </summary>
        [CommandMethod("TIANHUACAD", "THWRI", CommandFlags.Modal)]
        public void ThAutoAreaOutlines()
        {
            // 选择防火分区
            PromptSelectionOptions options = new PromptSelectionOptions()
            {
                SingleOnly = true,
                AllowDuplicates = false,
                MessageForAdding = "请选择防火分区",
                RejectObjectsOnLockedLayers = true,
            };
            var filterlist = OpFilter.Bulid(o =>
                o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName);
            var result = Active.Editor.GetSelection(options, filterlist);
            if (result.Status != PromptStatus.OK)
            {
                return;
            }

            foreach(ObjectId frame in result.Value.GetObjectIds())
            {
                // 先获取现有的面积框线
                var outlines = new ObjectIdCollection();
                filterlist = OpFilter.Bulid(o =>
                    o.Dxf((int)DxfCode.LayerName) == ThWSSCommon.AreaOutlineLayer &
                    o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Polyline)).DxfName);
                result = Active.Editor.SelectByPolyline(
                    frame,
                    PolygonSelectionMode.Window,
                    filterlist);
                if (result.Status == PromptStatus.OK)
                {
                    foreach (ObjectId obj in result.Value.GetObjectIds())
                    {
                        outlines.Add(obj);
                    }
                }

                // 获取房间轮廓线
                var newOutlines = new ObjectIdCollection();
                void handler(object s, ObjectEventArgs e)
                {
                    if (e.DBObject is Polyline polyline)
                    {
                        if (polyline.Layer == ThWSSCommon.AreaOutlineLayer)
                        {
                            newOutlines.Add(e.DBObject.ObjectId);
                        }
                    }
                }
                Active.Database.ObjectAppended += handler;
                using (var roomManager = new ThRoomDbManager(Active.Database))
                using (var engine = new ThRoomRecognitionEngine())
                {
                    engine.Acquire(Active.Database, null, frame);
                }
                Active.Database.ObjectAppended -= handler;

                // 设置房间轮廓线颜色
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach (ObjectId obj in newOutlines)
                    {
                        acadDatabase.Element<Polyline>(obj, true).ColorIndex = 130;
                    }
                }

                // 删除重复的轮廓线
                var frames = new DBObjectCollection();
                var newFrames = new DBObjectCollection();
                var duplicatedFrames = new DBObjectCollection();
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    foreach (ObjectId obj in outlines)
                    {
                        frames.Add(acadDatabase.Element<Polyline>(obj));
                    }
                    foreach (ObjectId obj in newOutlines)
                    {
                        newFrames.Add(acadDatabase.Element<Polyline>(obj));
                    }
                    foreach (Polyline polyline in newFrames)
                    {
                        if (frames.ContainsDuplication(polyline))
                        {
                            duplicatedFrames.Add(polyline);
                        }
                    }
                    foreach (Polyline polyline in duplicatedFrames)
                    {
                        polyline.UpgradeOpen();
                        polyline.Erase();
                    }
                }
            }
        }

        /// <summary>
        /// 用户自绘面积框线
        /// </summary>
        [CommandMethod("TIANHUACAD", "THWRD", CommandFlags.Modal)]
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

                // 添加到当前图纸
                acadDatabase.ModelSpace.Add(frame);

                // 设置颜色和图层
                frame.ColorIndex = 191;
                frame.LayerId = acadDatabase.Database.CreateAreaOutlineLayer();
            }
        }

        /// <summary>
        /// 识别已绘制的面积框线
        /// </summary>
        [CommandMethod("TIANHUACAD", "THWRR", CommandFlags.Modal)]
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
            var outlines = new ObjectIdCollection();
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
                    outlines.Add(obj);
                }
            }

            // 通过“炸”外参获取的房间框线被创建在由外参引入的临时图层上
            // 当外参“卸载”后，这个临时外参图层将失效
            // 为了避免这个情况，将置于临时外参图层的房间框线复制到指定图层中
            // https://www.keanw.com/2009/05/importing-autocad-layers-from-xrefs-using-net.html
            using (var outlineManager = new ThAreaOutlineDbManager(Active.Database))
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var frames = new DBObjectCollection();
                foreach (ObjectId obj in outlines)
                {
                    frames.Add(acadDatabase.Element<Polyline>(obj));
                }

                foreach (ObjectId obj in outlineManager.Geometries)
                {
                    var outline = acadDatabase.Database.AreaOutline(obj);
                    if (outline != null && 
                        pline.Contains(outline) && 
                        !frames.ContainsDuplication(outline))
                    {
                        acadDatabase.ModelSpace.Add(outline);
                        outline.ColorIndex = 70;
                        outline.LayerId = acadDatabase.Database.CreateAreaOutlineLayer();
                    }
                }
            }
        }

#if DEBUG

        /// <summary>
        /// 提取指定区域内的梁信息
        /// </summary>
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
                foreach (var beam in beams)
                {
                    acdb.ModelSpace.Add(beam.BeamBoundary);
                }
            }
        }

        /// <summary>
        /// 提取所选图元的梁信息
        /// </summary>
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

#endif

        [CommandMethod("ShowTaskDialog")]
        public void ShowTaskDialog()
        {

            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // create the task dialog and initialize the callback method

                TaskDialog td = CreateTaskDialog();

                td.Callback = new TaskDialogCallback(ThTaskDialogCallback);



                // create the callback data and convert it to an IntPtr

                // using GCHandle

                MyData cbData = new MyData();

                GCHandle cbDataHandle = GCHandle.Alloc(cbData);

                td.CallbackData = GCHandle.ToIntPtr(cbDataHandle);


                // Just to minimize the "Regenerating model" messages
                using (new ThAppTools.ManagedSystemVariable("NOMUTT", 1))
                {
                    td.Show(AcadApp.MainWindow.Handle);
                }




                // If the dialog was not cancelled before it finished
                // adding the lines then commit transaction
                if (memberCallbackData.counter >= 100)
                    tr.Commit();

                // be sure to clean up the gc handle before returning
                cbDataHandle.Free();
            }
        }

        [CommandMethod("ShowTaskDialogWithDataMember")]
        public void ShowTaskDialogWithDataMember()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // create the task dialog and initialize the callback method
                TaskDialog td = CreateTaskDialog();
                td.Callback = new TaskDialogCallback(ThTaskDialogCallbackUsingMemberData);



                // make sure the callback data is initialized before 
                // invoking the task dialog
                memberCallbackData.Reset();

                // Just to minimize the "Regenerating model" messages
                using (new ThAppTools.ManagedSystemVariable("NOMUTT", 1))
                {
                    td.Show(AcadApp.MainWindow.Handle);
                }



                // If the dialog was not cancelled before it finished
                // adding the lines then commit transaction
                if (memberCallbackData.counter >= 100)
                    tr.Commit();
            }
        }

        #region HELPER METHODS
        // helper method for processing the callback both in the 
        // data-member case and the callback argument case
        private bool handleCallback(ActiveTaskDialog taskDialog,
                                    TaskDialogCallbackArgs args,
                                    MyData callbackData)
        {
            // This gets called continuously until we finished completely
            if (args.Notification == TaskDialogNotification.Timer)
            {
                // To make it longer we do some delay in every second call
                if (callbackData.delay)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                else
                {
                    callbackData.counter += 10;
                    taskDialog.SetProgressBarRange(0, 100);
                    taskDialog.SetProgressBarPosition(callbackData.counter);

                    using (AcadDatabase acadDatabase = AcadDatabase.Active())
                    {
                        //// This is the main action - adding 100 lines 1 by 1
                        //Database db = HostApplicationServices.WorkingDatabase;
                        //Transaction tr = db.TransactionManager.TopTransaction;

                        //BlockTable bt = (BlockTable)tr.GetObject(

                        //  db.BlockTableId, OpenMode.ForRead);

                        //BlockTableRecord ms = (BlockTableRecord)tr.GetObject(

                        //  bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                        //Line ln = new Line(

                        //  new Point3d(0, callbackData.counter, 0),

                        //  new Point3d(10, callbackData.counter, 0));

                        //ms.AppendEntity(ln);

                        //tr.AddNewlyCreatedDBObject(ln, true);



                        //// To make it appear on the screen - might be a bit costly

                        //tr.TransactionManager.QueueForGraphicsFlush();

                        //acApp.DocumentManager.MdiActiveDocument.Editor.Regen();

                        Line ln = new Line(
                            new Point3d(0, callbackData.counter, 0),
                            new Point3d(10, callbackData.counter, 0));
                        acadDatabase.ModelSpace.Add(ln);
                    }

                    // We are finished
                    if (callbackData.counter >= 100)
                    {
                        // We only have a cancel button, 
                        // so this is what we can press
                        taskDialog.ClickButton(2);
                        //taskDialog.ClickButton((int)WinForms.DialogResult.Cancel);
                        return true;
                    }
                }

                callbackData.delay = !callbackData.delay;
            }
            else if (args.Notification == TaskDialogNotification.ButtonClicked)
            {
                // we only have a cancel button
                //if (args.ButtonId == (int)WinForms.DialogResult.Cancel)
                if (args.ButtonId == 2)
                {
                    return false;
                }
            }

            return true;
        }

        private TaskDialog CreateTaskDialog()
        {
            TaskDialog td = new TaskDialog();
            td.WindowTitle = "Adding lines";
            td.ContentText = "This operation adds 10 lines one at a " +
                            "time and might take a bit of time.";
            //td.EnableHyperlinks = true;
            //td.ExpandedText = "This operation might be lengthy.";
            //td.ExpandFooterArea = true;
            //td.AllowDialogCancellation = true;
            td.ShowProgressBar = true;
            td.CallbackTimer = true;
            //td.CommonButtons = TaskDialogCommonButtons.Cancel;
            return td;
        }

        #endregion

        #region TASK DIALOG USING CALLBACK DATA ARGUMENT
        /////////////////////////////////////////////////////////////////
        // This sample uses a local instance of the callback data. 
        // Since the TaskDialog class needs to convert the callback data
        // to an IntPtr to pass it across the managed-unmanaged divide,
        // be sure to convert it to an IntPtr before passing it off
        // to the TaskDialog instance. 
        //
        // This case requires more code than the member-based sample 
        // below, but is useful when a callback is shared 
        // between multiple task dialogs.
        /////////////////////////////////////////////////////////////////

        // task dialog callback that uses the mpCallbackData argument
        public bool ThTaskDialogCallback(ActiveTaskDialog taskDialog,
                                        TaskDialogCallbackArgs args,
                                        object mpCallbackData)
        {
            // convert the callback data from an IntPtr to the actual
            // object using GCHandle
            GCHandle callbackDataHandle =
              GCHandle.FromIntPtr((IntPtr)mpCallbackData);
            MyData callbackData = (MyData)callbackDataHandle.Target;

            // use the helper method to do the actual processing
            return handleCallback(taskDialog, args, callbackData);

        }

        #endregion

        #region TASK DIALOG USING DATA-MEMBER-BASED CALLBACK DATA
        /////////////////////////////////////////////////////////////////
        // This sample uses a data member for the callback data. 
        // This avoids having to pass the callback data as an IntPtr.
        /////////////////////////////////////////////////////////////////

        // member-based callback data - 
        // used with MemberTaskDialogCallback
        MyData memberCallbackData = new MyData();

        // task dialog callback that uses the callback data member; 
        // does not use mpCallbackData
        public bool ThTaskDialogCallbackUsingMemberData(
          ActiveTaskDialog taskDialog,
          TaskDialogCallbackArgs args,
          object mpCallbackData)
        {
            // use the helper method to do the actual processing
            return handleCallback(taskDialog, args, memberCallbackData);
        }

        #endregion

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
