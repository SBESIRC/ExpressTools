using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrame
{
    // 公建楼层
    public class AOccupancyStorey : IEquatable<AOccupancyStorey>, IComparable<AOccupancyStorey>
    {
        public char tag;
        public int number;
        public bool standard;

        #region Equality
        public bool Equals(AOccupancyStorey other)
        {
            if (other == null) return false;
            return (this.tag == other.tag) && (this.number == other.number) && (this.standard == other.standard);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as AOccupancyStorey);
        }
        public override int GetHashCode()
        {
            return new { number, standard }.GetHashCode();
        }
        #endregion

        public int CompareTo(AOccupancyStorey other)
        {
            if (other == null)
            {
                return 1;
            }

            // 比较楼层
            return this.number.CompareTo(other.number);
        }
    }

    // 住宅楼层
    public class ResidentialStorey : IEquatable<ResidentialStorey>, IComparable<ResidentialStorey>
    {
        public char tag;
        public int number;
        public bool standard;

        #region Equality
        public bool Equals(ResidentialStorey other)
        {
            if (other == null) return false;
            return (this.tag == other.tag) && (this.number == other.number) && (this.standard == other.standard);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as ResidentialStorey);
        }
        public override int GetHashCode()
        {
            return new { number, standard }.GetHashCode();
        }
        #endregion

        public int CompareTo(ResidentialStorey other)
        {
            if (other == null)
            {
                return 1;
            }

            // 比较楼层
            return this.number.CompareTo(other.number);
        }
    }

    // 混合楼层
    public class CompositeStorey : IEquatable<CompositeStorey>, IComparable<CompositeStorey>
    {
        public char tag;
        public int number;
        public bool standard;

        #region Equality
        public bool Equals(CompositeStorey other)
        {
            if (other == null) return false;
            return (this.tag == other.tag) && (this.number == other.number) && (this.standard == other.standard);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as CompositeStorey);
        }
        public override int GetHashCode()
        {
            return new { number, standard }.GetHashCode();
        }
        #endregion

        public int CompareTo(CompositeStorey other)
        {
            if (other == null)
            {
                return 1;
            }

            // 比较楼层
            return this.number.CompareTo(other.number);
        }
    }
}
