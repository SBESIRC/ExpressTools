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
        /// 柱平法缺失
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
    public class ColumnTableRecordInfo:ICloneable
    {
        private string hoopReinforcePattern = @"([%]{2}[130-133]{1}){1}[\s]{0,}\d+[@]{1}[\s]{0,}\d+([\s]{0,}[/]{1}[\s]{0,}\d+)?";
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
        /// 箍筋 %%132 8@100/200
        /// </summary>
        public string HoopReinforcement { get; set; } = "";
        /// <summary>
        /// 核芯区 ( %%132 8@100)
        /// </summary>
        public string JointCoreHoop { get; set; } = "";
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
            if(!ValidateCode())
            {
                return false;
            }
            if(!ValidateSpec())
            {
                return false;
            }
            if(!ValidateHoopReinforcementTypeNumber())
            {
                return false;
            }
            if(!ValidateHoopReinforcement(this.HoopReinforcement))
            {
                return false;
            }
            if(!string.IsNullOrEmpty(this.AllLongitudinalReinforcement))
            {
                if(!ValidateReinforcement(this.AllLongitudinalReinforcement))
                {
                    return false;
                }
            }
            if(!string.IsNullOrEmpty(this.AngularReinforcement))
            {
                if (!ValidateReinforcement(this.AngularReinforcement))
                {
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(this.BEdgeSideMiddleReinforcement))
            {
                if (!ValidateReinforcement(this.BEdgeSideMiddleReinforcement))
                {
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(this.HEdgeSideMiddleReinforcement))
            {
                if (!ValidateReinforcement(this.HEdgeSideMiddleReinforcement))
                {
                    return false;
                }
            }
            return true;
        }
        public void Handle()
        {
            this.AngularReinforcement = RemoveBrackets(this.AngularReinforcement);
            this.BEdgeSideMiddleReinforcement=RemoveBrackets(this.BEdgeSideMiddleReinforcement);
            this.HEdgeSideMiddleReinforcement = RemoveBrackets(this.HEdgeSideMiddleReinforcement);
            this.HoopReinforcement = RemoveBrackets(this.HoopReinforcement);
        }
        /// <summary>
        /// 更新节点核芯区值
        /// </summary>
        public void UpdateJointCoreHooping()
        {
            if(string.IsNullOrEmpty(this.JointCoreHoop) &&
               !string.IsNullOrEmpty(this.HoopReinforcement))
            {
                if(this.HoopReinforcement.IndexOf("/")>=0)
                {
                    this.JointCoreHoop = this.HoopReinforcement.Substring(0, this.HoopReinforcement.IndexOf("/"));
                }
                else
                {
                    this.JointCoreHoop = this.HoopReinforcement;
                }
            }
        }
        /// <summary>
        /// 去掉括号
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string RemoveBrackets(string content)
        {
            string result = "";
            if(string.IsNullOrEmpty(content))
            {
                return result;
            }
            content = content.Trim();
            content = content.Replace('（', '(');
            int index = content.IndexOf("(");
            if(index >0)
            {
                content = content.Substring(0, index);
            }
            return content;
        }
        /// <summary>
        /// 验证柱子代号
        /// </summary>
        /// <returns></returns>
        public bool ValidateCode()
        {
            if (!string.IsNullOrEmpty(this.Code))
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
        public bool ValidateSpec()
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
            bool res = false;
            if(string.IsNullOrEmpty(content))
            {
                return res;
            }
            string pattern = @"\d+[\s]{0,}[%]{2}[0-9]{3}[\s]{0,}\d+";
            string[] strs= content.Split('+');
            foreach(string str in strs)
            {
                if (Regex.IsMatch(str, pattern))
                {
                    return true;
                }
                else
                {
                    byte[] buffers = Encoding.UTF32.GetBytes(str);
                    int a = 0;
                    int startIndex = -1;
                    bool isNumberOr132 = true; //只允许数字和130,131,132,133符号
                    for (int i = 0; i < buffers.Length; i = i + 4)
                    {
                        if ((buffers[i] >= 48 && buffers[i] <= 57) || buffers[i] == 132 || buffers[i] == 32)
                        {
                            if (buffers[i] == 133 || buffers[i] == 132 || buffers[i] == 131 || buffers[i] == 130)
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
                    if (isNumberOr132 && a >= 1 && startIndex > 0 && startIndex < buffers.Length - 1)
                    {
                        res= true;
                        break;
                    }                     
                }
            }
            return res;
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
                            if (buffers[i] == 130 || buffers[i] == 131 ||
                                buffers[i] == 132 || buffers[i] == 133)
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
        public bool ValidateHoopReinforcementTypeNumber()
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
        public bool ValidateJointCoreHooping(string jointCoreHooping)
        {
            bool isValid = false;
            string pattern1 = @"(\（）|\()(.*)(\)|\）)";//判断是否在括号内
            string pattern2 = "\\s{0,}(\\{1}u{1}\\d{4})?\\s{0,}\\d+\\s{0,}[@]{1}\\s{0,}\\d+\\s{0,}";
            if (!string.IsNullOrEmpty(jointCoreHooping))
            {
                if (Regex.IsMatch(jointCoreHooping, pattern1))
                {
                    if (Regex.IsMatch(jointCoreHooping, pattern2))
                    {
                        isValid = true;
                    }
                }
            }
            return isValid;
        }
        /// <summary>
        /// 提取节点核心区箍筋
        /// </summary>
        /// <param name="jointCoreHooping"></param>
        /// <returns></returns>
        public string ExtractJointCoreHooping(string jointCoreHooping)
        {
            string jointCoreHoopingValue = "";
            string pattern = "\\s{0,}\\d+\\s{0,}[@]{1}\\s{0,}\\d+\\s{0,}";
            var mc = Regex.Matches(jointCoreHooping, pattern);
            foreach (Match item in mc)
            {
                jointCoreHoopingValue = item.Groups[0].Value;
                break;
            }
            return jointCoreHoopingValue;
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
            if (ValidateReinforcement(content))
            {
                string firstDataStr = "";
                string secondDataStr = "";
                string match = "";
                index = ThColumnInfoUtils.IndexOfSpecialChar(content,out match);
                if (index > 0)
                {
                    firstDataStr = content.Substring(0, index);
                    secondDataStr = content.Substring(index + match.Length);
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
                            if (buffers[i] == 133 || buffers[i] == 132 ||
                                buffers[i] == 131 || buffers[i] == 130)
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
                string match = "";
                index = ThColumnInfoUtils.IndexOfSpecialChar(content, out match);
                if (index > 0)
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
                            if (buffers[i] == 133 || buffers[i] == 132 || 
                                buffers[i] == 131 || buffers[i] == 130)
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
        /// <summary>
        /// 验证当前的属性是否有空项
        /// </summary>
        /// <returns></returns>
        public bool ValidateEmpty()
        {
            if (string.IsNullOrEmpty(this.Spec) || string.IsNullOrEmpty(this.AngularReinforcement) ||
                string.IsNullOrEmpty(this.BEdgeSideMiddleReinforcement) || string.IsNullOrEmpty(this.HEdgeSideMiddleReinforcement)
                || string.IsNullOrEmpty(this.HoopReinforcement) || string.IsNullOrEmpty(this.HoopReinforcementTypeNumber))
            {
                return false;
            }
            return true;
        }
        public string Replace132(string content)
        {
            string res = content;
            content = BaseFunction.ReplaceReinforceChar(content);
            byte[] buffers = Encoding.UTF32.GetBytes(content);
            int startIndex = 0;
            string newStr = "";
            for (int i = 0; i < buffers.Length; i++)
            {
                if (buffers[i] == 133 || buffers[i] == 132 || buffers[i] == 131 || buffers[i] == 130)
                {
                    string matchChar = BaseFunction.GetReinforceChar(buffers[i]);
                    int count = i - startIndex;
                    newStr += Encoding.UTF32.GetString(buffers, startIndex, count);
                    newStr += matchChar;
                    startIndex = i + 4;
                }
            }
            if (buffers.Length - startIndex > 0)
            {
                newStr += Encoding.UTF32.GetString(buffers, startIndex, buffers.Length - startIndex);
            }
            res = newStr;

            return res;
        }
        /// <summary>
        /// 处理所有纵筋、角筋、B边/边纵筋
        /// </summary>
        /// <param name="reinforceContent"></param>
        /// <returns></returns>
        private string HandleReinfoceContent(string reinforceContent)
        {
            string res = reinforceContent;
            string[] strs = reinforceContent.Split('+');
            List<string> contents = new List<string>();
            foreach (string str in strs)
            {
                byte[] buffers = Encoding.UTF32.GetBytes(str);
                int lastIndex = -1;
                for (int i = 0; i < buffers.Length; i++)
                {
                    if (buffers[i] >= 48 && buffers[i] <= 57)
                    {
                        lastIndex = i;
                    }
                }
                byte[] newBuffers = new byte[lastIndex + 4];
                if (lastIndex > 0)
                {
                    for (int i = 0; i < lastIndex + 4; i++)
                    {
                        newBuffers[i] = buffers[i];
                    }
                }
                string newContent = Encoding.UTF32.GetString(newBuffers);
                contents.Add(newContent);
            }
            if (strs.Length > 1)
            {
                res = string.Join("+", contents.ToArray());
            }
            else
            {
                res = contents[0];
            }
            return res;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
