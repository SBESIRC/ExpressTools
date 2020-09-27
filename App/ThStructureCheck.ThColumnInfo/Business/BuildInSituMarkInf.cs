using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class BuildInSituMarkInf
    {
        private double findBHSideMiddleTextOffsetDisRatio = 0.2; //查找范围由柱子长边和短边之和的一半再乘以这个比例，用于查找柱子B边和H边偏移的文字
        private Curve columnFrame;
        private List<DBText> dBTexts=new List<DBText>();
        private Document doc;
        private Dictionary<TextRotation, DBText> sideTextDic = new Dictionary<TextRotation, DBText>();
        private DistinguishCBH distinguishCBH;

        //用于校对
        private int bSideNum; 
        private int hSideNum;
        private int angularNum;
        private string bSideReinforcement = "";
        private string hSideReinforcement = "";
        private string angularReinforcement = "";
        private StringBuilder sb;

        private bool valid = true;
        public bool Valid
        {
            get { return valid; }
        }
        /// <summary>
        /// 生成的柱表记录
        /// </summary>
        public ColumnTableRecordInfo Ctri { get; set; } = new ColumnTableRecordInfo();

        public BuildInSituMarkInf(Curve polyline, List<DBText> dBTexts)
        {
            this.columnFrame = polyline;
            this.dBTexts = dBTexts;
            this.doc = ThColumnInfoUtils.GetMdiActiveDocument();
            sb = new StringBuilder();
        }
        public BuildInSituMarkInf(Curve polyline)
        {
            this.columnFrame = polyline;
            this.doc = ThColumnInfoUtils.GetMdiActiveDocument();
            sb = new StringBuilder();
        }
        public void Build()
        {
            Distinguish();
            CollectSideTextDic();
            CollectMarkInfo();
            CollectReinforceInfo(Ctri.AllLongitudinalReinforcement);
        } 
        private void Distinguish()
        {
            this.distinguishCBH = new DistinguishCBH(this.columnFrame);
            distinguishCBH.Distinguish();
            this.Ctri.HoopReinforcementTypeNumber = distinguishCBH.TypeNumber;
        }
        private void CollectMarkInfo()
        {
            //以下是整理信息，包括识别到的柱框边缘的文字信息
            string antiSeismicGrade = "";
            for (int i = 0; i < dBTexts.Count; i++)
            {
                if (string.IsNullOrEmpty(dBTexts[i].TextString))
                {
                    continue;
                }                
                if (BaseFunction.IsColumnCode(dBTexts[i].TextString.ToUpper()))
                {
                    //柱编号
                    this.Ctri.Code = dBTexts[i].TextString;
                }
                else if (dBTexts[i].TextString.ToUpper().Contains("x") || dBTexts[i].TextString.ToUpper().Contains("X")
                    || dBTexts[i].TextString.ToUpper().Contains("×") || dBTexts[i].TextString.ToUpper().Contains("×"))
                {
                    //柱规格
                    this.Ctri.Spec = dBTexts[i].TextString;
                }
                else if (new ColumnTableRecordInfo().ValidateReinforcement(dBTexts[i].TextString) ||
                    new ColumnTableRecordInfo().ValidateReinforcement(HandleReinfoceContent(dBTexts[i].TextString)))
                {
                    this.Ctri.AllLongitudinalReinforcement = dBTexts[i].TextString;
                }
                else if (new ColumnTableRecordInfo().ValidateHoopReinforcement(dBTexts[i].TextString))
                {
                    this.Ctri.HoopReinforcement = dBTexts[i].TextString;
                    if (new ColumnTableRecordInfo().ValidateJointCoreHooping(dBTexts[i].TextString))
                    {
                        this.Ctri.JointCoreHoop = new ColumnTableRecordInfo().ExtractJointCoreHooping(dBTexts[i].TextString);
                    }
                }
                else if (dBTexts[i].TextString.Contains("抗震"))
                {
                    antiSeismicGrade = dBTexts[i].TextString;
                }
                else if(new ColumnTableRecordInfo().ValidateJointCoreHooping(dBTexts[i].TextString))
                {
                    this.Ctri.JointCoreHoop = new ColumnTableRecordInfo().ExtractJointCoreHooping(dBTexts[i].TextString);
                }
            }
            if (string.IsNullOrEmpty(this.Ctri.Remark) && !string.IsNullOrEmpty(antiSeismicGrade))
            {
                this.Ctri.Remark = antiSeismicGrade;
            }
        }
        /// <summary>
        /// 收集箍筋信息
        /// </summary>
        private void CollectReinforceInfo(string reinforce)
        {
            // 详细规则参见纵筋识别情况.dwg
            //获取角筋
            string[] strs = reinforce.Split('+');
            //判断集中标注中是否有角筋
            GetAngularReinforcement(strs);
            //从原位标注提取b、h边纵筋规格
            GetBHSideReinforceNum();
            if (!ValidateReinforceFormat(strs) || !ValidateSideReinforceFormat())
            {
                this.valid = false;
                return;
            }
            if (strs.Length==1)
            {
                ReinforceHasOneStr(strs);
            }
            else if(strs.Length == 2)
            {
                if(!string.IsNullOrEmpty(this.angularReinforcement))
                {
                    ReinforceHasTwoStr(strs);
                }
                else
                {
                    ReinforceHasTwoStrWithOutCorner(strs);
                }
                this.Ctri.AllLongitudinalReinforcement = "";
            }
            else if(strs.Length == 3)
            {
                ReinforceHasThreeStr(strs);
                this.Ctri.AllLongitudinalReinforcement = "";
            }
            if (this.valid)
            {
                this.Ctri.AngularReinforcement = this.angularReinforcement;
                this.Ctri.BEdgeSideMiddleReinforcement = this.bSideReinforcement;
                this.Ctri.HEdgeSideMiddleReinforcement = this.hSideReinforcement;
            }
            else
            {
                this.Ctri.AngularReinforcement = "";
                this.Ctri.BEdgeSideMiddleReinforcement = "";
                this.Ctri.HEdgeSideMiddleReinforcement = "";
            }
        }
        private bool ValidateReinforceFormat(string[] strs)
        {
            bool res = true;
            foreach (string str in strs)
            {
                if(this.Ctri.ValidateReinforcement(str) || this.Ctri.ValidateReinforcement(HandleReinfoceContent(str)))
                {
                    continue;
                }
                res = false;
                this.sb.Append("\nB、H边纵筋或角筋格式不正确。");
                break;
            }
            return res;
        }
        private bool ValidateSideReinforceFormat()
        {
            bool res = true;
            if (!string.IsNullOrEmpty(this.bSideReinforcement))
            {
                res = new ColumnTableRecordInfo().ValidateReinforcement(this.bSideReinforcement);
                if (!res)
                {
                    sb.Append("\nb边 [" + this.bSideReinforcement + "] 格式有问题");
                    return res;
                }
                if (this.bSideNum != this.distinguishCBH.BEdgeNum * 2)
                {
                    this.valid = false;
                    this.sb.Append("原位标注的B边和识别的B边不一致");
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(this.hSideReinforcement))
            {
                res = new ColumnTableRecordInfo().ValidateReinforcement(this.hSideReinforcement);
                if (!res)
                {
                    sb.Append("\nh边 [" + this.bSideReinforcement + "] 格式有问题");
                    return res;
                }
                if (this.hSideNum != this.distinguishCBH.HEdgeNum * 2)
                {
                    this.valid = false;
                    this.sb.Append("原位标注的H边和识别的H边不一致");
                    return false;
                }
            }
            //if (this.sideTextDic.Count != 0 && this.sideTextDic.Count != 2)
            //{
            //    sb.Append("\nB、H边标注只能标两个或不标");
            //    res = false;
            //}
            return res;
        }
        /// <summary>
        /// 全部纵筋没有加号分割
        /// </summary>
        /// <returns></returns>
        private void ReinforceHasOneStr(string[] strs)
        {
            //获取集中数量
            List<int> totalReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(strs[0]);
            int num = 0;
            if (totalReinforceDatas.Count == 2)
            {
                num = totalReinforceDatas[0];
            }
            //有原位标注
            if(this.sideTextDic.Count==2)
            {
                string bSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.bSideReinforcement);                
                string hSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.hSideReinforcement);                
                string cornerReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(strs[0]);               
                //总数比原位多表示总数,就用总数来比
                if (num >= (this.bSideNum + this.hSideNum+4))
                {
                    if (hSideReinforceSuffix != cornerReinforceSuffix)
                    {
                        this.valid = false;
                        this.sb.Append("B边标注和集中标注格式不一致");
                        return;
                    }
                    if (hSideReinforceSuffix != bSideReinforceSuffix)
                    {
                        this.valid = false;
                        this.sb.Append("H边标注和集中标注格式不一致");
                        return;
                    }
                    if (this.distinguishCBH.TotalNum != num)
                    {
                        this.valid = false;
                        this.sb.Append("集中标注的总数和识别的总数不一致");
                        return;
                    }
                    else
                    {
                        this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + cornerReinforceSuffix;
                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + cornerReinforceSuffix;
                        this.hSideReinforcement = this.distinguishCBH.HEdgeNum + cornerReinforceSuffix;
                    }
                }
                else
                {
                    //把原位标注的数量和集中标注加在一起
                    if (this.distinguishCBH.TotalNum != (this.bSideNum + this.hSideNum + num))
                    {
                        this.valid = false;
                        this.sb.Append("集中标注的总数和识别的总数不一致");
                        return;
                    }
                    else
                    {                        
                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + bSideReinforceSuffix;                       
                        this.hSideReinforcement = this.distinguishCBH.HEdgeNum + hSideReinforceSuffix;                        
                        this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + cornerReinforceSuffix;
                    }
                }
            }
            else 
            {
                //无原位
                if (this.distinguishCBH.TotalNum != num)
                {
                    this.valid = false;
                    this.sb.Append("集中标注的总数和识别的总数不一致");
                    return;
                }
                else
                {
                    string cornerReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(strs[0]);
                    this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + cornerReinforceSuffix;
                    this.bSideReinforcement = this.distinguishCBH.BEdgeNum + cornerReinforceSuffix;
                    this.hSideReinforcement = this.distinguishCBH.HEdgeNum + cornerReinforceSuffix;
                }
            }
            if(this.distinguishCBH.TotalNum == num)
            {
                this.Ctri.AllLongitudinalReinforcement = strs[0];
            }
            else
            {
                this.Ctri.AllLongitudinalReinforcement = "";
            }
        }
        /// <summary>
        /// 集中标注的全部纵筋有一个加号分割(含有角筋)
        /// </summary>
        /// <returns></returns>
        private void ReinforceHasTwoStr(string[] strs)
        {
            //获取总数数量
            int num = 0;
            foreach (string str in strs)
            {
                List<int> totalReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(HandleReinfoceContent(str));
                if (totalReinforceDatas.Count == 2)
                {
                    num += totalReinforceDatas[0];
                }
            }
            if(this.distinguishCBH.TotalNum!= num)
            {
                this.valid = false;
                this.Ctri.AllLongitudinalReinforcement = "";
                this.sb.Append("集中标注的总数和识别的总数不一致");
                return;
            }
            string reinforce = "";
            string angularStr = ""; //带有角筋数量的字样
            foreach(string str in strs)
            {
                if(str.IndexOf("角筋")<0)
                {
                    reinforce = str;                    
                }
                else
                {
                    angularStr = str;
                }
            }
            List<int> reinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(reinforce);
            List<int> angularDatas = new ColumnTableRecordInfo().GetReinforceDatas(angularStr);
            string suffix = new ColumnTableRecordInfo().GetReinforceSuffix(reinforce);           
            int reinNum = reinforceDatas[0];

            string cornerReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.angularReinforcement);
            this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + cornerReinforceSuffix;
            //有原位标注
            if (this.sideTextDic.Count==2)
            {
                string bSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.bSideReinforcement);
                string hSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.hSideReinforcement);
                List<int> bSideDatas= new ColumnTableRecordInfo().GetReinforceDatas(this.bSideReinforcement);
                List<int> hSideDatas = new ColumnTableRecordInfo().GetReinforceDatas(this.hSideReinforcement);
                if(!(bSideDatas[0]==this.distinguishCBH.BEdgeNum &&
                   hSideDatas[0] == this.distinguishCBH.HEdgeNum))
                {
                    this.sb.Append("B边或H边格式和集中标注不一致");
                    this.valid = false;
                    return;
                }
                //含角筋字样的数量和识别角筋数量一致
                if(angularDatas[0] == this.distinguishCBH.CornerNum * 4)
                {
                    //B、H边标注文字的格式和纵筋不一致
                    if (bSideReinforceSuffix != suffix && hSideReinforceSuffix != suffix)
                    {
                        this.valid = false;
                        this.sb.Append("B边或H边格式和集中标注不一致");
                        return;
                    }
                    if(reinNum==(this.distinguishCBH.BEdgeNum+ this.distinguishCBH.HEdgeNum)*2)
                    {
                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + bSideReinforceSuffix;
                        this.hSideReinforcement = this.distinguishCBH.HEdgeNum + hSideReinforceSuffix;
                    }
                }
                else
                {
                    //表示有一部分分到角筋里面
                    int cornerLeftNum = angularDatas[0] - this.distinguishCBH.CornerNum * 4;
                    if(cornerLeftNum== this.distinguishCBH.BEdgeNum &&
                        cornerReinforceSuffix == bSideReinforceSuffix)
                    {
                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + cornerReinforceSuffix;
                        if(reinNum== this.distinguishCBH.HEdgeNum*2 && suffix== hSideReinforceSuffix)
                        {
                            this.hSideReinforcement = this.distinguishCBH.HEdgeNum + hSideReinforceSuffix;
                        }
                    }
                    else if(cornerLeftNum== this.distinguishCBH.HEdgeNum &&
                        cornerReinforceSuffix == hSideReinforceSuffix)
                    {
                        this.hSideReinforcement = this.distinguishCBH.HEdgeNum + cornerReinforceSuffix;
                        if (reinNum == this.distinguishCBH.BEdgeNum * 2 && suffix == bSideReinforceSuffix)
                        {
                            this.bSideReinforcement = this.distinguishCBH.BEdgeNum + bSideReinforceSuffix;
                        }
                    }
                }                
            }
            else if(this.sideTextDic.Count == 1)
            {
                //B边有标注
                if(this.sideTextDic.ContainsKey(TextRotation.Horizontal))
                {
                    string bMarkText = this.sideTextDic[TextRotation.Horizontal].TextString;
                    List<int> bMarkDatas = new ColumnTableRecordInfo().GetReinforceDatas(bMarkText);
                    string bMarkSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(bMarkText);
                    //如果含角筋字样的纵筋数量和识别的角筋数量一致，其余归B、H边分
                    if (angularDatas[0] == this.distinguishCBH.CornerNum * 4)
                    {
                        //B边纵筋和文字中的纵筋格式一致
                        if (bMarkSuffix == suffix && reinNum > bMarkDatas[0] &&
                            reinNum == (this.distinguishCBH.BEdgeNum + this.distinguishCBH.HEdgeNum) * 2)
                        {
                            this.bSideReinforcement = this.distinguishCBH.BEdgeNum + suffix;
                            this.hSideReinforcement = this.distinguishCBH.HEdgeNum + suffix;
                        }
                    }
                    else
                    {
                        if (bMarkDatas.Count == 2)
                        {
                            //表示B边表达的文字和该纵筋是一致
                            if (bMarkDatas[0] == reinNum && bMarkSuffix == suffix)
                            {
                                //表示B边识别的纵筋数量也保持一致
                                if (this.distinguishCBH.BEdgeNum * 2 == reinNum)
                                {
                                    this.bSideReinforcement = this.distinguishCBH.BEdgeNum + suffix;
                                    //剩余归角筋和H边纵筋分配
                                    if (angularDatas[0] == this.distinguishCBH.CornerNum * 4 + this.distinguishCBH.HEdgeNum * 2)
                                    {
                                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + cornerReinforceSuffix;
                                    }
                                }
                            }
                            //未能正确分配
                            if (string.IsNullOrEmpty(this.bSideReinforcement) && string.IsNullOrEmpty(this.hSideReinforcement))
                            {
                                //B边纵筋分配到角筋里了
                                if (angularDatas[0] == this.distinguishCBH.CornerNum * 4 + bMarkDatas[0] * 2 &&
                                    cornerReinforceSuffix == bMarkSuffix)
                                {
                                    this.bSideReinforcement = this.distinguishCBH.BEdgeNum + cornerReinforceSuffix;
                                    //未分配的纵筋数量和H边数量一致
                                    if (reinNum == this.distinguishCBH.HEdgeNum * 2)
                                    {
                                        this.hSideReinforcement = this.distinguishCBH.HEdgeNum + suffix;
                                    }
                                }
                            }
                        }
                    }
                }
                //H边有标注
                else if (this.sideTextDic.ContainsKey(TextRotation.Vertical))
                {
                    string hMarkText = this.sideTextDic[TextRotation.Vertical].TextString;
                    List<int> hMarkDatas = new ColumnTableRecordInfo().GetReinforceDatas(hMarkText);
                    string hMarkSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(hMarkText);
                    //角筋文字的数量和识别的角筋数量一致
                    if (angularDatas[0] == this.distinguishCBH.CornerNum * 4)
                    {
                        //H边纵筋和文字中的纵筋格式一致
                        if (hMarkSuffix == suffix && reinNum > hMarkDatas[0] &&
                            reinNum == (this.distinguishCBH.BEdgeNum + this.distinguishCBH.HEdgeNum) * 2)
                        {
                            this.bSideReinforcement = this.distinguishCBH.BEdgeNum + suffix;
                            this.hSideReinforcement = this.distinguishCBH.HEdgeNum + suffix;
                        }
                    }
                    //角筋文字的数量大于识别的角筋数量，说明包括纵筋数量
                    else
                    {
                        if (hMarkDatas.Count == 2)
                        {
                            //表示H边表达的文字和该纵筋是一致
                            if (hMarkDatas[0] == reinNum && hMarkSuffix == suffix)
                            {
                                //表示H边识别的纵筋数量也保持一致
                                if (this.distinguishCBH.HEdgeNum * 2 == reinNum)
                                {
                                    this.hSideReinforcement = this.distinguishCBH.HEdgeNum + suffix;
                                    //剩余归角筋和H边纵筋分配
                                    if (angularDatas[0] == this.distinguishCBH.CornerNum * 4 + this.distinguishCBH.BEdgeNum * 2)
                                    {
                                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + cornerReinforceSuffix;
                                    }
                                }
                            }
                            //未能正确分配
                            if(string.IsNullOrEmpty(this.bSideReinforcement) && string.IsNullOrEmpty(this.hSideReinforcement))
                            {
                                //H边纵筋分配到角筋里了
                                if (angularDatas[0]== this.distinguishCBH.CornerNum * 4 + hMarkDatas[0]*2 &&
                                    cornerReinforceSuffix== hMarkSuffix)
                                {
                                    this.hSideReinforcement = this.distinguishCBH.HEdgeNum + cornerReinforceSuffix;
                                    //未分配的纵筋数量和B边数量一致
                                    if(reinNum== this.distinguishCBH.BEdgeNum*2)
                                    {
                                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + suffix;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //集中标注第一个与识别的B边数量一致
                //如果含角筋字样的纵筋数量和识别的角筋数量一致，其余归B、H边分
                if (angularDatas[0] == this.distinguishCBH.CornerNum * 4)
                {
                    //B边纵筋和文字中的纵筋格式一致
                    if (reinNum == (this.distinguishCBH.BEdgeNum + this.distinguishCBH.HEdgeNum) * 2)
                    {
                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + suffix;
                        this.hSideReinforcement = this.distinguishCBH.HEdgeNum + suffix;
                    }
                }
                else
                {
                    int cornerLeftNum = angularDatas[0] - this.distinguishCBH.CornerNum * 4;
                    if(cornerLeftNum!=reinNum)
                    {
                        if(this.distinguishCBH.BEdgeNum*2== cornerLeftNum &&
                            this.distinguishCBH.HEdgeNum * 2== reinNum)
                        {
                            this.bSideReinforcement = this.distinguishCBH.BEdgeNum + cornerReinforceSuffix;
                            this.hSideReinforcement = this.distinguishCBH.HEdgeNum + suffix;
                        }
                        else if (this.distinguishCBH.BEdgeNum * 2 == reinNum &&
                            this.distinguishCBH.HEdgeNum * 2 == cornerLeftNum)
                        {
                            this.bSideReinforcement = this.distinguishCBH.BEdgeNum + suffix;
                            this.hSideReinforcement = this.distinguishCBH.HEdgeNum + cornerReinforceSuffix;
                        }
                    }                    
                }
            }
        }
        /// <summary>
        /// 集中标注的全部纵筋有一个加号分割(不含有角筋)
        /// </summary>
        /// <param name="strs"></param>
        private void ReinforceHasTwoStrWithOutCorner(string[] strs)
        {
            //获取总数数量
            int num = 0;
            foreach (string str in strs)
            {
                List<int> totalReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(str);
                if (totalReinforceDatas.Count == 2)
                {
                    num += totalReinforceDatas[0];
                }
            }
            if (this.distinguishCBH.TotalNum != num)
            {
                this.valid = false;
                this.Ctri.AllLongitudinalReinforcement = "";
                this.sb.Append("集中标注的总数和识别的总数不一致");
                return;
            }
            List<int> firstReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(strs[0]);
            List<int> secondReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(strs[1]);
            string firstSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(RemoveAngularChar(strs[0]));
            string secondSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(RemoveAngularChar(strs[1]));
            int firstReinNum = firstReinforceDatas[0];
            int secondReinNum = secondReinforceDatas[0];
            //有原位标注
            if (this.sideTextDic.Count == 2)
            {
                string bSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.bSideReinforcement);
                this.bSideReinforcement = this.distinguishCBH.BEdgeNum + bSideReinforceSuffix;
                string hSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.hSideReinforcement);
                this.hSideReinforcement = this.distinguishCBH.HEdgeNum + hSideReinforceSuffix;
                if(bSideReinforceSuffix== hSideReinforceSuffix)
                {
                    int bhSum = this.bSideNum + this.hSideNum;
                    bool angularMatched = false;
                    //B、H总数、后缀与集中标注第一个一致,则第二个可能是角筋数量
                    if(bhSum==firstReinNum && bSideReinforceSuffix==firstSuffix)
                    {
                        if(secondReinNum==this.distinguishCBH.CornerNum*4)
                        {
                            this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + secondSuffix;
                            angularMatched = true;
                        }                        
                    }
                    //B、H总数、后缀与集中标注第二个一致,则第一个可能是角筋数量
                    else if (bhSum == secondReinNum && bSideReinforceSuffix == secondSuffix)
                    {
                        if (firstReinNum == this.distinguishCBH.CornerNum * 4)
                        {
                            this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + firstSuffix;
                            angularMatched = true;
                        }
                    }
                    if(!angularMatched)
                    {
                        this.valid = false;
                        this.sb.Append("无法找出角筋的型号");
                        return;
                    }
                }
                //b边原位标注数量和 第一个标注数量相同，表示把角筋合到第二个数值里了
                else if (this.bSideNum == firstReinNum && firstSuffix == bSideReinforceSuffix)
                {
                    this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + secondSuffix;
                    if(secondReinNum!= this.distinguishCBH.CornerNum * 4+this.distinguishCBH.HEdgeNum*2)
                    {
                        this.valid = false;
                        this.sb.Append("识别的角筋和H边纵筋数和集中标注里的角筋和H边的合计数不一致");
                        return;
                    }
                }
                //b边原位标注数量和 第二个标注数量相同，表示把角筋合到第一个数值里了
                else if (this.bSideNum == secondReinNum && bSideReinforceSuffix == secondSuffix)
                {
                    this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + firstSuffix;
                    if (firstReinNum != this.distinguishCBH.CornerNum * 4 + this.distinguishCBH.HEdgeNum * 2)
                    {
                        this.valid = false;
                        this.sb.Append("识别的角筋和H边纵筋数和集中标注里的角筋和H边的合计数不一致");
                        return;
                    }
                }
                //h边原位标注数量和第一个标注数量相同，表示把角筋合到第二个数值里了
                else if (this.hSideNum == firstReinNum && firstSuffix == hSideReinforceSuffix)
                {
                    this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + secondSuffix;
                    if (secondReinNum != this.distinguishCBH.CornerNum * 4 + this.distinguishCBH.BEdgeNum * 2)
                    {
                        this.valid = false;
                        this.sb.Append("识别的角筋和B边纵筋数和集中标注里的角筋和B边的合计数不一致");
                        return;
                    }
                }
                //h边原位标注数量和第二个标注数量相同，表示把角筋合到第一个数值里了
                else if (this.hSideNum == secondReinNum && secondSuffix == hSideReinforceSuffix)
                {
                    this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + firstSuffix;
                    if (firstReinNum != this.distinguishCBH.CornerNum * 4 + this.distinguishCBH.BEdgeNum * 2)
                    {
                        this.valid = false;
                        this.sb.Append("识别的角筋和B边纵筋数和集中标注里的角筋和B边的合计数不一致");
                        return;
                    }
                }
                else
                {
                    this.sb.Append("\n无法从集中标注分配角筋型号、B边纵筋型号、H边纵筋型号");
                    this.valid = false;
                    return;
                }
            }
            else
            {
                bool isMatched = false;
                //集中标注第一个与识别的B边数量一致
                if (firstReinNum == this.distinguishCBH.BEdgeNum * 2 )
                {
                    //集中标注第二个数量等于识别的H边+角筋数量，表示角筋在第二个字符串里
                    if (secondReinNum == this.distinguishCBH.HEdgeNum * 2 + this.distinguishCBH.CornerNum * 4)
                    {
                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + firstSuffix;
                        this.hSideReinforcement = this.distinguishCBH.HEdgeNum + secondSuffix;
                        this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + secondSuffix;
                        isMatched = true;
                    }                   
                }
                //集中标注第一个与识别的H边数量一致
                else if (firstReinNum == this.distinguishCBH.HEdgeNum * 2)
                {
                    //集中标注第二个数量等于识别的B边+角筋数量，表示角筋在第二个字符串里
                    if (secondReinNum == this.distinguishCBH.BEdgeNum * 2 + this.distinguishCBH.CornerNum * 4)
                    {
                        this.hSideReinforcement = this.distinguishCBH.BEdgeNum + firstSuffix;
                        this.bSideReinforcement = this.distinguishCBH.HEdgeNum + secondSuffix;
                        this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + secondSuffix;
                        isMatched = true;
                    }                   
                }
                //集中标注第二个与识别的B边数量一致
                else if (secondReinNum == this.distinguishCBH.BEdgeNum * 2)
                {
                    //集中标注第一个数量等于识别的H边+角筋数量，表示角筋在第一个字符串里
                    if (firstReinNum == this.distinguishCBH.HEdgeNum * 2 + this.distinguishCBH.CornerNum * 4)
                    {
                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + secondSuffix;
                        this.hSideReinforcement = this.distinguishCBH.HEdgeNum + firstSuffix;
                        this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + firstSuffix;
                        isMatched = true;
                    }
                }
                //集中标注第二个与识别的H边数量一致
                else if (secondReinNum == this.distinguishCBH.HEdgeNum * 2)
                {
                    //集中标注第一个数量等于识别的B边+角筋数量，表示角筋在第一个字符串里
                    if (firstReinNum == this.distinguishCBH.BEdgeNum * 2 + this.distinguishCBH.CornerNum * 4)
                    {
                        this.hSideReinforcement = this.distinguishCBH.HEdgeNum + secondSuffix;
                        this.bSideReinforcement = this.distinguishCBH.BEdgeNum + firstSuffix;                        
                        this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + firstSuffix;
                        isMatched = true;
                    }                    
                }
                if(!isMatched)
                {
                    this.sb.Append("\n无法从集中标注分配角筋型号、B边纵筋型号、H边纵筋型号");
                    this.valid = false;
                }
            }
        }
        /// <summary>
        /// 全部纵筋有两个加号分割
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        private void ReinforceHasThreeStr(string[] strs)
        {
            //获取总数数量
            int num = 0;
            int cornerCount = 0;
            string firstRein = ""; ;
            string secondRein = "";
            for(int i=0;i<strs.Length;i++) 
            {
                List<int> totalReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(strs[i]);
                if (totalReinforceDatas.Count == 2)
                {
                    num += totalReinforceDatas[0];
                }
                if(strs[i].IndexOf("角筋")>0)
                {
                    cornerCount++;
                    switch(i)
                    {
                        case 0:
                            firstRein = strs[1];
                            secondRein = strs[2];
                            break;
                        case 1:
                            firstRein = strs[0];
                            secondRein = strs[2];
                            break;
                        case 2:
                            firstRein = strs[0];
                            secondRein = strs[1];
                            break;
                    }
                }
            }
            if(cornerCount>1)
            {
                this.valid = false;
                this.sb.Append("集中标注的角筋标识太多!");
                return;
            }
            if (this.distinguishCBH.TotalNum != num)
            {
                this.valid = false;
                this.Ctri.AllLongitudinalReinforcement = "";
                this.sb.Append("集中标注的总数和识别的总数不一致");
                return;
            }
            if (cornerCount==0)
            {
                ReinforceHasThreeStrWithOutCorner(strs);
                return;
            }
            List<int> firstReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(firstRein);
            List<int> secondReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(secondRein);
            string firstSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(firstRein);
            string secondSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(secondRein);
            int firstReinNum = firstReinforceDatas[0];
            int secondReinNum = secondReinforceDatas[0];

            if (!string.IsNullOrEmpty(this.angularReinforcement))
            {
                string cornerReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.angularReinforcement);
                this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + cornerReinforceSuffix;
            }

            //有原位标注
            if (this.sideTextDic.Count == 2)
            {
                string bSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.bSideReinforcement);
                string hSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.hSideReinforcement);
                //原位B边标注和第一个字符串一致,原位H边标注和第二个字符串一致
                if ((this.bSideNum==firstReinNum && bSideReinforceSuffix == firstSuffix) &&
                    (this.hSideNum==secondReinNum && hSideReinforceSuffix == secondSuffix))
                {
                    this.bSideReinforcement = this.distinguishCBH.BEdgeNum + firstSuffix;
                    this.hSideReinforcement = this.distinguishCBH.HEdgeNum + secondSuffix;
                }
                //原位B边标注和第二个字符串一致,原位H边标注和第一个字符串一致
                else if ((this.bSideNum == secondReinNum && bSideReinforceSuffix == secondSuffix) &&
                    (this.hSideNum == firstReinNum && hSideReinforceSuffix == firstSuffix))
                {
                    this.bSideReinforcement = this.distinguishCBH.BEdgeNum + secondSuffix;
                    this.hSideReinforcement = this.distinguishCBH.HEdgeNum + firstSuffix;
                }
                else
                {
                    this.valid = false;
                    this.sb.Append("原位标注和集中标注不一致");
                    return;
                }
            }
            else
            {
                //没有原位标注
                if (firstReinNum == secondReinNum)
                {
                    this.sb.Append("\n无法从集中标注分配角筋型号、B边纵筋型号、H边纵筋型号");
                    this.valid = false;
                    return;
                }
                //集中标注第一个与识别的B边数量一致,集中标注第二个与识别的H边数量一致
                if (firstReinNum == this.distinguishCBH.BEdgeNum * 2 && 
                    secondReinNum== this.distinguishCBH.HEdgeNum * 2)
                {
                    this.bSideReinforcement = this.distinguishCBH.BEdgeNum + firstSuffix;
                    this.hSideReinforcement = this.distinguishCBH.HEdgeNum + secondSuffix;
                }
                //集中标注第二个与识别的B边数量一致,集中标注第一个与识别的H边数量一致
                if (firstReinNum == this.distinguishCBH.HEdgeNum * 2 &&
                     secondReinNum == this.distinguishCBH.BEdgeNum * 2)
                {
                    this.bSideReinforcement = this.distinguishCBH.BEdgeNum + secondSuffix;
                    this.hSideReinforcement = this.distinguishCBH.HEdgeNum + firstSuffix;
                }
                else
                {
                    this.sb.Append("\n无法从集中标注分配角筋型号、B边纵筋型号、H边纵筋型号");
                    this.valid = false;
                    return;
                }
            }
        }
        private void ReinforceHasThreeStrWithOutCorner(string[] strs)
        {
            //有原位标注
            if (this.sideTextDic.Count == 2)
            {                
                string bSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.bSideReinforcement);
                this.bSideReinforcement = this.distinguishCBH.BEdgeNum + bSideReinforceSuffix;
                string hSideReinforceSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(this.hSideReinforcement);
                this.hSideReinforcement = this.distinguishCBH.HEdgeNum + hSideReinforceSuffix;

                int bFindIndex = -1, hFindIndex = -1;
                for(int i=0;i<strs.Length;i++)
                {
                    List<int> reinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(strs[i]);
                    string suffix = new ColumnTableRecordInfo().GetReinforceSuffix(strs[i]);
                    int reinNum = reinforceDatas[0];
                    if(bSideReinforceSuffix== suffix && this.bSideNum== reinNum)
                    {
                        if(bFindIndex < 0)
                        {
                            bFindIndex = i;
                        }
                    }
                    if(hSideReinforceSuffix == suffix && this.hSideNum == reinNum)
                    {
                        if(hFindIndex<0)
                        {
                            hFindIndex = i;
                        }
                    }
                }
                if(bFindIndex>0 && hFindIndex>0)
                {
                    string angularStr = "";
                    if (bFindIndex == 0)
                    {
                        if(hFindIndex == 1)
                        {
                            angularStr = strs[2];
                        }
                        else
                        {
                            angularStr = strs[1];
                        }
                    }
                    else if (bFindIndex == 1)
                    {
                        if (hFindIndex == 0)
                        {
                            angularStr = strs[2];
                        }
                        else
                        {
                            angularStr = strs[0];
                        }
                    }
                    else if(bFindIndex == 2)
                    {
                        if (hFindIndex == 0)
                        {
                            angularStr = strs[1];
                        }
                        else
                        {
                            angularStr = strs[0];
                        }
                    }
                    if(!string.IsNullOrEmpty(angularStr))
                    {
                        string cornerSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(angularStr);
                        this.angularReinforcement = this.distinguishCBH.CornerNum * 4 + cornerSuffix;
                    }
                }
                else
                {
                    this.sb.Append("\n无法从集中标注分配角筋型号、B边纵筋型号、H边纵筋型号");
                    this.valid = false;
                    return;
                }
            }
            else
            {
                List<int> firstReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(strs[0]);
                List<int> secondReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(strs[1]);
                List<int> thirdReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(strs[2]);
                string firstSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(strs[0]);
                string secondSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(strs[1]);
                string thirdSuffix = new ColumnTableRecordInfo().GetReinforceSuffix(strs[2]);
                int firstReinNum = firstReinforceDatas[0];
                int secondReinNum = secondReinforceDatas[0];
                int thirdReinNum = secondReinforceDatas[0];
                //没有原位标注
                if (firstReinNum == secondReinNum || firstReinNum==thirdReinNum)
                {
                    this.sb.Append("\n无法从集中标注分配角筋型号、B边纵筋型号、H边纵筋型号");
                    this.valid = false;
                    return;
                }
                if(this.distinguishCBH.CornerNum * 4 == firstReinNum)
                {
                    this.angularReinforcement = firstReinNum + firstSuffix;
                    if (this.distinguishCBH.BEdgeNum * 2 == secondReinNum &&
                     this.distinguishCBH.HEdgeNum * 2 == thirdReinNum)
                    {
                        this.bSideReinforcement = secondReinNum + secondSuffix;
                        this.hSideReinforcement = thirdReinNum + thirdSuffix;
                    }
                    else if(this.distinguishCBH.BEdgeNum * 2 == thirdReinNum &&
                     this.distinguishCBH.HEdgeNum * 2 == secondReinNum)
                    {
                        this.bSideReinforcement = thirdReinNum + thirdSuffix;
                        this.hSideReinforcement = secondReinNum + secondSuffix;
                    }
                    else
                    {
                        this.sb.Append("\n无法从集中标注分配角筋型号、B边纵筋型号、H边纵筋型号");
                        this.valid = false;
                        return;
                    }
                }
                else if(this.distinguishCBH.CornerNum * 4== secondReinNum)
                {
                    this.angularReinforcement = secondReinNum + secondSuffix;
                    if (this.distinguishCBH.BEdgeNum * 2 == firstReinNum &&
                    this.distinguishCBH.HEdgeNum * 2 == thirdReinNum)
                    {
                        this.bSideReinforcement = firstReinNum + firstSuffix;
                        this.hSideReinforcement = thirdReinNum + thirdSuffix;
                    }
                    else if (this.distinguishCBH.BEdgeNum * 2 == thirdReinNum &&
                    this.distinguishCBH.HEdgeNum * 2 == firstReinNum)
                    {
                        this.bSideReinforcement = thirdReinNum + thirdSuffix;
                        this.hSideReinforcement = firstReinNum + firstSuffix;
                    }
                    else
                    {
                        this.sb.Append("\n无法从集中标注分配角筋型号、B边纵筋型号、H边纵筋型号");
                        this.valid = false;
                        return;
                    }
                }
                else if (this.distinguishCBH.CornerNum * 4 == thirdReinNum)
                {
                    this.angularReinforcement = thirdReinNum + thirdSuffix;
                    if (this.distinguishCBH.BEdgeNum * 2 == firstReinNum &&
                    this.distinguishCBH.HEdgeNum * 2 == secondReinNum)
                    {
                        this.bSideReinforcement = firstReinNum + firstSuffix;
                        this.hSideReinforcement = secondReinNum + secondSuffix;
                    }
                    else if (this.distinguishCBH.BEdgeNum * 2 == secondReinNum &&
                    this.distinguishCBH.HEdgeNum * 2 == firstReinNum)
                    {
                        this.bSideReinforcement = secondReinNum + secondSuffix;
                        this.hSideReinforcement = firstReinNum + firstSuffix;
                    }
                    else
                    {
                        this.sb.Append("\n无法从集中标注分配角筋型号、B边纵筋型号、H边纵筋型号");
                        this.valid = false;
                        return;
                    }
                }
                else
                {
                    this.sb.Append("\n无法从集中标注分配角筋型号、B边纵筋型号、H边纵筋型号");
                    this.valid = false;
                    return;
                }
            }
        }
        private string RemoveAngularChar(string content)
        {
            string result = content;
            content = content.Replace("（","(");
            if(content.IndexOf("角筋")>=0)
            {
                int index = content.IndexOf("(");
                if(index>0)
                {
                    result = content.Substring(0, index);
                }
                else
                {
                    index = content.IndexOf("角筋");
                    result = content.Substring(0, index);
                }
            }
            return result;
        }
        /// <summary>
        /// 获取B边数量
        /// </summary>
        private void GetBHSideReinforceNum()
        {
            if (this.sideTextDic.Count == 2)
            {
                this.bSideReinforcement = this.sideTextDic[TextRotation.Horizontal].TextString;
                this.hSideReinforcement = this.sideTextDic[TextRotation.Vertical].TextString;
                List<int> bSideReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(this.bSideReinforcement);
                List<int> hSideReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(this.hSideReinforcement);
                if (bSideReinforceDatas.Count == 2)
                {
                    this.bSideNum = bSideReinforceDatas[0]*2;
                }
                if (hSideReinforceDatas.Count == 2)
                {
                    this.hSideNum = hSideReinforceDatas[0]*2;
                }
            }
            else if(this.sideTextDic.Count==1)
            {
                if(this.sideTextDic.ContainsKey(TextRotation.Horizontal))
                {
                    this.bSideReinforcement = this.sideTextDic[TextRotation.Horizontal].TextString;
                    List<int> bSideReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(this.bSideReinforcement);
                    if (bSideReinforceDatas.Count == 2)
                    {
                        this.bSideNum = bSideReinforceDatas[0] * 2;
                    }
                }
                if(this.sideTextDic.ContainsKey(TextRotation.Vertical))
                {
                    this.hSideReinforcement = this.sideTextDic[TextRotation.Vertical].TextString;
                    List<int> hSideReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(this.hSideReinforcement);
                    if (hSideReinforceDatas.Count == 2)
                    {
                        this.hSideNum = hSideReinforceDatas[0] * 2;
                    }
                }
            }
        }
        /// <summary>
        /// 从集中标注中获取角筋型号和数量
        /// </summary>
        /// <param name="strs"></param>
        private void GetAngularReinforcement(string[] strs)
        {
            string angularRein = "";
            //如果全部纵筋中有角筋先提取出来
            foreach (string str in strs)
            {
                if (str.IndexOf("角筋") >= 0)
                {
                    angularRein = str;
                    angularRein=angularRein.Replace('（', '(');
                    if (angularRein.IndexOf("(") >= 0)
                    {
                        angularRein = angularRein.Substring(0, angularRein.IndexOf("("));
                    }
                    else
                    {
                        angularRein = angularRein.Substring(0, angularRein.IndexOf("角"));
                    }
                    break;
                }
            }
            this.angularReinforcement= angularRein;
            if(!string.IsNullOrEmpty(this.angularReinforcement))
            {
                List<int> bSideReinforceDatas = new ColumnTableRecordInfo().GetReinforceDatas(this.angularReinforcement);
                this.angularNum = bSideReinforceDatas[0];
            }
        }
        /// <summary>
        /// 查找柱边框旁边的文字
        /// </summary>
        private void CollectSideTextDic()
        {
            List<Point3d> boundaryPts = ThColumnInfoUtils.GetPolylinePts(this.columnFrame);
            boundaryPts = boundaryPts.Select(i => ThColumnInfoUtils.TransPtFromWcsToUcs(i)).ToList();
            double minX = boundaryPts.OrderBy(i => i.X).Select(i => i.X).FirstOrDefault();
            double minY = boundaryPts.OrderBy(i => i.Y).Select(i => i.Y).FirstOrDefault();
            double minZ = boundaryPts.OrderBy(i => i.Z).Select(i => i.Z).FirstOrDefault();
            double maxX = boundaryPts.OrderByDescending(i => i.X).Select(i => i.X).FirstOrDefault();
            double maxY = boundaryPts.OrderByDescending(i => i.Y).Select(i => i.Y).FirstOrDefault();
            double maxZ = boundaryPts.OrderByDescending(i => i.Z).Select(i => i.Z).FirstOrDefault();

            double offsetDis = (Math.Abs(maxX - minX) + Math.Abs(maxY - minY)) / 2.0;
            offsetDis *= this.findBHSideMiddleTextOffsetDisRatio;

            Point3d pt1 = new Point3d(minX - offsetDis, minY - offsetDis, minZ);
            Point3d pt2 = new Point3d(maxX + offsetDis, maxY + offsetDis, minZ);

            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Text") }; //后期根据需要再追加搜索条件
            
            SelectionFilter textSf = new SelectionFilter(tvs);

            PromptSelectionResult psr = ThColumnInfoUtils.SelectByRectangle(doc.Editor,
                           pt1, pt2, PolygonSelectionMode.Crossing, textSf);
            if (psr.Status == PromptStatus.OK)
            {
                List<ObjectId> objIds = psr.Value.GetObjectIds().ToList();
                List<DBText> sideTexts = objIds.Select(i => ThColumnInfoDbUtils.GetEntity(this.doc.Database, i) as DBText).ToList();
                this.sideTextDic = GetValidHoopReinforce(sideTexts, minX, minY, maxX, maxY); //查找柱子外框边的两个文字
            }
        }
        private Dictionary<TextRotation, DBText> GetValidHoopReinforce(List<DBText> dbTexts, double minX, double minY, double maxX, double maxY)
        {
            Dictionary<TextRotation, DBText> textDic = new Dictionary<TextRotation, DBText>();
            dbTexts = dbTexts.Where(i => !string.IsNullOrEmpty(i.TextString) &&
            new ColumnTableRecordInfo().ValidateReinforcement(i.TextString)).Select(i => i).ToList();
            //把方框以内的文字给去掉
            List<DBText> filterDbTexts = new List<DBText>();
            foreach (DBText dbText in dbTexts)
            {
                Extents3d extents = ThColumnInfoUtils.GeometricExtentsImpl(dbText);
                if (extents == null)
                {
                    continue;
                }
                if(!(extents.MinPoint.X >= minX && extents.MinPoint.X >= maxX &&
                                extents.MaxPoint.X >= minX && extents.MaxPoint.X >= maxX &&
                                extents.MinPoint.Y >= minY && extents.MinPoint.Y >= maxY &&
                                extents.MaxPoint.Y >= minY && extents.MaxPoint.Y >= maxY))
                {
                    filterDbTexts.Add(dbText);
                }
            }
            List<DBText> xDirText = dbTexts.Where(i => GetDbTextRotation(i) == TextRotation.Horizontal).Select(i => i).ToList();
            List<DBText> yDirText = dbTexts.Where(i => GetDbTextRotation(i) == TextRotation.Vertical).Select(i => i).ToList();
            double middleX = (minX + maxX) / 2.0;
            double middleY = (minY + maxY) / 2.0;
            xDirText = xDirText.OrderBy(i => Math.Abs(i.Position.X - middleX)).Select(i => i).ToList();
            yDirText = yDirText.OrderBy(i => Math.Abs(i.Position.Y - middleY)).Select(i => i).ToList();
            if (xDirText != null && xDirText.Count > 0)
            {
                textDic.Add(TextRotation.Horizontal, xDirText[0]);
            }
            if (yDirText != null && yDirText.Count > 0)
            {
                textDic.Add(TextRotation.Vertical, yDirText[0]);
            }
            return textDic;
        }
        private TextRotation GetDbTextRotation(DBText dBText)
        {
            var wcs2Ucs = ThColumnInfoUtils.WCS2UCS();
            double ang = 0;
            using (var clone = dBText.GetTransformedCopy(wcs2Ucs))
            {
                ang = (clone as DBText).Rotation;
            }
            ang = ThColumnInfoUtils.RadToAng(ang);
            if (Math.Abs(ang - 90) <= 1.0 || Math.Abs(ang - 270) <= 1.0)
            {
                return TextRotation.Vertical;
            }
            else if (Math.Abs(ang - 0.0) <= 1.0 || Math.Abs(ang - 180) <= 1.0)
            {
                return TextRotation.Horizontal;
            }
            else
            {
                return TextRotation.Oblique;
            }
        }
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
    }
}
