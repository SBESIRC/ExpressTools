using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;

namespace THColumnInfo
{
    public class ThColumnInfDbUtils
    {
        public static Entity GetEntity(Database database,ObjectId objId)
        {
            Entity entity;
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                entity= acadDatabase.ModelSpace.Element(objId);
            }
            return entity;
        }
        public static ObjectId GetObjId(string strHandle,Database db)
        {
            ObjectId id = ObjectId.Null;
            try
            {
                Handle handle = new Handle();
                Int64 nHandle = Convert.ToInt64(strHandle, 16);
                handle = new Handle(nHandle);
                id = db.GetObjectId(false, handle, 0);
            }
            catch (System.FormatException)
            {
            }
            return id;
        }       
 
    }
}
