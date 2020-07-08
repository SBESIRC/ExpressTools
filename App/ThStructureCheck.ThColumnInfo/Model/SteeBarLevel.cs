using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    public class SteeBarLevel //: CNotifyPropertyChange
    {
        private string steelBarGrade = "";
        private double sblValue = 0.0;
        private string matchStr = "";
        public string SteelBarGrade
        {
            get
            {
                return steelBarGrade;
            }
            set
            {
                steelBarGrade = value;
                //NotifyPropertyChange("SteelBarGrade");
            }
        }
        public double Value
        {
            get
            {
                return sblValue;
            }
            set
            {
                sblValue = value;
                //NotifyPropertyChange("Value");
            }
        }
            
        public string MatchStr
        {
            get
            {
                return matchStr;
            }
            set
            {
                matchStr = value;
                //NotifyPropertyChange("MatchStr");
            }
        }
    }
}
