using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using ThColumnInfo.View;
using System.Windows.Threading;
using System.Windows.Input;

namespace ThColumnInfo.ViewModel
{
    public class ParameterSetVM
    {
        public ParameterSetInfo ParaSetInfo { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand PointBottomFloorCommand { get; set; }
        public DelegateCommand PointColumnTableLayerCommand { get; set; }
        public ParameterSet Owner { get; set; }
        private string columnParameterSetKey = "ColumnParameterSet";
        public static bool isOpened = false;
        public ParameterSetVM()
        {
            LoadParameters();
            this.SaveCommand = new DelegateCommand();
            this.CancelCommand = new DelegateCommand();
            this.PointBottomFloorCommand = new DelegateCommand();
            this.PointColumnTableLayerCommand = new DelegateCommand();
            this.SaveCommand.ExecuteAction = new Action<object>(this.SaveCommandExecute);
            this.CancelCommand.ExecuteAction = new Action<object>(this.CancelCommandExecute);
            this.PointBottomFloorCommand.ExecuteAction = new Action<object>(this.PointBottomFloorCommandExecute);
            this.PointColumnTableLayerCommand.ExecuteAction = new Action<object>(this.PointColumnLayerTableCommandExecute);
        }
        private void LoadParameters()
        {
            this.ParaSetInfo = new ParameterSetInfo();
            ReadParaFromDatabase();
            //初始化柱列表
            List<string> columnTableLayers = ThColumnInfoUtils.GetLayerList("Tab");
            if (!string.IsNullOrEmpty(this.ParaSetInfo.ColumnTableLayer))
            {
                if (columnTableLayers.IndexOf(this.ParaSetInfo.ColumnTableLayer) < 0)
                {
                    columnTableLayers.Add(this.ParaSetInfo.ColumnTableLayer);
                }
            }            
            columnTableLayers.ForEach(i => this.ParaSetInfo.ColumnTableLayerList.Add(i));
            if (string.IsNullOrEmpty(this.ParaSetInfo.AntiSeismicGrade) && this.ParaSetInfo.AntiseismicGradeList.Count > 0)
            {
                this.ParaSetInfo.AntiSeismicGrade = this.ParaSetInfo.AntiseismicGradeList[0];
            }
            if (string.IsNullOrEmpty(this.ParaSetInfo.ConcreteStrength) && this.ParaSetInfo.ConcreteStrengthList.Count > 0)
            {
                if (this.ParaSetInfo.ConcreteStrengthList.IndexOf("C30") >= 0)
                {
                    this.ParaSetInfo.ConcreteStrength = "C30";
                }
                else
                {
                    this.ParaSetInfo.ConcreteStrength = this.ParaSetInfo.ConcreteStrengthList[0];
                }
            }
            if(string.IsNullOrEmpty(this.ParaSetInfo.StructureType) && this.ParaSetInfo.StructureTypeList.Count > 0)
            {
                this.ParaSetInfo.StructureType = this.ParaSetInfo.StructureTypeList[0];
            }
            if(string.IsNullOrEmpty(this.ParaSetInfo.BottomFloorLayer))
            {
                GetInitBottomFloorName();
            }
        }
        private void GetInitBottomFloorName()
        {
            ThStandardSignManager tm = ThStandardSignManager.LoadData();
            if(tm.StandardSigns.Count>0)
            {
                this.ParaSetInfo.BottomFloorLayer = tm.StandardSigns.Last().InnerFrameName;
            }
        }
        private void SaveCommandExecute(object parameter)
        {
            if(string.IsNullOrEmpty(this.ParaSetInfo.ColumnLayer))
            {
                System.Windows.MessageBox.Show("柱子图层不能为空");
                return;
            }
            if(this.ParaSetInfo.ProtectLayerThickness<=0)
            {
                System.Windows.MessageBox.Show("保护层厚度要大于0");
                return;
            }
            if(this.ParaSetInfo.FloorCount<=0)
            {
                System.Windows.MessageBox.Show("自然层总层数要大于0");
                return;
            }
            if(string.IsNullOrEmpty(this.ParaSetInfo.ColumnTableLayer.Trim()))
            {
                ParameterSetTip parameterSetTip = new ParameterSetTip();
                parameterSetTip.Topmost = true;
                parameterSetTip.Owner = this.Owner;
                parameterSetTip.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                parameterSetTip.ShowDialog();
                if (!parameterSetTip.IsGoOn)
                {
                    SetBtnFocus();
                    return;
                }
            }
            SaveParaToDatabase();
            System.Windows.MessageBox.Show("参数保存成功!");
            Owner.Close();
            isOpened = false;
        }
        private void SetBtnFocus()
        {
            Owner.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => Keyboard.Focus(Owner.btnPointColumnTableLayer)));
        }
        private void SaveParaToDatabase()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = doc.LockDocument())
            {
                ObjectId namedDictId = ThColumnInfoUtils.AddNamedDictionary(doc.Database, this.columnParameterSetKey);
                if (namedDictId == ObjectId.Null)
                {
                    return;
                }
                //抗震等级
                TypedValue antiSeismicGradeTV = new TypedValue((int)DxfCode.ExtendedDataAsciiString, this.ParaSetInfo.AntiSeismicGrade);
                ThColumnInfoUtils.AddXrecord(namedDictId, "AntiSeismicGrade", new List<TypedValue> { antiSeismicGradeTV });

                //柱图层
                TypedValue columnLayerTV = new TypedValue((int)DxfCode.ExtendedDataAsciiString, this.ParaSetInfo.ColumnLayer);
                ThColumnInfoUtils.AddXrecord(namedDictId, "ColumnLayer", new List<TypedValue> { columnLayerTV });

                //柱表图层
                TypedValue columnTableLayerTV = new TypedValue((int)DxfCode.ExtendedDataAsciiString, this.ParaSetInfo.ColumnTableLayer);
                ThColumnInfoUtils.AddXrecord(namedDictId, "ColumnTableLayer", new List<TypedValue> { columnTableLayerTV });

                //混凝土强度
                TypedValue concreteStrengthTV = new TypedValue((int)DxfCode.ExtendedDataAsciiString, this.ParaSetInfo.ConcreteStrength);
                ThColumnInfoUtils.AddXrecord(namedDictId, "ConcreteStrength", new List<TypedValue> { concreteStrengthTV });

                //自然层总层数
                TypedValue floorCountTV = new TypedValue((int)DxfCode.Int16, this.ParaSetInfo.FloorCount);
                ThColumnInfoUtils.AddXrecord(namedDictId, "FloorCount", new List<TypedValue> { floorCountTV });

                //是否为IV类场地较高建筑
                TypedValue fourClassHigherArchTV = new TypedValue((int)DxfCode.Bool, this.ParaSetInfo.IsFourClassHigherArchitecture);
                ThColumnInfoUtils.AddXrecord(namedDictId, "IsFourClassHigherArchitecture", new List<TypedValue> { fourClassHigherArchTV });

                //底层图框名
                TypedValue bottomFloorLayerTV = new TypedValue((int)DxfCode.ExtendedDataAsciiString, this.ParaSetInfo.BottomFloorLayer);
                ThColumnInfoUtils.AddXrecord(namedDictId, "BottomFloorLayer", new List<TypedValue> { bottomFloorLayerTV });

                //保护层厚度
                TypedValue protectLayerThicknessTV = new TypedValue((int)DxfCode.Real, this.ParaSetInfo.ProtectLayerThickness);
                ThColumnInfoUtils.AddXrecord(namedDictId, "ProtectLayerThickness", new List<TypedValue> { protectLayerThicknessTV });

                //结构类型
                TypedValue structureTypeTV = new TypedValue((int)DxfCode.ExtendedDataAsciiString, this.ParaSetInfo.StructureType);
                ThColumnInfoUtils.AddXrecord(namedDictId, "StructureType", new List<TypedValue> { structureTypeTV });
            }
        }
        public void SaveFloorCountToDatabase()
        {
            if(this.ParaSetInfo.FloorCount==0)
            {
                return;
            }
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (DocumentLock docLock = doc.LockDocument())
            {
                ObjectId namedDictId = ThColumnInfoUtils.AddNamedDictionary(doc.Database, this.columnParameterSetKey);
                if (namedDictId == ObjectId.Null)
                {
                    return;
                }
                //自然层总层数
                TypedValue floorCountTV = new TypedValue((int)DxfCode.Int16, this.ParaSetInfo.FloorCount);
                ThColumnInfoUtils.AddXrecord(namedDictId, "FloorCount", new List<TypedValue> { floorCountTV });
            }
        }
        private void ReadParaFromDatabase()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            ObjectId namedDictId = ThColumnInfoUtils.AddNamedDictionary(doc.Database, this.columnParameterSetKey);
            if (namedDictId == ObjectId.Null)
            {
                return;
            }
            //抗震等级
            List<TypedValue> antiSeismicGradeTvs = ThColumnInfoUtils.GetXRecord(namedDictId, "AntiSeismicGrade");
            if(antiSeismicGradeTvs.Count>0)
            {
                this.ParaSetInfo.AntiSeismicGrade = (string)antiSeismicGradeTvs[0].Value;
            }

            //柱图层
            List<TypedValue> columnLayerTvs = ThColumnInfoUtils.GetXRecord(namedDictId, "ColumnLayer");
            if (columnLayerTvs.Count > 0)
            {
                this.ParaSetInfo.ColumnLayer = (string)columnLayerTvs[0].Value;
            }

            //柱表图层
            List<TypedValue> columnTableLayerTvs = ThColumnInfoUtils.GetXRecord(namedDictId, "ColumnTableLayer");
            if (columnTableLayerTvs.Count > 0)
            {
                this.ParaSetInfo.ColumnTableLayer = (string)columnTableLayerTvs[0].Value;
            }

            //混凝土强度
            List<TypedValue> concreteStrengthTvs = ThColumnInfoUtils.GetXRecord(namedDictId, "ConcreteStrength");
            if (concreteStrengthTvs.Count > 0)
            {
                this.ParaSetInfo.ConcreteStrength = (string)concreteStrengthTvs[0].Value;
            }

            //自然层总层数
            List<TypedValue> floorCountTvs = ThColumnInfoUtils.GetXRecord(namedDictId, "FloorCount");
            if (floorCountTvs.Count > 0)
            {
                this.ParaSetInfo.FloorCount = (short)floorCountTvs[0].Value;
            }

            //是否为IV类场地较高建筑
            List<TypedValue> fourClassHigherArchTvs = ThColumnInfoUtils.GetXRecord(namedDictId, "IsFourClassHigherArchitecture");
            if (fourClassHigherArchTvs.Count > 0)
            {
                short value = (short)fourClassHigherArchTvs[0].Value;
                if (value == 0)
                {
                    this.ParaSetInfo.IsFourClassHigherArchitecture = false;
                }
                else
                {
                    this.ParaSetInfo.IsFourClassHigherArchitecture = true;
                }
            }

            //底层图框名            
            List<TypedValue> bottomFloorLayerTvs = ThColumnInfoUtils.GetXRecord(namedDictId, "BottomFloorLayer");
            if (bottomFloorLayerTvs.Count > 0)
            {
                this.ParaSetInfo.BottomFloorLayer = (string)bottomFloorLayerTvs[0].Value;
            }

            //保护层厚度
            List<TypedValue> protectLayerThicknessTvs = ThColumnInfoUtils.GetXRecord(namedDictId, "ProtectLayerThickness");
            if (protectLayerThicknessTvs.Count > 0)
            {
                this.ParaSetInfo.ProtectLayerThickness = (double)protectLayerThicknessTvs[0].Value;
            }

            //结构类型
            List<TypedValue> structureTypeTvs = ThColumnInfoUtils.GetXRecord(namedDictId, "StructureType");
            if (structureTypeTvs.Count > 0)
            {
                this.ParaSetInfo.StructureType = (string)structureTypeTvs[0].Value;
            }
        }
        private void CancelCommandExecute(object parameter)
        {
            if(Owner != null)
            {
                Owner.Close();
                isOpened = false;
            }
        }
        private void PointBottomFloorCommandExecute(object parameter)
        {
            Owner.Visibility = System.Windows.Visibility.Hidden;
            string bottomFloorName = GetBottomFloorName();
            if(!string.IsNullOrEmpty(bottomFloorName))
            {
                this.ParaSetInfo.BottomFloorLayer = bottomFloorName;
            }
            Owner.Visibility = System.Windows.Visibility.Visible;
        }
        private void PointColumnLayerTableCommandExecute(object parameter)
        {
            Owner.Visibility = System.Windows.Visibility.Hidden;
            string columnTableLayerName = GetColumnTableLayerName();
            if(!string.IsNullOrEmpty(columnTableLayerName))
            {
                if(this.ParaSetInfo.ColumnTableLayerList.IndexOf(columnTableLayerName)<0)
                {
                    this.ParaSetInfo.ColumnTableLayerList.Add(columnTableLayerName);
                }
                this.ParaSetInfo.ColumnTableLayer = columnTableLayerName;
            }
            Owner.Visibility = System.Windows.Visibility.Visible;
        }
        private string GetColumnTableLayerName()
        {
            string columnTableLayerName = "";
            Document doc = ThColumnInfoUtils.GetMdiActiveDocument();
            Editor ed = doc.Editor;
            bool doMark = true;
            ObjectId objId = ObjectId.Null;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                while (doMark)
                {
                    PromptEntityResult per = ed.GetEntity("\n请选择柱表框线的任一线段：");
                    if (per.Status == PromptStatus.OK)
                    {
                        objId = per.ObjectId;
                        DBObject dbObj = trans.GetObject(objId, OpenMode.ForRead);
                        if (dbObj is Curve curve)
                        {
                            columnTableLayerName = curve.Layer;
                            break;
                        }
                    }
                    else if(per.Status==PromptStatus.Cancel)
                    {
                        doMark = false;
                    }
                }
                trans.Commit();
            }
            return columnTableLayerName;
        }
        private string GetBottomFloorName()
        {
            string innerFrameName = "";
            Document doc = ThColumnInfoUtils.GetMdiActiveDocument();
            Editor ed = doc.Editor;
            bool doMark = true;
            ObjectId objId = ObjectId.Null;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                while (doMark)
                {
                    innerFrameName = "";
                    PromptEntityResult per = ed.GetEntity("\n请选择标准的内框：");
                    if (per.Status == PromptStatus.OK)
                    {
                        objId = per.ObjectId;
                        DBObject dbObj = trans.GetObject(objId, OpenMode.ForRead);
                        if (dbObj is BlockReference)
                        {
                            BlockReference br = dbObj as BlockReference;
                            if (br.Name.ToUpper().Contains("THAPE") && br.Name.ToUpper().Contains("INNER"))
                            {
                                foreach (ObjectId id in br.AttributeCollection)
                                {
                                    AttributeReference ar = trans.GetObject(id, OpenMode.ForRead) as AttributeReference;
                                    if (ar.Tag == "内框名称")
                                    {
                                        if (!string.IsNullOrEmpty(ar.TextString))
                                        {
                                            innerFrameName = ar.TextString;
                                        }
                                        break;
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(innerFrameName))
                            {
                                break;
                            }
                        }
                    }
                    else if (per.Status == PromptStatus.Cancel)
                    {
                        doMark = false;
                    }
                }
                trans.Commit();
            }
            return innerFrameName;
        }
    }
}
