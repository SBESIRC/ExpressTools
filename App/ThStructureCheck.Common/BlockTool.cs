using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThStructureCheck.Common
{
    public static class BlockTool
    {
        public static ObjectId CreateBlock(this Database db ,string blkName, List<Entity> ents)
        {
            ObjectId blkId = ObjectId.Null;
            if (string.IsNullOrEmpty(blkName))
            {
                return blkId;
            }
            List<Entity> filterEnts = ents.Where(o => o.ObjectId == ObjectId.Null && !o.IsDisposed && !o.IsErased).ToList();
            if (filterEnts.Count() == 0)
            {
                return blkId;
            }            
            using (Transaction trans= db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId,OpenMode.ForRead) as BlockTable;
                if(!bt.Has(blkName))
                {
                    bt.UpgradeOpen();
                    BlockTableRecord btr = new BlockTableRecord();
                    btr.Name = blkName;
                    filterEnts.ForEach(o => btr.AppendEntity(o));
                    blkId=bt.Add(btr);
                    trans.AddNewlyCreatedDBObject(btr, true);
                    bt.DowngradeOpen();
                }
                else
                {
                    blkId = bt[blkName];
                }
                trans.Commit();
            }
            return blkId;
        }
        public static ObjectId InsertBlock(this Database db, Point3d position,string blkName,Scale3d scale,double rotateAngle)
        {
            ObjectId blkRefId = ObjectId.Null;
            if (string.IsNullOrEmpty(blkName))
            {
                return blkRefId;
            }            
            using (Transaction trans=db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId,OpenMode.ForRead) as BlockTable;
                if(bt.Has(blkName))
                {
                    BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForRead) as BlockTableRecord;
                    btr.UpgradeOpen();
                    BlockReference br = new BlockReference(position, bt[blkName]);
                    br.ScaleFactors = scale;
                    br.Rotation = rotateAngle;
                    blkRefId = btr.AppendEntity(br);
                    trans.AddNewlyCreatedDBObject(br, true);
                    btr.DowngradeOpen();
                }
                trans.Commit();
            }
            return blkRefId;
        }
    }
}
