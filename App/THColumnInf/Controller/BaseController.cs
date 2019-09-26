using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THColumnInfo
{
    public class BaseController
    {
        public BaseController(CNotifyPropertyChange model)
        {
            m_model = model;
        }
        private CNotifyPropertyChange m_model = null;
        public CNotifyPropertyChange Model
        {
            get { return m_model; }
        }
    }
}
