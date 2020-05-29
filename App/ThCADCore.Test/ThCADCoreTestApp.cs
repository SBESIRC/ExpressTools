using AcHelper;
using Linq2Acad;
using ThCADCore.NTS;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThCADCore.Test
{
    public class ThCADCoreTestApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "ThMBB", CommandFlags.Modal)]
        public void ThMBB()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                acadDatabase.ModelSpace.Add(pline.MinimumBoundingBox());
            }
        }

        [CommandMethod("TIANHUACAD", "ThMBC", CommandFlags.Modal)]
        public void ThMBC()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                acadDatabase.ModelSpace.Add(pline.MinimumBoundingCircle());
            }
        }

        [CommandMethod("TIANHUACAD", "ThConvexHull", CommandFlags.Modal)]
        public void ThConvexHull()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                acadDatabase.ModelSpace.Add(pline.ConvexHull());
            }
        }

        [CommandMethod("TIANHUACAD", "ThEnvelope", CommandFlags.Modal)]
        public void ThEnvelope()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                acadDatabase.ModelSpace.Add(pline.GetOctagonalEnvelope());
            }
        }

        [CommandMethod("TIANHUACAD", "ThBoundary", CommandFlags.Modal)]
        public void ThBoundary()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new DBObjectCollection();
                foreach(var obj in result.Value.GetObjectIds())
                {
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                foreach(var obj in objs.Boundaries())
                {
                    acadDatabase.ModelSpace.Add(obj as Entity);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThRegionBoundary", CommandFlags.Modal)]
        public void ThRegionBoundary()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var region = acadDatabase.Element<Region>(result.ObjectId);
                var polygon = region.ToNTSPolygon();
                acadDatabase.ModelSpace.Add(polygon.Shell.ToDbPolyline());
            }
        }

        [CommandMethod("TIANHUACAD", "ThMerge", CommandFlags.Modal)]
        public void ThMerge()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new DBObjectCollection();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                foreach (var obj in objs.Merge())
                {
                    acadDatabase.ModelSpace.Add(obj as Entity);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThPolygonizer", CommandFlags.Modal)]
        public void ThPolygonizer()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new DBObjectCollection();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    objs.Add(acadDatabase.Element<Entity>(obj));
                }
                foreach (var obj in objs.Polygons())
                {
                    acadDatabase.ModelSpace.Add(obj as Entity);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThVoronoiDiagram", CommandFlags.Modal)]
        public void ThVoronoiDiagram()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var pline = acadDatabase.Element<Polyline>(result.ObjectId);
                foreach (Entity diagram in pline.VoronoiDiagram())
                {
                    diagram.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(diagram);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThTrim", CommandFlags.Modal)]
        public void ThTrim()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetEntity("请选择对象");
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var result2 = Active.Editor.GetEntity("请选择框线");
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }

                var curve = acadDatabase.Element<Polyline>(result.ObjectId);
                var frame = acadDatabase.Element<Polyline>(result2.ObjectId);
                foreach (Entity diagram in frame.Trim(curve))
                {
                    diagram.ColorIndex = 1;
                    acadDatabase.ModelSpace.Add(diagram);
                }
            }
        }

        [CommandMethod("TIANHUACAD", "ThLoops", CommandFlags.Modal)]
        public void ThLoops()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.GetSelection();
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                var objs = new DBObjectCollection();
                foreach (var obj in result.Value.GetObjectIds())
                {
                    if(acadDatabase.Element<Entity>(obj) is Curve curve)
                    {
                        objs.Add(curve);
                    }
                    else if(acadDatabase.Element<Entity>(obj) is BlockReference br)
                    {
                        DBObjectCollection explodeObjs = new DBObjectCollection();
                        TianHua.AutoCAD.Utility.ExtensionTools.ThBlockReferenceExtensions.Burst(br, explodeObjs);
                        foreach(DBObject dbObj in explodeObjs)
                        {
                            if(dbObj is Curve curveEnt)
                            {
                                objs.Add(curveEnt);
                            }
                        }
                    }
                }
                foreach (var obj in objs.FindLoops())
                {
                    acadDatabase.ModelSpace.Add(obj as Entity);
                }
            }
        }
    }
}
