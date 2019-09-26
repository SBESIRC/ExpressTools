using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using THColumnInfo.Controller;

namespace THColumnInfo
{
    public class SearchFieldVM: CNotifyPropertyChange
    {
       public SearchFields m_UIModel { get; set; }
       public UIController m_UIController { get; set; }
        public SearchFieldVM(SearchFields searchFields)
        {
            m_UIModel = searchFields;
            if(m_UIController==null)
            {
                m_UIController = new UIController(m_UIModel);
            }
            this._layerNameList = CadOperation.GetLayerNameList();
        }
        private List<string> _layerNameList = new List<string>();

        public List<string> LayerNameList
        {
            get
            {
                return _layerNameList;
            }
            set
            {
                _layerNameList = value;
                this.NotifyPropertyChange("LayerNameList");
            }
        }
    }
}
