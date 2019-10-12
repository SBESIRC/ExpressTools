using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Linq2Acad;

namespace ThAreaFrameConfig.Model
{
    [Serializable()]
    public class ThFireCompartment : IEquatable<ThFireCompartment>, IComparable<ThFireCompartment>
    {
        public enum FCType
        {
            FCCommerce,
            FCUnderGroundParking
        }

        // 序号
        public int Number { get; set; }

        // 子项编号
        public UInt16 Subkey { get; set; }

        // 楼层
        public Int16 Storey { get; set; }

        // 序号
        public UInt16 Index { get; set; }

        // 自动灭火系统
        public bool SelfExtinguishingSystem { get; set; }

        // 面积框线
        public List<ThFireCompartmentAreaFrame> Frames;

        // 构造函数
        public ThFireCompartment(UInt16 subKey, Int16 storey, UInt16 index)
        {
            InitFireCompartment(subKey, storey, index);
        }

        public static ThFireCompartment Commerce(string sn)
        {
            Int16 storey = 0;
            UInt16 subKey = 0, index = 0;
            if (!sn.MatchCommerceSerialNumber(ref subKey, ref storey, ref index))
            {
                return null;
            }
            return new ThFireCompartment(subKey, storey, index);
        }

        public static ThFireCompartment UnderGroundParking(string sn)
        {
            UInt16 index = 0;
            if (!sn.MatchUndergroundParkingSerialNumber(ref index))
            {
                return null;
            }

            return new ThFireCompartment(0, -1, index);
        }

        private void InitFireCompartment(UInt16 subKey, Int16 storey, UInt16 index)
        {
            Number = 0;
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
                // 考虑到地下楼层，按绝对值比较
                return Math.Abs(Storey).CompareTo(Math.Abs(other.Storey));
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
                if (Type == FCType.FCCommerce)
                {
                    return ThFireCompartmentUtil.CommerceSerialNumber(Subkey, Storey, Index);
                }
                else if (Type == FCType.FCUnderGroundParking)
                {
                    return ThFireCompartmentUtil.UndergroundParkingSerialNumber(Index);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        // 面积
        public double Area
        {
            get
            {
                return Frames.Where(o => o.IsDefined)
                    .ToList()
                    .Sum(o => o.Area);
            }
        }

        // 疏散密度
        public double EvacuationDensity
        {
            get
            {
                return Frames.Where(o => o.IsDefined)
                    .ToList()
                    .Sum(o => o.EvacuationDensity);
            }
        }

        // 最远疏散距离
        public double EvacuationDistance
        {
            get
            {
                return Frames.Where(o => o.IsDefined)
                    .ToList()
                    .Max(o => o.EvacuationDistance);
            }
        }

        // 安全出口
        public UInt16 EmergencyExit
        {
            get
            {
                return Frames.Where(o => o.IsDefined)
                    .ToList()
                    .Max(o => o.EmergencyExit);
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

        // 类型
        public FCType Type
        {
            get
            {
                if ((Subkey == 0) && (Storey < 0))
                {
                    return FCType.FCUnderGroundParking;
                }
                else if ((Subkey > 0) && (Storey > 0))
                {
                    return FCType.FCCommerce;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
