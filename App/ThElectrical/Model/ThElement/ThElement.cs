using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ThElectrical.Model.ThElement
{
    public delegate void NotifyEventHandler(object sender);

    public class ThElement : INotifyPropertyChanged
    {
        public NotifyEventHandler NotifyEvent;//观察者通知

        public event PropertyChangedEventHandler PropertyChanged;//属性更改通知

        /// <summary>
        /// 属性发生改变时调用该方法发出通知
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObjectId ElementId { get; set; }

        public ThElement(ObjectId id)
        {
            this.ElementId = id;
        }

        #region 新增对订阅号列表的维护操作
        public void AddObserver(NotifyEventHandler ob)
        {
            NotifyEvent += ob;
        }
        public void RemoveObserver(NotifyEventHandler ob)
        {
            NotifyEvent -= ob;
        }

        #endregion

        public void Update()
        {
            if (NotifyEvent != null)
            {
                NotifyEvent(this);
            }
        }

        /// <summary>
        /// 将更新后的值写入模型空间
        /// </summary>
        public virtual void UpdateToDwg()
        {

        }

    }
}
