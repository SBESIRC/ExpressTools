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
    }
}
