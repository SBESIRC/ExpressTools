using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcadException = Autodesk.AutoCAD.Runtime.Exception;

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
                if (ThMirrorEngine.Instance.Targets.ContainsKey(entity.ObjectId))
                {
                    TransformBy(ThMirrorEngine.Instance.Targets[entity.ObjectId], transform);
                }
            }
        }

        private void TransformBy(ThMirrorData mirrorData, Matrix3d transform)
        {
            // 处理子实体
            var unhandledObjs = new DBObjectCollection();
            foreach (DBObject obj in mirrorData.blockEntities)
            {
                try
                {
                    var entity = obj as Entity;
                    entity.TransformBy(transform);
                }
                catch(AcadException e)
                {
                    // 若文字未找到指定的字体，则会抛出eMissingSymbolTableRec异常
                    // 对于这样的文字，本身在CAD中就无法显示，这里我们正好可以剔除掉这些文字
                    if (e.ErrorStatus == ErrorStatus.MissingSymbolTableRecord)
                    {
                        unhandledObjs.Add(obj);
                    }
                }
            }

            // 剔除掉未处理子实体
            foreach(DBObject obj in unhandledObjs)
            {
                mirrorData.blockEntities.Remove(obj);
            }

            // 处理嵌套块
            foreach (var item in mirrorData.nestedBlockReferences)
            {
                TransformBy(item, transform);
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
