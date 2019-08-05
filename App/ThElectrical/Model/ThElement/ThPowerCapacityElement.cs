using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;


namespace ThElectrical.Model.ThElement
{

    //功率容量
    public class ThPowerCapacityElement : ThElement
    {
        //标准容量值
        private string _capacityValue;
        public string CapacityValue
        {
            get
            {
                return _capacityValue;
            }
            set
            {
                _capacityValue = value;
                RaisePropertyChanged("CapacityValue");
            }
        }

        //真实功率值
        private string _realCapacity;
        public string RealCapacity
        {
            get
            {
                return _realCapacity;
            }
            set
            {
                _realCapacity = value;
                RaisePropertyChanged("RealCapacity");
            }
        }


        public Point3d Center { get; set; }//中心位置
        public ThPowerCapacityElement(ObjectId id) : base(id)
        {
            using (var tr = id.Database.TransactionManager.StartOpenCloseTransaction())
            {
                this.RealCapacity = id.GetObjectByID<DBText>(tr).TextString;
                this.Center = id.GetObjectByID<DBText>(tr).Get3DCenter();

                SetEachPro();

                tr.Commit();
            }
        }

        public void SetEachPro()
        {
            //转换失败的话，做异常处理
            try
            {
                //找到功率型号里大于等于当前值的最小的一档,就是我们要的选型
                this.CapacityValue = ThELectricalUtils.powerValueRanges.Select(p => Convert.ToDouble(p)).Where(p => p >= Convert.ToDouble(this.RealCapacity)).Min().ToString();
            }
            catch (Exception)
            {

            }

        }

        public void ResetByValue()
        {
            //
            try
            {
                this.RealCapacity = this.CapacityValue;
            }
            catch (Exception)
            {

            }

        }


        public override void UpdateToDwg()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;

            using (doc.LockDocument())
            using (var tr = this.ElementId.Database.TransactionManager.StartOpenCloseTransaction())
            {
                var txt = this.ElementId.GetObjectByID<DBText>(tr, OpenMode.ForWrite);
                txt.TextString = this.RealCapacity;

                tr.Commit();
            }
        }



    }
}
