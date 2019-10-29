using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace THRecordingService
{
    public static class APIMessage
    {
        public static THConfig m_Config = new THConfig() { AppVersion = "V1.0", ServerUrl = @"https://cybros.thape.com.cn", SSOUrl = @"https://sso.thape.com.cn/users/sign_in" };

        //public static fmMsg m_fm = new fmMsg();

        public static string SignIn(string _Username, string _Password)
        {
            try
            {

                string _Result = string.Empty;
                HttpWebRequest _Reuest = (HttpWebRequest)WebRequest.Create(new Uri(m_Config.SSOUrl));
                _Reuest.Method = "POST";
                _Reuest.Accept = "application/json";
                _Reuest.ContentType = "application/json";
                _Reuest.Headers.Add("JWT-AUD", "CAD");
                SignIn _SignIn = new SignIn();
                User _User = new User();
                _User.username = _Username;
                _User.password = _Password;
                _SignIn.user = _User;
                string _PostData = JsonConvert.SerializeObject(_SignIn);
                byte[] _Data = Encoding.UTF8.GetBytes(_PostData);
                _Reuest.ContentLength = _Data.Length; //请求长度
                using (Stream _ReStream = _Reuest.GetRequestStream()) //获取
                {
                    _ReStream.Write(_Data, 0, _Data.Length);//向当前流中写入字节
                    _ReStream.Close(); //关闭当前流
                }
                HttpWebResponse _Response = (HttpWebResponse)_Reuest.GetResponse();
                Stream _Stream = _Response.GetResponseStream();
                using (StreamReader _Reader = new StreamReader(_Stream, Encoding.UTF8))
                {
                    _Result = _Response.Headers.GetValues("Authorization")[0];

                    // _Result = _Reader.ReadToEnd();
                }
                return _Result;
            }
            catch (Exception e)
            {
                //FuncMsgbox.AlertError(m_fm, e.Message);
                return e.Message;
            }
        }

        public static async Task<bool> me()
        {

            string url = "https://cybros.thape.com.cn/api/me";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJndW9jaHVuemhvbmdAdGhhcGUuY29tLmNuIiwic2NwIjoidXNlciIsImF1ZCI6InBhdyIsImlhdCI6MTU2ODY4ODY5NywiZXhwIjoxNTY4Njk1ODk3LCJqdGkiOiIzZDY1NjFmMy1iYzg5LTQwOWEtYmE3Ny1mNWJlNzU0MDQ2N2EifQ.7qj2zWNkYTNvDYx1z6bFTjJkGABlqVUGK39o6U4OdDY");
            request.Headers.Add("JWT-AUD", "paw");

            request.Method = "GET";

            string postData = "{}";
            byte[] byte1 = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byte1.Length;
            Stream newStream = request.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);
            newStream.Close();

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    return true;
                    //process the response
                }
            }
        }

        public static string CADUserInfo(string _Token)
        {

            try
            {
                string _Url = m_Config.ServerUrl + "/api/me";
                HttpWebRequest _Request = (HttpWebRequest)WebRequest.Create(new Uri(_Url));
                _Request.Accept = "application/json";
                _Request.ContentType = "application/json";
                _Request.Headers.Add("Authorization", _Token);
                _Request.Headers.Add("JWT-AUD", "CAD");
                string _PostData = "{}";
                _Request.Method = "OPTIONS";
                byte[] _Byte = Encoding.UTF8.GetBytes(_PostData);
                _Request.ContentLength = _Byte.Length;
                Stream _NewStream = _Request.GetRequestStream();
                _NewStream.Write(_Byte, 0, _Byte.Length);
                _NewStream.Close();

                using (WebResponse _Response = _Request.GetResponse())
                {
                    using (Stream stream = _Response.GetResponseStream())
                    {
                        using (StreamReader _Reader = new StreamReader(stream, Encoding.UTF8))
                        {

                            return _Reader.ReadToEnd();
                        }

                    }
                }
            }
            catch(Exception e)
            {
                //FuncMsgbox.AlertError(m_fm, e.Message);
                return string.Empty;
            }

        }


        public static async Task<bool> CADSession(string _Token, string _PostData)
        {

            string _Url = m_Config.ServerUrl + "/api/cad_session";

            HttpWebRequest _Request = (HttpWebRequest)WebRequest.Create(new Uri(_Url));
            _Request.Accept = "application/json";
            _Request.ContentType = "application/json";
            _Request.Headers.Add("Authorization", _Token);
            _Request.Headers.Add("JWT-AUD", "CAD");
            _Request.Method = "POST";
            byte[] _Byte = Encoding.UTF8.GetBytes(_PostData);
            _Request.ContentLength = _Byte.Length;
            Stream _Stream = _Request.GetRequestStream();
            _Stream.Write(_Byte, 0, _Byte.Length);
            _Stream.Close();
            using (WebResponse response = await _Request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    //FuncMsgbox.AlertInfo(m_fm, "调用成功：" + _PostData);
                    //MessageBox.Show(_PostData);
                    return true;
                }
            }
        }


        public static async Task<bool> CADOperation(string _Token, string _PostData)
        {

            string _Url = m_Config.ServerUrl + "/api/cad_operation";

            HttpWebRequest _Request = (HttpWebRequest)WebRequest.Create(new Uri(_Url));
            _Request.Accept = "application/json";
            _Request.ContentType = "application/json";
            _Request.Headers.Add("Authorization", _Token);
            _Request.Headers.Add("JWT-AUD", "CAD");
            _Request.Method = "POST";

            byte[] _Byte = Encoding.UTF8.GetBytes(_PostData);
            _Request.ContentLength = _Byte.Length;
            Stream _Stream = _Request.GetRequestStream();
            _Stream.Write(_Byte, 0, _Byte.Length);
            _Stream.Close();

            using (WebResponse _Response = await _Request.GetResponseAsync())
            {
                using (Stream _Str = _Response.GetResponseStream())
                {
                    //FuncMsgbox.AlertInfo(m_fm, "调用成功：" + _PostData);
                    return true;
                }
            }
        }



    }
}

