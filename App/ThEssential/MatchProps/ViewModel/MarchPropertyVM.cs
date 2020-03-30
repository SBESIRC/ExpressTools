using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.IO;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThEssential.MatchProps
{
    public class MarchPropertyVM : INotifyPropertyChanged
    {
        private MarchPropertySet marchPropertySet;

        private ICommand okCommand;
        private ICommand cancelCommand;
        private ICommand mouseRightClickCommand;

        private bool executed=false;
        /// <summary>
        /// 是否继续执行
        /// </summary>
        public bool Executed
        {
            get { return executed; }
            set { executed = true; }
        }

        public MarchPropertyVM()
        {
            this.marchPropertySet = new MarchPropertySet();
            this.marchPropertySet.Read();
        }

        public MarchPropertySet MarchPropSet
        {
            get
            {
                return marchPropertySet;
            }
            set
            {
                marchPropertySet = value;
                OnPropertyChanged("MarchPropSet");
            }
        }
        public Window Owner { get; set; }
        #region----------执行命令----------
        public ICommand OkCommand => okCommand ?? (okCommand = new DelegateCommand(param =>
         {
             Confirm();
         }));

        public ICommand CancelCommand => cancelCommand ?? (cancelCommand = new DelegateCommand(param =>
        {
            Cancel();
        }));
        public ICommand MouseRightClickCommand => mouseRightClickCommand ?? (mouseRightClickCommand = new DelegateCommand(param =>
        {
            MouseRightClick();
        }));
        /// <summary>
        /// 确定按钮
        /// </summary>
        public void Confirm()
        {
            this.executed = true;
            marchPropertySet.Save();
            EditAlias();
            Owner.Close();
        }
        private void EditAlias()
        {
            try
            {
                string acadCommandName = "*MATCHPROP";
                string thCommandName = "*THMA";
                string roaming = (string)AcadApp.GetSystemVariable("ROAMABLEROOTPREFIX");
                string acadPgpFilePath = roaming + "Support\\acad.pgp";
                FileInfo fi = new FileInfo(acadPgpFilePath);
                if (!fi.Exists)
                {
                    return;
                }
                List<string> allLines = new List<string>();
                StreamReader sr = File.OpenText(acadPgpFilePath);
                string nextLine;
                while ((nextLine = sr.ReadLine()) != null)
                {
                    allLines.Add(nextLine);
                }
                sr.Close();
                for (int i = 0; i < allLines.Count; i++)
                {
                    string currentLineContent = allLines[i];
                    if (currentLineContent.ToUpper().IndexOf(acadCommandName) >= 0)
                    {
                        if (this.marchPropertySet.EditAliasOp)
                        {
                            currentLineContent= currentLineContent.ToUpper().Replace(acadCommandName, thCommandName);
                        }
                    }
                    else if (currentLineContent.ToUpper().IndexOf(thCommandName) >= 0)
                    {
                        if (!this.marchPropertySet.EditAliasOp)
                        {
                            currentLineContent=currentLineContent.ToUpper().Replace(thCommandName, acadCommandName);
                        }
                    }
                    allLines[i] = currentLineContent;
                }
                FileInfo pgpFile = new FileInfo(acadPgpFilePath);
                StreamWriter sw = pgpFile.CreateText();
                foreach (var s in allLines)
                {
                    sw.WriteLine(s);
                }
                sw.Close();
            }
            catch(System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message,"读写acad.pgp");
            }
        }
        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel()
        {
            this.executed = false;
            Owner.Close();
        }
        /// <summary>
        /// 鼠标右击
        /// </summary>
        void MouseRightClick()
        {

        }
        #endregion
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
