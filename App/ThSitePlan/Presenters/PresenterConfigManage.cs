using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

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
            List<ColorGeneralDataModel> _List = new List<ColorGeneralDataModel>();

            _List.Add(new ColorGeneralDataModel() { ID = "1", PID = "1", Type = "0", Name = "原始场地线稿", PSD_Color = "255, 215, 0", PSD_Transparency = 55, CAD_Frame = "原始线稿", CAD_Layer = new LayerDataModel() { ID = "1", Name = "P-OUTD" }, CAD_Script = "拍平闭合优化", DataType = "0" });

            _List.Add(new ColorGeneralDataModel() { ID = "2", PID = "2", Type = "1", Name = "建筑物", PSD_Color = "255, 255, 255", PSD_Transparency = 100, CAD_Frame = "NULL", CAD_Layer = null, CAD_Script = "NULL", DataType = "0" });

            _List.Add(new ColorGeneralDataModel() { ID = "3", PID = "2", Type = "1", Name = "高层建筑", PSD_Color = "255, 255, 255", PSD_Transparency = 100, CAD_Frame = "NULL", CAD_Layer =null, CAD_Script = "NULL", DataType = "0" });
            _List.Add(new ColorGeneralDataModel() { ID = "4", PID = "3", Type = "0", Name = "建筑信息", PSD_Color = "135, 206, 235", PSD_Transparency = 21, CAD_Frame = "建筑信息", CAD_Layer = new LayerDataModel() { ID = "2", Name = "P-OUTD-F" } , CAD_Script = "无" });
            _List.Add(new ColorGeneralDataModel() { ID = "5", PID = "3", Type = "0", Name = "建筑线稿", PSD_Color = "50, 205, 50", PSD_Transparency = 51, CAD_Frame = "建筑线稿", CAD_Layer = new LayerDataModel() { ID = "3", Name = "P-OUTD-B" } , CAD_Script = "拍平闭合优化", DataType = "0" });
            _List.Add(new ColorGeneralDataModel() { ID = "6", PID = "3", Type = "0", Name = "建筑色块", PSD_Color = "255, 215, 0", PSD_Transparency = 100, CAD_Frame = "建筑色块", CAD_Layer = new LayerDataModel() { ID = "3", Name = "P-OUTD-B" }, CAD_Script = "线稿生成填充", DataType = "0" });

            _List.Add(new ColorGeneralDataModel() { ID = "7", PID = "2", Type = "1", Name = "多层建筑", PSD_Color = "255, 255, 255", PSD_Transparency = 100, CAD_Frame = "NULL", CAD_Layer = null, CAD_Script = "NULL", DataType = "0" });
            _List.Add(new ColorGeneralDataModel() { ID = "8", PID = "7", Type = "0", Name = "建筑信息", PSD_Color = "135, 206, 235", PSD_Transparency = 100, CAD_Frame = "建筑信息", CAD_Layer = new LayerDataModel() { ID = "2", Name = "P-OUTD-F" }, CAD_Script = "无", DataType = "0" });
            _List.Add(new ColorGeneralDataModel() { ID = "9", PID = "7", Type = "0", Name = "建筑线稿", PSD_Color = "50, 205, 50", PSD_Transparency = 100, CAD_Frame = "建筑线稿", CAD_Layer = new LayerDataModel() { ID = "3", Name = "P-OUTD-B" }, CAD_Script = "拍平闭合优化", DataType = "0" });
            _List.Add(new ColorGeneralDataModel() { ID = "10", PID = "7", Type = "0", Name = "建筑色块", PSD_Color = "255, 215, 0", PSD_Transparency = 100, CAD_Frame = "建筑色块", CAD_Layer = new LayerDataModel() { ID = "3", Name = "P-OUTD-B" }, CAD_Script = "线稿生成填充", DataType = "0" });


            _List.Add(new ColorGeneralDataModel() { ID = "11", PID = "11", Type = "0", Name = "全局阴影", PSD_Color = "112, 128, 144", PSD_Transparency = 100, CAD_Frame = "全局阴影", CAD_Layer = new LayerDataModel() { ID = "4", Name = "P-AI" }, CAD_Script = "虚拟阴影", DataType = "0" });

            _List.Add(new ColorGeneralDataModel() { ID = "12", PID = "11", Type = "1", Name = "树木", PSD_Color = "255, 255, 255", PSD_Transparency = 100, CAD_Frame = "NULL", CAD_Layer = null, CAD_Script = "NULL", DataType = "0" });
            _List.Add(new ColorGeneralDataModel() { ID = "13", PID = "12", Type = "1", Name = "行道树", PSD_Color = "255, 255, 255", PSD_Transparency = 100, CAD_Frame = "NULL", CAD_Layer = null, CAD_Script = "NULL", DataType = "0" });


            _List.Add(new ColorGeneralDataModel() { ID = "14", PID = "13", Type = "0", Name = "树木线稿", PSD_Color = "46, 139, 87", PSD_Transparency = 100, CAD_Frame = "树木线稿", CAD_Layer = new LayerDataModel() { ID = "5", Name = "P-Land" } , CAD_Script = "拍平闭合优化", DataType = "0" });
            _List.Add(new ColorGeneralDataModel() { ID = "15", PID = "13", Type = "0", Name = "树木色块", PSD_Color = "46, 139, 87", PSD_Transparency = 100, CAD_Frame = "树木色块", CAD_Layer = new LayerDataModel() { ID = "5", Name = "P-Land" }, CAD_Script = "线稿生成填充", DataType = "0" });

            return _List;
        }
    }
}
