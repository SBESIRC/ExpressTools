using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThXClip
{
    public class ModelObjInfo
    {
        public ObjectId Id { get; set; } = ObjectId.Null;
        public DBObjectCollection DbObjs { get; set; } = new DBObjectCollection();
        public bool NotHandle { get; set; } = true;
    }
}
