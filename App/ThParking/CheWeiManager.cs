using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianHua.AutoCAD.Parking
{
    /// <summary>
    /// 用来管理配置信息的类
    /// </summary>
    public class CheWeiManager
    {
        public string PolyLayerName { get; set; }//车位轨迹图层
        public string NumberLayerName { get; set; }//车位编号图层
        public double NumberHeight { get; set; }//车位编号文字高度
        public string NumberTextStyle { get; set; }//车位编号文字样式名
        public double OffsetDis { get; set; }//编号向上偏移距离

        public CheWeiManager(string polyLayerName, string numbaerLayerName, string numberTextStyle, double height,double offsetDis)
        {
            this.PolyLayerName = polyLayerName;
            this.NumberLayerName = numbaerLayerName;
            this.NumberTextStyle =numberTextStyle;
            this.NumberHeight = height;
            this.OffsetDis = offsetDis;
        }

        ///// <summary>
        ///// 获取和当前cad相关的objectid初始化信息
        ///// </summary>
        ///// <param name="db"></param>
        //public void GetObjectIdSetting(TextStyleTable st)
        //{
        //    //重新给numberstyle赋值，确定当前文字样式的Objectid
        //    var gg = st[this.NumberTextStyle.Key];
        //    var tt = this.NumberTextStyle.Key;
        //    this.NumberTextStyle = new KeyValuePair<string, ObjectId>(tt, gg);
        //}

    }
}
