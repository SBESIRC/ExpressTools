using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Linq2Acad;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThFireCompartmentAreaFrame
    {
        // 面积
        public double Area
        {
            get
            {
                return Frame.AreaEx();
            }
        }

        // 面积框线
        public IntPtr Frame { get; set; }

        // 状态
        public bool IsDefined
        {
            get
            {
                return Frame != (IntPtr)0;
            }
        }
    }

    [Serializable()]
    public class ThFireCompartment : IEquatable<ThFireCompartment>, IComparable<ThFireCompartment>
    {
        // 子项编号
        public UInt16 Subkey { get; set; }

        // 楼层
        public UInt16 Storey { get; set; }

        // 序号
        public UInt16 Index { get; set; }

        // 自动灭火系统
        public bool SelfExtinguishingSystem { get; set; }

        // 面积框线
        public List<ThFireCompartmentAreaFrame> Frames;

        // 构造函数
        public ThFireCompartment(UInt16 subKey, UInt16 storey, UInt16 index)
        {
            InitFireCompartment(subKey, storey, index);
        }

        // 构造函数
        public ThFireCompartment(string sn)
        {
            UInt16 subKey = 0, storey = 0, index = 0;
            sn.MatchCommerceSerialNumber(ref subKey, ref storey, ref index);
            InitFireCompartment(subKey, storey, index);
        }

        private void InitFireCompartment(UInt16 subKey, UInt16 storey, UInt16 index)
        {
            SelfExtinguishingSystem = true;
            Subkey = subKey; Storey = storey; Index = index;
            Frames = new List<ThFireCompartmentAreaFrame>();
        }

        public bool Equals(ThFireCompartment other)
        {
            if (other == null)
            {
                return false;
            }

            return this.SerialNumber == other.SerialNumber;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ThFireCompartment compartment = obj as ThFireCompartment;
            if (compartment != null)
            {
                return Equals(compartment);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.SerialNumber.GetHashCode();
        }

        public int CompareTo(ThFireCompartment other)
        {
            if (other == null)
            {
                return 1;
            }

            // 比较子项编号
            if (this.Subkey != other.Subkey)
            {
                return this.Subkey.CompareTo(other.Subkey);
            }

            // 在子项编号相等的情况下，继续比较楼层
            if (this.Storey != other.Storey)
            {
                return this.Storey.CompareTo(other.Storey);
            }

            // 在楼层相等的情况下，继续比较编号
            return this.Index.CompareTo(other.Index);
        }

        public static bool operator ==(ThFireCompartment compartment1, ThFireCompartment compartment2)
        {
            if (((object)compartment1) == null || ((object)compartment2) == null)
                return Object.Equals(compartment1, compartment2);

            return compartment1.Equals(compartment2);
        }

        public static bool operator !=(ThFireCompartment compartment1, ThFireCompartment compartment2)
        {
            if (((object)compartment1) == null || ((object)compartment2) == null)
                return !Object.Equals(compartment1, compartment2);

            return !compartment1.Equals(compartment2);
        }

        // 编号
        public string SerialNumber
        {
            get
            {
                return ThFireCompartmentUtil.CommerceSerialNumber(Subkey, Storey, Index);
            }
        }

        // 面积
        public double Area
        {
            get
            {
                double area = 0.0;
                Frames.Where(o => o.IsDefined)
                    .ToList()
                    .ForEach(o => area += o.Area);
                return area;
            }
        }

        // 是否合并
        public bool IsMerged
        {
            get
            {
                return Frames.Where(o => o.IsDefined).Count() > 1;
            }
        }

        // 是否定义
        public bool IsDefined
        {
            get
            {
                return Frames.Where(o => o.IsDefined).Count() > 0;
            }
        }
    }
}
