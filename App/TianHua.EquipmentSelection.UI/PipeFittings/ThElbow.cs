using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianHua.FanSelection.UI.PipeFittings
{
    public class ThElbow
    {
        /// <summary>
        /// 弯头角度(弧度)
        /// </summary>
        public double ElbowDegree { get; set; }

        /// <summary>
        /// 管道直径
        /// </summary>
        public double PipeDiameter { get; set; }

        /// <summary>
        /// 末端直管段长度
        /// </summary>
        public double ReserveLength { get; set; }

        /// <summary>
        /// 中心点
        /// </summary>
        public Point3d CenterPoint { get; set; }

        public ThElbow(double elbowdegree, double pipediameter, Point3d connerpoint)
        {
            ElbowDegree = elbowdegree * Math.PI/180;
            PipeDiameter = pipediameter;
            CenterPoint = connerpoint + new Vector3d(-PipeDiameter, -Math.Abs(pipediameter*Math.Tan(0.5 * ElbowDegree)),0);
        }

        public void DrawElbow()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                //创建弯头内外侧圆弧
                acadDatabase.Database.AddLayer("Auot_DUCT-加压送风管");
                Arc outerarc = new Arc(CenterPoint, 1.5 * PipeDiameter, 0, ElbowDegree)
                {
                    Layer = "Auot_DUCT-加压送风管",
                    ColorIndex = 1
                };
                Arc innerarc = new Arc(CenterPoint, 0.5 * PipeDiameter, 0, ElbowDegree)
                {
                    Layer = "Auot_DUCT-加压送风管",
                    ColorIndex = 1
                };
                //创建弯头两端的50mm延申段
                Line outerendextendline = new Line()
                {
                    StartPoint = outerarc.EndPoint,
                    EndPoint = outerarc.EndPoint + new Vector3d(-50*Math.Sin(ElbowDegree), 50 * Math.Cos(ElbowDegree),0),
                    Layer = "Auot_DUCT-加压送风管",
                    ColorIndex = 1
                };
                Line innerendextendline = new Line()
                {
                    StartPoint = innerarc.EndPoint,
                    EndPoint = innerarc.EndPoint + new Vector3d(-50 * Math.Sin(ElbowDegree), 50 * Math.Cos(ElbowDegree), 0),
                    Layer = "Auot_DUCT-加压送风管",
                    ColorIndex = 1
                };
                Line outerstartextendline = new Line()
                {
                    StartPoint = outerarc.StartPoint,
                    EndPoint = outerarc.StartPoint + new Vector3d(0, -50, 0),
                    Layer = "Auot_DUCT-加压送风管",
                    ColorIndex = 1
                };
                Line innerstartextendline = new Line()
                {
                    StartPoint = innerarc.StartPoint,
                    EndPoint = innerarc.StartPoint + new Vector3d(0, -50, 0),
                    Layer = "Auot_DUCT-加压送风管",
                    ColorIndex = 1
                };

                //创建弯头中心线圆弧
                acadDatabase.Database.AddLayer("Auot_DUCT-加压送风中心线");
                Arc centerarc = new Arc(CenterPoint, PipeDiameter, 0, ElbowDegree)
                {
                    Layer = "Auot_DUCT-加压送风中心线",
                    ColorIndex = 5
                };

                //创建弯头端线
                acadDatabase.Database.AddLayer("Auot_DUCT-加压送风管端线");
                Line startplaneline = new Line()
                {
                    StartPoint = innerarc.StartPoint,
                    EndPoint = outerarc.StartPoint,
                    Layer = "Auot_DUCT-加压送风管端线",
                    ColorIndex = 2
                };

                Line endplaneline = new Line()
                {
                    StartPoint = innerarc.EndPoint,
                    EndPoint = outerarc.EndPoint,
                    Layer = "Auot_DUCT-加压送风管端线",
                    ColorIndex = 2
                };

                //创建两端50mm延申段的端线
                Line startextendsealline = new Line()
                {
                    StartPoint = outerstartextendline.EndPoint,
                    EndPoint = innerstartextendline.EndPoint,
                    Layer = "Auot_DUCT-加压送风管端线",
                    ColorIndex = 2
                };

                Line endextendsealline = new Line()
                {
                    StartPoint = outerendextendline.EndPoint,
                    EndPoint = innerendextendline.EndPoint,
                    Layer = "Auot_DUCT-加压送风管端线",
                    ColorIndex = 2
                };


                List<Entity> addentities = new List<Entity>()
                {
                    outerarc,
                    centerarc,
                    innerarc,
                    startplaneline,
                    endplaneline,
                    outerendextendline,
                    innerendextendline,
                    outerstartextendline,
                    innerstartextendline,
                    startextendsealline,
                    endextendsealline
                };
                foreach (var ent in addentities)
                {
                    acadDatabase.ModelSpace.Add(ent);
                }
            }

        }
    }
}
