using System;
using AcHelper;
using Linq2Acad;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    public class ThSitePlanBuilding : IDisposable
    {
        public string Name { get; set; }
        public ObjectId Region { get; set; }
        public Database Database { get; set; }
        public ThSitePlanBuilding(Database database, ObjectId region, string name)
        {
            Name = name;
            Region = region;
            Database = database;
            ThSitePlanDbEngine.Instance.Initialize(Database);
        }

        public void Dispose()
        {
            //
        }

        private ObjectId Frame()
        {
            return ThSitePlanDbEngine.Instance.FrameByName(Name);
        }

        public Point3dCollection Polygon()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(Database))
            {
                return acadDatabase.Element<Region>(Region).Vertices();
            }
        }

        public UInt32 Floor()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(Database))
            {
                // 当前所在的图框
                var frame = Frame();
                // 当前所在图框中的ROI
                var polygon = Polygon();
                if (polygon.Count == 0)
                {
                    return 0;
                }

                // 遍历所有同组的图框，寻找所有的文字信息
                var currentGroup = ThSitePlanConfigService.Instance.FindGroupByItemName(Name);
                var contents = new List<string>();
                foreach (var item in currentGroup.Items)
                {
                    if (item is ThSitePlanConfigItem it)
                    {
                        //获取item的图框
                        var itemName = it.Properties["Name"] as string;
                        var itemFrame = ThSitePlanDbEngine.Instance.FrameByName(itemName);
                        if (frame == itemFrame)
                        {
                            continue;
                        }

                        // 图框直接的偏移量，用来在被引用的图框中定位相对位置（区域）
                        Vector3d offset = Database.FrameOffset(frame, itemFrame);
                        // 已知当前图框中的某个区域，计算在被引用图框中同等相对位置的区域
                        var referencePolygon = new Point3dCollection();
                        foreach (Point3d vertex in polygon)
                        {
                            referencePolygon.Add(vertex.TransformBy(Matrix3d.Displacement(offset)));
                        }

                        // 在被引用图框中的区域内提取文字图元
                        var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[]
                        {
                            RXClass.GetClass(typeof(MText)).DxfName,
                            RXClass.GetClass(typeof(DBText)).DxfName,
                        }));
                        PromptSelectionResult psr = Active.Editor.SelectByPolygon(
                            referencePolygon,
                            PolygonSelectionMode.Window,
                            filter);
                        if (psr.Status != PromptStatus.OK)
                        {
                            continue;
                        }

                        foreach (var objId in psr.Value.GetObjectIds())
                        {
                            var text = acadDatabase.Element<Entity>(objId);
                            if (text is DBText dBText)
                            {
                                contents.Add(dBText.TextString);
                            }
                            else if (text is MText mText)
                            {
                                contents.Add(mText.Contents);
                            }
                        }
                    }
                }

                // 识别标注信息，获取楼层信息
                using (var annoations = new ThSitePlanBuildingAnnotations(contents.ToArray()))
                {
                    return annoations.Floor();
                }
            }
        }
    }
}
