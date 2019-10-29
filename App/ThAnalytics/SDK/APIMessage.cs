using System;
using System.Linq;
using RestSharp;

namespace ThAnalytics.SDK
{
    public static class APIMessage
    {
        public static THConfig m_Config = new THConfig()
        {
            AppVersion = "V1.0",
            ServerUrl = @"https://cybros.thape.com.cn",
            SSOUrl = @"https://sso.thape.com.cn/users/sign_in"
        };

        public static string SignIn(string _Username, string _Password)
        {
            var client = new RestClient(m_Config.SSOUrl);
            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("JWT-AUD", "CAD");
            request.AddJsonBody(new SignIn()
            {
                user = new User()
                {
                    username = _Username,
                    password = _Password
                }
            });
            var response = client.Execute(request);
            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response. Check inner details for more info.";
                throw new Exception(message, response.ErrorException);
            }

            var enumerator = response.Headers.Where(o => o.Name == "Authorization");
            return enumerator.ElementAt(0).Value as string;
        }

        public static UserDetails CADUserInfo(string _Token)
        {
            var client = new RestClient(m_Config.ServerUrl);
            var request = new RestRequest("/api/me", Method.OPTIONS);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("JWT-AUD", "CAD");
            request.AddHeader("Authorization", _Token);
            request.AddBody("{}");
            var response = client.Execute<UserDetails>(request);
            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response. Check inner details for more info.";
                throw new Exception(message, response.ErrorException);
            }

            return response.Data;
        }

        public static void CADSession(string _Token, Sessions sessions)
        {
            var client = new RestClient(m_Config.ServerUrl);
            var request = new RestRequest("/api/cad_session", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("JWT-AUD", "CAD");
            request.AddHeader("Authorization", _Token);
            request.AddJsonBody(sessions);
            client.ExecuteAsync(request, response => {
                //Console.WriteLine(response.Content);
            });
        }

        public static void CADOperation(string _Token, InitiConnection initiConnection)
        {
            var client = new RestClient(m_Config.ServerUrl);
            var request = new RestRequest("/api/cad_operation", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("JWT-AUD", "CAD");
            request.AddHeader("Authorization", _Token);
            request.AddJsonBody(initiConnection);
            client.ExecuteAsync(request, response => {
                //Console.WriteLine(response.Content);
            });
        }
    }
}

