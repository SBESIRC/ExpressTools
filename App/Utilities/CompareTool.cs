using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{

        public class CompareElemnet<T> : IEqualityComparer<T>
        {
            public Func<T, T, bool> Func { get; set; }
            public bool Equals(T x, T y)
            {
                //xList.Intersect(yList).Count() > 0
                return Func(x, y);
            }
            public int GetHashCode(T obj)
            {
                return 1;
            }

            public CompareElemnet(Func<T, T, bool> func)
            {
                this.Func = func;
            }

        }
  
}
