using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThColumnInfo.Service
{
    class CheckResultCadInteractive
    {
        /// <summary>
        /// 获取构件属性定义要设置的对象
        /// </summary>
        /// <returns></returns>
        public static List<ObjectId> GetShowObjIds()
        {
            List<ObjectId> showObjIds = new List<ObjectId>();
            Document document = ThColumnInfoUtils.GetMdiActiveDocument();
            TypedValue[] tvs = new TypedValue[]
                    {
                new TypedValue((int)DxfCode.ExtendedDataRegAppName,ThColumnInfoUtils.thColumnFrameRegAppName),
                new TypedValue((int)DxfCode.Start,"LWPOLYLINE,Text"),
                    };
            SelectionFilter sf = new SelectionFilter(tvs);
            PromptSelectionResult psr = document.Editor.SelectAll(sf);
            if (psr.Status == PromptStatus.OK)
            {
                showObjIds = psr.Value.GetObjectIds().ToList();
                using (Transaction trans = document.TransactionManager.StartTransaction())
                {
                    for (int i = 0; i < showObjIds.Count; i++)
                    {
                        Entity ent = trans.GetObject(showObjIds[i], OpenMode.ForRead) as Entity;
                        if (!ent.Visible)
                        {
                            showObjIds.RemoveAt(i);
                            i = i - 1;
                        }
                    }
                    trans.Commit();
                }
            }
            return showObjIds;
        }
        public static void LocateInnerFrame(ThStandardSign thStandardSign)
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = document.LockDocument())
            {
                Extents3d extents = ThColumnInfoUtils.GeometricExtentsImpl(thStandardSign.Br);
                if (extents == null)
                {
                    return;
                }
                COMTool.ZoomWindow(ThColumnInfoUtils.TransPtFromUcsToWcs(extents.MinPoint)
                    , ThColumnInfoUtils.TransPtFromUcsToWcs(extents.MaxPoint));
            }
        }
        public static void LocateColumnFrameIds(List<ObjectId> frameIds)
        {
            double offsetDis = 4000;
            Document document = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = document.LockDocument())
            {
                using (Transaction trans = document.TransactionManager.StartTransaction())
                {
                    List<Point3d> pts = new List<Point3d>();
                    foreach (ObjectId objId in frameIds)
                    {
                        if (objId.IsNull || objId.IsErased || !objId.IsValid)
                        {
                            continue;
                        }
                        Entity ent = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                        Extents3d extents = ThColumnInfoUtils.GeometricExtentsImpl(ent);
                        if (extents == null)
                        {
                            continue;
                        }
                        pts.Add(extents.MinPoint);
                        pts.Add(extents.MaxPoint);
                    }
                    double minX = pts.OrderBy(i => i.X).First().X;
                    double minY = pts.OrderBy(i => i.Y).First().Y;
                    double maxX = pts.OrderByDescending(i => i.X).First().X;
                    double maxY = pts.OrderByDescending(i => i.Y).First().Y;

                    COMTool.ZoomWindow(ThColumnInfoUtils.TransPtFromUcsToWcs(new Point3d(minX, minY, 0.0) + new Vector3d(-offsetDis, -offsetDis, 0))
                   , ThColumnInfoUtils.TransPtFromUcsToWcs(new Point3d(maxX, maxY, 0.0) + new Vector3d(offsetDis, offsetDis, 0)));
                    trans.Commit();
                }
            }
        }
        public static void ShowHideFrameIds(List<ObjectId> frameIds, bool visible)
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = document.LockDocument())
            {
                using (Transaction trans = document.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId objId in frameIds)
                    {
                        if (objId == ObjectId.Null || objId.IsErased || objId.IsValid == false)
                        {
                            continue;
                        }
                        Entity ent = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                        ent.UpgradeOpen();
                        ent.Visible = visible;
                        ent.DowngradeOpen();
                    }
                    trans.Commit();
                }
            }
        }
    }
}
