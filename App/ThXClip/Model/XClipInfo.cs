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
        /// Xclip默认保留内部
        /// </summary>
        public bool KeepInternal { get; set; } = false;
        /// <summary>
        /// 块名
        /// </summary>
        public string BlockName { get; set; } = "";
        /// <summary>
        /// 块路径
        /// </summary>
        public List<string> BlockPath { get; set; } = new List<string>();
    }
}
