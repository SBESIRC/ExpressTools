using System;
using AcHelper;
using Linq2Acad;
using ThCADCore.NTS;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ThSitePlan.Engine
{
    public class ThSitePlanBuildingShadow
    {
        public string Name { get; set; }
        public UInt32 Floor { get; set; }
        public Database Database { get; set; }
        public ObjectIdCollection Regions { get; set; }

        /// <summary>
        /// 根据建筑物面域和建筑物楼层，计算并创建其阴影面域
        /// </summary>
        public static ThSitePlanBuildingShadow CreateShadow(ThSitePlanBuilding building)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(building.Database))
            {
                // 获取建筑楼层信息失败，比如建筑信息丢失
                // 这种情况下，没有足够信息创建阴影，返回失败
                UInt32 floor = building.Floor();
                if (floor == 0)
                {
                    return null;
                }

                // 创建建筑物阴影面域
                var angle = Properties.Settings.Default.shadowAngle * Math.PI / 180.0;
                var length = floor / Math.Tan(angle);
                Matrix3d rotation = Matrix3d.Rotation(angle, Vector3d.ZAxis, Point3d.Origin);
                var shadows = building.Region.CreateShadowRegion(Vector3d.XAxis.TransformBy(rotation).MultiplyBy(length));
                // 根据建筑面域计算阴影面域失败，比如建筑面域的形状异形
                // 这种情况下，没有足够信息创建阴影，返回失败
                if (shadows.Count < 1)
                {
                    return null;
                }

                // 设置建筑物阴影面域图层
                acadDatabase.Database.MoveToLayer(shadows, ThSitePlanCommon.LAYER_GLOBAL_SHADOW);

                // 返回阴影面域对象
                return new ThSitePlanBuildingShadow()
                {
                    Floor = floor,
                    Regions = shadows,
                    Name = building.Name,
                    Database = building.Database
                };
            }
        }

        public static ThSitePlanBuildingShadow CreateSimpleShadow(ThSitePlanBuilding building, uint default_floor)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(building.Database))
            {
                var length = Properties.Settings.Default.shadowLengthScale * default_floor;
                var angle = Properties.Settings.Default.shadowAngle * Math.PI / 180.0;
                Matrix3d rotation = Matrix3d.Rotation(angle, Vector3d.ZAxis, Point3d.Origin);
                Vector3d offset = Vector3d.XAxis.TransformBy(rotation).MultiplyBy(length);
                var shadowRegion = building.Region.CreateSimpleShadowRegion(offset);

                // 设置建筑物阴影面域图层
                acadDatabase.Database.MoveToLayer(shadowRegion, ThSitePlanCommon.LAYER_GLOBAL_SHADOW);

                // 返回阴影面域
                return new ThSitePlanBuildingShadow()
                {
                    Floor = default_floor,
                    Regions = new ObjectIdCollection() { shadowRegion },
                    Name = building.Name,
                    Database = building.Database
                };
            }
        }

        public void ProjectShadow(ThSitePlanBuilding building)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(Database))
            {
                foreach(ObjectId region in Regions)
                {
                    // 在阴影面域范围内寻找可能存在的被遮挡的建筑
                    var filterlist = OpFilter.Bulid(o =>
                        o.Dxf((int)DxfCode.Start) == RXClass.GetClass(typeof(Region)).DxfName &
                        o.Dxf((int)DxfCode.LayerName) == ThSitePlanCommon.LAYER_BUILD_HATCH);
                    var psr = Active.Editor.SelectByRegion(
                        region,
                        PolygonSelectionMode.Crossing,
                        filterlist);
                    if (psr.Status != PromptStatus.OK)
                    {
                        continue;
                    }

                    var sObjs = new List<Polyline>();
                    foreach (ObjectId obj in psr.Value.GetObjectIds())
                    {
                        if (obj == region)
                        {
                            continue;
                        }

                        using (var buildInfo = new ThSitePlanBuilding(Database, obj, Name))
                        {
                            //遮挡建筑物
                            sObjs.AddRange(CalShadowBlockByBuliding(building, buildInfo));
                        }
                    }

                    var buildings = new ObjectIdCollection()
                    {
                        // 产生阴影的建筑物
                        building.Region,
                    };

                    var differences = region.CreateDifferenceShadowRegion(sObjs);
                    if (differences.Count != 0)
                    {
                        foreach (var difference in differences)
                        {
                            //根据新阴影创建填充
                            acadDatabase.ModelSpace.Add(difference).CreateHatchWithPolygon();
                        }

                        //删除原阴影
                        acadDatabase.Element<Region>(region, true).Erase();
                    }
                    else
                    {
                        //根据原阴影创建填充
                        var thisBuilding = new ObjectIdCollection()
                        {
                            // 产生阴影的建筑物
                            building.Region,
                        };
                        var thisDifferences = region.CreateDifferenceShadowRegion(thisBuilding);
                        if (thisDifferences.Count != 0)
                        {
                            foreach (var difference in thisDifferences)
                            {
                                //根据新阴影创建填充
                                acadDatabase.ModelSpace.Add(difference).CreateHatchWithPolygon();
                            }

                            //删除原阴影
                            acadDatabase.Element<Region>(region, true).Erase();
                        }
                        else
                        {
                            region.CreateHatchWithPolygon();
                        }
                    }
                }
            }
        }

        public List<Polyline> CalShadowBlockByBuliding(ThSitePlanBuilding building, ThSitePlanBuilding blockBuilding)
        {
            List<Polyline> resPoly = new List<Polyline>();
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                var blockShadow = acad.Element<Region>(blockBuilding.Region);
                Polyline bBuildingPoly = blockShadow.ToNTSPolygon().Shell.ToDbPolyline();

                var shadow = acad.Element<Region>(building.Region);
                Polyline buildingPoly = shadow.ToNTSPolygon().Shell.ToDbPolyline();
                if (building.Floor() <= blockBuilding.Floor())
                {
                    resPoly.Add(bBuildingPoly);
                    return resPoly;
                }


                // 创建建筑物阴影面域
                var angle = Properties.Settings.Default.shadowAngle * Math.PI / 180.0;
                var length = (building.Floor() - blockBuilding.Floor()) / Math.Tan(angle);
                Matrix3d rotation = Matrix3d.Rotation(angle, Vector3d.ZAxis, Point3d.Origin);
                var shadows = building.Region.CreateShadowRegion(Vector3d.XAxis.TransformBy(rotation).MultiplyBy(length));

                resPoly = blockBuilding.Region.CreateDifferenceShadowRegion(shadows);
            }
            

            return resPoly;
        }
    }
}
