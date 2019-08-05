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

        [CommandMethod("TIANHUACAD", "THLDC", CommandFlags.Modal)]
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
    }
}
