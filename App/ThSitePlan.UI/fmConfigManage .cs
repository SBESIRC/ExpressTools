using DevExpress.XtraTreeList.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TianHua.Publics.BaseCode;

using DevExpress.Utils;

using DevExpress.XtraEditors.Controls;

using DevExpress.XtraEditors.Popup;
using DevExpress.XtraEditors;
using System.Runtime.InteropServices;
using DevExpress.XtraTreeList;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using System.Collections;
using DevExpress.XtraTreeList.Columns;
using DevExpress.LookAndFeel;

namespace ThSitePlan.UI
{
    public partial class fmConfigManage : DevExpress.XtraEditors.XtraForm, IConfigManage
    {

        public List<ColorGeneralDataModel> m_ListColorGeneral { get; set; }

        public List<LayerDataModel> m_ListLayer { get; set; }

        public List<string> m_ListScript { get; set; }

        public PresenterConfigManage m_Presenter;

        public const string m_CloseUpKey = "+{F1}";

        public string m_ColorGeneralConfig { get; set; }

        public bool m_ColumnError { get; set; }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bSCan, int dwFlags, int dwExtraInfo);


        /// <summary>
        /// 显示设备上下文环境的句柄。
        /// </summary>
        private IntPtr m_Hdc = IntPtr.Zero;

        /// <summary>
        /// 指向窗口的句柄。
        /// </summary>
        private readonly IntPtr m_Wnd = IntPtr.Zero;

        public MouseHook m_MouseHook;


        fmMobilePanel m_fmMobilePanel;


        public void RessetPresenter()
        {
            if (m_Presenter != null)
            {
                this.Dispose();
                m_Presenter = null;
            }
            m_Presenter = new PresenterConfigManage(this);
        }


        public fmConfigManage()
        {

            InitializeComponent();
        }



        private void fmConfigManage_Load(object sender, EventArgs e)
        {
            RessetPresenter();

            //TreeList.AllowDrop = true;


            this.TreeList.ParentFieldName = "PID";
            this.TreeList.KeyFieldName = "ID";

            TreeList.DataSource = m_ListColorGeneral;
            this.TreeList.ExpandAll();

            TreeList.OptionsDragAndDrop.DragNodesMode = DragNodesMode.Multiple;
            this.TreeList.OptionsBehavior.ShowEditorOnMouseUp = true;
            this.TreeList.OptionsBehavior.CloseEditorOnLostFocus = false;
            this.TreeList.OptionsBehavior.KeepSelectedOnClick = false;
            this.TreeList.OptionsBehavior.SmartMouseHover = false;
            //PopupContainer.PopupControl = PopCtl;
            //Gdc.DataSource = m_ListLayer;
            //THLookUpEdit.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;




            ComBoxScript.Items.Clear();
            ComBoxScript.Items.AddRange(m_ListScript);

            m_MouseHook = new MouseHook();
            m_MouseHook.MouseMoveEvent += m_MouseHook_MouseMoveEvent;
            m_MouseHook.MouseClickEvent += m_MouseHook_MouseClickEvent;
            m_MouseHook.MouseRightClickEvent += m_MouseHook_MouseRightClickEvent;


            var _FilterString = @" Name <>  '未识别对象'";

            _FilterString += @" OR SortID  <>  0 ";

            TreeList.ActiveFilterString = _FilterString;



            //SetEditCloseUpKey();

        }

        /// <summary>
        /// 设置相关下拉编辑器快捷键激活按键
        /// </summary>
        public void SetEditCloseUpKey()
        {
            DevExpress.Utils.KeyShortcut _ShortCut = new DevExpress.Utils.KeyShortcut((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F1));
            ColorEdit.CloseUpKey = _ShortCut;
            MEdit.CloseUpKey = _ShortCut;
            ComBoxLayer.CloseUpKey = _ShortCut;
            ComBoxScript.CloseUpKey = _ShortCut;
        }


        private void TreeList_GetSelectImage(object sender, DevExpress.XtraTreeList.GetSelectImageEventArgs e)
        {
            if (e.Node == null) return;
            TreeListNode _Node = e.Node;
            var _Type = _Node.GetValue("Type");
            var _ID = _Node.GetValue("ID");
            var ImgType = _Node.GetValue("ImgType");
            if (_Type == null || _ID == null) { return; }
            if (FuncStr.NullToStr(_Type) == "1")
            {

                //var _List = m_ListColorGeneral.FindAll(p => FuncStr.NullToStr(p.PID) == FuncStr.NullToStr(_ID)
                //&& FuncStr.NullToInt(p.CAD_ScriptID) > 0);

                if (FuncStr.NullToStr(ImgType) == "1")
                    e.NodeImageIndex = 2;
                else
                    e.NodeImageIndex = 0;
            }
            else
            {
                e.NodeImageIndex = 1;
            }


        }


        private void ColorEdit_ColorChanged(object sender, EventArgs e)
        {

        }

        private void TreeList_CellValueChanged(object sender, DevExpress.XtraTreeList.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "PSD_Color")
            {
                TreeList.PostEditor();
                var _ID = FuncStr.NullToStr(e.Node.GetValue("ID"));
                var _ColorGeneral = m_ListColorGeneral.Find(p => p.ID == _ID);
                if (_ColorGeneral == null) { return; }
                Color _Color = (Color)e.Value;
                byte R = _Color.R;
                byte G = _Color.G;
                byte B = _Color.B;
                _ColorGeneral.PSD_Color = R + "," + G + "," + B;
                //e.Value = GetColor(_ColorGeneral.PSD_Color);
                TreeList.Refresh();
            }

            if (e.Column.FieldName == "CAD_Script")
            {
                TreeList.PostEditor();
                var _ID = FuncStr.NullToStr(e.Node.GetValue("ID"));
                var _PID = FuncStr.NullToStr(e.Node.GetValue("PID"));
                var _Type = FuncStr.NullToStr(e.Node.GetValue("Type"));
                if (_Type == "0")
                {
                    var _List = m_ListColorGeneral.FindAll(p => p.PID == _PID);
                    if (_List == null || _List.Count == 0) { return; }
                    var _ListTemp = _List.FindAll(s => FuncStr.NullToInt(s.CAD_ScriptID) > 0);
                    var _ColorGeneral = m_ListColorGeneral.Find(p => p.ID == _PID);
                    if (_ColorGeneral == null) { return; }
                    if (_ListTemp != null && _ListTemp.Count > 0)
                    {
                        _ColorGeneral.ImgType = "1";
                    }
                    else
                    {
                        _ColorGeneral.ImgType = "0";
                    }
                }
            }

            if (e.Column.FieldName == "Name")
            {
                TreeList.PostEditor();
                var _ID = FuncStr.NullToStr(e.Node.GetValue("ID"));
                var _PID = FuncStr.NullToStr(e.Node.GetValue("PID"));
                var _Type = FuncStr.NullToStr(e.Node.GetValue("Type"));


            }

        }

        private void ColorEdit_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if (FuncStr.NullToStr(e.Button.Tag) == "Spectroscope")
            {
                m_MouseHook.SetHook();
                Tim.Start();
                //this.TopMost = true;
                m_fmMobilePanel = new fmMobilePanel();
                m_fmMobilePanel.Show();

                m_Hdc = WinApi.GetDC(m_Wnd);
                m_fmMobilePanel.MovePanel();
            }
        }


        private void m_MouseHook_MouseClickEvent(object sender, MouseEventArgs e)
        {
            if (!m_fmMobilePanel.HasChildren)
            {
                m_fmMobilePanel.Close();
                m_MouseHook.UnHook();
                Tim.Stop();
                return;
            }
            System.Drawing.Point p = MousePosition;
            uint _Color = WinApi.GetPixel(m_Hdc, p.X, p.Y);
            byte R = WinApi.GetRValue(_Color);
            byte G = WinApi.GetGValue(_Color);
            byte B = WinApi.GetBValue(_Color);
            SetCellColor(Color.FromArgb(R, G, B));
            m_fmMobilePanel.Close();
            m_MouseHook.UnHook();
            Tim.Stop();

        }



        public void SetCellColor(Color _Color)
        {
            var _FocusedColumn = TreeList.FocusedColumn;
            if (_FocusedColumn == null || _FocusedColumn.FieldName != "PSD_Color") { return; }
            byte R = _Color.R;
            byte G = _Color.G;
            byte B = _Color.B;
            TreeList.FocusedNode.SetValue("PSD_Color", R + "," + G + "," + B);
            TreeList.Refresh();
        }



        private void m_MouseHook_MouseMoveEvent(object sender, MouseEventArgs e)
        {

            m_fmMobilePanel.MovePanel();
        }

        private void m_MouseHook_MouseRightClickEvent(object sender, MouseEventArgs e)
        {
            if (Tim.Enabled)
            {
                m_fmMobilePanel.Close();
                m_MouseHook.UnHook();
                Tim.Stop();
                this.TopMost = false;
            }
        }


        private void Tim_Tick(object sender, EventArgs e)
        {
            //Cursor = Cursors.Cross;
            System.Drawing.Point p = MousePosition;
            uint _Color = WinApi.GetPixel(m_Hdc, p.X, p.Y);
            byte R = WinApi.GetRValue(_Color);
            byte G = WinApi.GetGValue(_Color);
            byte B = WinApi.GetBValue(_Color);
            m_fmMobilePanel.MoveMagnify(Color.FromArgb(R, G, B));
            //m_fmMobilePanel.MoveMagnify(GetColor(R + "," + G + "," + B));
            m_fmMobilePanel.TopMost = true;
            //colorPickEdit1.EditValue = Color.FromArgb(R, G, B);
            //SetCellColor(Color.FromArgb(R, G, B));
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            if (keyData == Keys.Escape)
            {
                if (Tim.Enabled)
                {
                    m_fmMobilePanel.Close();
                    m_MouseHook.UnHook();
                    Tim.Stop();
                    this.TopMost = false;
                }
            }


            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void fmConfigManage_FormClosed(object sender, FormClosedEventArgs e)
        {

        }



        private void TreeList_CustomNodeCellEditForEditing(object sender, DevExpress.XtraTreeList.GetCustomNodeCellEditEventArgs e)
        {
            var _TreeList = sender as TreeList;
            if (_TreeList == null) { return; }
            var _ColorGeneral = _TreeList.GetFocusedRow() as ColorGeneralDataModel;
            if (_ColorGeneral == null) { return; }

            if (e.Column.FieldName == "CAD_Layer_Value")
            {
                var _Edit = TreeList.RepositoryItems["PopupContainer"] as DevExpress.XtraEditors.Repository.RepositoryItemPopupContainerEdit;

                var _Ctl = _Edit.PopupControl as DevExpress.XtraEditors.PopupContainerControl;

                if (_Ctl == null) { return; }

                var _Gdv = _Ctl.Controls["Gdc"] as DevExpress.XtraGrid.GridControl;

                _Gdv.DataSource = _ColorGeneral.CAD_Layer;
                e.RepositoryItem = _Edit;
            }
        }

        private void TreeList_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "PSD_Color")
            {
                var _ID = FuncStr.NullToStr(e.Node.GetValue("ID"));
                var _ColorGeneral = m_ListColorGeneral.Find(p => p.ID == _ID);
                if (_ColorGeneral == null) { return; }
                if (_ColorGeneral.Type == "1") { e.DisplayText = string.Empty; }
                if (_ColorGeneral.Type == "0")
                {
                    //Color _Color = Color.FromArgb(R, G, B);
                    //byte R = _Color.R;
                    //byte G = _Color.G;
                    //byte B = _Color.B;
                    e.DisplayText = _ColorGeneral.PSD_Color;
                }
            }

            if (e.Column.FieldName == "PSD_Transparency")
            {
                var _ID = FuncStr.NullToStr(e.Node.GetValue("ID"));
                var _ColorGeneral = m_ListColorGeneral.Find(p => p.ID == _ID);
                if (_ColorGeneral == null) { return; }
                //if (_ColorGeneral.Type == "1") { e.DisplayText = string.Empty; }
                e.DisplayText = _ColorGeneral.PSD_Transparency + "%";
            }

            if (e.Column.FieldName == "CAD_Layer_Value")
            {
                var _ID = FuncStr.NullToStr(e.Node.GetValue("ID"));
                var _ColorGeneral = m_ListColorGeneral.Find(p => p.ID == _ID);
                if (_ColorGeneral == null) { return; }
                //if (_ColorGeneral.Type == "1") { e.DisplayText = string.Empty; }
                if (_ColorGeneral.CAD_Layer == null || _ColorGeneral.CAD_Layer.Count == 0) { e.DisplayText = string.Empty; return; }
                e.DisplayText = string.Empty;
                _ColorGeneral.CAD_Layer.ForEach(p =>
                {
                    e.DisplayText += p.Name + ";";
                });
            }



        }

        private void TreeList_ShownEditor(object sender, EventArgs e)
        {



        }

        private void TreeList_ShowingEditor(object sender, CancelEventArgs e)
        {
            var _TreeList = sender as TreeList;
            if (_TreeList == null) { return; }

            var _ColorGeneral = _TreeList.GetFocusedRow() as ColorGeneralDataModel;
            if (_ColorGeneral == null) { return; }
            if (_ColorGeneral.Type == "1")
            {
                if (_TreeList.FocusedColumn.FieldName != "Name" && _TreeList.FocusedColumn.FieldName != "PSD_Transparency")
                {
                    e.Cancel = true;
                    return;
                }
            }


        }

        public Color GetColor(string _ColorStr)
        {
            int _ARGBvalue = 0;
            Color _Color = Color.FromName(_ColorStr);
            if ((_Color.A + _Color.R + _Color.G + _Color.B) == 0)
            {
                int.TryParse(_ColorStr, System.Globalization.NumberStyles.HexNumber, null, out _ARGBvalue);
                _Color = Color.FromArgb(_ARGBvalue);
            }
            return _Color;
        }

        private void TreeList_InvalidValueException(object sender, InvalidValueExceptionEventArgs e)
        {
            if (e.Value is Color) { e.ExceptionMode = DevExpress.XtraEditors.Controls.ExceptionMode.Ignore; }

        }

        private void TreeList_MouseDown(object sender, MouseEventArgs e)
        {
            TreeListHitInfo _HitInfo = (sender as TreeList).CalcHitInfo(new Point(e.X, e.Y));

            TreeListNode _Node = _HitInfo.Node;

            if (e.Button == MouseButtons.Right)
            {
                if (_Node != null)
                {
                    TreeList.FocusedColumn = _HitInfo.Column;

                    if (_HitInfo.Column.FieldName == "Name")
                    {
                        _Node.TreeList.FocusedNode = _Node;

                        var _Type = _Node.TreeList.FocusedNode.GetValue("Type");
                        if (FuncStr.NullToStr(_Type) == "0")
                        {
                            MenuItemNewGroup.Enabled = false;
                        }
                        else
                        {
                            MenuItemNewGroup.Enabled = true;
                        }

                        ContMenu.Show(MousePosition.X, MousePosition.Y);
                    }

                    if (_HitInfo.Column.FieldName == "PSD_Transparency")
                    {
                        _Node.TreeList.FocusedNode = _Node;

                        ContextMenuTransparency.Show(MousePosition.X, MousePosition.Y);
                    }

                    if (_HitInfo.Column.FieldName == "PSD_Color")
                    {
                        _Node.TreeList.FocusedNode = _Node;

                        keybd_event((byte)Keys.F4, 0, 0, 0);
                    }


                    if (_HitInfo.Column.FieldName == "CAD_Layer")
                    {
                        _Node.TreeList.FocusedNode = _Node;

                        keybd_event((byte)Keys.F4, 0, 0, 0);
                    }

                    if (_HitInfo.Column.FieldName == "CAD_Script")
                    {
                        _Node.TreeList.FocusedNode = _Node;

                        keybd_event((byte)Keys.F4, 0, 0, 0);
                    }

                }

            }

            if (e.Button == MouseButtons.Left)
            {
                if (_HitInfo.Band == null && _HitInfo.Column == null)
                {
                    keybd_event((byte)Keys.Escape, 0, 0, 0);
                }


                //if (_HitInfo.Column == null)
                //{
                //    TreeList.OptionsDragAndDrop.DragNodesMode = DragNodesMode.None;
                //}

                //else if (_HitInfo.Column.FieldName == "Name" && _Node != null)
                //{

                //    TreeList.PostEditor();

                //    _Node.TreeList.FocusedNode = _Node;

                //    var _DataType = _Node.TreeList.FocusedNode.GetValue("DataType");
                //    if (FuncStr.NullToStr(_DataType) == "0")
                //    {
                //        TreeList.OptionsDragAndDrop.DragNodesMode = DragNodesMode.None;
                //    }
                //    else
                //    {
                //        TreeList.OptionsDragAndDrop.DragNodesMode = DragNodesMode.Multiple;
                //    }


                //}

                if (TreeList.FocusedColumn != null && TreeList.FocusedColumn.FieldName == "Name" && _Node != null)
                {
                    TreeList.PostEditor();
                    _Node.TreeList.FocusedNode = _Node;
                }



            }

            if (e.Button == MouseButtons.Left && e.Clicks == 2)
            {
                var _TreeList = sender as TreeList;
                if (_TreeList == null) { return; }
                var _FocusedColumn = _TreeList.FocusedColumn;
                if (_FocusedColumn.FieldName == "Name" || _FocusedColumn.FieldName == "PSD_Transparency")
                {

                    _FocusedColumn.OptionsColumn.AllowEdit = true;
                    _TreeList.ShowEditor();
                }

            }



        }

        private void layoutControlGroup1_MouseDown(object sender, MouseEventArgs e)
        {
            SimulationEsc(e);
        }

        private void SimulationEsc(MouseEventArgs e)
        {
            TreeListHitInfo _HitInfo = TreeList.CalcHitInfo(new Point(e.X, e.Y));

            if (e.Button == MouseButtons.Left)
            {
                if (_HitInfo.Band == null)
                {
                    keybd_event((byte)Keys.Escape, 0, 0, 0);
                }

            }
        }

        private void fmConfigManage_MouseDown(object sender, MouseEventArgs e)
        {
            SimulationEsc(e);
        }

        private void MenuItemNewPeerGroup_Click(object sender, EventArgs e)
        {
            var _ColorGeneral = TreeList.GetFocusedRow() as ColorGeneralDataModel;
            if (_ColorGeneral == null) { return; }
            var _Json = FuncJson.Serialize(_ColorGeneral);
            var _NewColorGeneral = FuncJson.Deserialize<ColorGeneralDataModel>(_Json);
            _NewColorGeneral.ID = Guid.NewGuid().ToString();
            if (_ColorGeneral.ID == _ColorGeneral.PID)
                _NewColorGeneral.PID = _NewColorGeneral.ID;
            _NewColorGeneral.DataType = "1";
            _NewColorGeneral.Name = "未命名群组";
            _NewColorGeneral.Type = "1";
            _NewColorGeneral.PSD_Color = "255, 255, 255";
            _NewColorGeneral.PSD_Transparency = 100;
            _NewColorGeneral.CAD_Layer = null;
            _NewColorGeneral.CAD_Script = "NULL";
            _NewColorGeneral.CAD_Frame = "NULL";
            //m_ListColorGeneral.Add(_NewColorGeneral);

            var _Inidex = m_ListColorGeneral.IndexOf(_ColorGeneral);
            m_ListColorGeneral.Insert(_Inidex + 1, _NewColorGeneral);
            TreeList.RefreshDataSource();
            this.TreeList.ExpandAll();
        }

        private void MenuItemNewGroup_Click(object sender, EventArgs e)
        {
            var _ColorGeneral = TreeList.GetFocusedRow() as ColorGeneralDataModel;
            if (_ColorGeneral == null) { return; }
            var _Json = FuncJson.Serialize(_ColorGeneral);
            var _NewColorGeneral = FuncJson.Deserialize<ColorGeneralDataModel>(_Json);
            _NewColorGeneral.ID = Guid.NewGuid().ToString();
            _NewColorGeneral.PID = _ColorGeneral.ID;
            _NewColorGeneral.DataType = "1";
            _NewColorGeneral.Name = "未命名群组";
            _NewColorGeneral.Type = "1";

            _NewColorGeneral.PSD_Color = "255, 255, 255";
            _NewColorGeneral.PSD_Transparency = 100;
            _NewColorGeneral.CAD_Layer = null;
            _NewColorGeneral.CAD_Script = "NULL";
            _NewColorGeneral.CAD_Frame = "NULL";
            //m_ListColorGeneral.Add(_NewColorGeneral);
            var _Inidex = m_ListColorGeneral.IndexOf(_ColorGeneral);
            m_ListColorGeneral.Insert(_Inidex + 1, _NewColorGeneral);
            TreeList.RefreshDataSource();
            this.TreeList.ExpandAll();
        }

        private void MenuItemNewLayer_Click(object sender, EventArgs e)
        {
            var _ColorGeneral = TreeList.GetFocusedRow() as ColorGeneralDataModel;
            if (_ColorGeneral == null) { return; }
            var _Json = FuncJson.Serialize(_ColorGeneral);
            var _NewColorGeneral = FuncJson.Deserialize<ColorGeneralDataModel>(_Json);
            _NewColorGeneral.ID = Guid.NewGuid().ToString();
            if (_ColorGeneral.Type == "1")
            {
                _NewColorGeneral.PID = _ColorGeneral.ID;
            }
            else if (_ColorGeneral.ID == _ColorGeneral.PID)
            {
                _NewColorGeneral.PID = _NewColorGeneral.ID;
            }
            else
            {
                _NewColorGeneral.PID = _ColorGeneral.PID;
            }


            _NewColorGeneral.DataType = "1";
            _NewColorGeneral.Name = "未命名图层";
            _NewColorGeneral.Type = "0";
            _NewColorGeneral.PSD_Color = "255, 255, 255";
            _NewColorGeneral.PSD_Transparency = 100;
            _NewColorGeneral.CAD_Layer = null;
            _NewColorGeneral.CAD_Script = "";
            _NewColorGeneral.CAD_Frame = "未命名图层";
            //m_ListColorGeneral.Add(_NewColorGeneral);
            var _Inidex = m_ListColorGeneral.IndexOf(_ColorGeneral);
            m_ListColorGeneral.Insert(_Inidex + 1, _NewColorGeneral);
            TreeList.RefreshDataSource();
            this.TreeList.ExpandAll();
        }

        private void MenuItemRename_Click(object sender, EventArgs e)
        {
            keybd_event((byte)Keys.F2, 0, 0, 0);
        }

        private void MenuItemCopy_Click(object sender, EventArgs e)
        {

            var _ColorGeneral = TreeList.GetFocusedRow() as ColorGeneralDataModel;
            if (_ColorGeneral == null || TreeList.FocusedNode == null) { return; }
            List<ColorGeneralDataModel> _ListTemp = new List<ColorGeneralDataModel>();
            string _Guid = Guid.NewGuid().ToString();
            var _Json = FuncJson.Serialize(_ColorGeneral);
            var _NewColorGeneral = FuncJson.Deserialize<ColorGeneralDataModel>(_Json);
            if (_ColorGeneral.ID == _ColorGeneral.PID)
            {
                _NewColorGeneral.ID = _Guid;
                _NewColorGeneral.PID = _NewColorGeneral.ID;
            }
            else
            {
                _NewColorGeneral.ID = _Guid;
            }
            _NewColorGeneral.DataType = "1";
            _NewColorGeneral.Name = SetColorGeneralName(_NewColorGeneral);
            _ListTemp.Add(_NewColorGeneral);
            CopyChildNodes(_ColorGeneral, _ListTemp, _Guid);
            var _Inidex = m_ListColorGeneral.IndexOf(_ColorGeneral);
            m_ListColorGeneral.InsertRange(_Inidex + 1, _ListTemp);
            TreeList.RefreshDataSource();
            this.TreeList.ExpandAll();

        }


        public string SetColorGeneralName(ColorGeneralDataModel _NewColorGeneral)
        {
            var _List = m_ListColorGeneral.FindAll(p => p.Name.Contains(_NewColorGeneral.Name) && p.PID == _NewColorGeneral.PID && p.ID != _NewColorGeneral.ID);
            if (_List == null || _List.Count == 0) { return _NewColorGeneral.Name + " - 副本"; }
            for (int i = 1; i < 10000; i++)
            {
                if (i == 1)
                {
                    var _ListTemp1 = m_ListColorGeneral.FindAll(p => p.Name == _NewColorGeneral.Name + " - 副本" && p.PID == _NewColorGeneral.PID && p.ID != _NewColorGeneral.ID);
                    if (_ListTemp1 == null || _ListTemp1.Count == 0) { return _NewColorGeneral.Name + " - 副本"; }
                }
                else
                {
                    var _ListTemp = m_ListColorGeneral.FindAll(p => p.Name == _NewColorGeneral.Name + " - 副本(" + i + ")" && p.PID == _NewColorGeneral.PID && p.ID != _NewColorGeneral.ID);
                    if (_ListTemp == null || _ListTemp.Count == 0) { return _NewColorGeneral.Name + " - 副本(" + i + ")"; }
                }

            }
            return string.Empty;
        }


        private void MenuItemDelete_Click(object sender, EventArgs e)
        {
            TreeList.DeleteNode(TreeList.FocusedNode);


            //var _ColorGeneral = TreeList.GetFocusedRow() as ColorGeneralDataModel;
            ////if (_ColorGeneral == null || _ColorGeneral.DataType == "0") { return; }
            //if (_ColorGeneral == null) { return; }
            //List<ColorGeneralDataModel> _ListTemp = new List<ColorGeneralDataModel>();
            //GetChildNodes(_ColorGeneral, _ListTemp);
            //_ListTemp.ForEach(p => m_ListColorGeneral.Remove(p));
            //m_ListColorGeneral.Remove(_ColorGeneral);

            //TreeList.RefreshDataSource();
            //this.TreeList.ExpandAll();
        }

        private void MenuItemOpacity_Click(object sender, EventArgs e)
        {
            var _ToolStripMenuItem = sender as ToolStripMenuItem;
            var _FocusedColumn = TreeList.FocusedColumn;
            if (_FocusedColumn == null || _FocusedColumn.FieldName != "PSD_Transparency") { return; }
            TreeList.FocusedNode.SetValue("PSD_Transparency", FuncStr.NullToInt(_ToolStripMenuItem.Tag));
            TreeList.Refresh();
        }

        private void CopyChildNodes(ColorGeneralDataModel _ColorGeneral, List<ColorGeneralDataModel> _List, string _Guid)
        {
            if (_ColorGeneral == null) { return; }
            var _ListTmp = m_ListColorGeneral.FindAll(p => p.PID == _ColorGeneral.ID && p.ID != p.PID);
            if (_ListTmp.Count > 0)
            {
                foreach (var _Tmp in _ListTmp)
                {
                    var _Json = FuncJson.Serialize(_Tmp);
                    var _NewColorGeneral = FuncJson.Deserialize<ColorGeneralDataModel>(_Json);
                    _NewColorGeneral.ID = Guid.NewGuid().ToString();
                    _NewColorGeneral.PID = _Guid;
                    _NewColorGeneral.DataType = "1";
                    _NewColorGeneral.Name = SetColorGeneralName(_NewColorGeneral);
                    _List.Add(_NewColorGeneral);
                    var _ListTemp = m_ListColorGeneral.FindAll(p => p.PID == _Tmp.ID && p.ID != p.PID);
                    if (_ListTemp.Count > 0)
                    {
                        CopyChildNodes(_Tmp, _List, _NewColorGeneral.ID);
                    }
                }
            }
        }


        private void GetChildNodes(ColorGeneralDataModel _ColorGeneral, List<ColorGeneralDataModel> _List)
        {
            if (_ColorGeneral == null) { return; }
            //if (_ColorGeneral.ID == _ColorGeneral.PID) { return; }
            var _ListTmp = m_ListColorGeneral.FindAll(p => p.PID == _ColorGeneral.ID && p.ID != p.PID);
            if (_ListTmp.Count > 0)
            {
                foreach (var _Tmp in _ListTmp)
                {
                    _List.Add(_Tmp);
                    var _ListTemp = m_ListColorGeneral.FindAll(p => p.PID == _Tmp.ID);
                    if (_ListTemp.Count > 0)
                    {
                        GetChildNodes(_Tmp, _List);
                    }
                }
            }
        }



        private void GetChildNodes(TreeListNode _ParentNode, List<TreeListNode> _List)
        {
            if (_ParentNode.Nodes.Count > 0)
            {
                foreach (TreeListNode _Node in _ParentNode.Nodes)
                {
                    _List.Add(_Node);
                    if (_Node.Nodes.Count > 0)
                    {
                        GetChildNodes(_Node, _List);
                    }
                }
            }
        }




        private void Gdv_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            var _Gdv = sender as GridView;
            if (_Gdv == null || _Gdv.RowCount == 0) { return; }
            var _ColorGeneral = TreeList.GetFocusedRow() as ColorGeneralDataModel;
            if (_ColorGeneral == null) { return; }
            if (_Gdv.FocusedColumn.FieldName == "DeleteImg")
            {
                LayerDataModel _Layer = _Gdv.GetRow(_Gdv.FocusedRowHandle) as LayerDataModel;
                if (_Layer == null) { return; }
                _ColorGeneral.CAD_Layer.Remove(_Layer);
                _Gdv.RefreshData();

            }
            if (_Gdv.FocusedColumn.FieldName == "Name")
            {
                TreeList.PostEditor();
                TreeList.CloseEditor();
                TreeList.RefreshDataSource();

            }
        }

        private TreeListNode GetDragNode(IDataObject _Data)
        {
            return (TreeListNode)_Data.GetData(typeof(TreeListNode));
        }


        private void TreeList_DragDrop(object sender, DragEventArgs e)
        {
            TreeListNode _DragNode, _TargetNode;
            TreeList _Tree = sender as TreeList;
            Point _P = _Tree.PointToClient(new Point(e.X, e.Y));
            _DragNode = e.Data.GetData(typeof(TreeListNode)) as TreeListNode;
            _TargetNode = _Tree.CalcHitInfo(_P).Node;
            var _Type = _TargetNode.GetValue("Type");
            var _ID = _TargetNode.GetValue("ID");
            var _PID = _TargetNode.GetValue("PID");



            if (FuncStr.NullToStr(_Type) == "0" && FuncStr.NullToStr(_ID) == FuncStr.NullToStr(_PID))
            {
                _Tree.SetNodeIndex(_DragNode, _Tree.GetNodeIndex(_TargetNode));

                e.Effect = DragDropEffects.None;
            }
            else if (FuncStr.NullToStr(_Type) == "0")
            {


                _DragNode.SetValue("PID", _TargetNode.GetValue("PID"));
                _Tree.SetNodeIndex(_DragNode, _Tree.GetNodeIndex(_TargetNode));

                e.Effect = DragDropEffects.None;
            }



        }

        private void TreeList_DragEnte(object sender, DragEventArgs e)
        {

        }


        private void TreeList_DragOver(object sender, DragEventArgs e)
        {
            SetDragEffect(e, GetNodeByLocation(TreeList, new Point(e.X, e.Y)));
        }


        private TreeListNode GetNodeByLocation(TreeList _TreeList, Point _Location)
        {
            TreeListHitInfo _HitInfo = _TreeList.CalcHitInfo(_TreeList.PointToClient(_Location));
            return _HitInfo.Node;
        }

        public void SetDragEffect(DragEventArgs e, TreeListNode _TargetNode)
        {
            if (_TargetNode == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            e.Effect = DragDropEffects.Move;
            var _ID = _TargetNode.GetValue("ID");
            var _Type = _TargetNode.GetValue("Type");
            var _DataType = _TargetNode.GetValue("DataType");
            var _Name = _TargetNode.GetValue("Name");

            TreeListNode _DragdNode = GetDragNode(e.Data);
            var _DragID = _DragdNode.GetValue("ID");
            var _DragType = _DragdNode.GetValue("Type");
            var _DragDataType = _DragdNode.GetValue("DataType");
            var _DragName = _DragdNode.GetValue("Name");

            //BtnHelp.Text = _Name.ToString();

            //BtnCancel.Text = _DragName.ToString();

            if (FuncStr.NullToStr(_Type) == "0")
            {
                e.Effect = DragDropEffects.Move;
                return;
            }

            //if(FuncStr.NullToStr(_DragType) == "1")
            //{
            //    e.Effect = DragDropEffects.None;
            //    return;
            //}


            ////当被拖动的节点无法处理时显示禁止图标
            //if (draggedRow == null ||
            // (!draggedRow.Table.TableName.Equals("UserTable", StringComparison.CurrentCultureIgnoreCase) &&
            // !draggedRow.Table.TableName.Equals("DepartmentTable", StringComparison.CurrentCultureIgnoreCase)))
            //{
            //    e.Effect = DragDropEffects.None;
            //    return;
            //}

            ////当不存在目标节点或目标节点的层级大于等于被拖动节点时（后者仅针对树内拖拽），显示禁止图标
            //if (_TargetNode == null || draggedNode == _TargetNode || _TargetNode.HasAsParent(draggedNode))
            //{
            //    e.Effect = DragDropEffects.None;
            //    return;
            //}

        }

        private void TreeList_MouseDoubleClick(object sender, MouseEventArgs e)
        {


        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            TreeList.PostEditor();
            var _List = TreeList.GetNodeList();
            SetSrotID(_List);
            m_ColorGeneralConfig = FuncJson.Serialize(m_ListColorGeneral);
        }

        private void SetSrotID(List<TreeListNode> _List)
        {
            if (_List == null || _List.Count == 0) { return; }
            for (int i = 0; i < _List.Count(); i++)
            {
                var _ID = _List[i].GetValue("ID");
                var _ColorGeneral = m_ListColorGeneral.Find(s => FuncStr.NullToStr(s.ID) == FuncStr.NullToStr(_ID));
                _ColorGeneral.SortID = i;
            }
            m_ListColorGeneral = m_ListColorGeneral.OrderBy(p => p.SortID).ToList();
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog _SaveFileDialog = new SaveFileDialog();
            _SaveFileDialog.Filter = "Json Files(.json)|.json";
            _SaveFileDialog.RestoreDirectory = true;
            var DialogResult = _SaveFileDialog.ShowDialog();
            if (DialogResult == DialogResult.OK)
            {
                TreeList.PostEditor();
                var _List = TreeList.GetNodeList();
                SetSrotID(_List);
                var m_ColorGeneralConfig = FuncJson.Serialize(m_ListColorGeneral);
                m_Presenter.UpdateConfig();
                var _FilePath = _SaveFileDialog.FileName.ToString();
                FuncFile.Write(_FilePath, m_ColorGeneralConfig);
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            var _OpenFileDialog = new OpenFileDialog();
            _OpenFileDialog.Filter = "Json Files (*.Json)|*.Json";
            var _Result = _OpenFileDialog.ShowDialog();
            if (_Result == DialogResult.OK)
            {


                var _Json = FuncFile.ReadTxt(_OpenFileDialog.FileName);
                var _List = FuncJson.Deserialize<List<ColorGeneralDataModel>>(_Json);
                if (_List != null && _List.Count > 0)
                {
                    m_ListColorGeneral = m_Presenter.SetImgType(_List);

                    TreeList.DataSource = m_ListColorGeneral;
                    this.TreeList.ExpandAll();
                }


            }

        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            m_ListColorGeneral = m_Presenter.InitColorGeneral();
            TreeList.DataSource = m_ListColorGeneral;
            this.TreeList.ExpandAll();
        }


        private void TreeList_Click(object sender, EventArgs e)
        {

        }








        private void PictureEdit_Click(object sender, EventArgs e)
        {

            BtnPick.PerformClick();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            //this.defaultLookAndFeel1.LookAndFeel.SkinName = SkinStyle.VisualStudio2013Dark;
        }

        private void BtnPick_Click(object sender, EventArgs e)
        {

            var _FocusedColumn = TreeList.FocusedColumn;
            var _ColorGeneral = TreeList.GetFocusedRow() as ColorGeneralDataModel;
            if (_ColorGeneral == null) { return; }
            var _List = m_Presenter.AddLayer(this.Handle);

            if (_List != null || _List.Count > 0)
            {
                _List.ForEach(p =>
                {
                    if (_ColorGeneral.CAD_Layer == null) { _ColorGeneral.CAD_Layer = new List<LayerDataModel>(); }
                    var _Layer = _ColorGeneral.CAD_Layer.Find(s => s.Name == p);
                    if (_Layer == null)
                    {
                        LayerDataModel _LayerModel = new LayerDataModel();
                        _LayerModel.ID = FuncStr.NullToStr(Guid.NewGuid());
                        _LayerModel.Name = p;
                        _ColorGeneral.CAD_Layer.Add(_LayerModel);
                    }
                    TreeList.RefreshDataSource();
                });
            }

        }

        private void TreeList_DoubleClick(object sender, EventArgs e)
        {
            //var _TreeList = sender as TreeList;
            //if (_TreeList == null) { return; }
            //var _FocusedColumn = _TreeList.FocusedColumn;
            //if (_FocusedColumn.FieldName == "Name")
            //{
            //    _FocusedColumn.OptionsColumn.AllowEdit = true;
            //}
        }

        private void TreeList_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
        {
            var _TreeList = sender as TreeList;
            if (_TreeList == null) { return; }
            var _ColorGeneral = _TreeList.GetFocusedRow() as ColorGeneralDataModel;
            if (_ColorGeneral == null) { return; }
            var _FocusedColumn = _TreeList.FocusedColumn;
            if (_FocusedColumn.FieldName == "Name")
            {

                var _List = m_ListColorGeneral.FindAll(p => p.PID == _ColorGeneral.PID && FuncStr.NullToStr(p.Name) == FuncStr.NullToStr(e.Value) && p.ID != _ColorGeneral.ID);
                if (_List.Count > 0)
                {
                    e.Valid = false;
                    e.ErrorText = "组内名称冲突!";
                    _TreeList.PostEditor();
                    return;
                }

            }


        }

        private void TextName_EditValueChanging(object sender, ChangingEventArgs e)
        {


        }

        private void ComBoxScript_EditValueChanging(object sender, ChangingEventArgs e)
        {

            var _ColorGeneral = TreeList.GetFocusedRow() as ColorGeneralDataModel;
            if (_ColorGeneral == null) { return; }
            var _FocusedColumn = TreeList.FocusedColumn;

            if (FuncStr.NullToStr(e.NewValue) != EnumCADScript.无.ToString())
            {

                var _List = m_ListColorGeneral.FindAll(p => p.PID == _ColorGeneral.PID && p.Type == "0");
                if (_List.Count <= 1)
                {
                    //e.Valid = false;
                    //e.ErrorText = "缺少创建脚本关联的图层对象！!";
                    XtraMessageBox.Show("缺少创建脚本关联的图层对象!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.NewValue = e.OldValue;
                    return;
                }

            }


        }

        private void TreeList_HiddenEditor(object sender, EventArgs e)
        {
            ColName.OptionsColumn.AllowEdit = false;
            //ColColor.OptionsColumn.AllowEdit = false;
            ColTransparency.OptionsColumn.AllowEdit = false;
        }

        private void TreeList_StartSorting(object sender, EventArgs e)
        {

        }
    }
}
