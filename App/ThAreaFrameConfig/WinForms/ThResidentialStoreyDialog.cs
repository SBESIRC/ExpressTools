using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ThAreaFrameConfig.WinForms
{
    public partial class ThResidentialStoreyDialog : DevExpress.XtraEditors.XtraForm
    {
        public string Storey
        {
            get
            {
                if (radioButton_common.Checked)
                    return "c" + textBox_storey.Text;
                else if (radioButton_even.Checked)
                    return "o" + textBox_storey.Text;
                else if (radioButton_odd.Checked)
                    return "j" + textBox_storey.Text;
                else
                    return "";
            }
        }

        public ThResidentialStoreyDialog(string storey)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(storey))
            {
                if (storey.StartsWith("c"))
                {
                    // 所有楼层
                    radioButton_common.Checked = true;
                }
                else if (storey.StartsWith("o"))
                {
                    // 所有偶数楼层
                    radioButton_even.Checked = true;
                }
                else if (storey.StartsWith("j"))
                {
                    // 所有奇数楼层
                    radioButton_odd.Checked = true;
                }

                textBox_storey.Text = storey.Substring(1);
            }
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

            // 匹配X^Y
            string pattern = @"-?\d+[\^]-?\d+";
            Match m = Regex.Match(storey, pattern);
            while (m.Success)
            {
                int[] numbers = Array.ConvertAll(m.Value.Split('^'), int.Parse);
                floors.AddRange(Enumerable.Range(numbers[0], (numbers[1] - numbers[0] + 1)));

                m = m.NextMatch();
            }

            // 匹配X'Y
            pattern = @"-?\d+'-?\d+";
            m = Regex.Match(storey, pattern);
            while (m.Success)
            {
                int[] numbers = Array.ConvertAll(m.Value.Split('\''), int.Parse);
                floors.AddRange(numbers);

                m = m.NextMatch();
            }

            // 匹配数字
            pattern = @"-?\d+";
            m = Regex.Match(storey, pattern);
            while (m.Success)
            {
                floors.Add(Int16.Parse(m.Value));

                m = m.NextMatch();
            }

            return (floors.Count > 0);
        }
    }
}
