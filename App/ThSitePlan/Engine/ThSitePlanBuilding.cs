using System;
using AcHelper;
using Linq2Acad;
using GeometryExtensions;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

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

        private ObjectId ReferenceFrame()
        {
            return ThSitePlanDbEngine.Instance.FrameByName(ReferenceName(Name));
        }

        private string ReferenceName(string name)
        {
            if (name == "建筑物-场地内建筑-建筑色块")
            {
                return "建筑物-场地内建筑-建筑信息";
            }
            else if (name == "建筑物-场地外建筑-建筑色块")
            {
                return "建筑物-场地外建筑-建筑信息";
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private Vector3d Offset(Database database, ObjectId frame1, ObjectId frame2)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var pline1 = acadDatabase.Element<Polyline>(frame1);
                var pline2 = acadDatabase.Element<Polyline>(frame2);
                return pline2.Centroid() - pline1.Centroid();
            }
        }

        public UInt32 Floor()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(Database))
            {
                var frame = Frame();
                var referenceFrame = ReferenceFrame();
                Vector3d offset = Offset(Database, frame, referenceFrame);
                var referenceRegion = Region.CopyWithMove(offset);
                var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[]
                {
                    RXClass.GetClass(typeof(MText)).DxfName,
                    RXClass.GetClass(typeof(DBText)).DxfName,
                }));
                PromptSelectionResult psr = Active.Editor.SelectByRegion(
                    referenceRegion,
                    PolygonSelectionMode.Window,
                    filter);
                if (psr.Status != PromptStatus.OK)
                {
                    return 0;
                }

                var contents = new List<string>();
                foreach (var item in psr.Value.GetObjectIds())
                {
                    var text = acadDatabase.Element<Entity>(item);
                    if (text is DBText dBText)
                    {
                        contents.Add(dBText.TextString);
                    }
                    else if (text is MText mText)
                    {
                        contents.Add(mText.Contents);
                    }
                }

                using (var annoations = new ThSitePlanBuildingAnnotations(contents.ToArray()))
                {
                    return annoations.Floor();
                }
            }
        }
    }
}
