using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ThXClip
{
    public class XClipInfo
    {
        /// <summary>
        /// XClip依附的块Id
        /// </summary>
        public ObjectId AttachBlockId { get; set; } = ObjectId.Null;
        /// <summary>
        /// XClip边界点
        /// </summary>
        public Point2dCollection Pts { get; set; } = new Point2dCollection();
        /// <summary>
        /// Xclip默认修剪内部
        /// </summary>
        public bool TrimInside { get; set; } = true;
        /// <summary>
        /// 嵌套块(从里到外)
        /// </summary>
        public List<ObjectId> NestedBlockIds { get; set; } = new List<ObjectId>(); 
    }

}
