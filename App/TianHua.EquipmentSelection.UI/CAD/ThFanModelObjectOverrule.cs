using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;

namespace TianHua.FanSelection.UI.CAD
{
    public class ThFanModelObjectOverrule : ObjectOverrule
    {
        public override void Erase(DBObject dbObject, bool erasing)
        {
            //
        }

        public override DBObject DeepClone(DBObject dbObject, DBObject ownerObject, IdMapping idMap, bool isPrimary)
        {
            DBObject result = base.DeepClone(dbObject, ownerObject, idMap, isPrimary);

            if (!string.IsNullOrEmpty(dbObject.ObjectId.GetModelIdentifier()))
            {
                if (idMap[dbObject.ObjectId] != null)
                {
                    ObjectId targetId = idMap[dbObject.ObjectId].Value;
                    if (targetId != ObjectId.Null)
                    {
                        //
                    }
                }
            }

            return result;
        }

        public override DBObject WblockClone(DBObject dbObject, RXObject ownerObject, IdMapping idMap, bool isPrimary)
        {
            throw new NotImplementedException();
        }
    }
}
