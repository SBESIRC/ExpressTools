using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;

namespace ThXClip
{
    public class ThXClipDbUtils
    {
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="database"></param>
        /// <param name="objId"></param>
        /// <returns></returns>
        public static Entity GetEntity(Database database, ObjectId objId)
        {
            Entity entity;
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                entity = acadDatabase.ModelSpace.Element(objId);
            }
            return entity;
        }

    }
}
