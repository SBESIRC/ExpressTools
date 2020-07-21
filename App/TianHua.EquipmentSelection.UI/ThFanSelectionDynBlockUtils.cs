using Linq2Acad;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;


namespace TianHua.FanSelection.UI
{
    public static class ThFanSelectionDynBlockUtils
    {
        // Reference:
        //  https://adndevblog.typepad.com/autocad/2012/05/accessing-visible-entities-in-a-dynamic-block.html
        public static Dictionary<string, ObjectIdCollection> DynablockVisibilityStates(this ThFSBlockReference blockReference)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(blockReference.HostDatabase))
            {
                var groups = new Dictionary<string, ObjectIdCollection>();
                var btr = acadDatabase.Blocks.ElementOrDefault(blockReference.EffectiveName);
                if (btr == null)
                {
                    return groups;
                }

                if (!btr.IsDynamicBlock)
                {
                    return groups;
                }

                if (btr.ExtensionDictionary.IsNull)
                {
                    return groups;
                }

                var dict = acadDatabase.Element<DBDictionary>(btr.ExtensionDictionary);
                if (!dict.Contains("ACAD_ENHANCEDBLOCK"))
                {
                    return groups;
                }

                ObjectId graphId = dict.GetAt("ACAD_ENHANCEDBLOCK");
                var parameterIds = graphId.acdbEntGetObjects((short)DxfCode.HardOwnershipId);
                foreach (object parameterId in parameterIds)
                {
                    ObjectId objId = (ObjectId)parameterId;
                    if (objId.ObjectClass.Name == "AcDbBlockVisibilityParameter")
                    {
                        var visibilityParam = objId.acdbEntGetTypedVals();
                        var enumerator = visibilityParam.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.TypeCode == 303)
                            {
                                string group = (string)enumerator.Current.Value;
                                enumerator.MoveNext();
                                int nbEntitiesInGroup = (int)enumerator.Current.Value;
                                var entities = new ObjectIdCollection();
                                for (int i = 0; i < nbEntitiesInGroup; ++i)
                                {
                                    enumerator.MoveNext();
                                    entities.Add((ObjectId)enumerator.Current.Value);
                                }
                                groups.Add(group, entities);
                            }
                        }
                        break;
                    }
                }
                return groups;
            }
        }
    }
}
