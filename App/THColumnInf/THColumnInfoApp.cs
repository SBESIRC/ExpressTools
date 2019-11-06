using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System.Collections.Generic;
using System.Text;

[assembly: CommandClass(typeof(ThColumnInfo.ThColumnInfoCommands))]
[assembly: ExtensionApplication(typeof(ThColumnInfo.ThColumnInfoApp))]
namespace ThColumnInfo
{
    public class ThColumnInfoApp : IExtensionApplication
    {
        public void Initialize()
        {
            //throw new NotImplementedException();
        }

        public void Terminate()
        {
            //throw new NotImplementedException();
        }
    }
    public class ThColumnInfoCommands
    {
        [CommandMethod("TIANHUACAD", "ThCic", CommandFlags.Modal)]
        public void ThColumnInfoCheckWindow()
        {
            //CheckPalette.Instance.Show();
            //DataPalette.Instance.Show();
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptPointResult ppr1 = ed.GetPoint("\n请选择要校对的柱子第一个范围点");
            if(ppr1.Status!=PromptStatus.OK)
            {
                return;
            }
            PromptPointResult ppr2 = ed.GetCorner("\n请选择要校对的柱子第二个范围点", ppr1.Value);
            if (ppr2.Status != PromptStatus.OK)
            {
                return;
            }
            Point3d firstPt = ppr1.Value;
            Point3d secondPt = ppr2.Value;
            try
            {
                //获取本地柱子的位置和柱表信息
                IDataSource dataSource = new ExtractColumnPosition(firstPt, secondPt, ExtractColumnDetailInfoMode.InSitu);
                dataSource.Extract(); 
                //获取数据库的信息
                string dbPath = @"D:\1111\dtlmodel.ydb";
                IDatabaseDataSource dbDataSource = new ExtractYjkColumnInfo(dbPath);
                dbDataSource.Extract(1);
                //让用户指定柱子的位置
                ThDrawColumns thDrawColumns = new ThDrawColumns(dbDataSource.ColumnInfs);
                thDrawColumns.Draw();
                //位置确定后，关联本地柱子
                ThRelateColumn thRelateColumn = new ThRelateColumn(dataSource.ColumnInfs, thDrawColumns.ColumnRelateInfs);
                thRelateColumn.Relate();
            }
            catch(System.Exception ex)
            {
            }            
        }
    }
}
