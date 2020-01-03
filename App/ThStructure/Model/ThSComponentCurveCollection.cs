using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThStructure.Model
{
    public class ThSComponentCurve
    {
        public Polyline Geometry { get; }
        public ThSComponentCurve(Polyline curve)
        {
            this.Geometry = curve;
        }
    }

    public class ThSComponentCurveCollection : IList<ThSComponentCurve>
    {
        private readonly List<ThSComponentCurve> _curves = new List<ThSComponentCurve>();

        public ThSComponentCurve this[int index]
        {
            get { return _curves[index]; }
            set { _curves[index] = value; }
        }

        public int Count
        {
            get { return _curves.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(ThSComponentCurve item)
        {
            _curves.Add(item);
        }

        public void Clear()
        {
            _curves.Clear();
        }

        public bool Contains(ThSComponentCurve item)
        {
            return _curves.Contains(item);
        }

        public void CopyTo(ThSComponentCurve[] array, int arrayIndex)
        {
            _curves.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ThSComponentCurve> GetEnumerator()
        {
            foreach (ThSComponentCurve seg in _curves) yield return seg;
        }

        public int IndexOf(ThSComponentCurve item)
        {
            return _curves.IndexOf(item);
        }

        public void Insert(int index, ThSComponentCurve item)
        {
            _curves.Insert(index, item);
        }

        public bool Remove(ThSComponentCurve item)
        {
            return _curves.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _curves.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
