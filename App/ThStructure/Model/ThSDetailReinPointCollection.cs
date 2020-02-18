using System;
using System.Collections;
using System.Collections.Generic;

namespace ThStructure.Model
{
    public class ThSDetailReinPointCollection : IList<ThSDetailReinPoint>
    {
        private readonly List<ThSDetailReinPoint> _points = new List<ThSDetailReinPoint>();

        public ThSDetailReinPoint this[int index]
        {
            get { return _points[index]; }
            set { _points[index] = value; }
        }

        public int Count
        {
            get { return _points.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(ThSDetailReinPoint item)
        {
            _points.Add(item);
        }

        public void Clear()
        {
            _points.Clear();
        }

        public bool Contains(ThSDetailReinPoint item)
        {
            return _points.Contains(item);
        }

        public void CopyTo(ThSDetailReinPoint[] array, int arrayIndex)
        {
            _points.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ThSDetailReinPoint> GetEnumerator()
        {
            foreach (ThSDetailReinPoint seg in _points) yield return seg;
        }

        public int IndexOf(ThSDetailReinPoint item)
        {
            return _points.IndexOf(item);
        }

        public void Insert(int index, ThSDetailReinPoint item)
        {
            _points.Insert(index, item);
        }

        public bool Remove(ThSDetailReinPoint item)
        {
            return _points.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _points.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
