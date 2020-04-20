using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using acadApp = Autodesk.AutoCAD.ApplicationServices;
using ThColumnInfo.ViewModel;
using ThColumnInfo.View;
using System.Windows.Forms;

[assembly: CommandClass(typeof(ThColumnInfo.ThColumnInfoCommands))]
[assembly: ExtensionApplication(typeof(ThColumnInfo.ThColumnInfoApp))]
namespace ThColumnInfo
{
    public class ThColumnInfoApp : IExtensionApplication
    {
        public void Initialize()
        {
            DocumentCollection dc = acadApp.Application.DocumentManager;
            dc.DocumentDestroyed += Dc_DocumentDestroyed;
            dc.DocumentActivated += Dc_DocumentActivated;
        }

        private void Dc_DocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            if (e.Document == null)
            {
                return;
            }
            try
            {
                bool hasCurrentFileName = false;
                foreach (TreeNode tn in CheckPalette._checkResult.tvCheckRes.Nodes)
                {
                    ThStandardSignManager thStandardSignManager = tn.Tag as ThStandardSignManager;
                    if (thStandardSignManager == null)
                    {
                        continue;
                    }
                    if (thStandardSignManager.DocPath == e.Document.Name)
                    {
                        hasCurrentFileName = true;
                        break;
                    }
                }
                if(!hasCurrentFileName)
                {
                    if(DataPalette.ShowPaletteMark)
                    {
                        DataPalette._dateResult.ClearDataGridView();
                    }
                }
                else
                {
                    if(DataPalette.ShowPaletteMark)
                    {
                        CheckPalette._checkResult.ExportDetailData(CheckPalette._checkResult.LastShowDetailNode);
                    }
                }
                //if (CheckPalette._ps!=null && !CheckPalette._ps.IsDisposed)
                //{
                //    CheckPalette._ps.Visible = hasCurrentFileName;
                //}
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "Dc_DocumentActivated");
            }
        }

        private void Dc_DocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
            try
            {
                if (CheckPalette._checkResult == null || CheckPalette._checkResult.tvCheckRes == null ||
                    CheckPalette._checkResult.tvCheckRes.Nodes.Count == 0)
                {
                    return;
                }
                if (CheckPalette._checkResult.tvCheckRes.Nodes[0].Tag == null)
                {
                    return;
                }
                for (int i = 0; i < CheckPalette._checkResult.tvCheckRes.Nodes.Count; i++)
                {
                    ThStandardSignManager thStandardSignManager =
                        CheckPalette._checkResult.tvCheckRes.Nodes[i].Tag as ThStandardSignManager;
                    if(thStandardSignManager==null)
                    {
                        continue;
                    }
                    if(thStandardSignManager.DocPath== e.FileName)
                    {
                        CheckPalette._checkResult.tvCheckRes.Nodes.RemoveAt(i);
                        break;
                    }
                }
                if(CheckPalette._checkResult.tvCheckRes.Nodes.Count==0)
                {
                    CheckPalette._ps.Visible = false;
                    DataPalette._ps.Visible = false;
                }
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "Dc_DocumentDestroyed");
            }
        }
        public void Terminate()
        {
            DocumentCollection dc = acadApp.Application.DocumentManager;
            dc.DocumentDestroyed -= Dc_DocumentDestroyed;
            dc.DocumentActivated -= Dc_DocumentActivated;
        }
    }
    public class ThColumnInfoCommands
    {
        [CommandMethod("TIANHUACAD", "ThCPI", CommandFlags.Modal)]
        public void ThColumnParameterSet()
        {
            if(ParameterSetVM.isOpened)
            {
                return;
            }
            try
            {
                ParameterSetVM parameterSetVM = new ParameterSetVM();
                if(parameterSetVM.ParaSetInfo.FloorCount==0 && CheckPalette._checkResult.tvCheckRes.SelectedNode!=null)
                {
                    if(CheckPalette._checkResult.tvCheckRes.SelectedNode.Tag!=null &&
                        CheckPalette._checkResult.tvCheckRes.SelectedNode.Tag.GetType()==typeof(ThStandardSignManager))
                    {
                        ThStandardSignManager tssm = CheckPalette._checkResult.tvCheckRes.SelectedNode.Tag as ThStandardSignManager;
                        parameterSetVM.ParaSetInfo.FloorCount = tssm.StandardSigns.Count;
                    }
                }
                ParameterSet parameterSet = new ParameterSet(parameterSetVM);
                parameterSetVM.Owner = parameterSet;
                parameterSet.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                parameterSet.Topmost = true;
                parameterSet.Show();
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex);
                ParameterSetVM.isOpened = false;
            }
        }
        
        [CommandMethod("TIANHUACAD", "ThCRC", CommandFlags.Modal)]
        public void ThColumnInfoCrc()
        {
            try
            {
                //显示结果
                CheckPalette.Instance.Show();
            }
            catch (System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "ThColumnInfoCrc");
            }
        }
    }
}
