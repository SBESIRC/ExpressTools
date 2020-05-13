using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ThSitePlan
{
    public class LayerDataModel
    {
        public string Name { get; set; }

        public string ID { get; set; }

        public Image DeleteImg
        {
            get
            {
                return this.GetImg();
            }
        }

        private Bitmap GetImg()
        {
            return Properties.Resources.删除_细;
        }


        /// <summary>
        /// 重写ToSring方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name.Trim();
        }

        /// <summary>
        /// 主键
        /// </summary>
        public object 主键
        {
            get { return this.ID; }
        }

        /// <summary>
        /// 重载==
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator ==(LayerDataModel A, LayerDataModel B)
        {
            if ((object)A == null && (object)B == null)
            {
                return true;
            }
            else if ((object)A == null || (object)B == null)
            {
                return false;
            }
            return A.ID.Trim() == B.ID.Trim();
        }

        /// <summary>
        /// 重载!=
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool operator !=(LayerDataModel A, LayerDataModel B)
        {
            if ((object)A == null && (object)B == null)
            {
                return false;
            }
            else if ((object)A == null || (object)B == null)
            {
                return true;
            }
            return A.ID.Trim() != B.ID.Trim();
        }

        /// <summary>
        /// 重写Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is LayerDataModel)) { return false; }
            var tmp = obj as LayerDataModel;
            return tmp == this;
        }

        public override int GetHashCode()
        {
            var _HashCode = -465313509;
            _HashCode = _HashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            _HashCode = _HashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ID);
            _HashCode = _HashCode * -1521134295 + EqualityComparer<Image>.Default.GetHashCode(DeleteImg);
            _HashCode = _HashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(主键);
            return _HashCode;
        }
    }
}
