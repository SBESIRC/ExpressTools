using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace TianHua.AutoCAD.Parking
{
    public class ParkingLot
    {
        public BlockReference Ent { get; set; }//车位实体
        public Polyline Boundry { get; set; }//实体边界
        public double BoundryRotation { get; set; }//世界坐标系下的车位角度
        public List<DBText> Numbers { get; set; }//车位编号
        public int Count { get; set; }//编号次数

        public ParkingLot(BlockReference blockReference, int count)
        {
            this.Ent = blockReference;
            this.Boundry = GetWaiJieRec();
            this.BoundryRotation = GetBoundryRotation();
            this.Count = count;
        }

        /// <summary>
        ///为车位赋值编号
        /// </summary>
        /// <param name="number"></param>
        public void SetNumber(ThNumberInfo numberInfo, ThNumberStyle numberStyle)
        {
            //计算车位的中心
            Point3d position = CalBoundryCenter();

            //接下来开始实现在世界坐标系下的，多个编号的偏移
            var numbers = new List<DBText>();
            for (int i = 0; i < Count; i++)
            {
                //向上偏移指定距离后，再向下偏移0/(D/2)/D/(D*1.5)。。。。使其回到中心
                var vector = new Vector3d(0, numberStyle.OffsetDis * (i - (Count - 1) * 0.5), 0);
                var bcsPosition = position.TransformBy(Matrix3d.Displacement(vector));

                //实例化第一个编号(加上前缀后缀)，中心为上述中心
                var text = new DBTextEx(numberInfo.NumberWithFix(numberInfo.NumberWithComplementaryAdd(i)), bcsPosition, numberStyle.NumberHeight, numberStyle.NumberLayerName, 0);
                //按中心按指定的中心进行旋转
                text.TransformBy(Matrix3d.Rotation(GetRealAngle(), Vector3d.ZAxis, position));

                numbers.Add(text);


                #region 原来不居中的算法
                ////System.Windows.Forms.MessageBox.Show(Math.Cos(baseText.Rotation).ToString());
                ////计算当前编号，基于其实编号的偏移量，确定中心位置
                //var addx = Math.Abs(Math.Sin(baseText.Rotation) * numberStyle.OffsetDis * i);
                ////var movex = Math.Sin(baseText.Rotation) * (numberStyle.OffsetDis * ((Count - 1) * 0.5));

                ////第一、第三象限都是负的
                //addx = (baseText.Rotation <= Math.PI / 2 && baseText.Rotation >= 0) || (baseText.Rotation <= Math.PI * 1.5 && baseText.Rotation >= Math.PI) ? -addx : addx;
                //var bcsPosition = new Point3d(position.X + addx, position.Y + Math.Abs(Math.Cos(baseText.Rotation) * numberStyle.OffsetDis * i), 0);

                ////var bcsPosition = new Point3d(position.X + addx + movex, position.Y + (numberStyle.OffsetDis * i - numberStyle.OffsetDis * ((Count - 1) * 0.5)) * Math.Abs(Math.Cos(baseText.Rotation)), 0);

                ////实例化第一个编号,中心位置如上述，旋转角度跟随块参照,并为编号进行补位操作和前后缀操作
                ////var gg = new DBTextEx(numberInfo.NumberWithFix(numberInfo.StartNumber + i), bcsPosition, numberStyle.NumberHeight, numberStyle.NumberLayerName, this.BoundryRotation);
                //var gg = new DBTextEx(numberInfo.NumberWithFix(numberInfo.NumberWithComplementaryAdd(i)), bcsPosition, numberStyle.NumberHeight, numberStyle.NumberLayerName, this.BoundryRotation);

                ////如果文字的旋转角度为[0,90]或者[270,360)，文字的显示就是我们要的向上或者向左，其他情况减掉180翻过来
                ////******270°的改为了开区间
                //if (!((gg.Rotation <= Math.PI / 2 && gg.Rotation >= 0) || (gg.Rotation < Math.PI * 2 && gg.Rotation > Math.PI * 1.5)))
                //{
                //    gg.Rotation -= Math.PI;
                //}

                //numbers.Add(gg);
                #endregion
            }
            this.Numbers = numbers;

            #region 在块定义下，实现的车位多编号，除了镜像都是正确的
            //****希望实现的效果，是文字和块参照永远呈水平，但存在原始块本身就不是水平的状态，故不能以块参照的角度决定文字角度，而是要以块定义和块参照共同决定文字角度

            ////复制对象
            //var cloneBoundry = (Polyline)this.Boundry.Clone();
            ////转成水平的
            //cloneBoundry.TransformBy(this.Ent.BlockTransform.Inverse());

            ////编号位置为块定义的外边接中心
            //var position = cloneBoundry.GetCenter().toPoint3d();

            //var numbers = new List<DBText>();
            //for (int i = 0; i < Count; i++)
            //{
            //    //在块定义的坐标系下进行向上偏移值的设定
            //    var bcsPosition = new Point3d(position.X, position.Y + manager.OffsetDis * i, 0);
            //    //实例化第一个编号
            //    var gg = new DBTextEx((Convert.ToInt32(number) + i).ToString(), bcsPosition, manager.NumberHeight, manager.NumberLayerName);
            //    //将编号对应的块参照的实际位置变回去
            //    gg.TransformBy(this.Ent.BlockTransform);

            //    //如果文字的旋转角度为[0,90]或者(270,360)，文字的显示就是我们要的向上或者向左，其他情况减掉180翻过来
            //    if (!((gg.Rotation <= Math.PI / 2 && gg.Rotation >= 0) || (gg.Rotation < Math.PI * 2 && gg.Rotation > Math.PI * 1.5)))
            //    {
            //        gg.Rotation -= Math.PI;
            //    }

            //    numbers.Add(gg);
            //}
            //this.Numbers = numbers; 
            #endregion

        }

        /// <summary>
        /// 计算车位中心
        /// </summary>
        /// <returns></returns>
        private Point3d CalBoundryCenter()
        {
            //复制对象
            var cloneBoundry = (Polyline)this.Boundry.Clone();
            //转成水平的
            cloneBoundry.TransformBy(this.Ent.BlockTransform.Inverse());

            //找到块定义中的中心位置
            var position = cloneBoundry.GetCenter().toPoint3d();
            //回到世界坐标系求出中心
            position = position.TransformBy(this.Ent.BlockTransform);
            return position;
        }

        /// <summary>
        /// 求块参照的外边界
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private Polyline GetWaiJieRec()
        {
            //var angel = block.Rotation;
            ////如果有角度就需要旋转

            //复制对象
            var cloneBlock = (BlockReference)this.Ent.Clone();
            //转成水平的
            cloneBlock.TransformBy(this.Ent.BlockTransform.Inverse());

            //Polyline poly = new Polyline();
            //poly.CreateRectangle(cloneBlock.GeometricExtents.MinPoint.toPoint2d(), cloneBlock.GeometricExtents.MaxPoint.toPoint2d());

            var poly = new PolylineRec(cloneBlock.GeometricExtents.MinPoint.toPoint2d(), cloneBlock.GeometricExtents.MaxPoint.toPoint2d());

            //转回去
            poly.TransformBy(this.Ent.BlockTransform);

            return poly;

        }

        /// <summary>
        /// 计算世界坐标系下，车位边界与Y轴的夹角
        /// </summary>
        /// <returns></returns>
        private double GetBoundryRotation()
        {
            var lines = new DBObjectCollection();
            this.Boundry.Explode(lines);

            var longLine = lines.Cast<Line>().MaxElement(l => l.Length);

            //将与X轴夹角减去90°得到Y轴夹角
            return longLine.Angle - Math.PI / 2;

            //超过90°，就改为负角度

        }


        /// <summary>
        /// 按文字排列由下到上的方式，且始终朝向正Y负X方向，计算文字实际旋转的角度
        /// </summary>
        /// <returns></returns>
        private double GetRealAngle()
        {
            var lines = new DBObjectCollection();
            this.Boundry.Explode(lines);

            var longLine = lines.Cast<Line>().MaxElement(l => l.Length);

            //让两坐标点按Y值从大到小排序,此为文字的纵向排列标准，X从大到小排序,此为文字的横向排列标准
            var pts = new List<Point3d>() { longLine.StartPoint, longLine.EndPoint }.OrderByDescending(pt => pt.Y).ThenByDescending(pt=>pt.X).ToList();

            //确认标准后，重新画长边，以此确认车位边界的整体旋转角度
            var realLine = new Line(pts[0], pts[1]);

            //转换为与Y轴的夹角进行计算
            var angle = realLine.Angle - Math.PI / 2;

            //如果第一象限，说明转的对，不用动
            //如果转到第二象限，说明翻过来了，不要翻过来，减掉180
            //如果第三象限，说明翻过来了，不要翻过来，减掉180
            //如果第四象限，也转的对，不用动

            //由文字朝向正Y负X方向，结合四象限情况，确认最终文字旋转角度
            if (angle > Math.PI / 2 && angle < Math.PI * 1.5)
            {
                angle -= Math.PI;
            }
            return angle;
        }


    }
}
