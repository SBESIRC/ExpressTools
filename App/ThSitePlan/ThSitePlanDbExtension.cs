using AcHelper;
using Linq2Acad;
using DotNetARX;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using ThCADCore.NTS;
using GeometryExtensions;
using Autodesk.AutoCAD.BoundaryRepresentation;

namespace ThSitePlan
{
    public static class ThSitePlanDbExtension
    {
        public static ObjectIdCollection SelectAll(this Database database, string layer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var objs = new ObjectIdCollection();
                acadDatabase.ModelSpace
                    .OfType<Entity>()
                    .Where(o => o.Layer == layer)
                    .ForEachDbObject(o => objs.Add(o.ObjectId));
                return objs;
            }
        }

        public static ObjectIdCollection CopyWithMove(this Database database,
            ObjectIdCollection objs, 
            Matrix3d displacement,
            bool bErase = false)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var clones = new ObjectIdCollection();
                using (IdMapping idMap = new IdMapping())
                {
                    acadDatabase.Database.DeepCloneObjects(objs,
                            acadDatabase.Database.CurrentSpaceId, idMap, false);
                    foreach (IdPair pair in idMap)
                    {
                        if (pair.IsPrimary && pair.IsCloned)
                        {
                            clones.Add(pair.Value);
                            var entity = acadDatabase.Element<Entity>(pair.Value, true);
                            entity.TransformBy(displacement);
                            if (bErase)
                            {
                                acadDatabase.Element<Entity>(pair.Key, true).Erase();
                            }
                        }
                    }
                }
                return clones;
            }
        }

        public static ObjectId CopyWithMove(this ObjectId objectId, Vector3d offset)
        {
            var objs = new ObjectIdCollection()
            {
                objectId
            };
            var clones = objectId.Database.CopyWithMove(objs, Matrix3d.Displacement(offset));
            if (clones.Count == 1)
            {
                return clones[0];
            }
            else
            {
                return ObjectId.Null;
            }
        }

        public static void CopyWithMove(this Database database, ObjectId frame, Vector3d offset)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                    frame,
                    PolygonSelectionMode.Crossing,
                    null);
                if (psr.Status == PromptStatus.OK)
                {
                    var objs = new ObjectIdCollection(psr.Value.GetObjectIds());
                    acadDatabase.Database.CopyWithMove(objs, Matrix3d.Displacement(offset));

                }
            }
        }

        // 这里实现了一个特殊的"Move"操作。
        // 简单的"Move"操作，即直接通过调用Entity.TransformBy()，
        // 在这样的应用场景下，简单的"Move"操作有问题：
        //  1. 在一个框线的区域内用Editor.SelectXXX()选择对象
        //  2. 将选择的对象移动到另外一个区域
        //  3. 继续在框线内用Editor.SelectXXX()选择对象
        //  4. 将选择的对象移动到另外一个区域
        // 这里实现的特殊的"Move"操作的流程是这样的：
        //  1. 在一个区域内用Editor.SelectXXX()选择对象
        //  2. 将选择对象复制到另外一个区域
        //  3. 将原区域内的原选择对象删除
        //  4. 继续在框线内用Editor.SelectXXX()选择对象
        //  5. 将原区域内的原选择对象删除
        public static void Move(this Database database, ObjectIdCollection objs, Matrix3d displacement)
        {
            CopyWithMove(database, objs, displacement, true);
        }

        public static ObjectId Frame(this Database database, string layer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var frame = acadDatabase.ModelSpace
                    .OfType<Polyline>()
                    .Where(o => o.Layer == layer)
                    .FirstOrDefault();
                if (frame != null)
                {
                    return frame.ObjectId;
                }

                return ObjectId.Null;
            }
        }

        public static bool IsBlockReferenceExplodable(this ObjectId blockReferenceId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                try
                {
                    var blockReference = acadDatabase.Element<BlockReference>(blockReferenceId);
                    return blockReference.IsBlockReferenceExplodable();
                }
                catch
                {
                    // 图元不是块引用
                    return false;
                }
            }
        }

        public static bool IsBlockReferenceExplodable(this BlockReference blockReference)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                try
                {
                    // 对于动态块，BlockReference.Name返回的可能是一个匿名块的名字（*Uxxx）
                    // 对于这样的动态块，我们并不需要访问到它的“原始”动态块定义，我们只关心它“真实”的块定义
                    var blockTableRecord = acadDatabase.Blocks.Element(blockReference.Name);

                    // 暂时不支持动态块，外部参照，覆盖
                    if (blockTableRecord.IsDynamicBlock ||
                        blockTableRecord.IsFromExternalReference ||
                        blockTableRecord.IsFromOverlayReference)
                    {
                        return false;
                    }

                    // 忽略图纸空间和匿名块
                    if (blockTableRecord.IsLayout || blockTableRecord.IsAnonymous)
                    {
                        return false;
                    }

                    // 忽略不可“炸开”的块
                    if (!blockTableRecord.Explodable)
                    {
                        return false;
                    }

                    // 可以“炸”的块
                    return true;
                }
                catch
                {
                    // 图元不是块引用
                    return false;
                }
            }
        }

        public static void CreateHatchWithPolygon(this ObjectId polygon)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(polygon.Database))
            {
                try
                {
                    // Hatch外轮廓
                    var dbObjIds = new ObjectIdCollection()
                    {
                        polygon
                    };

                    // 填充面积框线
                    Hatch hatch = new Hatch();
                    ObjectId hatchId = ObjectId.Null;

                    hatchId = acadDatabase.ModelSpace.Add(hatch);
                    hatch.SetHatchPattern(HatchPatternType.PreDefined, ThSitePlanCommon.hatch_pattern);
                    hatch.ColorIndex = ThSitePlanCommon.hatch_color_index;

                    // 外圈轮廓
                    hatch.Associative = true;
                    hatch.AppendLoop(HatchLoopTypes.External, dbObjIds);

                    // 重新生成Hatch纹理
                    hatch.EvaluateHatch(true);
                }
                catch
                {
                    // 未知错误
                }
            }
        }

        // 对于一堆杂乱的“封闭”曲线，这里实现了一个“构面”的检测：
        // 即通过遍历每一个封闭曲线，调用Region.CreateFromCurves()。
        // 若无Exception抛出，则这个“封闭”曲线可以构面；否则则构面失败。
        public static ObjectIdCollection CreateRegionLoops(this Database database, DBObjectCollection polygons)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var loops = new ObjectIdCollection();
                foreach (DBObject polygon in polygons)
                {
                    try
                    {
                        var loop = polygon as Polyline;
                        var items = new DBObjectCollection()
                        {
                            loop
                        };
                        // 若可以创建Region，即为一个正确的loop
                        using (var regions = Region.CreateFromCurves(items))
                        {
                            loops.Add(acadDatabase.ModelSpace.Add(loop));
                        }
                    }
                    catch
                    {
                        // 未知错误
                    }
                }
                return loops;
            }
        }

        public static ObjectIdCollection CreateRegionLoops(this Database database, ObjectIdCollection collection)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var loops = new ObjectIdCollection();
                foreach (ObjectId obj in collection)
                {
                    try
                    {
                        using (var curves = new DBObjectCollection()
                        {
                            acadDatabase.Element<Curve>(obj),
                        })
                        using (var regions = Region.CreateFromCurves(curves))
                        {
                            loops.Add(obj);
                        }
                    }
                    catch
                    {
                        // 未知错误
                    }
                }
                return loops;
            }
        }

        public static ObjectIdCollection CreateShadowRegion(this ObjectId obj, Vector3d offset)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(obj.Database))
            {
                var lines = new DBObjectCollection();
                var region = acadDatabase.Element<Region>(obj);
                var displacement = Matrix3d.Displacement(offset);
                var offsetRegion = region.GetTransformedCopy(displacement) as Region;

                // 阴影连线
                using (var brepRegion1 = new Brep(region))
                {
                    foreach (Autodesk.AutoCAD.BoundaryRepresentation.Vertex vertex in brepRegion1.Vertices)
                    {
                        lines.Add(new Line(vertex.Point, vertex.Point + offset));
                    }
                }

                // 将面域“炸”成多个线段，便于后面的Noding Process
                region.Explode(lines);
                offsetRegion.Explode(lines);
                var shadows = new ObjectIdCollection();
                foreach (DBObject polygon in lines.PolygonizeLines())
                {
                    try
                    {
                        var loop = polygon as Polyline;
                        var items = new DBObjectCollection()
                        {
                            loop
                        };
                        // 若可以创建Region，即为一个正确的loop
                        var regions = Region.CreateFromCurves(items);
                        shadows.Add(acadDatabase.ModelSpace.Add(regions[0] as Region));
                    }
                    catch
                    {
                        // 未知错误
                    }
                }
                return shadows;
            }
        }

        public static Region CreateDifferenceShadowRegion(this ObjectId shadowObj, ObjectId buildingObj)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(shadowObj.Database))
            {
                var shadow = acadDatabase.Element<Region>(shadowObj);
                var building = acadDatabase.Element<Region>(buildingObj);
                var difference = shadow.Difference(building);
                if (difference != null)
                {
                    difference.SetPropertiesFrom(shadow);
                }
                return difference;
            }
        }

        public static ObjectIdCollection CreateBoundaries(this Database database, ObjectIdCollection objs)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var dbObjs = new DBObjectCollection();
                foreach(ObjectId obj in objs)
                {
                    dbObjs.Add(acadDatabase.Element<Entity>(obj));
                }
                return acadDatabase.Database.CreateRegionLoops(dbObjs.Boundaries());
            }
        }

        public static DBObjectCollection GetPolyLineBounding(this DBObjectCollection dBObjects, Tolerance tolerance)
        {
            DBObjectCollection resBounding = new DBObjectCollection();
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                foreach (var dbObj in dBObjects)
                {
                    if (dbObj is Polyline)
                    {
                        Polyline polyline = dbObj as Polyline;
                        if (polyline.NumberOfVertices < 4)
                        {
                            continue;
                        }

                        List<Point2d> points = new List<Point2d>();
                        for (int i = 0; i < polyline.NumberOfVertices; i++)
                        {
                            if (points.Where(x => x.IsEqualTo(polyline.GetPoint2dAt(i), tolerance)).Count() <= 0)
                            {
                                points.Add(polyline.GetPoint2dAt(i));
                            }
                        }

                        Polyline resPolyline = new Polyline(points.Count)
                        {
                            Closed = true,
                        };
                        Point2d thisP = points.First();
                        int index = 0;
                        resPolyline.AddVertexAt(index, thisP, 0, 0, 0);
                        points.Remove(thisP);
                        while (points.Count > 0)
                        {
                            thisP = points.OrderBy(x => x.GetDistanceTo(thisP)).First();
                            index++;
                            resPolyline.AddVertexAt(index, thisP, 0, 0, 0);
                            points.Remove(thisP);
                        }
                        resBounding.Add(resPolyline);
                    }
                }
            }

            return resBounding;
        }

        public static void MoveToLayer(this Database database, ObjectIdCollection objs, string layerName)
        {
            using (AcadDatabase acdb = AcadDatabase.Use(database))
            {
                var layerId = LayerTools.AddLayer(database, layerName);
                foreach (ObjectId obj in objs)
                {
                    acdb.Element<Entity>(obj, true).LayerId = layerId;
                }
            }
        }

        public static Vector3d FrameOffset(this Database database, ObjectId frame1, ObjectId frame2)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var pline1 = acadDatabase.Element<Polyline>(frame1);
                var pline2 = acadDatabase.Element<Polyline>(frame2);
                return pline2.Centroid() - pline1.Centroid();
            }
        }

        public static Polyline CreatePolyline(this Extents3d extents)
        {
            var pline = new Polyline()
            {
                Closed = true,
            };
            pline.CreatePolyline(new Point3dCollection()
            {
                extents.MinPoint,
                extents.MinPoint + Vector3d.YAxis * extents.Height(),
                extents.MaxPoint,
                extents.MinPoint + Vector3d.XAxis * extents.Width(),
            });
            return pline;
        }
    }
}
