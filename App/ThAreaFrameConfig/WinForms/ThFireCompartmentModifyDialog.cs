using System;
using System.Data;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DevExpress.XtraEditors;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThFireCompartmentModifyDialog : XtraForm
    {
        public Int16? Storey
        {
            get
            {
                if (Int16.TryParse(textBox_storey.Text, out Int16 storey))
                {
                    return storey;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool? SelfExtinguishingSystem
        {
            get
            {
                switch(comboBox_self_extinguishing_system.SelectedIndex)
                {
                    case 0:
                        return true;
                    case 1:
                        return false;
                    default:
                        return null;
                }
            }
        }

        public ThFireCompartmentModifyDialog()
        {
            InitializeComponent();
        }
    }
}
