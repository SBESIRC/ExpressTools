using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;


namespace ThElectrical.Model.ThElement
{
    public class ThBranchSwitchElement : ThElement
    {
        public const string normalProvider = "凯保";
        public const string const_KaiBaoSingleReg = @"KB[O0][^Dd]";
        public const string const_MainCurrentReg = @"\d{2,}C";
        public const string const_BranchSwitchCurrentReg = @"M\d{1,}[.]?\d*";

        //厂商
        private string _provider;
        public string Provider
        {
            get
            {
                return _provider;
            }
            set
            {
                _provider = value;
                RaisePropertyChanged("Provider");
            }
        }

        //主额定电流
        private string _mainCurrent;
        public string MainCurrent
        {
            get
            {
                return _mainCurrent;
            }
            set
            {
                _mainCurrent = value;
                RaisePropertyChanged("MainCurrent");
            }
        }

        //开关电流
        private string _branchSwitchCurrent;
        public string BranchSwitchCurrent
        {
            get
            {
                return _branchSwitchCurrent;
            }
            set
            {
                _branchSwitchCurrent = value;
                RaisePropertyChanged("BranchSwitchCurrent");
            }
        }

        //开关规格
        private string _switchType;
        public string SwitchType
        {
            get
            {
                return _switchType;
            }
            set
            {
                _switchType = value;
                RaisePropertyChanged("SwitchType");
            }
        }
        public Point3d MinPoint { get; set; }//左下角点
        public ThBranchSwitchElement(ObjectId id) : base(id)
        {
            using (var tr = id.Database.TransactionManager.StartOpenCloseTransaction())
            {
                this.SwitchType = id.GetObjectByID<DBText>(tr).TextString;

                this.MinPoint = id.GetObjectByID<DBText>(tr).GeometricExtents.MinPoint;

                SetEachPro();

                tr.Commit();
            }
        }

        /// <summary>
        /// 赋初始值
        /// </summary>
        public void SetEachPro()
        {
            //目前只支持凯保单速
            if (Regex.Match(this.SwitchType, const_KaiBaoSingleReg, RegexOptions.IgnoreCase).Success)
            {
                this.Provider = normalProvider;

                var mainCurrentReg = Regex.Match(this.SwitchType, const_MainCurrentReg);
                if (mainCurrentReg.Success)
                {
                    this.MainCurrent = mainCurrentReg.Value;
                }

                var branchCurrentReg = Regex.Match(this.SwitchType, const_BranchSwitchCurrentReg);
                if (branchCurrentReg.Success)
                {
                    this.BranchSwitchCurrent = branchCurrentReg.Value;
                }
            }
        }

        /// <summary>
        /// 根据主电流重新设定
        /// </summary>
        public void ResetByMainCurrent()
        {
            try
            {
                //在凯保单速的时候才替换
                if (Regex.Match(this.SwitchType, const_KaiBaoSingleReg, RegexOptions.IgnoreCase).Success)
                {
                    this.SwitchType = Regex.Replace(this.SwitchType, const_MainCurrentReg, this.MainCurrent);
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 根据开关电流重新设定
        /// </summary>
        public void ResetByBranchSwitchCurrent()
        {
            try
            {
                //在凯保单速的时候才替换
                if (Regex.Match(this.SwitchType, const_KaiBaoSingleReg, RegexOptions.IgnoreCase).Success)
                {
                    this.SwitchType = Regex.Replace(this.SwitchType, const_BranchSwitchCurrentReg, this.BranchSwitchCurrent);
                }
            }
            catch (Exception)
            {

            }
        }



        public void ReceiveAndUpdate(object obj)
        {
            ThPowerCapacityElement poEle = obj as ThPowerCapacityElement;

            var powerCapacity = poEle.CapacityValue;

            //打开数据库查询
            using (var db = AcadDatabase.Open(ThElectrical.ThELectricalUtils.BlockTemplateFilePath(), DwgOpenMode.ReadOnly))
            {
                //找出两张表的数据，然后根据电流值进行连接，这里注意小于16的一律按照某个值进行统一处理的问题
                var kaiBaoSwitchTable = db.ModelSpace.OfType<Table>().First(t => t.Cells[0, 0].Value != null && t.Cells[0, 0].GetRealTextString() == ThELectricalUtils.kaiBaoTableName);

                var cableTable = db.ModelSpace.OfType<Table>().First(t => t.Cells[0, 0].Value != null && t.Cells[0, 0].GetRealTextString() == ThELectricalUtils.cableTableName);

                var rows = kaiBaoSwitchTable.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => kaiBaoSwitchTable.Rows[i]).Join(cableTable.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => cableTable.Rows[i]), r1 => kaiBaoSwitchTable.Cells[r1.Skip(2).First().Row, r1.Skip(2).First().Column].GetRealTextString().Left("M"), r2 => Convert.ToDouble(cableTable.Cells[r2.First().Row, r2.First().Column].GetRealTextString()), (r1, r2) => new
                {
                    r1,
                    r2
                }, new CompareChildElement<object, string, double>((i, j) =>
                {
                    if (j == 16)
                    {
                        return Convert.ToDouble(i) <= j;
                    }
                    else
                    {
                        return Convert.ToDouble(i) == j;
                    }
                }));

                var resultRow = rows.FirstOrDefault(a => kaiBaoSwitchTable.Cells[a.r1.Skip(3).First().Row, a.r1.Skip(3).First().Column].GetRealTextString() == powerCapacity);

                this.MainCurrent = kaiBaoSwitchTable.Cells[resultRow.r1.ElementAt(1).Row, resultRow.r1.ElementAt(1).Column].GetRealTextString();

                this.BranchSwitchCurrent = kaiBaoSwitchTable.Cells[resultRow.r1.ElementAt(2).Row, resultRow.r1.ElementAt(2).Column].GetRealTextString();
            }

        }

        /// <summary>
        /// 将新值写入
        /// </summary>
        public override void UpdateToDwg()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;

            using (doc.LockDocument())
            using (var tr = this.ElementId.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var txt = this.ElementId.GetObjectByID<DBText>(tr,OpenMode.ForWrite);
                txt.TextString = this.SwitchType;

                tr.Commit();
            }
        }

    }
}
