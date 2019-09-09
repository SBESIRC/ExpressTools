using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThRoomBoundary.topo
{
    class TopoUtils
    {
        /// <summary>
        /// 获取包含点的最小轮廓,topo算法的上层进行数据的处理，处理为直线段
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="point3"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> MakeStructureMinLoop(List<LineSegment2d> lines, Point2d point)
        {
            var profiles = TopoSearch.MakeMinProfileLoopsInner(lines, point);
            return profiles;
        }

        /// <summary>
        /// 获取闭合轮廓
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> MakeSrcProfiles(List<LineSegment2d> lines)
        {
            var profiles = TopoSearch.MakeSrcProfileLoops(lines);
            return profiles;
        }
    }

    //class MinProfile
    //{
    //    private List<List<LineSegment2d>> m_LineLoops = new List<List<LineSegment2d>>();
    //    public List<List<LineSegment2d>> LineLoops
    //    {
    //        get { return m_LineLoops; }
    //    }

    //    public MinProfile(List<LineSegment2d> lines, Point2d point)
    //    {
    //        var profiles = TopoSearch.MakeMinProfileLoopsInner(lines, point);
    //        var wLineLoops = new List<List<LineSegment2d>>();
    //        if (profiles != null)
    //        {
    //            m_LineLoops.AddRange(profiles);
    //        }
    //    }
    //}
}
