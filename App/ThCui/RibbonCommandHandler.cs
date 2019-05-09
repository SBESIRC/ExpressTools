using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using System;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.AutoCAD.ThCui
{
    public class RibbonCommandHandler : System.Windows.Input.ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;//确定此命令可以在其当前状态下执行
        }
        //当出现影响是否应执行该命令的更改时发生
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            //获取发出命令的按钮对象
            RibbonButton button = parameter as RibbonButton;
            //如果发出命令的不是按钮或按钮未定义命令参数，则返回
            if (button == null || button.CommandParameter == null) return;

            //根据按钮的命令参数，执行对应的AutoCAD命令
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            #region 正确匹配esc次数，但不能捕获lsp的command，先放着
            ////执行之前，先根据实际运行情况，决定按esc的次数
            //string esc = "";
            //string cmds = (string)AcadApp.GetSystemVariable("CMDNAMES");
            //if (cmds.Length > 0)
            //{
            //    int cmdNum = cmds.Split(new char[] { '\'' }).Length;
            //    for (int i = 0; i < cmdNum; i++)
            //        esc += '\x03';
            //} 
            #endregion
            //执行命令，先退出所有当前命令，再执行
            doc.SendStringToExecute("\x03\x03" + button.CommandParameter.ToString()+" ", true, false, true);
        }
    }
}
