using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Engine
{
    public abstract class ThModeltRecognitionEngine
    {
        /// <summary>
        /// 从图纸中提取出来的对象的集合
        /// </summary>
        public abstract List<ThModelElement> Elements { get; set; }

        /// <summary>
        /// 从一个模型元素内提取对象
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public abstract bool Acquire(ThModelElement element);

        /// <summary>
        /// 从图纸的一个区域内提取对象
        /// </summary>
        /// <param name="database"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public abstract bool Acquire(Database database, Polyline floor, ObjectId frame);

        /// <summary>
        /// 从图纸的指定框线提取对象
        /// </summary>
        /// <param name="database"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public abstract bool Acquire(Database database, Polyline floor, ObjectIdCollection frames);


        /// <summary>
        /// 从图纸的指定框线提取对象
        /// </summary>
        /// <param name="database"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public abstract bool Acquire(Database database, Polyline floor, DBObjectCollection frames);
    }
}
