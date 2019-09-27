using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;


namespace ThXClip
{
    public class WipeOutInfo
    {
        /// <summary>
        /// WipeOut Id
        /// </summary>
        public ObjectId Id { get; set; } = ObjectId.Null;
        /// <summary>
        /// WipeOut 所在块的嵌套顺序（从里到外）
        /// </summary>
        public List<ObjectId> NestedBlockIds { get; set; } = new List<ObjectId>();

        /// <summary>
        /// WipeOut 外围边界点
        /// </summary>
        public Point2dCollection Pts { get; set; } =  new Point2dCollection();
        /// <summary>
        /// 所在的块Id
        /// </summary>
        public ObjectId BlkId { get; set; } = ObjectId.Null; 
    }
}
