using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThElectricalSysDiagram
{
    public abstract class ThDraw
    {
        public List<ThElement> Elements { get; set; }//转换元素
        public abstract List<ThElement> GetElements();//获取要转换的元素
        public abstract void ImportRule();//导入规则
        public abstract void Deal();//处理转换图形

        //删除源图形
        public void Erase()
        {
            using (var currentDb = AcadDatabase.Active())
            {
                //将源对象删除
                currentDb.ModelSpace.OfType<BlockReference>().Join(this.Elements, b => b.ObjectId, ele => ele.ElementId, (b, ele) => b).UpgradeOpen().ForEach(b => b.Erase());
            }
        }

        //配置规则条件函数
        protected abstract Func<ThElement, string> InfoFunc();

        /// <summary>
        /// 尝试fun的导入规则
        /// </summary>
        public void Import()
        {
            using (var currentDb = AcadDatabase.Active())
            {
                using (var sourceDb = AcadDatabase.Open(ThElectricalTask.filePath, DwgOpenMode.ReadOnly))
                {
                    //打开外部库的块表记录，根据上面求出的要进行转换的块名,找出其中需要导入的记录信息，注意去重
                    var ids = sourceDb.Blocks.Join(this.Elements, btr => btr.Name, InfoFunc(), (btr, info) => btr.ObjectId).Distinct();

                    //从源数据库向目标数据库复制块表记录
                    sourceDb.Database.WblockCloneObjects(new ObjectIdCollection(ids.ToArray()), currentDb.Database.BlockTableId, new IdMapping(), DuplicateRecordCloning.Replace, false);
                }
            }
        }
    }
}
