using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ThElectrical.Model.ThElement
{
    public class ThOutCableElement : ThElement
    {
        public const string exceptValue = "未知";
        public const string phaseReg = @"x\d+[.]?\d*";
        public const string groundReg = @"[+]E\d+[.]?\d*";
        public const string pipeSizeReg = @"SC\d{2,}";
        public const string ngReg = @"[nN][gG]";

        public bool ContianNG { get; set; }//是否包含NG
        public string Provider { get; set; }//厂商
        public string Number { get; set; }//芯数

        private string _phaseWireStyle;
        public string PhaseWireStyle
        {
            get
            {
                return _phaseWireStyle;
            }
            set
            {
                _phaseWireStyle = value;
                RaisePropertyChanged("PhaseWireStyle");
            }
        }

        //相线规格
        private string _groundWireStyle;
        public string GroundWireStyle
        {
            get
            {
                return _groundWireStyle;
            }
            set
            {
                _groundWireStyle = value;
                RaisePropertyChanged("GroundWireStyle");
            }
        }

        //地线规格
        private string _pipeSizeStyle;
        public string PipeSizeStyle
        {
            get
            {
                return _pipeSizeStyle;
            }
            set
            {
                _pipeSizeStyle = value;
                RaisePropertyChanged("PipeSizeStyle");
            }
        }

        //管材规格
        private string _pipeMatiralStyle;
        public string PipeMatiralStyle
        {
            get
            {
                return _pipeMatiralStyle;
            }
            set
            {
                _pipeMatiralStyle = value;
                RaisePropertyChanged("PipeMatiralStyle");
            }
        }//电缆材质
        public string InstallLocation { get; set; }//敷设部位

        //全部的值
        private string _cableType;


        public string CableType
        {
            get
            {
                return _cableType;
            }
            set
            {
                _cableType = value;
                RaisePropertyChanged("CableType");
            }
        }

        public Point3d MinPoint { get; set; }//左下角点
        public ThOutCableElement(ObjectId id) : base(id)
        {
            using (var tr = id.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var dbText = id.GetObjectByID<DBText>(tr);

                this.CableType = dbText.TextString;
                this.MinPoint = dbText.GeometricExtents.MinPoint;

                //赋予初始值，为未知，如果匹配失败，则依旧是未知
                //this.Provider = ThOutCableElement.exceptValue;
                //this.Number = ThOutCableElement.exceptValue;
                //this.PhaseWireStyle = ThOutCableElement.exceptValue;
                //this.GroundWireStyle = ThOutCableElement.exceptValue;
                //this.InstallLocation = ThOutCableElement.exceptValue;
                //this.PipeSizeStyle= ThOutCableElement.exceptValue;

                SetEachPro2();

                tr.Commit();
            }

        }

        /// <summary>
        /// 为电缆规格具体赋值
        /// </summary>
        /// <param name="text"></param>
        private void SetEachPro(string text)
        {
            //-\d+x\d+[+]*E\d+-,匹配WDZA-YJY-4x4+E4-CT（SC25-SCE/WE）
            //-\d+x\d[.]?\d*[+]E\d+[.]?\d*-

            //匹配第一种:4x4+E4或者3x2.5+E2.5
            var sizeValueReg = Regex.Match(this.CableType, @"-\d+x\d[.]?\d*[+]E\d+[.]?\d*-");

            if (sizeValueReg.Success)
            {
                //取出相线芯数，此模式为总数量-1
                var numberReg = Regex.Match(sizeValueReg.Value, @"\d+x");
                this.Number = numberReg.Success ? (Convert.ToInt32(numberReg.Value.Replace("x", "")) - 1).ToString() : ThOutCableElement.exceptValue;

                //取出规格4x4+E4,3x2.5+E2.5
                var wireStyleReg = Regex.Match(sizeValueReg.Value, @"\d[.]?\d*[+]E\d+[.]?\d*");

                if (wireStyleReg.Success)
                {
                    //进一步得到相线规格
                    this.PhaseWireStyle = wireStyleReg.Value.Right("+");
                    this.GroundWireStyle = wireStyleReg.Value.Left("+");
                }
                //else
                //{
                //    this.PhaseWireStyle = ThOutCableElement.exceptValue;
                //    this.GroundWireStyle = ThOutCableElement.exceptValue;
                //}

            }

        }

        /// <summary>
        /// 各属性赋值
        /// </summary>
        /// <param name="text"></param>
        public void SetEachPro2()
        {
            //匹配相线的规格
            var phaseWireStyleReg = Regex.Match(this.CableType, phaseReg);
            if (phaseWireStyleReg.Success)
            {
                this.PhaseWireStyle = phaseWireStyleReg.Value.Left("x");
            }

            //匹配地线的规格
            var groundWireStyleReg = Regex.Match(this.CableType, groundReg);
            if (groundWireStyleReg.Success)
            {
                this.GroundWireStyle = groundWireStyleReg.Value.Left("+");
            }

            //匹配敷设方式
            var installationStyleReg = Regex.Match(this.CableType, @"SC\d{2,}");
            if (installationStyleReg.Success)
            {
                this.PipeSizeStyle = installationStyleReg.Value;
            }

            //含有NG的就是矿物绝缘
            var pipeSizeReg = Regex.Match(this.CableType, ngReg);
            if (pipeSizeReg.Success)
            {
                this.PipeMatiralStyle = "矿物绝缘电缆穿管";
            }
            else
            {
                this.PipeMatiralStyle = "普通电缆穿管";
            }

        }


        /// <summary>
        /// 重新设定相线的值
        /// </summary>
        /// <param name="text"></param>
        public void ResetByPhase()
        {
            try
            {
                this.CableType = Regex.Replace(this.CableType, phaseReg, "x" + this.PhaseWireStyle);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 重新设定地线的值
        /// </summary>
        public void ResetByGround()
        {
            try
            {
                this.CableType = Regex.Replace(this.CableType, groundReg, "+" + this.GroundWireStyle);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 重新设定管径尺寸
        /// </summary>
        public void ResetByPipeSize()
        {
            //这里替换可能会失败，所以套用try-catch模块，防止产生null，导致系统崩溃
            try
            {
                this.CableType = Regex.Replace(this.CableType, pipeSizeReg, this.PipeSizeStyle);
            }
            catch (Exception)
            {


            }
        }

        /// <summary>
        /// 更新电缆信息
        /// </summary>
        /// <param name="obj"></param>
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

                if (resultRow!=null)
                {
                    //更新相线、地线，并且根据是否包含NG，选择对应的电缆尺寸
                    this.PhaseWireStyle = cableTable.Cells[resultRow.r2.ElementAt(1).Row, resultRow.r2.ElementAt(1).Column].GetRealTextString();

                    this.GroundWireStyle = cableTable.Cells[resultRow.r2.ElementAt(2).Row, resultRow.r2.ElementAt(2).Column].GetRealTextString();

                    this.PipeSizeStyle = Regex.Match(this.CableType, ngReg).Success ? cableTable.Cells[resultRow.r2.ElementAt(4).Row, resultRow.r2.ElementAt(4).Column].GetRealTextString() : cableTable.Cells[resultRow.r2.ElementAt(3).Row, resultRow.r2.ElementAt(3).Column].GetRealTextString();
                }


            }

        }


        public override void UpdateToDwg()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;

            using (doc.LockDocument())
            using (var tr = this.ElementId.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var txt = this.ElementId.GetObjectByID<DBText>(tr, OpenMode.ForWrite);
                txt.TextString = this.CableType;

                tr.Commit();
            }
        }


    }
}
