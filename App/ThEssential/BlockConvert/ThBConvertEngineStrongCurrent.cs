using System;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.BlockConvert
{
    public class ThBConvertEngineStrongCurrent : ThBConvertEngine
    {
        public override void MatchProperties(ObjectId blkRef, ThBConvertBlockReference source)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                using (var lines = new ObjectIdCollection())
                using (var texts = new ObjectIdCollection())
                {
                    // 打开块引用
                    var blockReference = acadDatabase.Element<BlockReference>(blkRef, true);

                    // 将块引用炸开，获取炸开后的文字对象
                    void handler(object s, ObjectEventArgs e)
                    {
                        if (e.DBObject is DBText text)
                        {
                            texts.Add(e.DBObject.ObjectId);
                        }
                        else if (e.DBObject is Line line)
                        {
                            lines.Add(e.DBObject.ObjectId);
                        }
                    }
                    acadDatabase.Database.ObjectAppended += handler;
                    blockReference.ExplodeToOwnerSpace();
                    acadDatabase.Database.ObjectAppended -= handler;

                    // 将源块引用的属性填充在文字中
                    FillProperties(texts, source);

                    // 设置表框线
                    bool isFirePowerSupply = source.IsFirePowerSupply();
                    foreach (ObjectId line in lines)
                    {
                        acadDatabase.Element<Line>(line, true).ColorIndex = isFirePowerSupply ? 1 : 256;
                    }

                    // 删除块引用
                    blockReference.Erase();
                }
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
                blockReference.Position = srcBlockReference.Position;
            }
        }

        /// <summary>
        /// 将源块引用的属性“填充”到表格中
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="source"></param>
        private void FillProperties(ObjectIdCollection objs, ThBConvertBlockReference source)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach (ObjectId obj in objs)
                {
                    var text = acadDatabase.Element<DBText>(obj, true);
                    if (text.TextString == ThBConvertCommon.PROPERTY_LOAD_NUMBER)
                    {
                        // 负载编号：“设备符号"&"-"&"楼层-编号”
                        text.TextString = ThBConvertUtils.LoadSN(source);
                    }
                    else if (text.TextString == ThBConvertCommon.PROPERTY_POWER_QUANTITY)
                    {
                        // 电量：“电量”
                        text.TextString = ThBConvertUtils.LoadPower(source);
                    }
                    else if (text.TextString == ThBConvertCommon.PROPERTY_LOAD_USAGE)
                    {
                        // 负载用途：“负载用途”
                        text.TextString = ThBConvertUtils.LoadUsage(source);
                    }
                    else
                    {
                        // 未识别的文字，忽略
                    }
                }
            }
        }
    }
}
