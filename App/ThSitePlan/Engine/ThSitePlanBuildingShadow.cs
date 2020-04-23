﻿using System;
using AcHelper;
using Linq2Acad;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;


namespace ThSitePlan.Engine
{
    public class ThSitePlanBuildingShadow
    {
        public string Name { get; set; }
        public UInt32 Floor { get; set; }
        public ObjectId Region { get; set; }
        public Database Database { get; set; }

        /// <summary>
        /// 根据建筑物面域和建筑物楼层，计算并创建其阴影面域
        /// </summary>
        public static ThSitePlanBuildingShadow CreateShadow(ThSitePlanBuilding building)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(building.Database))
            {
                UInt32 floor = building.Floor();
                if (floor == 0)
                {
                    return null;
                }

                // 创建建筑物阴影面域
                var length = Properties.Settings.Default.shadowLengthScale * floor;
                var angle = Properties.Settings.Default.shadowAngle * Math.PI / 180.0;
                Matrix3d rotation = Matrix3d.Rotation(angle, Vector3d.ZAxis, Point3d.Origin);
                var shadows = building.Region.CreateShadowRegion(Vector3d.XAxis.TransformBy(rotation).MultiplyBy(length));
                if (shadows.Count != 1)
                {
                    return null;
                }

                // 设置建筑物阴影面域图层
                acadDatabase.Database.MoveToLayer(shadows, ThSitePlanCommon.LAYER_GLOBAL_SHADOW);

                // 返回阴影面域
                return new ThSitePlanBuildingShadow()
                {
                    Floor = floor,
                    Region = shadows[0],
                    Name = building.Name,
                    Database = building.Database
                };
            }
        }

        public void ProjectShadow()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(Database))
            {
                // 获取阴影面域的轮廓点
                var polygon = acadDatabase.Element<Region>(Region).Vertices();
                if (polygon.Count == 0)
                {
                    return;
                }

                // 在阴影面域范围内寻找可能存在的被遮挡的建筑
                var filterlist = OpFilter.Bulid(o => 
                    o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Region)).DxfName &
                    o.Dxf((int)DxfCode.LayerName) == ThSitePlanCommon.LAYER_BUILD_HATCH);
                var psr = Active.Editor.SelectByPolygon(
                    polygon,
                    PolygonSelectionMode.Crossing,
                    filterlist);
                if (psr.Status != PromptStatus.OK)
                {
                    return;
                }

                var sObjs = new ObjectIdCollection();
                foreach (ObjectId obj in psr.Value.GetObjectIds())
                {
                    if (obj == Region)
                    {
                        continue;
                    }

                    using (var buildInfo = new ThSitePlanBuilding(Database, obj, Name))
                    {
                        UInt32 buildFloor = buildInfo.Floor();
                        if (buildFloor > 0 && buildFloor < Floor * 0.5)
                        {
                            sObjs.Add(obj);
                        }
                    }
                }

                // 计算遮挡后的阴影
                var pObjs = new ObjectIdCollection(new ObjectId[] {
                    Region
                });
                Active.Editor.SubtractRegions(pObjs, sObjs);
            }
        }
    }
}