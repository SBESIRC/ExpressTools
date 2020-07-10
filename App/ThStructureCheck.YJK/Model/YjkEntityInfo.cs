﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThStructureCheck.Common.Interface;
using ThStructureCheck.YJK.Interface;

namespace ThStructureCheck.YJK.Model
{
    public abstract class YjkEntityInfo
    {
        private string dbPath = "";
        public int ID { get; set; }
        public string DbPath
        {
            get
            {
                return this.dbPath;
            }
            set
            {
                this.dbPath = value;
            }
        }
        public virtual Point GetCoordinate()
        {
            return new Point(0, 0, 0);
        }
    }
}