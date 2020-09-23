using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.FanSelection.UI
{
    public static class ThFanSelectionDocumentExtension
    {
        public static void ShowModelSelectionDialog(this Document document)
        {
            Form form = null;
            if (document.UserData.ContainsKey(ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI))
            {
                form = document.UserData[ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI] as Form;
            }
            if (form != null && !form.Visible)
            {
                AcadApp.ShowModelessDialog(form);
            }
        }

        public static void CreateModelSelectionDialog(this Document document)
        {
            if (!document.UserData.ContainsKey(ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI))
            {
                var form = new fmFanSelection();
                document.UserData[ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI] = form;
            }
        }

        public static void HideModelSelectionDialog(this Document document)
        {
            Form form = null;
            if (document.UserData.ContainsKey(ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI))
            {
                form = document.UserData[ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI] as Form;
            }
            if (form != null && form.Visible)
            {
                form.Hide();
                HidefmOverView(form);

            }
        }

        private static void HidefmOverView(Form _Form)
        {
            if (_Form.Text == "风机选型")
            {
                var _fmFanSelection = _Form as fmFanSelection;
                if (_fmFanSelection != null)
                {
                    _fmFanSelection.m_fmOverView.Hide();
                }
            }
        }

        public static void CloseModelSelectionDialog(this Document document)
        {
            Form form = null;
            if (document.UserData.ContainsKey(ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI))
            {
                form = document.UserData[ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI] as Form;
                document.UserData.Remove(ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI);
            }
            if (form != null)
            {
                form.Dispose();
            }
        }

        public static void PushModelSelectionDialogVisible(this Document document)
        {
            if (document.UserData.ContainsKey(ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI))
            {
                Form form = document.UserData[ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI] as Form;
                document.UserData[ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI_VISIBLE] = form.Visible;
            }
        }

        public static bool PopModelSelectionDialogVisible(this Document document)
        {
            bool visible = false;
            if (document.UserData.ContainsKey(ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI))
            {
                Form form = document.UserData[ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI] as Form;
                visible = (bool)document.UserData[ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI_VISIBLE];
            }
            return visible;
        }

        public static fmFanSelection Form(this Document document)
        {
            if (document.UserData.ContainsKey(ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI))
            {
                return document.UserData[ThFanSelectionUICommon.DOCUMENT_USER_DATA_UI] as fmFanSelection;
            }
            else
            {
                return null;
            }
        }
    }
}
