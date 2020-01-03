using System.Collections;
using System.Collections.Generic;

namespace ThStructure.Model
{
    // 组件参数
    public class ThSComponentParameter
    {
        public string Key { get; set; }
        public object Value { get; set; }

        public ThSComponentParameter(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    // 组件参数集合
    public class ThSComponentParameterCollection : IList<ThSComponentParameter>
    {
        private List<ThSComponentParameter> _parameters = new List<ThSComponentParameter>();

        public ThSComponentParameter this[int index]
        {
            get { return _parameters[index]; }
            set { _parameters[index] = value; }
        }

        public int Count
        {
            get { return _parameters.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(ThSComponentParameter item)
        {
            _parameters.Add(item);
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        public bool Contains(ThSComponentParameter item)
        {
            return _parameters.Contains(item);
        }

        public void CopyTo(ThSComponentParameter[] array, int arrayIndex)
        {
            _parameters.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ThSComponentParameter> GetEnumerator()
        {
            foreach (ThSComponentParameter seg in _parameters) yield return seg;
        }

        public int IndexOf(ThSComponentParameter item)
        {
            return _parameters.IndexOf(item);
        }

        public void Insert(int index, ThSComponentParameter item)
        {
            _parameters.Insert(index, item);
        }

        public bool Remove(ThSComponentParameter item)
        {
            return _parameters.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
