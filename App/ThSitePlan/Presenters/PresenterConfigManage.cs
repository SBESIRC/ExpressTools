using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ThSitePlan.Configuration;
using TianHua.Publics.BaseCode;

using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.Windows;
using AcHelper;
using Linq2Acad;

namespace ThSitePlan
{
    public class PresenterConfigManage : Presenter<IConfigManage>
    {
        public PresenterConfigManage(IConfigManage View) : base(View)
        {

        }

        public override void OnViewEvent()
        {

        }

        public override void OnViewLoaded()
        {
            View.m_ListColorGeneral = InitColorGeneral();

            View.m_ListLayer = InitLayer();

            View.m_ListScript = InitScript();

        }

        private List<string> InitScript()
        {
            List<string> _EnumList = new List<string>();
            foreach (var _Item in Enum.GetValues(typeof(EnumCADScript)))
            {
                string _Enum = string.Empty;
                _Enum = FuncStr.NullToStr(_Item);
                _EnumList.Add(_Enum);
            }
            return _EnumList;
        }

        private List<LayerDataModel> InitLayer()
        {
            List<LayerDataModel> _List = new List<LayerDataModel>();
            _List.Add(new LayerDataModel() { ID = "1", Name = "P-OUTD" });
            _List.Add(new LayerDataModel() { ID = "2", Name = "P-OUTD-F" });
            _List.Add(new LayerDataModel() { ID = "3", Name = "P-OUTD-B" });
            _List.Add(new LayerDataModel() { ID = "4", Name = "P-AI" });
            _List.Add(new LayerDataModel() { ID = "5", Name = "P-Land" });
            return _List;
        }

        public List<ColorGeneralDataModel> InitColorGeneral()
        {
            string _Txt = FuncStr.NullToStr(View.m_ColorGeneralConfig);
            if (!string.IsNullOrEmpty(_Txt))
            {
                var _ListColorGeneral = FuncJson.Deserialize<List<ColorGeneralDataModel>>(_Txt);

                ThSitePlanConfigItemGroup Root = new ThSitePlanConfigItemGroup();
                Root.Properties.Add("Name", ThSitePlanCommon.ThSitePlan_Frame_Name_Unused);
                FuncFile.ToConfigItemGroup(_ListColorGeneral, Root);
                SetImgType(_ListColorGeneral);
                return _ListColorGeneral;
            }

            return null;
        }


        public List<ColorGeneralDataModel> SetImgType(List<ColorGeneralDataModel> _List)
        {
            if (_List == null || _List.Count == 0) { return new List<ColorGeneralDataModel>(); }
            _List.ForEach(p =>
            {
                var _ListTemp = _List.FindAll(s => FuncStr.NullToStr(p.ID) == FuncStr.NullToStr(s.PID)
                   && FuncStr.NullToInt(s.CAD_ScriptID) > 0);
                if (_ListTemp != null && _ListTemp.Count > 0)
                    p.ImgType = "1";
            });
            return _List;
        }



        public void UpdateConfig()
        {

        }

        public List<string> AddLayer(IntPtr hWnd)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                List<string> laylist = new List<string>();
                while (true)
                {
                    PromptEntityOptions options = new PromptEntityOptions("\n请选择对象")
                    {
                        AllowNone = true,
                    };
                    var prs = Active.Editor.GetEntity(options);

                    if (prs.Status == PromptStatus.OK)
                    {
                        Entity entity = acadDatabase.Element<Entity>(prs.ObjectId);
                        laylist.Add(entity.Layer);
                    }
                    else if (prs.Status == PromptStatus.None)
                    {
                        break;
                    }
                    else
                    {
                        return new List<string>();
                    }
                }
                laylist = laylist.Distinct().ToList();
                return laylist;
            }
        }
    }
}
