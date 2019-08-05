using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThElectrical.View;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

//[assembly: CommandClass(typeof(ThElectrical.ThElectricalExchangeCommands))]
//[assembly: ExtensionApplication(typeof(ThElectrical.ThElectricalExchangeApp))]
namespace ThElectrical
{
    public class ThElectricalExchangeApp
    {
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }

        [CommandMethod("TIANHUACAD", "thlixuyin", CommandFlags.Modal)]
        public void ExchangeCmd()
        {
            //将程序有效期验证为3个月，一旦超过时限，要求用户更新，不进行命令注册
            var usualDate = new DateTime(2019, 7, 1);
            var dateTime = DateTime.Today;
            if ((dateTime - usualDate).Days > 62)
            {
                return;
            }

            var view = new ThElectricalExchangeView();
            AcadApp.ShowModelessWindow(view);
        }

        //[CommandMethod("TIANHUACAD", "thlixuyin", CommandFlags.Modal)]
        public void GG()
        {
            //打开数据库查询
            using (var db = AcadDatabase.Open(ThElectrical.ThELectricalUtils.BlockTemplateFilePath(), DwgOpenMode.ReadOnly))
            {
                var kaiBaoSwitchTable = db.ModelSpace.OfType<Table>().First(t => t.Cells[0, 0].Value != null && t.Cells[0, 0].GetRealTextString() == ThELectricalUtils.kaiBaoTableName);

                var cableTable = db.ModelSpace.OfType<Table>().First(t => t.Cells[0, 0].Value != null && t.Cells[0, 0].GetRealTextString() == ThELectricalUtils.cableTableName);

                cableTable.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => kaiBaoSwitchTable.Rows[i]).ForEach(r =>
                {
                    foreach (var item in r)
                    {
                        System.Windows.Forms.MessageBox.Show(cableTable.Cells[item.Row, item.Column].GetRealTextString());
                    }
                });


                //var rows = kaiBaoSwitchTable.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => kaiBaoSwitchTable.Rows[i]).Join(cableTable.Rows.Select((r, i) => i).Where(i => i > 1).Select(i => kaiBaoSwitchTable.Rows[i]), r1 => kaiBaoSwitchTable.Cells[r1.Skip(2).First().Row, r1.Skip(2).First().Column].GetRealTextString().Left("M"), r2 => Convert.ToDouble(cableTable.Cells[r2.First().Row, r2.First().Column].GetRealTextString()), (r1, r2) => new
                //{
                //    r1,
                //    r2
                //}, new CompareChildElement<object, string, double>((i, j) =>
                //   {
                //       if (j == 16)
                //       {
                //           return Convert.ToDouble(i) <= j;
                //       }
                //       else
                //       {
                //           return Convert.ToDouble(i) == j;
                //       }
                //   }));


                //rows.ForEach(r =>
                //{
                //    foreach (var item in r.r1)
                //    {
                //        System.Windows.Forms.MessageBox.Show(kaiBaoSwitchTable.Cells[item.Row, item.Column].GetRealTextString());
                //    }

                //    foreach (var item in r.r2)
                //    {
                //        System.Windows.Forms.MessageBox.Show(cableTable.Cells[item.Row, item.Column].GetRealTextString());
                //    }

                //});
            }

        }

    }


    //public class ThElectricalExchangeCommands
    //{

    //}
}
