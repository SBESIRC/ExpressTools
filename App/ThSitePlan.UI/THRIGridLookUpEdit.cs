using System.Drawing;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using System.Data;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using DevExpress.XtraEditors.Controls;
using System;
using DevExpress.XtraGrid;
using DevExpress.Data;
using TianHua.Publics.BaseCode;

namespace ThSitePlan.UI
{
    [UserRepositoryItem("THRIGridLookUpEdit")]
    public class THRIGridLookUpEdit : RepositoryItemGridLookUpEdit, ICloneable
    {
        static THRIGridLookUpEdit() { THRegisterGridLookUpEdit(); }

        /// <summary>
        /// 构造
        /// </summary>
        public THRIGridLookUpEdit() { }

        protected override GridControl CreateGrid()
        {
            return base.CreateGrid();
        }


        protected override void OnLoaded()
        {
            base.OnLoaded();
            AppendSortEvent();
        }

        /// <summary>
        /// 控件名称
        /// </summary>
        public const string THGridLookUpEditName = "THGridLookUpEdit";

        /// <summary>
        /// 筛选列文本
        /// </summary>
        public string THFiltercolumnText = string.Empty;


        /// <summary>
        /// 是否关闭默认displaymenber检索
        /// </summary>
        public bool _CloseDefaultFilter = false;

        /// <summary>
        /// 当前输入文本框文本信息
        /// </summary>
        public string THAllowEditText = string.Empty;

        /// <summary>
        /// 当前扩展过滤方式筛选文本(敲击空格按键后，只对当前文本列筛选)
        /// </summary>
        public string THAllowEditFilterExtText = "输入码1";

        /// <summary>
        /// 筛选主键
        /// </summary>
        private string _sortKey;

        /// <summary>
        /// 是否允许设置空值
        /// </summary>
        public bool THIsAllowNullSet = true;

        /// <summary>
        /// 是否按Escap退出
        /// </summary>
        public bool _THIsEscape = false;

        /// <summary>
        /// 默认筛选模式
        /// </summary>
        public Enum筛选模式 THScrType = Enum筛选模式.ScrConn;

        /// <summary>
        /// 重写EditorTypeName
        /// </summary>
        public override string EditorTypeName { get { return THGridLookUpEditName; } }

        /// <summary>
        /// 设置筛选列，根据FieldName筛选，多列用|分割，查询方式FieldName加%，如 %工号|%姓名% 未指定属性则按照所有显示列筛选。
        /// </summary>
        [DefaultValue(typeof(string), ""), Category("数据"), Description("设置筛选列，根据FieldName筛选，多列用|分割，查询方式FieldName加%，如 %工号|%姓名% 未指定属性则按照所有显示列筛选。")]
        public virtual string THFiltercolumn
        {
            set
            {
                if (THFiltercolumnText != value)
                {
                    THFiltercolumnText = value;
                    OnPropertiesChanged();
                }
            }
            get
            {
                return THFiltercolumnText;
            }
        }
        /// <summary>
        /// 是否关闭默认displaymenber检索
        /// </summary>
        [DefaultValue(typeof(bool), "false"), Category("数据"), Description("是否关闭默认displaymenber检索。")]
        public virtual bool CloseDefaultFilter
        {
            set
            {
                if (_CloseDefaultFilter != value)
                {
                    _CloseDefaultFilter = value;
                    OnPropertiesChanged();
                }
            }
            get
            {
                return _CloseDefaultFilter;
            }
        }
        /// <summary>
        /// 当前输入文本框文本信息
        /// </summary>
        [DefaultValue(typeof(string), ""), Category("数据"), Description("当前输入文本框文本信息。")]
        public virtual string THEditText
        {
            set
            {
                THAllowEditText = value;
            }
            get
            {
                return THAllowEditText;
            }
        }

        /// <summary>
        /// 当前输入文本框文本信息
        /// </summary>
        [DefaultValue(typeof(Enum筛选模式), ""), Category("数据"), Description("筛选类型。")]
        public virtual Enum筛选模式 THScr
        {
            set
            {
                THScrType = value;
            }
            get
            {
                return THScrType;
            }
        }

        /// <summary>
        /// 是否允许设置空值
        /// </summary>
        [DefaultValue(typeof(bool), "true"), Category("数据"), Description("是否允许设置空值。"), Obsolete]
        public virtual bool THIsAllowNull
        {
            set
            {
                if (THIsAllowNullSet != value)
                {
                    THIsAllowNullSet = value;
                    OnPropertiesChanged();
                }
            }
            get
            {
                return THIsAllowNullSet;
            }
        }

        /// <summary>
        /// 设置某个键值,当按下此键值的时候不弹出下拉框
        /// </summary>
        public Keys THHidePopupKey;

        /// <summary>
        /// 设置某个键值,当按下此键值的时候不弹出下拉框。
        /// </summary>
        [DefaultValue(typeof(Keys), "None"), Category("数据"), Description("设置某个键值,当按下此键值的时候不弹出下拉框。")]
        public virtual Keys THHidePopup
        {
            set
            {
                if (THHidePopupKey != value)
                {
                    THHidePopupKey = value;
                    OnPropertiesChanged();
                }
            }
            get
            {
                return THHidePopupKey;
            }
        }

        /// <summary>
        /// 是否按Escape退出来的
        /// </summary>
        [DefaultValue(typeof(bool), "false"), Category("数据"), Description("是否按Escape退出来的")]
        public virtual bool THIsEscape
        {
            set
            {
                if (_THIsEscape != value)
                {
                    _THIsEscape = value;
                    OnPropertiesChanged();
                }
            }
            get
            {
                return _THIsEscape;
            }
        }

        /// <summary>
        /// 当前输入文本框文本信息
        /// </summary>
        [DefaultValue(typeof(string), "输入码1"), Category("数据"), Description("当前扩展过滤方式筛选文本(敲击空格按键后，只对当前文本列筛选)。")]
        public virtual string THEditFilterExtText
        {
            set
            {
                if (THAllowEditFilterExtText != value)//UPDATE GAOEN
                {
                    THAllowEditFilterExtText = value;
                    OnPropertiesChanged();
                }
       
            }
            get
            {
                return THAllowEditFilterExtText;
            }
        }

        /// <summary>
        /// 筛选主键
        /// </summary>
        [DefaultValue(typeof(string), ""), Category("数据"), Description("筛选的主键。")]
        public string THSortKey
        {
            get { return _sortKey; }
            set
            {
                _sortKey = value;
                AppendSortEvent();
            }
        }

        internal bool HasAddSortEvent;
        private void AppendSortEvent()
        {
            ResetColumnSort();
            if (HasAddSortEvent)
            {
                this.View.CustomColumnSort -= this.View_CustomColumnSort;
                this.HasAddSortEvent = false;
            }
            if (!string.IsNullOrEmpty(THSortKey))
            {
                var primaryKey = this.THSortKey;
                GridColumn sortColumn;
                foreach (GridColumn column in this.View.Columns)
                {
                    if (primaryKey == column.FieldName && column.Visible)
                    {
                        sortColumn = column;
                        if (!this.HasAddSortEvent)
                        {
                            this.View.CustomColumnSort += this.View_CustomColumnSort;
                            this.HasAddSortEvent = true;
                            sortColumn.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
                        }
                        sortColumn.SortMode = ColumnSortMode.Custom;
                        break;
                    }
                }
            }

        }

        private void ResetColumnSort()
        {
            foreach (GridColumn column in this.View.Columns)
            {
                if (column.Visible)
                {
                    column.SortMode = ColumnSortMode.Default;
                    column.SortOrder = ColumnSortOrder.None;
                }
            }
        }

        public void View_CustomColumnSort(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnSortEventArgs e)
        {
            GridView edit = sender as GridView;
            if (edit == null) return;
            var primaryKey = THSortKey;
            if (e.Column.FieldName == primaryKey)
            {
                e.Handled = true;
                var va1 = e.Value1.ToString().ToUpper();
                var va2 = e.Value2.ToString().ToUpper();
                var txt = this.THEditText.ToUpper();
                if (va1.Contains(txt) && va2.Contains(txt))
                {
                    if (va1.StartsWith(txt) && va2.StartsWith(txt))
                        e.Result = System.Collections.Comparer.Default.Compare(va1, va2);
                    else if (va1.StartsWith(txt) && !va2.StartsWith(txt))
                        e.Result = -1;
                    else if (!va1.StartsWith(txt) && va2.StartsWith(txt))
                        e.Result = 1;
                    else
                        e.Result = System.Collections.Comparer.Default.Compare(va1, va2);
                    return;
                }
                if (va1.Contains(txt) && !va2.Contains(txt))
                {
                    e.Result = -1;
                    return;
                }
                if (!va1.Contains(txt) && va2.Contains(txt))
                {
                    e.Result = 1;
                    return;
                }
                e.Result = System.Collections.Comparer.Default.Compare(va1, va2);
                return;


            }
        }

        public static void THRegisterGridLookUpEdit()
        {
            Image img = null;
            EditorRegistrationInfo.Default.Editors.Add(new EditorClassInfo(THGridLookUpEditName,
              typeof(THGridLookUpEdit), typeof(THRIGridLookUpEdit),
              typeof(GridLookUpEditBaseViewInfo), new ButtonEditPainter(), true, img));
        }


        public override void Assign(RepositoryItem item)
        {
            BeginUpdate();
            try
            {
                base.Assign(item);
                THRIGridLookUpEdit source =
                    item as THRIGridLookUpEdit;
                if (source == null) return;
                THFiltercolumnText = source.THFiltercolumn;
                THAllowEditFilterExtText = source.THEditFilterExtText;//ADD GAOEN
                _CloseDefaultFilter= source._CloseDefaultFilter;
                THHidePopupKey = source.THHidePopup;
            }
            finally
            {
                EndUpdate();
            }
        }
    }

    public class THGridLookUpEdit : GridLookUpEdit
    {
        /// <summary>
        /// 重写KeyDown
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (IsDesignMode) return;
            this.Popup += new System.EventHandler(THGridLookUpEdit_Popup);
            this.EditValueChanging += new ChangingEventHandler(THGridLookUpEdit_EditValueChanging);
            this.KeyUp += new KeyEventHandler(THGridLookUpEdit_KeyUp);
            this.Closed += new ClosedEventHandler(THGridLookUpEdit_Closed);
        }


        protected override void UpdateEditValueOnClose(PopupCloseMode closeMode, bool acceptValue, object newValue, object oldValue)
        {
            if (this.Properties.View.RowCount == 0 || this.Text.Trim() == "")
                this.Properties.THEditText = this.Text;
            base.UpdateEditValueOnClose(closeMode, acceptValue, newValue, oldValue);
        }

        protected override void EndAcceptEditValue()
        {
            this.Properties.THEditText = this.Text;
            base.EndAcceptEditValue();
        }

        void THGridLookUpEdit_Closed(object sender, ClosedEventArgs e)
        {
            if (this.Properties.View.RowCount == 0 || this.Text.Trim() == "")
            {
                this.EditValue = DBNull.Value;
            }

            this.Popup -= new System.EventHandler(THGridLookUpEdit_Popup);
            this.EditValueChanging -= new ChangingEventHandler(THGridLookUpEdit_EditValueChanging);
            this.KeyUp -= new KeyEventHandler(THGridLookUpEdit_KeyUp);
            this.Closed -= new ClosedEventHandler(THGridLookUpEdit_Closed);
        }
        /// <summary>
        /// 键盘按下弹起触发
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">KeyEventArgs e</param>
        void THGridLookUpEdit_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.Properties.TextEditStyle == TextEditStyles.Standard)
            {

                if (!this.IsPopupOpen)
                {
                    try
                    {
                        if (this.EditValue != null &&
                            this.EditValue.ToString().Length > 0
                            && this.EditValue.ToString().Substring(0, 1) == FuncStr.KeyCodeToStr(this.Properties.THHidePopup))
                        {
                            this.ClosePopup();
                        }
                        else
                        {
                            if (e.KeyData == Keys.Escape || e.KeyData == Keys.Enter || e.Control || e.Alt || (e.Shift && this.Properties.THScr ==Enum筛选模式.ScrLike))
                            {
                                if (e.KeyData == Keys.Escape) { this.Properties.THIsEscape = true; }
                                this.ClosePopup();
                                this.SelectAll();
                            }
                            else
                                if ((e.KeyCode.GetHashCode() >= 65 && e.KeyCode.GetHashCode() <= 90) ||
                                (e.KeyCode.GetHashCode() >= 48 && e.KeyCode.GetHashCode() <= 57) ||
                                (e.KeyCode.GetHashCode() >= 97 && e.KeyCode.GetHashCode() <= 105) ||
                                e.KeyCode.GetHashCode() == 91 ||
                                e.KeyCode.GetHashCode() == 8 ||
                                (this.EditValue != null && this.EditValue.ToString().Length > 0 && 
                                this.EditValue.ToString().Substring(this.EditValue.ToString().Length - 1, 1) == "?" && this.Properties.THScr == Enum筛选模式.ScrLike))
                                {
                                    this.ShowPopup();
                                }
                        }
                    }
                    catch { };
                }
            }
        }

        /// <summary>
        /// EditValue改变触发
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">ChangingEventArgs e</param>
        void THGridLookUpEdit_EditValueChanging(object sender, ChangingEventArgs e)
        {
            if (((GridLookUpEdit)sender).IsPopupOpen)
            {
                ((GridLookUpEdit)sender).BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate
                {
                    THGridLookUpEditFunction.FilterLookupGridLookUpEdit(this);
                }));
            }
        }

        /// <summary>
        /// 当筛选GRID显示触发
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">System.EventArgs e</param>
        void THGridLookUpEdit_Popup(object sender, System.EventArgs e)
        {
            THGridLookUpEditFunction.FilterLookupGridLookUpEdit(this);
        }

        static THGridLookUpEdit()
        {
            THRIGridLookUpEdit.THRegisterGridLookUpEdit();
        }

        /// <summary>
        /// 构造
        /// </summary>
        public THGridLookUpEdit()
        {

        }

        public override string EditorTypeName
        {
            get
            {
                return
                    THRIGridLookUpEdit.THGridLookUpEditName;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new THRIGridLookUpEdit Properties
        {
            get { return base.Properties as THRIGridLookUpEdit; }
        }
    }

    public class THGridLookUpEditFunction
    {
        /// <summary>
        /// 筛选实现
        /// </summary>
        /// <param name="sender">object sender</param>
        public static void FilterLookupGridLookUpEdit(object sender)
        {
            THGridLookUpEdit edit = sender as THGridLookUpEdit;
            if (edit == null) return;
            bool _bIsFilterExt = false;
            GridView gridView = edit.Properties.View as GridView;

            FieldInfo fi = gridView.GetType().GetField("extraFilter", BindingFlags.NonPublic | BindingFlags.Instance);
            List<CriteriaOperator> _ListCriteriaOperator = new List<CriteriaOperator>();
            if (!string.IsNullOrEmpty(edit.Properties.THEditFilterExtText) && edit.Properties.View.Columns.ColumnByFieldName(edit.Properties.THEditFilterExtText) != null)
            {
                if (edit != null && edit.EditValue != null && edit.EditValue.ToString().Length > 0 && edit.EditValue.ToString().Substring(0, 1).Equals(" "))
                {
                    _bIsFilterExt = true;
                    _ListCriteriaOperator.Add(new BinaryOperator(edit.Properties.THEditFilterExtText, "%" + FuncStr.NullToStr(edit.EditValue)+ "%", BinaryOperatorType.Like));
                }
                else if (edit != null && edit.EditValue != null && edit.EditValue.ToString().Length > 0 && edit.EditValue.ToString().Substring(edit.EditValue.ToString().Length - 1, 1).Equals(" "))
                {
                    _bIsFilterExt = true; 
                    _ListCriteriaOperator.Add(new BinaryOperator(edit.Properties.THEditFilterExtText, "%" + FuncStr.NullToStr(edit.EditValue) + "%", BinaryOperatorType.Like));
                }
            }
            if (!_bIsFilterExt)
            {
                switch (edit.Properties.THScr)
                {
                    case Enum筛选模式.ScrConn:
                        {
                            string _lsEFFiltercolumnText = edit.Properties.THFiltercolumn;
                            if (_lsEFFiltercolumnText != "")
                            {
                                string[] _lsStrFilter = _lsEFFiltercolumnText.Split('|');
                                foreach (string _lsStr in _lsStrFilter)
                                {
                                    foreach (GridColumn _GridColumn in edit.Properties.View.Columns)
                                    {
                                        if (_lsStr.Replace("%", "") == _GridColumn.FieldName)
                                        {
                                            _ListCriteriaOperator.Add(GetBinaryOperator(_lsStr, edit.AutoSearchText, edit.Properties.THScr));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (GridColumn _GridColumn in edit.Properties.View.Columns)
                                {
                                    _ListCriteriaOperator.Add(new BinaryOperator(_GridColumn.FieldName, "%" + edit.AutoSearchText + "%", BinaryOperatorType.Like));
                                }
                            }
                        }
                        break;
                    case Enum筛选模式.ScrLike:
                        {
                            string _lsEFFiltercolumnText = edit.Properties.THFiltercolumn;
                            if (_lsEFFiltercolumnText != "")
                            {
                                string[] _lsStrFilter = _lsEFFiltercolumnText.Replace("%", "").Split('|');
                                foreach (string _lsStr in _lsStrFilter)
                                {
                                    foreach (GridColumn _GridColumn in edit.Properties.View.Columns)
                                    {
                                        if (_lsStr == _GridColumn.FieldName)
                                        {
                                            _ListCriteriaOperator.Add(GetBinaryOperator(_lsStr, edit.AutoSearchText, edit.Properties.THScr));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (GridColumn _GridColumn in edit.Properties.View.Columns)
                                {
                                    _ListCriteriaOperator.Add(new BinaryOperator(_GridColumn.FieldName, "%" + edit.AutoSearchText + "%", BinaryOperatorType.Like));
                                }
                            }
                        }
                        break;
                }
            }
            if (edit.Properties.CloseDefaultFilter==true)
            {
                List<CriteriaOperator> _ListCriteriaOperator1 = new List<CriteriaOperator>();
                _ListCriteriaOperator1.Add(new BinaryOperator(edit.Properties.View.Columns[0].FieldName, ";", BinaryOperatorType.Equal));
                string filterCondition1 = new GroupOperator(GroupOperatorType.Or, _ListCriteriaOperator1).ToString();
                fi.SetValue(gridView, filterCondition1);
                MethodInfo mi1 = gridView.GetType().GetMethod("ApplyColumnsFilterEx", BindingFlags.NonPublic | BindingFlags.Instance);
                mi1.Invoke(gridView, null);
                edit.SelectedText = "";
            }

            string filterCondition = new GroupOperator(GroupOperatorType.Or, _ListCriteriaOperator).ToString();
            fi.SetValue(gridView, filterCondition);
            MethodInfo mi = gridView.GetType().GetMethod("ApplyColumnsFilterEx", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(gridView, null);
        }

        /// <summary>
        /// 返回筛选方式
        /// </summary>
        /// <param name="psStrFilterText">筛选方向</param>
        /// <param name="psAutoSearchText">筛选文本</param>
        /// <returns>BinaryOperator</returns>
        private static BinaryOperator GetBinaryOperator(string psStrFilterText, string psAutoSearchText, Enum筛选模式 _en筛选模式)
        {
            BinaryOperator _BinaryOp = null;

            switch (_en筛选模式)
            {
                case Enum筛选模式.ScrConn:
                    switch (GetFilterDirection(psStrFilterText))
                    {
                        case Enum筛选方式.All:
                            _BinaryOp = new BinaryOperator(psStrFilterText.Replace("%", ""), "%" + psAutoSearchText + "%", BinaryOperatorType.Like);
                            break;
                        case Enum筛选方式.Left:
                            _BinaryOp = new BinaryOperator(psStrFilterText.Replace("%", ""), "%" + psAutoSearchText, BinaryOperatorType.Like);
                            break;
                        case Enum筛选方式.Right:
                            _BinaryOp = new BinaryOperator(psStrFilterText.Replace("%", ""), psAutoSearchText + "%", BinaryOperatorType.Like);
                            break;
                    }
                    break;
                case Enum筛选模式.ScrLike:
                    if (psAutoSearchText.Contains("?") && psAutoSearchText.Length > 1)
                    {
                        if (psAutoSearchText.Substring(0, 1) == "?" && psAutoSearchText.Substring(psAutoSearchText.Length - 1, 1) == "?")
                        {
                            _BinaryOp = new BinaryOperator(psStrFilterText, "%" + psAutoSearchText.Substring(1, psAutoSearchText.Length - 1).Substring(0, psAutoSearchText.Substring(1, psAutoSearchText.Length - 1).Length - 1) + "%", BinaryOperatorType.Like);
                        }
                        else if (psAutoSearchText.Substring(0, 1) == "?")
                        {
                            _BinaryOp = new BinaryOperator(psStrFilterText, "%" + psAutoSearchText.Substring(1, psAutoSearchText.Length - 1), BinaryOperatorType.Like);
                        }
                        else if (psAutoSearchText.Substring(psAutoSearchText.Length - 1, 1) == "?")
                        {
                            _BinaryOp = new BinaryOperator(psStrFilterText, psAutoSearchText.Substring(0, psAutoSearchText.Length - 1) + "%", BinaryOperatorType.Like);
                        }
                    }
                    else
                    {
                        _BinaryOp = new BinaryOperator(psStrFilterText, "%" + psAutoSearchText + "%", BinaryOperatorType.Like);
                    }
                    break;
            }
            return _BinaryOp;
        }

        /// <summary>
        /// 通过设置属性返回筛选方式
        /// </summary>
        /// <param name="psStrFilterText"></param>
        /// <returns></returns>
        private static Enum筛选方式 GetFilterDirection(string psStrFilterText)
        {
            Enum筛选方式 _en筛选方式 = new Enum筛选方式();
            if (psStrFilterText.IndexOf("%") == 0 && psStrFilterText.LastIndexOf("%") == psStrFilterText.Length - 1)
            {
                _en筛选方式 = Enum筛选方式.All;
            }
            else if (psStrFilterText.IndexOf("%") == 0)
            {
                _en筛选方式 = Enum筛选方式.Left;
            }
            else if (psStrFilterText.LastIndexOf("%") == psStrFilterText.Length - 1)
            {
                _en筛选方式 = Enum筛选方式.Right;
            }
            return _en筛选方式;
        }
    }

    /// <summary>
    /// en筛选方式
    /// </summary>
    public enum Enum筛选方式
    {
        All = 0,
        Left = 1,
        Right = 2
    }

    public enum Enum筛选模式
    {
        ScrConn = 0,
        ScrLike = 1
    }
}


