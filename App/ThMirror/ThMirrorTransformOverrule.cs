using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThMirror
{
    public class ThMirrorTransformOverrule : TransformOverrule
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly ThMirrorTransformOverrule instance = new ThMirrorTransformOverrule();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static ThMirrorTransformOverrule() { }
        internal ThMirrorTransformOverrule() { }
        public static ThMirrorTransformOverrule Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        public override bool IsApplicable(RXObject overruledSubject)
        {
            throw new NotImplementedException();
        }

        public override void TransformBy(Entity entity, Matrix3d transform)
        {
            base.TransformBy(entity, transform);
            if (entity.ObjectId.IsValid)
            {
                foreach(var mirrorData in ThMirrorEngine.Instance.Targets)
                {
                    if (mirrorData.blockRefenceId != entity.ObjectId)
                    {
                        continue;
                    }

                    foreach (DBObject obj in mirrorData.blockEntities)
                    {
                        if (obj is Entity ent)
                        {
                            ent.TransformBy(transform);
                        }
                    }
                }
            }
        }

        public override void Explode(Entity entity, DBObjectCollection entitySet)
        {
            base.Explode(entity, entitySet);
        }

        public override bool CloneMeForDragging(Entity entity)
        {
            return base.CloneMeForDragging(entity);
        }

        public override Entity GetTransformedCopy(Entity entity, Matrix3d transform)
        {
            return base.GetTransformedCopy(entity, transform);
        }

        public override bool HideMeForDragging(Entity entity)
        {
            return base.HideMeForDragging(entity);
        }
    }
}
