using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using Linq2Acad;

namespace THColumnInfo.Controller
{
    public class CadOperation
    {

        [DllImport("acdb19.dll",
    CharSet = CharSet.Unicode,
    CallingConvention = CallingConvention.Cdecl,
    EntryPoint = "?addFilter@AcDbIndexFilterManager@@YA?AW4ErrorStatus@Acad@@PAVAcDbBlockReference@@PAVAcDbFilter@@@Z")]
        private static extern int addFilter32(IntPtr pBlkRef, IntPtr pFilter);
        [DllImport("acdb19.dll",
            CharSet = CharSet.Unicode,
            CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "?addFilter@AcDbIndexFilterManager@@YA?AW4ErrorStatus@Acad@@PEAVAcDbBlockReference@@PEAVAcDbFilter@@@Z")]
        private static extern int addFilter64(IntPtr pBlkRef, IntPtr pFilter);


        // the clip rectangle needs to be provided
        // with (lower-left, upper-right) so we may
        // need to re-arrange depending on how the
        // user selected corners
        static Point2dCollection GetClipRectangle(
          Point3d p1, Point3d p2)
        {
            Point2dCollection clipRect =
              new Point2dCollection();

            double minX = p1.X;
            double minY = p1.Y;

            double maxX = p2.X;
            double maxY = p2.Y;

            if (minX > p2.X)
            {
                minX = p2.X;
                maxX = p1.X;
            }

            if (minY > p2.Y)
            {
                minY = p2.Y;
                maxY = p1.Y;
            }

            clipRect.Add(new Point2d(minX, minY));
            clipRect.Add(new Point2d(maxX, maxY));

            return clipRect;
        }
        /// <summary>
        /// 获取两点间的物体
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="layer"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static List<ObjectId> GetSelectionInBox(Point3d pt1, Point3d pt2, double offsetZ, string layer = "", string category = "")
        {
            List<ObjectId> objectIds = new List<ObjectId>();
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TypedValue[] tvs = null;
            PromptSelectionResult psr = null;
            List<ObjectId> findObjIds = new List<ObjectId>();
            if(layer!="" && category!="")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.LayerName, layer), new TypedValue((int)DxfCode.Start, category)};
                psr= Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else if(layer != "")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.LayerName, layer)};
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else if(category!="")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, category) };
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else
            {
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            if(psr==null || psr.Status!=PromptStatus.OK)
            {
                return objectIds;
            }
            else
            {
                findObjIds = psr.Value.GetObjectIds().ToList();
            }
            Point3d minPt = new Point3d(Math.Min(pt1.X,pt2.X), Math.Min(pt1.Y, pt2.Y), Math.Min(pt1.Z, pt2.Z)- offsetZ); //Box左下角点
            Point3d maxPt = new Point3d(Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y), Math.Max(pt1.Z, pt2.Z)+ offsetZ); ////Box右上角点

            foreach (ObjectId objId in findObjIds)
            {
               Entity entity= ThColumnInfDbUtils.GetEntity(db, objId);
                if(entity==null || entity.GeometricExtents==null)
                {
                    continue;
                }
                Extents3d extents3D = entity.GeometricExtents;
                Point3d midPt = BaseFunction.GetMidPt(extents3D.MinPoint, extents3D.MaxPoint);
                if(BaseFunction.CheckPtInBox(extents3D.MinPoint, minPt, maxPt) ||
                   BaseFunction.CheckPtInBox(extents3D.MaxPoint, minPt, maxPt) ||
                   BaseFunction.CheckPtInBox(midPt, minPt, maxPt))
                {
                    objectIds.Add(objId);
                }
                else
                {
                    //如果一个图形包括当前矩形框
                    if(BaseFunction.CheckPtInBox(new Point3d(minPt.X,minPt.Y, extents3D.MinPoint.Z), extents3D.MinPoint, extents3D.MaxPoint) &&
                        BaseFunction.CheckPtInBox(new Point3d(maxPt.X, maxPt.Y, extents3D.MinPoint.Z), extents3D.MinPoint, extents3D.MaxPoint))
                    {
                        if(extents3D.MinPoint.Z>= minPt.Z && extents3D.MinPoint.Z <= maxPt.Z)
                        {
                            objectIds.Add(objId);
                        }
                    }
                }
            }
            return objectIds;
        }
        /// <summary>
        /// 获取两点间的物体
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="layer"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static List<ObjectId> GetSelectionInRectangle(Point3d pt1, Point3d pt2, string layer = "", string category = "")
        {
            List<ObjectId> objectIds = new List<ObjectId>();
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TypedValue[] tvs = null;
            PromptSelectionResult psr = null;
            List<ObjectId> findObjIds = new List<ObjectId>();
            if (layer != "" && category != "")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.LayerName, layer), new TypedValue((int)DxfCode.Start, category) };
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else if (layer != "")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.LayerName, layer) };
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else if (category != "")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, category) };
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else
            {
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            if (psr == null || psr.Status != PromptStatus.OK)
            {
                return objectIds;
            }
            else
            {
                findObjIds = psr.Value.GetObjectIds().ToList();
            }
            Point3d minPt = new Point3d(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y), Math.Min(pt1.Z, pt2.Z)); //左下角点
            Point3d maxPt = new Point3d(Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y), Math.Max(pt1.Z, pt2.Z)); //右上角点

            foreach (ObjectId objId in findObjIds)
            {
                Entity entity = ThColumnInfDbUtils.GetEntity(db, objId);
                if (entity == null || entity.GeometricExtents == null)
                {
                    continue;
                }
                Extents3d extents3D = entity.GeometricExtents;
                Point3d midPt = BaseFunction.GetMidPt(extents3D.MinPoint, extents3D.MaxPoint);
                if (BaseFunction.CheckPtRectangle(extents3D.MinPoint, minPt, maxPt) ||
                   BaseFunction.CheckPtRectangle(extents3D.MaxPoint, minPt, maxPt) ||
                   BaseFunction.CheckPtRectangle(midPt, minPt, maxPt))
                {
                    objectIds.Add(objId);
                }
                else
                {
                    //如果一个图形包括当前矩形框
                    if (BaseFunction.CheckPtRectangle(new Point3d(minPt.X, minPt.Y, extents3D.MinPoint.Z), extents3D.MinPoint, extents3D.MaxPoint) &&
                        BaseFunction.CheckPtRectangle(new Point3d(maxPt.X, maxPt.Y, extents3D.MinPoint.Z), extents3D.MinPoint, extents3D.MaxPoint))
                    {
                        objectIds.Add(objId);
                    }
                }
            }
            return objectIds;
        }
        /// <summary>
        /// Check if the OS is 32 or 64 bit
        /// </summary>
        public static bool is64bits
        {
            get
            {
                return (Application.GetSystemVariable
            ("PLATFORM").ToString().IndexOf("64") > 0);
            }
        }
        public static List<string> GetLayerNameList()
        {
            List<string> layerNames = new List<string>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(Application.DocumentManager.MdiActiveDocument.Database))
            {
                layerNames= acadDatabase.Layers.Select(i => i.Name).ToList();
            }
            return layerNames;
        }
    }
}
