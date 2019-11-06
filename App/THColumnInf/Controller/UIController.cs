using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThColumnInfo
{
    public class UIController: BaseController
    {
        public UIController(CNotifyPropertyChange model):base(model)
        {
            btnOK_Clicked = new DelegateCommand();
            btnOK_Clicked.executeAction = new Action<object>(btnOKClicked);
        }
        public DelegateCommand btnOK_Clicked { get; set; }

        private void btnOKClicked(object obj)
        {
            System.Windows.Forms.MessageBox.Show("设置成功");
        }
    }
}
