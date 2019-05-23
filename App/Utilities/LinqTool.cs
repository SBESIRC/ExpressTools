using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class LinqTool
    {
        //让IEnumerable<T>也可以使用foreach
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> func)
        {
            foreach (var item in source)
                func(item);
        }


        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> from)
        {
            ObservableCollection<T> to = new ObservableCollection<T>();
            foreach (var f in from)
            {
                to.Add(f);
            }
            return to;
        }


        //自定义找某个属性为最大的那个元素
        public static TElement MaxElement<TElement, TData>(
          this IEnumerable<TElement> source,
          Func<TElement, TData> selector)
          where TData : IComparable<TData>
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (selector == null)
                throw new ArgumentNullException("selector");

            Boolean firstElement = true;
            TElement result = default(TElement);
            TData maxValue = default(TData);
            foreach (TElement element in source)
            {
                var candidate = selector(element);
                if (firstElement || (candidate.CompareTo(maxValue) > 0))
                {
                    firstElement = false;
                    maxValue = candidate;
                    result = element;
                }
            }
            return result;
        }
    }
}
