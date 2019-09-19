using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
 
namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    /// <summary>
    /// Provides the Offset() extension method for the Polyline type
    /// </summary>
    public static class ThPolylineExtension
    {
        /// <summary>
        /// Enumeration of offset side options
        /// </summary>
        public enum OffsetSide
        {
            In, Out, Left, Right, Both
        }

        /// <summary>
        /// Offset the source polyline to specified side(s).
        /// </summary>
        /// <param name="source">The polyline to be offseted.</param>
        /// <param name="offsetDist">The offset distance.</param>
        /// <param name="side">The offset side(s).</param>
        /// <returns>A polyline sequence resulting from the offset of the source polyline.</returns>
        public static IEnumerable<Polyline> Offset(this Polyline source, double offsetDist, OffsetSide side)
        {
            offsetDist = Math.Abs(offsetDist);
            using (var plines = new DisposableSet<Polyline>())
            {
                IEnumerable<Polyline> offsetRight = source.GetOffsetCurves(offsetDist).Cast<Polyline>();
                plines.AddRange(offsetRight);
                IEnumerable<Polyline> offsetLeft = source.GetOffsetCurves(-offsetDist).Cast<Polyline>();
                plines.AddRange(offsetLeft);
                double areaRight = offsetRight.Select(pline => pline.Area).Sum();
                double areaLeft = offsetLeft.Select(pline => pline.Area).Sum();
                switch (side)
                {
                    case OffsetSide.In:
                        return plines.RemoveRange(
                           areaRight < areaLeft ? offsetRight : offsetLeft);
                    case OffsetSide.Out:
                        return plines.RemoveRange(
                           areaRight < areaLeft ? offsetLeft : offsetRight);
                    case OffsetSide.Left:
                        return plines.RemoveRange(offsetLeft);
                    case OffsetSide.Right:
                        return plines.RemoveRange(offsetRight);
                    case OffsetSide.Both:
                        plines.Clear();
                        return offsetRight.Concat(offsetLeft);
                    default:
                        return null;
                }
            }
        }
    }

    public interface IDisposableCollection<T> : ICollection<T>, IDisposable
       where T : IDisposable
    {
        void AddRange(IEnumerable<T> items);
        IEnumerable<T> RemoveRange(IEnumerable<T> items);
    }

    public class DisposableSet<T> : HashSet<T>, IDisposableCollection<T>
       where T : IDisposable
    {
        public DisposableSet()
        {
        }

        public DisposableSet(IEnumerable<T> items)
        {
            AddRange(items);
        }

        public void Dispose()
        {
            if (base.Count > 0)
            {
                System.Exception last = null;
                var list = this.ToList();
                this.Clear();
                foreach (T item in list)
                {
                    if (item != null)
                    {
                        try
                        {
                            item.Dispose();
                        }
                        catch (System.Exception ex)
                        {
                            last = last ?? ex;
                        }
                    }
                }
                if (last != null)
                    throw last;
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            base.UnionWith(items);
        }

        public IEnumerable<T> RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            base.ExceptWith(items);
            return items;
        }
    }

}


