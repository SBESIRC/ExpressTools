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
                form.Close();
            }
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
