using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class ShowTools
    {
        /// <summary>
        /// 实体集体高亮
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ents"></param>
        public static void Highlight<T>(this IEnumerable<T> ents) where T : Entity
        {
            foreach (var item in ents)
            {
                item.Highlight();
            }
        }
    }
}
