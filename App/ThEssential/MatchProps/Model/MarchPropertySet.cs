using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThEssential.MatchProps
{
    public class MarchPropertySet : INotifyPropertyChanged
    {
        public MarchPropertySet()
        {
        }
        private bool layerOp=false;
        private bool colorOp = false;
        private bool lineTypeOp = false;
        private bool lineWeightOp = false;
        private bool textSizeOp = false;
        private bool textContentOp = false;
        private bool textDirectionOp = false;
        private bool acadDefaultConfigOp=true;
        private bool editAliasOp = false;
        public bool LayerOp
        {
            get { return layerOp; }
            set
            {
                layerOp = value;
                OnPropertyChanged("LayerOp");
            }
        }
        public bool ColorOp
        {
            get { return colorOp; }
            set
            {
                colorOp = value;
                OnPropertyChanged("ColorOp");
            }
        }
        public bool LineTypeOp
        {
            get { return lineTypeOp; }
            set
            {
                lineTypeOp = value;
                OnPropertyChanged("LineTypeOp");
            }
        }
        public bool LineWeightOp
        {
            get { return lineWeightOp; }
            set
            {
                lineWeightOp = value;
                OnPropertyChanged("LineWeightOp");
            }
        }
        public bool TextSizeOp
        {
            get { return textSizeOp; }
            set
            {
                textSizeOp = value;
                OnPropertyChanged("TextSizeOp");
            }
        }
        public bool TextContentOp
        {
            get { return textContentOp; }
            set
            {
                textContentOp = value;
                OnPropertyChanged("TextContentOp");
            }
        }
        public bool TextDirectionOp
        {
            get { return textDirectionOp; }
            set
            {
                textDirectionOp = value;
                OnPropertyChanged("TextDirectionOp");
            }
        }
        public bool AcadDefaultConfigOp
        {
            get { return acadDefaultConfigOp; }
            set
            {
                acadDefaultConfigOp = value;
                OnPropertyChanged("AcadDefaultConfigOp");
            }
        }
        public bool EditAliasOp
        {
            get { return editAliasOp; }
            set
            {
                editAliasOp = value;
                OnPropertyChanged("EditAliasOp");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void Read()
        {
            this.layerOp = Properties.Settings.Default.LayerOp;
            this.colorOp = Properties.Settings.Default.ColorOp;
            this.lineTypeOp = Properties.Settings.Default.LineTypeOp;
            this.lineWeightOp = Properties.Settings.Default.LineWeightOp;
            this.textSizeOp = Properties.Settings.Default.TextSizeOp;
            this.textContentOp = Properties.Settings.Default.TextContentOp;
            this.textDirectionOp = Properties.Settings.Default.TextDirectionOp;
            this.acadDefaultConfigOp = Properties.Settings.Default.AcadDefaultConfigOp;
            this.editAliasOp = Properties.Settings.Default.EditAliasOp;
        }
        public void Save()
        {
            Properties.Settings.Default.LayerOp = this.layerOp;
            Properties.Settings.Default.ColorOp = this.colorOp;
            Properties.Settings.Default.LineTypeOp = this.lineTypeOp;
            Properties.Settings.Default.LineWeightOp = this.lineWeightOp;
            Properties.Settings.Default.TextSizeOp = this.textSizeOp;
            Properties.Settings.Default.TextContentOp = this.textContentOp;
            Properties.Settings.Default.TextDirectionOp = this.textDirectionOp;
            Properties.Settings.Default.AcadDefaultConfigOp = this.acadDefaultConfigOp;
            Properties.Settings.Default.EditAliasOp = this.editAliasOp;
            Properties.Settings.Default.Save();
        }
    }
}
