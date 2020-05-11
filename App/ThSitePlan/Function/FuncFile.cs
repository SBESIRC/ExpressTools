using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ThSitePlan.Configuration;
using TianHua.Publics.BaseCode;

namespace ThSitePlan
{
    public static class FuncFile
    {
        /// <summary>
        /// 获取ApplicationData的运行路径
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationData()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }


        public static string ReadTxt(string _Path)
        {
            try
            {
                using (StreamReader _StreamReader = File.OpenText(_Path))
                {
                    return _StreamReader.ReadToEnd();
                }
                //StreamReader _StreamReader = new StreamReader(_Path, Encoding.Default);

                //return _StreamReader.ReadToEnd();
            }
            catch
            {
                return string.Empty;

            }
        }

        public static void Write(string _Path, string _JSON)
        {
            try
            {
                FileStream _FileStream = new FileStream(_Path, FileMode.Create);
                StreamWriter _StreamWriter = new StreamWriter(_FileStream);
                _StreamWriter.Write(_JSON);
                _StreamWriter.Flush();
                _StreamWriter.Close();
                _FileStream.Close();
            }
            catch
            {


            }
        }



        public static void ToConfigItemGroup(List<ColorGeneralDataModel> _List, ThSitePlanConfigItemGroup _ConfigItemGroup)
        {
            if (_List == null || _List.Count == 0) { return; }

            _List.ForEach(p =>
            {
                if (p.ID == p.PID)
                {

                    if (p.Type == "0")
                    {
                        _ConfigItemGroup.AddItem(new ThSitePlanConfigItem()
                        {
                            Properties = new Dictionary<string, object>()
                                {
                                    { "Name", FuncStr.NullToStr( p.Name)},
                                    { "Color", p.PSD_Color},
                                    { "Opacity", p.PSD_Transparency },
                                    { "CADFrame", p.CAD_Frame },
                                    { "CADLayer", p.CAD_Layer }
                                }
                        });
                    }
                    else
                    {
                        ThSitePlanConfigItemGroup _ThSitePlanConfigItemGroup = new ThSitePlanConfigItemGroup()
                        {
                            Properties = new Dictionary<string, object>()
                                {
                                    { "Name", FuncStr.NullToStr( p.Name)},
                                    { "Opacity", p.PSD_Transparency },
                                }
                        };
                        _ConfigItemGroup.AddGroup(_ThSitePlanConfigItemGroup);
                        ToConfigItem(_List, p, _ThSitePlanConfigItemGroup);

                    }



                }
            });
        }


        public static void ToConfigItem(List<ColorGeneralDataModel> _ListColorGeneral, ColorGeneralDataModel _ColorGeneral, ThSitePlanConfigItemGroup _ConfigItemGroup)
        {
            if (_ColorGeneral == null || _ListColorGeneral == null || _ListColorGeneral.Count == 0) { return; }
            var _List = _ListColorGeneral.FindAll(p => p.PID == _ColorGeneral.ID && p.ID != _ColorGeneral.ID);
            _List.ForEach(p =>
            {
                if (p.Type == "0")
                {
                    _ConfigItemGroup.AddItem(new ThSitePlanConfigItem()
                    {
                        Properties = new Dictionary<string, object>()
                                {
                                    { "Name", FuncStr.NullToStr( p.Name)},
                                    { "Color", p.PSD_Color},
                                    { "Opacity", p.PSD_Transparency },
                                    { "CADFrame", p.CAD_Frame },
                                    { "CADLayer", LayerListToListStr(p.CAD_Layer) }
                                }
                    });
                }
                else
                {
                    ThSitePlanConfigItemGroup _ThSitePlanConfigItemGroup = new ThSitePlanConfigItemGroup()
                    {
                        Properties = new Dictionary<string, object>()
                                {
                                    { "Name", FuncStr.NullToStr( p.Name)},
                                    { "Opacity", p.PSD_Transparency },
                                }
                    };
                    _ConfigItemGroup.AddGroup(_ThSitePlanConfigItemGroup);
                    ToConfigItem(_ListColorGeneral, p, _ThSitePlanConfigItemGroup);
                }

            });
        }

        public static List<string> LayerListToListStr(List<LayerDataModel> _List)
        {
            if (_List == null || _List.Count == 0) { return new List<string>(); }
            List<string> _ListStr = new List<string>();
            _List.ForEach(p => _ListStr.Add(FuncStr.NullToStr(p.Name)));
            return _ListStr;
        }

        //加密
        public static byte[] Encryption(string Express, string _Key)
        {
            CspParameters _CspParameters = new CspParameters();
            _CspParameters.KeyContainerName = _Key;//密匙容器的名称，保持加密解密一致才能解密成功
            using (RSACryptoServiceProvider _Rsa = new RSACryptoServiceProvider(_CspParameters))
            {
                byte[] _Plaindata = Encoding.Default.GetBytes(Express);//将要加密的字符串转换为字节数组
                byte[] _Encryptdata = _Rsa.Encrypt(_Plaindata, false);//将加密后的字节数据转换为新的加密字节数组
                return _Encryptdata;//将加密后的字节数组转换为字符串
            }
        }

        //解密
        public static string Decrypt(byte[] Ciphertext, string _Key)
        {
            CspParameters _CspParameters = new CspParameters();
            _CspParameters.KeyContainerName = _Key;
            using (RSACryptoServiceProvider _Rsa = new RSACryptoServiceProvider(_CspParameters))
            {
                byte[] _Encryptdata = Ciphertext;
                byte[] _Decryptdata = _Rsa.Decrypt(_Encryptdata, false);
                return Encoding.Default.GetString(_Decryptdata);
            }
        }

    }
}
