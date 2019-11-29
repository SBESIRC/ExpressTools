using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public enum ErrorMsg
    {
        /// <summary>
        /// 柱编号缺失
        /// </summary>
        CodeEmpty,
        /// <summary>
        ///  参数识别不全
        /// </summary>
        InfNotCompleted,
        /// <summary>
        /// 数据正确
        /// </summary>
        OK
    }
    /// <summary>
    /// 柱表记录信息
    /// </summary>
    public class ColumnTableRecordInfo
    {
        private string hoopReinforcePattern = @"([%]{2}132){1}[\s]{0,}\d+[@]{1}[\s]{0,}\d+([\s]{0,}[/]{1}[\s]{0,}\d+)?";
        /// <summary>
        /// 柱号(KZ1)
        /// </summary>
        public string Code { get; set; } = "";
        /// <summary>
        /// 标高(基础顶~-1.300)
        /// </summary>
        public string Level { get; set; } = "";
        /// <summary>
        /// 规格(bxh ; 600x600)
        /// </summary>
        public string Spec { get; set; } = "";
        /// <summary>
        /// 角筋
        /// </summary>
        public string AngularReinforcement { get; set; } = "";
        /// <summary>
        /// b边一侧中部筋
        /// </summary>
        public string BEdgeSideMiddleReinforcement { get; set; } = "";
        /// <summary>
        /// h边一侧中部筋
        /// </summary>
        public string HEdgeSideMiddleReinforcement { get; set; } = "";
        /// <summary>
        /// 全部纵筋
        /// </summary>
        public string AllLongitudinalReinforcement { get; set; } = "";
        /// <summary>
        /// 箍筋(8@100/200)
        /// </summary>
        public string HoopReinforcement { get; set; } = "";
        /// <summary>
        /// 箍筋类型号(1（4×4）)
        /// </summary>
        public string HoopReinforcementTypeNumber { get; set; } = "";
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = "";
        public bool Validate()
        {
            if(ValidateCode() && ValidateSpec() && ValidateHoopReinforcementTypeNumber())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 验证柱子代号
        /// </summary>
        /// <returns></returns>
        private bool ValidateCode()
        {
            if (!string.IsNullOrEmpty(this.Code) && this.Code.ToUpper().Contains("KZ"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 验证柱子规格
        /// </summary>
        /// <returns></returns>
        private bool ValidateSpec()
        {
            string pattern = @"\d+[\s]{0,}[xX×]{1}[\s]{0,}\d+";
            if (!string.IsNullOrEmpty(this.Spec) && Regex.IsMatch(this.Spec, pattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 验证角筋，中部筋
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool ValidateReinforcement(string content)
        {
            string pattern = @"\d+[\s]{0,}[%]{2}132{1}[\s]{0,}\d+";
            if (!string.IsNullOrEmpty(content))
            {
                if(Regex.IsMatch(content, pattern))
                {
                    return true;
                }
                else
                {
                    byte[] buffers = Encoding.UTF32.GetBytes(content);
                    int a = 0;
                    int startIndex = -1;
                    bool isNumberOr132 = true; //只允许数字和132符号
                    for(int i=0;i< buffers.Length; i=i+4)
                    {
                        if((buffers[i]>=48 && buffers[i] <= 57) || buffers[i]==132 || buffers[i]==32)
                        {
                            if (buffers[i] == 132)
                            {
                                startIndex = i;
                                a++;
                            }
                        }
                        else
                        {
                            isNumberOr132 = false;
                            break;
                        }                        
                    }
                    if(isNumberOr132 && a == 1 && startIndex>0 && startIndex< buffers.Length-1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 验证箍筋 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool ValidateHoopReinforcement(string content)
        {
            //8@100 / 200
            if (!string.IsNullOrEmpty(content))
            {               
                if (Regex.IsMatch(content, this.hoopReinforcePattern))
                {
                    return true;
                }
                else
                {
                    string pattern2 = @"\d+[\s]{0,}[@]{1}[\s]{0,}\d+[\s]{0,}([/]{1}[\s]{0,}\d+[\s]{0,})?";
                    if (Regex.IsMatch(content, pattern2))
                    {
                        byte[] buffers = Encoding.UTF32.GetBytes(content);
                        int a = 0;
                        int startIndex = -1;
                        for (int i = 0; i < buffers.Length; i++)
                        {
                            if (buffers[i] == 132)
                            {
                                startIndex = i;
                                a++;
                                break;
                            }
                        }
                        if (a == 1 && startIndex == 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 验证箍筋类型号
        /// </summary>
        /// <returns></returns>
        private bool ValidateHoopReinforcementTypeNumber()
        {
            string pattern = @"\d+[\s]{0,}[(（]{1}[\s]{0,}\d+[\s]{0,}[xX×]{1}[\s]{0,}\d+[\s]{0,}[)）]{1}";
            if (!string.IsNullOrEmpty(this.HoopReinforcementTypeNumber) && Regex.IsMatch(this.HoopReinforcementTypeNumber, pattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 获取角筋，中部筋中的数字
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public List<int> GetReinforceDatas(string content)
        {
            List<int> datas = new List<int>();
            int strNum = 0;
            int index = -1;
            if(ValidateReinforcement(content))
            {
                string firstDataStr = "";
                string secondDataStr = "";
                index = content.IndexOf("%%132");
                if(index>0)
                {
                    firstDataStr = content.Substring(0,index);
                    secondDataStr = content.Substring(index+5);

                }
                else
                {
                    byte[] buffers = Encoding.UTF32.GetBytes(content);
                    if (index > 0)
                    {
                        strNum = 5;
                    }
                    else
                    {
                        for (int i = 0; i < buffers.Length; i++)
                        {
                            if (buffers[i] == 132)
                            {
                                index = i;
                                strNum = 1;
                                break;
                            }
                        }
                    }
                    if (index <= 0)
                    {
                        return datas;
                    }
                    else
                    {
                        firstDataStr = Encoding.UTF32.GetString(buffers, 0, index);
                        secondDataStr = Encoding.UTF32.GetString(buffers, index + strNum * 4, buffers.Length - (index + strNum * 4));
                    }
                }
                if (Regex.IsMatch(firstDataStr, @"\d+([.]{1}\d+)?"))
                {
                    MatchCollection firstDatas = Regex.Matches(firstDataStr, @"\d+([.]{1}\d+)?");
                    if (firstDatas != null && firstDatas.Count > 0)
                    {
                        datas.Add(Convert.ToInt32(firstDatas[0].Value));
                    }
                }
                if (Regex.IsMatch(secondDataStr, @"\d+([.]{1}\d+)?"))
                {
                    MatchCollection secondDatas = Regex.Matches(secondDataStr, @"\d+([.]{1}\d+)?");
                    if (secondDatas != null && secondDatas.Count > 0)
                    {
                        datas.Add(Convert.ToInt32(secondDatas[0].Value));
                    }
                }
            }
            return datas;
        }
        /// <summary>
        /// 获取角筋，中部筋符号后的字符
        /// (4@100->@100)
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string GetReinforceSuffix(string content)
        {
            string suffix = "";
            int index = -1;
            if (ValidateReinforcement(content))
            {
                index = content.IndexOf("%%132");
                if (index>0)
                {
                    suffix = content.Substring(index);
                }
                else
                {
                    byte[] buffers = Encoding.UTF32.GetBytes(content);
                    if (index < 0)
                    {
                        for (int i = 0; i < buffers.Length; i++)
                        {
                            if (buffers[i] == 132)
                            {
                                index = i;
                                break;
                            }
                        }
                    }
                    if (index > 0)
                    {
                        suffix = Encoding.UTF32.GetString(buffers, index, buffers.Length - index);
                    }
                }
            }
            return suffix;
        }
    }
}
