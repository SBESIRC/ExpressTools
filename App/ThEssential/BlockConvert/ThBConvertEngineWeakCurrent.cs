using System;
using Linq2Acad;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.BlockConvert
{
    public class ThBConvertEngineWeakCurrent : ThBConvertEngine
    {
        public override void MatchProperties(ObjectId blkRef, ThBConvertBlockReference srcBlockReference)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                acadDatabase.Element<BlockReference>(blkRef, true).LayerId = ThBConvertDbUtils.BlockLayer();
            }

        }

        public override void SetDatbaseProperties(ObjectId blkRef, ThBConvertBlockReference srcBlockReference)
        {
            throw new NotImplementedException();
        }

        public override void TransformBy(ObjectId blkRef, ThBConvertBlockReference srcBlockReference)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var blockReference = acadDatabase.Element<BlockReference>(blkRef, true);
                blockReference.TransformBy(srcBlockReference.BlockTransform);
                // 额外调整旋转角度
                double rotation = srcBlockReference.Rotation;
                if (rotation > Math.PI / 2 && rotation <= Math.PI * 3 / 2)
                {
                    blockReference.TransformBy(Matrix3d.Rotation(Math.PI, Vector3d.ZAxis, blockReference.Position));
                }
            }
        }
    }
}
