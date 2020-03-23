using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace THSitePlanRoadDemo
{
    /// <summary>
    /// 二维平面坐标及其计算处理
    /// </summary>
    public class XY
    {
        private double m_x;
        private double m_y;
        public XY(double x, double y)
        {
            m_x = x;
            m_y = y;
        }

        /// <summary>
        /// 计算有向角度
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public double CalAngle(XY dir)
        {
            double val = m_x * dir.X + m_y * dir.Y;
            double tmp = Math.Sqrt(Math.Pow(m_x, 2) + Math.Pow(m_y, 2)) * Math.Sqrt(Math.Pow(dir.X, 2) + Math.Pow(dir.Y, 2));
            double angleRad = Math.Acos(CommonUtils.CutRadRange(val / tmp));

            if (CrossProduct(dir) < 0)
                return -angleRad;
            return angleRad;
        }

        public bool IsEqualTo(XY pt)
        {
            var pt1 = new Point2d(m_x, m_y);
            var pt2 = new Point2d(pt.X, pt.Y);
            if (pt1.IsEqualTo(pt2))
                return true;
            return false;
        }

        private double CrossProduct(XY dir)
        {
            return (m_x * dir.Y - m_y * dir.X);
        }

        public double GetLength()
        {
            var length = Math.Sqrt(Math.Pow(m_x, 2) + Math.Pow(m_y, 2));
            return length;
        }

        public static XY operator -(XY left, XY right)
        {
            return new XY(left.X - right.X, left.Y - right.Y);
        }

        public static XY operator +(XY left, XY right)
        {
            return new XY(left.X + right.X, left.Y + right.Y);
        }

        public double X
        {
            get { return m_x; }
            set { m_x = value; }
        }

        public double Y
        {
            get { return m_y; }
            set { m_y = value; }
        }
    }
}
