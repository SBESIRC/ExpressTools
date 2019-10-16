using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThXClip
{
    /// <summary>
    /// 块炸开后的物体结构顺序
    /// </summary>
    public class DraworderInfo
    {
        /// <summary>
        /// 提交到模型空间的Id
        /// </summary>
        public ObjectId Id { get; set; } = ObjectId.Null;
        /// <summary>
        /// 绘制 DrawOrder
        /// </summary>
        public int DrawIndex { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName { get; set; } = "";
        /// <summary>
        /// 从块炸开后的表格中来获取
        /// </summary>
        public DBObjectCollection DbObjs { get; set; } = new DBObjectCollection();
        /// <summary>
        /// 所属块Id
        /// </summary>
        public ObjectId BlockId { get; set; } = ObjectId.Null;
        /// <summary>
        /// 所在块的块名
        /// </summary>
        public string BlockName { get; set; } = "";
        /// <summary>
        /// 块路径
        /// </summary>
        public List<string> BlockPath { get; set; } = new List<string>();
    }
    public class EntInf
    {
        public Entity Ent { get; set; }
        public string BlockName { get; set; } = "";
        /// <summary>
        /// 块路径
        /// </summary>
        public List<string> BlockPath { get; set; } = new List<string>();
    }
}
