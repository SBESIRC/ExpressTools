using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ThSitePlan.Configuration;
using TianHua.Publics.BaseCode;

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
            List<string> _List = new List<string>();
            _List.Add("无");
            _List.Add("拍平闭合优化");
            _List.Add("线稿生成填充");
            _List.Add("虚拟阴影");
            return _List;

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

        private List<ColorGeneralDataModel> InitColorGeneral()
        {
            string _Txt = FuncStr.NullToStr(Properties.Resources.BasicStyle);
            var _ListColorGeneral = FuncJson.Deserialize<List<ColorGeneralDataModel>>(_Txt);
            ThSitePlanConfigItemGroup Root = new ThSitePlanConfigItemGroup();
            Root.Properties.Add("Name", "天华彩总");
            FuncFile.ToConfigItemGroup(_ListColorGeneral, Root);
            return _ListColorGeneral;
        }

        public void UpdateConfig()
        {
 
        }

    }
}
