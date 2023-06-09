﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThXClip
{
    public class BlockReferenceGeometryExtents3d
    {
        private BlockReference br = null;
        private Extents3d? geometryExtents3d=new Extents3d();
        public Extents3d? GeometryExtents3d
        {
            get
            {
                return geometryExtents3d;
            }
        }
        public BlockReferenceGeometryExtents3d(BlockReference br)
        {
            this.br = br;
        }
        public void GeometryExtents3dBestFit()
        {
            if (this.br == null)
            {
                return;
            }
            List<Entity> ents = new List<Entity>();
            try
            {
                ents = ThXClipCadOperation.Explode(this.br,false);
                ents = ents.Where(i => i.Visible).Select(i => i).ToList();
                List<Point3d> totalPts = new List<Point3d>();
                var res = from item in ents
                          where item is Curve && item.GeometricExtents != null
                          select new
                          {
                              MinPoint = item.GeometricExtents.MinPoint,
                              MaxPoint= item.GeometricExtents.MaxPoint
                          };
                res.ForEach(i => { totalPts.Add(i.MinPoint); totalPts.Add(i.MaxPoint); });                
                double minX = totalPts.OrderBy(i => i.X).First().X;
                double minY = totalPts.OrderBy(i => i.Y).First().Y;
                double minZ = totalPts.OrderBy(i => i.Z).First().Z;
                double maxX = totalPts.OrderByDescending(i => i.X).First().X;
                double maxY = totalPts.OrderByDescending(i => i.Y).First().Y;
                double maxZ = totalPts.OrderByDescending(i => i.Z).First().Z;
                Point3d minPt = new Point3d(minX, minY, minZ);
                Point3d maxPt = new Point3d(maxX, maxY, maxZ);
                geometryExtents3d = new Extents3d(minPt, maxPt);
            }
            catch(System.Exception ex)
            {
                ThXClipUtils.WriteException(ex, "GeometryExtents3dBestFit");
            }
            finally
            {
                ents.ForEach(i => i.Dispose());
            }
        }
    }
}
