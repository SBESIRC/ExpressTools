using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThSitePlan.UI
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


    }
}
