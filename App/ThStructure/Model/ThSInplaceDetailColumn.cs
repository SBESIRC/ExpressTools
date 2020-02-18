using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThStructure.Model
{
    public class ThSInplaceDetailColumn : ThSComponentDecorator
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="component"></param>
        public ThSInplaceDetailColumn(ThSComponent component)
            :base(component)
        {
            //
        }
    }
}
