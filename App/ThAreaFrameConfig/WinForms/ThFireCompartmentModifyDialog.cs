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

        private void textBox_storey_Validating(object sender, CancelEventArgs e)
        {
            if (!ValidStorey(textBox_storey.Text))
            {
                // Cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                textBox_storey.Select(0, textBox_storey.Text.Length);

                // Set the ErrorProvider error with the text to display. 
                this.errorProvider1.SetError(textBox_storey, "请输入正确的楼层格式");
            }
        }

        private void textBox_storey_Validated(object sender, EventArgs e)
        {
            // If all conditions have been met, clear the ErrorProvider of errors.
            errorProvider1.SetError(textBox_storey, "");
        }

        private bool ValidStorey(string storey)
        {
            var floors = new List<int>();

            // 匹配数字
            string pattern = @"^-?[0-9]+$";
            Match m = Regex.Match(storey, pattern);
            while (m.Success)
            {
                floors.Add(Int16.Parse(m.Value));

                m = m.NextMatch();
            }

            return floors.Count > 0;
        }
    }
}
