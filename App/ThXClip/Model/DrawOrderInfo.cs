using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThXClip
{
    /// <summary>
    /// DBObject 绘制顺序
    /// </summary>
    public class DrawOrderInfo
    {
        /// <summary>
        /// Id
        /// </summary>
        public ObjectId Id { get; set; } = ObjectId.Null;
        /// <summary>
        /// 绘制顺序
        /// </summary>
        public int DrawIndex { get; set; }
        /// <summary>
        /// 所附属的块Id
        /// </summary>
        public ObjectId ParentBlkId { get; set; } = ObjectId.Null;

        public string BlockName { get; set; } = "";

        public string TypeName { get; set; } = "";
    }
}
