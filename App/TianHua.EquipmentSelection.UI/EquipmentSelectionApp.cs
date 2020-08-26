using System;
using AcHelper;
using Linq2Acad;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.FanSelection.UI
{
    public class EquipmentSelectionApp : IExtensionApplication
    {
        private static string _customCmd = null;
        private static bool _runCustomCommand = false;
        private static ObjectId _selectedEntId = ObjectId.Null;
        private static CustomCommandMappers _customCommands = null;

        public void Initialize()
        {
            AddDoubleClickHandler();
        }

        public void Terminate()
        {
            RemoveDoubleClickHandler();
        }

        [CommandMethod("TIANHUACAD", "THFJ", CommandFlags.Modal)]
        public void ThEquipmentSelection()
        {
            var dwgName = Convert.ToInt32(AcadApp.GetSystemVariable("DWGTITLED"));
            if (dwgName == 0)
            {
                AcadApp.ShowAlertDialog("请先保存当前图纸!");
                return;
            }
            AcadApp.ShowModelessDialog(fmFanSelection.GetInstance());
        }

        [CommandMethod("TIANHUACAD", "THFJZH", CommandFlags.Modal)]
        public void ThEquipmentConvert()
        {
            fmConvert _fmConvert = new fmConvert();
            AcadApp.ShowModelessDialog(_fmConvert);
        }


        [CommandMethod("TIANHUACAD", "THFJEDIT", CommandFlags.UsePickSet)]
        public void ThEquipmentEdit()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                ObjectId entId = GetSelectedEntity();
                if (!entId.IsNull)
                {
                    var _Form = fmFanSelection.GetInstance();
                    AcadApp.ShowModelessDialog(_Form);
                    _Form.ShowFormByID(entId.GetModelIdentifier());
                }
            }
        }

        private ObjectId GetSelectedEntity()
        {
            PromptSelectionResult res = Active.Editor.GetSelection();
            if (res.Status == PromptStatus.OK)
            {
                return res.Value.GetObjectIds()[0];
            }
            else
            {
                return ObjectId.Null;
            }
        }

        private static void AddDoubleClickHandler()
        {
            AcadApp.BeginDoubleClick += Application_BeginDoubleClick;
            AcadApp.DocumentManager.DocumentDestroyed += DocumentManager_DocumentDestroyed;
            AcadApp.DocumentManager.DocumentBecameCurrent += DocumentManager_DocumentBecameCurrent;
            AcadApp.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;
            AcadApp.DocumentManager.DocumentLockModeChangeVetoed += DocumentManager_DocumentLockModeChangeVetoed;

            //Load custom command mappers
            _customCommands = CustomCommandsFactory.CreateDefaultCustomCommandMappers();
        }

        private static void RemoveDoubleClickHandler()
        {
            AcadApp.BeginDoubleClick -= Application_BeginDoubleClick;
            AcadApp.DocumentManager.DocumentDestroyed -= DocumentManager_DocumentDestroyed;
            AcadApp.DocumentManager.DocumentBecameCurrent -= DocumentManager_DocumentBecameCurrent;
            AcadApp.DocumentManager.DocumentLockModeChanged -= DocumentManager_DocumentLockModeChanged;
            AcadApp.DocumentManager.DocumentLockModeChangeVetoed -= DocumentManager_DocumentLockModeChangeVetoed;
        }

        private static void DocumentManager_DocumentBecameCurrent(object sender, DocumentCollectionEventArgs e)
        {
            var dwgName = Convert.ToInt32(AcadApp.GetSystemVariable("DWGTITLED"));
            var _fmFanSelection = fmFanSelection.GetInstance();
            _fmFanSelection.ReLoad();
            if (dwgName == 0)
            {
                _fmFanSelection.Hide();
                return;
            }
    
            //AcadApp.ShowModelessDialog(_fmFanSelection);
        }

        private static void DocumentManager_DocumentDestroyed(object sender, DocumentDestroyedEventArgs e)
        {
            if (AcadApp.DocumentManager.Count == 1)
            {
                fmFanSelection.GetInstance().Hide();
            }
        }

        private static void Application_BeginDoubleClick(object sender, BeginDoubleClickEventArgs e)
        {
            _customCmd = null;
            _selectedEntId = ObjectId.Null;

            //Get entity which user double-clicked on
            PromptSelectionResult res = Active.Editor.SelectAtPickBox(e.Location);
            if (res.Status == PromptStatus.OK)
            {
                ObjectId[] ids = res.Value.GetObjectIds();

                //Only when there is one entity selected, we go ahead to see
                //if there is a custom command supposed to target at this entity
                if (ids.Length == 1)
                {
                    //Find mapped custom command name
                    string cmd = _customCommands.GetCustomCommand(ids[0]);
                    if (!string.IsNullOrEmpty(cmd))
                    {
                        _selectedEntId = ids[0];
                        _customCmd = cmd;

                        if (System.Convert.ToInt32(Application.GetSystemVariable("DBLCLKEDIT")) == 0)
                        {
                            //Since "Double click editing" is not enabled, we'll
                            //go ahead to launch our custom command
                            LaunchCustomCommand(Active.Editor);
                        }
                        else
                        {
                            //Since "Double Click Editing" is enabled, a command
                            //defined in CUI/CUIX will be fired. Let the code return
                            //and wait the DocumentLockModeChanged and
                            //DocumentLockModeChangeVetoed event handlers do their job
                            return;
                        }
                    }
                }
            }
        }

        private static void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
        {
            _runCustomCommand = false;
            if (!e.GlobalCommandName.StartsWith("#"))
            {
                // Lock状态，可以看做命令开始状态
                var cmdName = e.GlobalCommandName;

                // 过滤"EATTEDIT"命令
                if (!cmdName.ToUpper().Equals("EATTEDIT"))
                {
                    return;
                }

                if (!_selectedEntId.IsNull &&
                    !string.IsNullOrEmpty(_customCmd) &&
                    !cmdName.ToUpper().Equals(_customCmd.ToUpper()))
                {
                    e.Veto();
                    _runCustomCommand = true;
                }
            }
        }

        private static void DocumentManager_DocumentLockModeChangeVetoed(object sender, DocumentLockModeChangeVetoedEventArgs e)
        {
            if (_runCustomCommand)
            {
                //Start custom command
                LaunchCustomCommand(Active.Editor);
            }
        }

        private static void LaunchCustomCommand(Editor ed)
        {
            //Create implied a selection set
            ed.SetImpliedSelection(new ObjectId[] { _selectedEntId });

            string cmd = _customCmd;

            _customCmd = null;
            _selectedEntId = ObjectId.Null;

            //Start the custom command which has UsePickSet flag set
            Active.Document.SendStringToExecute(string.Format("{0} ", cmd), true, false, false);
        }
    }
}