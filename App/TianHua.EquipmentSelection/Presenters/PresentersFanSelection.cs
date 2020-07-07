using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianHua.Publics.BaseCode;

namespace TianHua.FanSelection
{
    public class PresentersFanSelection : Presenter<IFanSelection>
    {
        public PresentersFanSelection(IFanSelection View) : base(View)
        {

        }

        public override void OnViewEvent()
        {

        }

        public override void OnViewLoaded()
        {
            View.m_ListScenario = InitScenario();

            View.m_ListFan = InitFan();

            View.m_ListVentStyle = GetVentStyle();

            View.m_ListVentConnect = GetVentConnect();

            View.m_ListVentLev = GetVentLev();

            View.m_ListEleLev = GetEleLev();

            View.m_ListMotorTempo = GetMotorTempo();

            View.m_ListMountType = GetMountType();

        }

        private List<FanDataModel> InitFan()
        {
            List<FanDataModel> _List = new List<FanDataModel>();

            //{
            //    FanDataModel _FanDataModel = new FanDataModel()
            //    {
            //        Scenario = EnumScenario.厨房排油烟.ToString(),
            //        ID = Guid.NewGuid().ToString(),
            //        PID = "0",
            //        Name = "地下1层厨房",
            //        InstallSpace = "",
            //        InstallFloor = "B1",
            //        VentNum = "1",
            //        VentQuan = 0,
            //        Remark = "",
            //        AirVolume = 8700,
            //        WindResis = 358,
            //        VentStyle = "前倾离心",
            //        VentConnect = "皮带",
            //        VentLev = "2级",
            //        EleLev = "2级",
            //        MotorTempo = 1450,
            //        FanModelName = "9A-4/030",
            //        MountType = "吊装",
            //    };
            //    _List.Add(_FanDataModel);
            //}

            //{
            //    FanDataModel _FanDataModel = new FanDataModel()
            //    {
            //        Scenario = EnumScenario.厨房排油烟.ToString(),
            //        ID = Guid.NewGuid().ToString(),
            //        PID = "0",
            //        Name = "地下2层厨房",
            //        InstallSpace = "",
            //        InstallFloor = "F1",
            //        VentNum = "1",
            //        VentQuan = 0,
            //        Remark = "",
            //        AirVolume = 8700,
            //        WindResis = 338,
            //        VentStyle = "前倾离心",
            //        VentConnect = "皮带",
            //        VentLev = "1级",
            //        EleLev = "1级",
            //        MotorTempo = 1450,
            //        FanModelName = "9A-4/030",
            //        MountType = "吊装",
            //    };
            //    _List.Add(_FanDataModel);
            //}

            return _List;
        }

        private List<string> InitScenario()
        {
            List<string> _EnumList = new List<string>();
            foreach (var _Item in Enum.GetValues(typeof(EnumScenario)))
            {
                string _Enum = string.Empty;
                _Enum = FuncStr.NullToStr(_Item);
                _EnumList.Add(_Enum);
            }
            return _EnumList;
        }


        public List<string> GetVentStyle()
        {
            List<string> _List = new List<string>();
            _List.Add("前倾离心");
            _List.Add("后倾离心");
            _List.Add("轴流");
            //_List.Add("混流");
            return _List;
        }


        public List<string> GetVentConnect()
        {
            List<string> _List = new List<string>();
            _List.Add("皮带");
            _List.Add("直连");
            return _List;
        }


        public List<string> GetVentLev()
        {
            List<string> _List = new List<string>();
            _List.Add("1级");
            _List.Add("2级");
            _List.Add("3级");
            return _List;
        }

        public List<string> GetEleLev()
        {
            List<string> _List = new List<string>();
            _List.Add("1级");
            _List.Add("2级");
            _List.Add("3级");
            return _List;
        }

        public List<int> GetMotorTempo()
        {
            List<int> _List = new List<int>();
            _List.Add(2900);
            _List.Add(1450);
            _List.Add(960);
            return _List;
        }

        public List<string> GetMountType()
        {
            List<string> _List = new List<string>();
            _List.Add("吊装");
            _List.Add("落地");
            return _List;
        }

    }
}
