using System;
using Linq2Acad;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMirror
{
    public class ThMirrorObjectOverrule : ObjectOverrule
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThMirrorObjectOverrule instance = new ThMirrorObjectOverrule();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThMirrorObjectOverrule() { }
        internal ThMirrorObjectOverrule() { }
        public static ThMirrorObjectOverrule Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public override bool IsApplicable(RXObject overruledSubject)
        {
            throw new NotImplementedException();
        }

        public override DBObject DeepClone(DBObject dbObject, DBObject ownerObject, IdMapping idMap, bool isPrimary)
        {
            DBObject result = base.DeepClone(dbObject, ownerObject, idMap, isPrimary);

            if (dbObject.ObjectId.IsBlockReferenceContainText())
            {
                if (idMap[dbObject.ObjectId] != null)
                {
                    ObjectId targetId = idMap[dbObject.ObjectId].Value;
                    if (targetId != ObjectId.Null)
                    {
                        using (AcadDatabase acadDatabase = AcadDatabase.Active())
                        {
                            var blockReference = acadDatabase.Element<BlockReference>(targetId);
                            ThMirrorEngine.Instance.Targets.Add(new ThMirrorData(blockReference));
                        }
                    }
                }
            }

            return result;
        }

        public override DBObject WblockClone(DBObject dbObject, RXObject ownerObject, IdMapping idMap, bool isPrimary)
        {
            return base.WblockClone(dbObject, ownerObject, idMap, isPrimary);
        }
    }
}
