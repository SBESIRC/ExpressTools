using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThSitePlan.UI
{
    public partial class fmMobilePanel : Form
    {


        int m_MagniFication = 2;
        Point m_p = Control.MousePosition;
        const int m_ImgWidth = 120;
        const int m_ImgHeight = 75;
        ///119, 74
        public fmMobilePanel()
        {
            InitializeComponent();
            //this.TopMost = true;
 
        
        }

        public void MoveMagnify(Color _RGB)
        {

            Bitmap _Bitmap = new Bitmap(m_ImgWidth / m_MagniFication, m_ImgHeight / m_MagniFication);
            Graphics _Graphics = Graphics.FromImage(_Bitmap);
            _Graphics.CopyFromScreen(
                     new Point(Cursor.Position.X - m_ImgWidth / (2 * m_MagniFication),
                               Cursor.Position.Y - m_ImgHeight / (2 * m_MagniFication)),
                     new Point(0, 0),
                     new Size(m_ImgWidth / m_MagniFication, m_ImgHeight / m_MagniFication)
                     );
            IntPtr _IntPtr = _Graphics.GetHdc();
            _Graphics.ReleaseHdc(_IntPtr);
            this.PanBG.BackgroundImage = (Image)_Bitmap;
            this.PanBG.BackgroundImageLayout = ImageLayout.Zoom;
            //labRGB.Text = _RGB;
            ColorPEdit.EditValue = _RGB;
        }

        public void MovePanel()
        {
            Top = MousePosition.Y + 20;
            Left = MousePosition.X;
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
