using System.Text;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using AcHelper;
using Linq2Acad;
using NFox.Cad.Collections;
using DotNetARX;

namespace ThSitePlan.Engine
{
    //public class ThSitePlanMoveWorker : ThSitePlanWorker
    //{
    //    public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
    //    {
    //        using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
    //        {
    //            if (configItem.Properties["CADScriptID"].ToString() == "0")
    //            {
    //                using (var objs = FilterWithBlackList(database, configItem, options))
    //                {
    //                    Vector3d offset = (Vector3d)options.Options["Offset"];
    //                    acadDatabase.Database.Move(objs, Matrix3d.Displacement(offset));
    //                }
    //                return true;
    //            }
    //            using (var objs = Filter(database, configItem, options))
    //            {
    //                Vector3d offset = (Vector3d)options.Options["Offset"];
    //                acadDatabase.Database.Move(objs, Matrix3d.Displacement(offset));
    //            }
    //            return true;
    //        }
    //    }

    //    public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
    //    {
    //        ObjectId originFrame = (ObjectId)options.Options["OriginFrame"];
    //        var layers = configItem.Properties["CADLayer"] as List<string>;
    //        var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray()));
    //        PromptSelectionResult psr = Active.Editor.SelectByPolyline(
    //            originFrame,
    //            PolygonSelectionMode.Crossing,
    //            filterlist);
    //        if (psr.Status == PromptStatus.OK)
    //        {
    //            return new ObjectIdCollection(psr.Value.GetObjectIds());
    //        }
    //        else
    //        {
    //            return new ObjectIdCollection();
    //        }
    //    }

    //    public ObjectIdCollection FilterWithBlackList(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
    //    {
    //        ObjectId originFrame = (ObjectId)options.Options["OriginFrame"];
    //        var layers = configItem.Properties["CADLayer"] as List<string>;
    //        var dxfNames = new string[]
    //        {
    //            RXClass.GetClass(typeof(Hatch)).DxfName,
    //        };
    //        var filterlist = OpFilter.Bulid(o => 
    //            o.Dxf((int)DxfCode.Start) != string.Join(",", dxfNames) & 
    //            o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray()));
    //        PromptSelectionResult psr = Active.Editor.SelectByPolyline(
    //            originFrame,
    //            PolygonSelectionMode.Crossing,
    //            filterlist);
    //        if (psr.Status == PromptStatus.OK)
    //        {
    //            return new ObjectIdCollection(psr.Value.GetObjectIds());
    //        }
    //        else
    //        {
    //            return new ObjectIdCollection();
    //        }
    //    }
    //}

    public class ThSitePlanCopyWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (var objs = Filter(database, configItem, options))
                {
                    Vector3d offset = (Vector3d)options.Options["Offset"];
                    acadDatabase.Database.CopyWithMove(objs, Matrix3d.Displacement(offset));
                }
                return true;
            }
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId originFrame = (ObjectId)options.Options["OriginFrame"];
            var layers = configItem.Properties["CADLayer"] as List<string>;
            var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray()));
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                originFrame,
                PolygonSelectionMode.Crossing,
                filterlist);
            if (psr.Status == PromptStatus.OK)
            {
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            }
            else
            {
                return new ObjectIdCollection();
            }
        }
    }

    public class ThSitePlanCopyWorkerEx : ThSitePlanWorker
    {
        private string[] DxfNames { get; set; }
        public ThSitePlanCopyWorkerEx(string[] dxfNames)
        {
            DxfNames = dxfNames;
        }

        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (var objs = Filter(database, configItem, options))
                {
                    Vector3d offset = (Vector3d)options.Options["Offset"];
                    acadDatabase.Database.CopyWithMove(objs, Matrix3d.Displacement(offset));
                }
                return true;
            }
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", DxfNames));

            // 首先判断图框内是否已经有指定的内容
            ObjectId frame = (ObjectId)options.Options["Frame"];
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                PolygonSelectionMode.Crossing,
                filterlist);
            if (psr.Status == PromptStatus.OK)
            {            
                // 若已经存在，则不需要复制
                return new ObjectIdCollection();
            }

            // 若不存在，则需要从指定图框复制指定的内容
            ObjectId originFrame = (ObjectId)options.Options["OriginFrame"];
            psr = Active.Editor.SelectByPolyline(
                originFrame,
                PolygonSelectionMode.Crossing,
                filterlist);
            if (psr.Status == PromptStatus.OK)
            {
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            }
            else
            {
                return new ObjectIdCollection();
            }
        }
    }

    public class ThSitePlanAddNameTextWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                ObjectId frame = (ObjectId)options.Options["Frame"];
                string frameName = (string)configItem.Properties["Name"];
                var frameObj = acadDatabase.Element<Polyline>(frame);
                var frameExtents = frameObj.GeometricExtents;
                var position = new Point3d(
                    frameExtents.MinPoint.X + ThSitePlanCommon.frame_annotation_offset_X,
                    frameExtents.MaxPoint.Y + ThSitePlanCommon.frame_annotation_offset_Y,
                    0);
                DBText tx = new DBText()
                {
                    Position = position,
                    TextString = frameName,
                    Layer = ThSitePlanCommon.LAYER_FRAME,
                    Height = 1.0 / 18.0 * frameExtents.Height(),
                };
                var TextobjId = acadDatabase.ModelSpace.Add(tx, true);

                // 更新框的标签
                frame.RemoveXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name);
                TypedValueList valueList = new TypedValueList
                {
                    { (int)DxfCode.ExtendedDataBinaryChunk,  Encoding.UTF8.GetBytes(frameName) },
                };
                frame.AddXData(ThSitePlanCommon.RegAppName_ThSitePlan_Frame_Name, valueList);

                return true;
            }
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            return new ObjectIdCollection();
        }
    }

    public class ThSitePlanTrimWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var ent = acadDatabase.Element<Entity>((ObjectId)options.Options["Frame"]);
                Active.Editor.TrimCmd(ent);
            }
            return true;
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            return new ObjectIdCollection();
        }
    }

}