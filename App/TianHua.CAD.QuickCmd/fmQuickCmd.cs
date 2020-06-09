using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraRichEdit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.Publics.BaseCode;

namespace TianHua.CAD.QuickCmd
{
    public partial class fmQuickCmd : DevExpress.XtraEditors.XtraForm
    {
        public List<QuickCmdDataModel> m_ListQuickCmd { get; set; }

        public List<QuickCmdDataModel> m_ListFilter = new List<QuickCmdDataModel>();

        public List<ProductivityDataModel> m_ListProductivity = new List<ProductivityDataModel>();

        public List<QuickCmdDataModel> m_ListDelete = new List<QuickCmdDataModel>();

        public List<DictDataModel> m_ListDict { get; set; }

        public RichEditControl m_RichEdit = new RichEditControl();

        public string m_RichEditText = string.Empty;

        public string m_PgpPath { get; set; }

        public string m_Profession { get; set; }

        public fmQuickCmd()
        {
            InitializeComponent();
        }

        public void InitQuickCmd(string _Path)
        {

            m_RichEdit.LoadDocument(_Path);

            if (m_RichEdit == null || m_RichEdit.Text == string.Empty) { return; }

            m_RichEditText = m_RichEdit.Text;

            m_ListQuickCmd = new List<QuickCmdDataModel>();

            m_ListDelete = new List<QuickCmdDataModel>();

            m_ListFilter = new List<QuickCmdDataModel>();

            var _List = m_RichEdit.Text.Split('\n');

            string _OldTxt = string.Empty;

            for (int i = 0; i < _List.Count(); i++)
            {

                if (_List[i].Trim() == string.Empty) { continue; }
                _OldTxt = _List[i].Trim();

                if (_List[i].Contains(",*")) { _List[i] = _List[i].Replace(",*", ",  *"); }

                MatchCollection _Matche = Regex.Matches(_List[i], @"[^\s]+");
                if (_Matche.Count == 2)
                {
                    if (!FuncStr.NullToStr(_Matche[0]).Contains(",") || !FuncStr.NullToStr(_Matche[1]).Contains("*")) { continue; }
                    QuickCmdDataModel _QuickCmd = new QuickCmdDataModel();
                    _QuickCmd.ID = i.ToString();
                    _QuickCmd.Statu = 0;
                    _QuickCmd.OldText = _OldTxt;
                    _QuickCmd.Remarks = string.Empty;
                    _QuickCmd.ShortcutKeys = FuncStr.NullToStr(_Matche[0]).Replace(",", "");
                    _QuickCmd.ReplaceKye = FuncStr.NullToStr(_Matche[0]);
                    _QuickCmd.Cmd = FuncStr.NullToStr(_Matche[1]).Replace("*", "");
                    _QuickCmd.ReplaceCmd = FuncStr.NullToStr(_Matche[1]);
                    if (m_ListDict != null && m_ListDict.Count > 0)
                    {
                        var _Dict = m_ListDict.Find(p => FuncStr.NullToStr(p.Cmd) == FuncStr.NullToStr(_QuickCmd.Cmd));
                        if (_Dict != null) _QuickCmd.Remarks = FuncStr.NullToStr(_Dict.Remarks);


                        var _Remove = m_ListDict.Find(p => FuncStr.NullToStr(p.Cmd) == FuncStr.NullToStr(_QuickCmd.Cmd) && p.Tag != "CAD");
                        if (_Remove != null)
                        {
                            m_ListFilter.Add(_QuickCmd);
                            continue;
                        }

                    }

                    m_ListQuickCmd.Add(_QuickCmd);
                }
            }

            ReplenishCAD();

            //ColCADCmd.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            //ColCADCmd.SortMode = DevExpress.XtraGrid.ColumnSortMode.Value;

            GdcCAD.DataSource = m_ListQuickCmd.OrderBy(p => p.Cmd).OrderBy(s => s.Statu);
            GdcCAD.Refresh();



        }

        private void ReplenishCAD()
        {
            if (m_ListDict != null && m_ListDict.Count > 0 && m_ListQuickCmd != null && m_ListQuickCmd.Count > 0)
            {
                m_ListDict.ForEach(p =>
                {
                    if (FuncStr.NullToStr(p.Tag) != "CAD") { return; }
                    var _Dict = m_ListQuickCmd.Find(s => FuncStr.NullToStr(s.Cmd) == FuncStr.NullToStr(p.Cmd));
                    if (_Dict == null)
                    {
                        QuickCmdDataModel _QuickCmd = new QuickCmdDataModel();
                        _QuickCmd.ID = Guid.NewGuid().ToString();
                        _QuickCmd.Statu = 1;
                        _QuickCmd.OldText = string.Empty;
                        _QuickCmd.Remarks = p.Remarks;
                        _QuickCmd.ShortcutKeys = string.Empty;
                        _QuickCmd.ReplaceKye = string.Empty;
                        _QuickCmd.Cmd = p.Cmd;
                        _QuickCmd.ReplaceCmd = string.Empty;
                        m_ListQuickCmd.Add(_QuickCmd);
                    }
                });
            }
        }

        private void fmQuickCmd_Load(object sender, EventArgs e)
        {
            //m_PgpPath = @"C:\Users\zhangxiaohui\Desktop\acad.pgp";
            //m_Profession = "电气";
            InitForm();

        }

        private void InitForm()
        {
            var _Json = ReadTxt(System.Environment.CurrentDirectory + @"\快捷键汇总.json");
            m_ListDict = FuncJson.Deserialize<List<DictDataModel>>(_Json);

            InitQuickCmd(m_PgpPath);
            InitProductivity(m_Profession);

            if (Tab.SelectedTabPage.Name == "PageCAD")
            {
                BtnAdd.Enabled = true;
                BtnDelte.Enabled = true;

            }
            else if (Tab.SelectedTabPage.Name == "PageProductivity")
            {
                BtnAdd.Enabled = false;
                BtnDelte.Enabled = false;
            }
        }

        public void InitProductivity(string _Tag)
        {
            if (m_ListDict == null || m_ListDict.Count == 0) { return; }
            var _List = m_ListDict.FindAll(p => p.Tag == _Tag);
            if (_List == null || _List.Count == 0) { return; }
            m_ListProductivity = new List<ProductivityDataModel>();
            _List.ForEach(p =>
            {
                ProductivityDataModel _Productivity = new ProductivityDataModel();
                _Productivity.ID = p.ID;
                _Productivity.ShortcutKeys = string.Empty;
                _Productivity.Cmd = p.Cmd;
                _Productivity.Remarks = p.Remarks;
                if (m_ListFilter != null && m_ListFilter.Count > 0)
                {
                    var _Filter = m_ListFilter.Find(s => FuncStr.NullToStr(s.Cmd) == FuncStr.NullToStr(_Productivity.Cmd));
                    if (_Filter != null) _Productivity.ShortcutKeys = _Filter.ShortcutKeys;
                }
                m_ListProductivity.Add(_Productivity);
            });
            if (m_ListProductivity != null && m_ListProductivity.Count > 0)
                m_ListProductivity = m_ListProductivity.OrderBy(p => p.Cmd).ToList(); ;
            GdcProductivity.DataSource = m_ListProductivity;
            GdcProductivity.Refresh();
        }

        private void GdvCAD_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            var _Gdv = sender as GridView;
            if (_Gdv == null) { return; }
            var _QuickCmdDataModel = _Gdv.GetFocusedRow() as QuickCmdDataModel;
            if (_QuickCmdDataModel == null) { return; }
            var _FocusedColumn = _Gdv.FocusedColumn;
            if (_FocusedColumn.FieldName == "ShortcutKeys" || _FocusedColumn.FieldName == "Cmd")
            {
                if (_QuickCmdDataModel.Statu == 0)
                    _QuickCmdDataModel.Statu = 2;
            }


        }
        private void GdvProductivity_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            var _Gdv = sender as GridView;
            if (_Gdv == null) { return; }
            var _Productivity = _Gdv.GetFocusedRow() as ProductivityDataModel;
            if (_Productivity == null) { return; }
            var _FocusedColumn = _Gdv.FocusedColumn;
            if (FuncStr.NullToStr(e.Value) == string.Empty) { return; }

            if (_FocusedColumn.FieldName == "ShortcutKeys")
            {
                var _ListProductivity = m_ListProductivity.FindAll(p => FuncStr.NullToStr(p.ShortcutKeys).ToUpper() == FuncStr.NullToStr(e.Value).ToUpper() && p.ID != _Productivity.ID);
                if (_ListProductivity.Count > 0)
                {

                    e.Valid = false;
                    e.ErrorText = "快捷键冲突!";
                    return;
                }


                var _List = m_ListQuickCmd.FindAll(p => FuncStr.NullToStr(p.ShortcutKeys).ToUpper() == FuncStr.NullToStr(e.Value).ToUpper() && p.ID != _Productivity.ID);
                if (_List.Count > 0)
                {
                    e.Valid = false;
                    e.ErrorText = "快捷键冲突!";
                    return;
                }

            }
        }

        private void GdvCAD_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            var _Gdv = sender as GridView;
            if (_Gdv == null) { return; }
            var _QuickCmdDataModel = _Gdv.GetFocusedRow() as QuickCmdDataModel;
            if (_QuickCmdDataModel == null) { return; }
            var _FocusedColumn = _Gdv.FocusedColumn;
            if (FuncStr.NullToStr(e.Value) == string.Empty) { return; }
            if (_FocusedColumn.FieldName == "ShortcutKeys")
            {
                var _List = m_ListQuickCmd.FindAll(p => FuncStr.NullToStr(p.ShortcutKeys).ToUpper() == FuncStr.NullToStr(e.Value).ToUpper() && p.ID != _QuickCmdDataModel.ID);
                if (_List.Count > 0)
                {
                    e.Valid = false;
                    e.ErrorText = "快捷键冲突!";
                    return;
                }

                var _ListProductivity = m_ListProductivity.FindAll(p => FuncStr.NullToStr(p.ShortcutKeys).ToUpper() == FuncStr.NullToStr(e.Value).ToUpper() && p.ID != _QuickCmdDataModel.ID);
                if (_ListProductivity.Count > 0)
                {
                    e.Valid = false;
                    e.ErrorText = "快捷键冲突!";
                    return;
                }

            }

            if (_FocusedColumn.FieldName == "Cmd")
            {

                if (_QuickCmdDataModel.Statu == 0 && FuncStr.NullToStr(e.Value) == string.Empty)
                {
                    e.Valid = false;
                    e.ErrorText = "命令不允许为空!";
                    return;
                }

                var _List = m_ListQuickCmd.FindAll(p => FuncStr.NullToStr(p.Cmd).ToUpper() == FuncStr.NullToStr(e.Value).ToUpper() && p.ID != _QuickCmdDataModel.ID);

                if (_List.Count > 0)
                {
                    e.Valid = false;
                    e.ErrorText = "命令冲突!";
                    return;
                }
            }

        }


        private void TxtCADKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = char.ToUpper(e.KeyChar);
        }

        private void TextCADCmd_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = char.ToUpper(e.KeyChar);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            BtnAdd.Focus();
            GdvCAD.OptionsSelection.MultiSelect = false;
            var _QuickCmd = new QuickCmdDataModel();
            _QuickCmd.ID = Guid.NewGuid().ToString();
            _QuickCmd.Statu = 1;
            _QuickCmd.OldText = string.Empty;
            _QuickCmd.Remarks = string.Empty;
            _QuickCmd.ShortcutKeys = string.Empty;
            _QuickCmd.ReplaceKye = string.Empty;
            _QuickCmd.Cmd = string.Empty;
            _QuickCmd.ReplaceCmd = string.Empty;
            m_ListQuickCmd.Add(_QuickCmd);
            GdcCAD.DataSource = m_ListQuickCmd;


            GdvCAD.FocusedRowHandle = m_ListQuickCmd.Count - 1;
            GdcCAD.RefreshDataSource();

            //GdvCAD.FocusedColumn = GdvCAD.Columns["ShortcutKeys"];
            //GdvCAD.ShowEditor();
            GdvCAD.OptionsSelection.MultiSelect = true;
            GdvCAD.FocusedColumn = GdvCAD.Columns["ShortcutKeys"];
            GdvCAD.ShowEditor();
        }

        private void BtnDelte_Click(object sender, EventArgs e)
        {
            GdvCAD.PostEditor();
            var _Focus = false;
            var _Rows = GdvCAD.GetSelectedRows();
            List<QuickCmdDataModel> _ListTemp = new List<QuickCmdDataModel>();
            var _QuickCmdDataModel = GdvCAD.GetFocusedRow() as QuickCmdDataModel;
            if (_QuickCmdDataModel == null) { return; }
            if (m_ListQuickCmd.Last() == _QuickCmdDataModel) { _Focus = true; }
            if (_Rows != null && _Rows.Count() > 0)
            {
                if (XtraMessageBox.Show("当前选中数据共：" + _Rows.Count() + "条,是否删除?", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    for (int i = 0; i < _Rows.Count(); i++)
                    {
                        var _Row = GdvCAD.GetRow(_Rows[i]) as QuickCmdDataModel;

                        _ListTemp.Add(_Row);
                        m_ListDelete.Add(_Row);
                    }

                    if (_ListTemp != null && _ListTemp.Count > 0)
                    {
                        _ListTemp.ForEach(p => m_ListQuickCmd.Remove(p));
                    }

                    GdcCAD.DataSource = m_ListQuickCmd;
                    if (_Focus) { GdvCAD.FocusedRowHandle = m_ListQuickCmd.Count - 1; }
                    GdcCAD.RefreshDataSource();



                }

            }
        }

        private void BtnLeftCmd_Click(object sender, EventArgs e)
        {
            if (m_ListDict == null || m_ListDict.Count == 0) { return; }
            var _List = m_ListDict.FindAll(p => p.Tag == m_Profession || p.Tag == "CAD");
            if (_List == null || _List.Count == 0) { return; }
            _List.ForEach(p =>
            {
                var _ListQuickCmd = m_ListQuickCmd.FindAll(s => s.Cmd == p.Cmd);
                if (_ListQuickCmd != null && _ListQuickCmd.Count > 0)
                {
                    _ListQuickCmd.ForEach(s =>
                    {
                        s.ShortcutKeys = p.LefthandKeys;
                        if (s.Statu == 0)
                            s.Statu = 2;
                    });
                }

                var _ListProductivity = m_ListProductivity.FindAll(s => s.Cmd == p.Cmd);
                if (_ListProductivity != null && _ListProductivity.Count > 0)
                {
                    _ListProductivity.ForEach(s => s.ShortcutKeys = p.LefthandKeys);
                }
            });

            GdcCAD.DataSource = m_ListQuickCmd.OrderBy(p => p.Cmd).OrderByDescending(s => s.Statu);
            GdcCAD.RefreshDataSource();

            GdcProductivity.DataSource = m_ListProductivity.OrderBy(p => p.Cmd);
            GdcProductivity.RefreshDataSource();
        }

        private void BtnDefault_Click(object sender, EventArgs e)
        {
            var _Json = ReadTxt(System.Environment.CurrentDirectory + @"\快捷键汇总.json");
            m_ListDict = FuncJson.Deserialize<List<DictDataModel>>(_Json);

            InitQuickCmd(System.Environment.CurrentDirectory + @"\default.pgp");
            InitProductivity(m_Profession);

            if (Tab.SelectedTabPage.Name == "PageCAD")
            {
                BtnAdd.Enabled = true;
                BtnDelte.Enabled = true;

            }
            else if (Tab.SelectedTabPage.Name == "PageProductivity")
            {
                BtnAdd.Enabled = false;
                BtnDelte.Enabled = false;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Write(m_PgpPath.Replace("acad.pgp", "oldacad.pgp"), m_RichEditText);
            SavePgpTxt();
            Write(m_PgpPath, m_RichEditText);
            this.Close();
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            var _OpenFileDialog = new OpenFileDialog();
            _OpenFileDialog.Filter = "Pgp Files (*.Pgp)|*.Pgp";
            var _Result = _OpenFileDialog.ShowDialog();
            if (_Result == DialogResult.OK)
            {
                InitQuickCmd(_OpenFileDialog.FileName);

            }

        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog _SaveFileDialog = new SaveFileDialog();
            _SaveFileDialog.Filter = "Pgp Files(.pgp)|.pgp";
            _SaveFileDialog.RestoreDirectory = true;
            var DialogResult = _SaveFileDialog.ShowDialog();
            if (DialogResult == DialogResult.OK)
            {
                SavePgpTxt();
                var _FilePath = _SaveFileDialog.FileName.ToString();
                Write(_FilePath, m_RichEditText);
            }
        }

        private void SavePgpTxt()
        {
            GdvCAD.PostEditor();
            if (m_RichEdit == null) { m_RichEdit = new RichEditControl(); }
            m_RichEditText = m_RichEdit.Text;
            SetListQuickCmdByProductivity();
            var _ListAdd = m_ListQuickCmd.FindAll(p => p.Statu == 1);
            if (_ListAdd != null && _ListAdd.Count > 0)
            {
                _ListAdd.ForEach(p =>
                {
                    if (FuncStr.NullToStr(p.Cmd) != string.Empty && FuncStr.NullToStr(p.ShortcutKeys) != string.Empty)
                    {
                        m_RichEditText += "\r\n" + FuncStr.NullToStr(p.ShortcutKeys) + ",   *" + FuncStr.NullToStr(p.Cmd);
                    }
                });
            }

            var _ListUpdate = m_ListQuickCmd.FindAll(p => p.Statu == 2);
            if (_ListUpdate != null && _ListUpdate.Count > 0)
            {
                _ListUpdate.ForEach(p =>
                {
                    if (FuncStr.NullToStr(p.Cmd) != string.Empty && FuncStr.NullToStr(p.OldText) != string.Empty)
                    {

                        string _Str = "\r\n" + FuncStr.NullToStr(p.ShortcutKeys) + ",   *" + FuncStr.NullToStr(p.Cmd);
                        m_RichEditText = m_RichEditText.Replace(FuncStr.NullToStr(p.OldText), _Str);
                    }
                });
            }

            if (m_ListDelete != null && m_ListDelete.Count > 0)
            {
                m_ListDelete.ForEach(p =>
                {
                    if (FuncStr.NullToStr(p.Cmd) != string.Empty)
                    {

                        m_RichEditText = m_RichEditText.Replace(FuncStr.NullToStr(p.OldText), " ");
                    }
                });
            }
        }

        private void SetListQuickCmdByProductivity()
        {
            if (m_ListProductivity == null || m_ListProductivity.Count == 0 || m_ListQuickCmd == null || m_ListQuickCmd.Count == 0) { return; }
            var _List = m_ListProductivity.FindAll(p => p.ShortcutKeys != string.Empty);
            if (_List == null || _List.Count == 0) { return; }
            _List.ForEach(p =>
           {
               var _Productivity = m_ListFilter.Find(s => s.Cmd == p.Cmd);
               if (_Productivity == null)
               {
                   QuickCmdDataModel _QuickCmd = new QuickCmdDataModel();
                   _QuickCmd.ID = Guid.NewGuid().ToString();
                   _QuickCmd.Statu = 1;
                   _QuickCmd.OldText = string.Empty;
                   _QuickCmd.Remarks = p.Remarks;
                   _QuickCmd.ShortcutKeys = p.ShortcutKeys;
                   _QuickCmd.ReplaceKye = string.Empty;
                   _QuickCmd.Cmd = p.Cmd;
                   _QuickCmd.ReplaceCmd = string.Empty;
                   m_ListQuickCmd.Add(_QuickCmd);
               }
               else
               {
                   _Productivity.ShortcutKeys = p.ShortcutKeys;
                   _Productivity.Statu = 2;
                   m_ListQuickCmd.Add(_Productivity);
               }

           });

        }

        public string ReadTxt(string _Path)
        {
            try
            {
                using (StreamReader _StreamReader = File.OpenText(_Path))
                {
                    return _StreamReader.ReadToEnd();
                }
            }
            catch
            {
                return string.Empty;

            }
        }

        public static void Write(string _Path, string _PGP)
        {
            try
            {
                FileStream _FileStream = new FileStream(_Path, FileMode.Create);
                StreamWriter _StreamWriter = new StreamWriter(_FileStream);
                _StreamWriter.Write(_PGP);
                _StreamWriter.Flush();
                _StreamWriter.Close();
                _FileStream.Close();
            }
            catch
            {


            }
        }

        private void TxtFilter_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = char.ToUpper(e.KeyChar);
        }

        public void SetFilter(DevExpress.XtraGrid.Views.Grid.GridView _Gdv)
        {
            GdvCAD.OptionsView.ShowAutoFilterRow = true;
            //gdv.OptionsFilter.AllowMultiSelectInCheckedFilterPopup = true;
            foreach (DevExpress.XtraGrid.Columns.GridColumn _Item in _Gdv.Columns)
            {
                _Item.OptionsFilter.AutoFilterCondition = DevExpress.XtraGrid.Columns.AutoFilterCondition.Contains;  //筛选条件设置为包含 
                _Item.OptionsFilter.FilterPopupMode = FilterPopupMode.CheckedList;//设置为过滤是可以多选
            }
        }

        private void TxtFilter_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {

            var _FilterString = @"ShortcutKeys LIKE '%" + e.NewValue + "%'";
            _FilterString += @" OR Cmd LIKE '%" + e.NewValue + "%'";
            _FilterString += @" OR Remarks LIKE '%" + e.NewValue + "%'";

            (GdvCAD as ColumnView).ActiveFilterString = _FilterString;
            (GdvProductivity as ColumnView).ActiveFilterString = _FilterString;

            if (Tab.SelectedTabPage.Name == "PageCAD")
            {
                if (FuncStr.NullToStr(e.NewValue) == string.Empty)
                {
                    BtnAdd.Enabled = true;
                }
                else
                {
                    BtnAdd.Enabled = false;
                }

            }


        }

        private void Tab_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (e.Page.Name == "PageCAD")
            {
                BtnAdd.Enabled = true;
                BtnDelte.Enabled = true;
                if (FuncStr.NullToStr(TxtFilter.Text) != string.Empty)
                {
                    BtnAdd.Enabled = false;
                }



            }
            else if (e.Page.Name == "PageProductivity")
            {
                BtnAdd.Enabled = false;
                BtnDelte.Enabled = false;
            }
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void TxtPKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.KeyChar = char.ToUpper(e.KeyChar);
        }

        private void GdvProductivity_InvalidValueException(object sender, DevExpress.XtraEditors.Controls.InvalidValueExceptionEventArgs e)
        {

            e.ExceptionMode = DevExpress.XtraEditors.Controls.ExceptionMode.NoAction;

            this.ToolTip.ShowBeak = true;

            this.ToolTip.ShowShadow = false;

            this.ToolTip.Rounded = false;

            var _Rect = GdvProductivity.ActiveEditor.Bounds;

            var _P = _Rect.Location;

            _P.Offset(this.Location.X + _Rect.Width, this.Location.Y + _Rect.Height + 45);

            this.ToolTip.ShowHint(e.ErrorText, _P);

        }

        private void GdvCAD_InvalidValueException(object sender, InvalidValueExceptionEventArgs e)
        {

            e.ExceptionMode = DevExpress.XtraEditors.Controls.ExceptionMode.NoAction;

            this.ToolTip.ShowBeak = true;

            this.ToolTip.ShowShadow = false;

            this.ToolTip.Rounded = false;

            var _Rect = GdvCAD.ActiveEditor.Bounds;

            var _P = _Rect.Location;

            _P.Offset(this.Location.X + _Rect.Width, this.Location.Y + _Rect.Height + 45);

            this.ToolTip.ShowHint(e.ErrorText, _P);
        }

        private void BtnOld_Click(object sender, EventArgs e)
        {

            if (!File.Exists(m_PgpPath.Replace("acad.pgp", "oldacad.pgp"))) { DevExpress.XtraEditors.XtraMessageBox.Show("没有可还原的文件！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);return; }

            var _Json = ReadTxt(System.Environment.CurrentDirectory + @"\快捷键汇总.json");
            m_ListDict = FuncJson.Deserialize<List<DictDataModel>>(_Json);

            InitQuickCmd(m_PgpPath.Replace("acad.pgp", "oldacad.pgp"));
            InitProductivity(m_Profession);

            if (Tab.SelectedTabPage.Name == "PageCAD")
            {
                BtnAdd.Enabled = true;
                BtnDelte.Enabled = true;

            }
            else if (Tab.SelectedTabPage.Name == "PageProductivity")
            {
                BtnAdd.Enabled = false;
                BtnDelte.Enabled = false;
            }
        }
    }
}
