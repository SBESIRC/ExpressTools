using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;

/*
 * 1、一般使用下面三种方式查询，按照使用频率排序
 *     FindElementByAccessibilityId   —— 通过AutomationID查询，最常用
 *     FindElementByName              —— 通过name查询，一般常用
 *     FindElementsByName             —— 通过name查询，返回一个collection，包含所有名字相同的控件信息，速度较慢
 * 2、标定了程序，所以如果要查找的控件不属于程序，则找不到，有如下两种解决方式：
 *     ——由于这类控件一般属于衍生控件，所以可以根据上一步操作推测其坐标，通过坐标对其操作
 *     ——新绑定一个缓存存放控件的父级程序，目前没有实现
 * 3、出现未找到控件的原因
 *     ——控件AutomationID或者name没有匹配  —— 确认控件信息
 *     ——窗口还未刷新就开始查询控件        —— 测试程序中主动加入等待
 * 4、关于鼠标
 *     ——AutoCAD.FindElementByName("关闭").Click();                             —— 直接通过控件的Click()点击  —— 没有可选项，点击就是控件中心位置
 *     ——AutoCAD.Mouse.Click(null);                                             —— 点击当前位置
 *     ——AutoCAD.Mouse.Click(AutoCAD.FindElementByName("关闭").Coordinates);    —— 点击控件中心位置
 *     ——如果要点击控件某个相对位置，使用先MouseMove(*.Coordinates, *, *)，再Click(null)
 * 5、关于键盘
 *     ——就目前来看，所有edit编辑框都是能用AutomationID精确匹配，直接SendKeys(*)即可
 *     ——AutoCAD.Mouse.SendKeys(*)，这个是向当前程序焦点处模拟键盘按键
*/

namespace SampleClass
{
    [TestFixture]
    class ThUiTest
    {
        private WindowsDriver<WindowsElement> AutoCAD;

        [SetUp]
        public void Setup()
        {
            // 启动CAD
            DesiredCapabilities capabilities = new DesiredCapabilities();
            capabilities.SetCapability("app", @"C:\Program Files\Autodesk\AutoCAD 2012 - Simplified Chinese\acad.exe");
            AutoCAD = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), capabilities);
            AutoCAD.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
        }

        [TearDown]
        public void Teardown()
        {
            // 关闭CAD
            AutoCAD.FindElementByName("关闭").Click();
            // 是否保存——否
            Thread.Sleep(TimeSpan.FromSeconds(1));
            AutoCAD.FindElementByAccessibilityId("7").Click();
        }
        
        [Test]
        public void Test_a_Basicaltest()
        {
            // 向命令行输入"STYLE\n"
            AutoCAD.FindElementByAccessibilityId("2").SendKeys("STYLE\n");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            // 颠倒
            AutoCAD.FindElementByAccessibilityId("906").Click();
            // 高度
            AutoCAD.FindElementByAccessibilityId("1265").Clear();
            AutoCAD.FindElementByAccessibilityId("1265").SendKeys("1.2");
            // 应用
            AutoCAD.FindElementByAccessibilityId("915").Click();
            // 关闭
            AutoCAD.FindElementByAccessibilityId("917").Click();
            // 画线
            AutoCAD.FindElementByAccessibilityId("2").SendKeys("LINE\n");
            // 缓存画布
            WindowsElement paintplate = AutoCAD.FindElementByAccessibilityId("59648");

            // 鼠标移动到画布相对于左上角(100,100)位置
            // 其中 *.Coordinates是*控件的中心坐标
            // 如果没有后面两项会移动到控件中心位置
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 100, 100);

            // 点击（如果为"null"则点击当前位置）
            AutoCAD.Mouse.Click(null);
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 200, 200);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 100, 200);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Keyboard.SendKeys("C\n");
        }

        [Test]
        public void Test_b_THBPStest()
        {
            // 画布窗口
            WindowsElement paintplate = AutoCAD.FindElementByAccessibilityId("59648");

            // 画线6次
            for (int i = 0; i < 6; i++)
            {
                AutoCAD.FindElementByAccessibilityId("2").SendKeys("PLINE\n");
                AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 300 + i * 150, 100);
                AutoCAD.Mouse.Click(null);
                AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 400 + i * 150, 100);
                AutoCAD.Mouse.Click(null);
                AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 300 + i * 150, 200);
                AutoCAD.Mouse.Click(null);
                AutoCAD.Keyboard.SendKeys("C\n");
            }

            // 打开和移动
            AutoCAD.FindElementByAccessibilityId("2").SendKeys("THBPS\n");
            Thread.Sleep(TimeSpan.FromSeconds(3));
            AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByName("天华单体框线图层规整").Coordinates, 100, 10);
            AutoCAD.Mouse.MouseDown(null);
            AutoCAD.Mouse.MouseMove(null, 0, 100);
            AutoCAD.Mouse.MouseUp(null);

            // 第一页测试
            AutoCAD.FindElementByAccessibilityId("textEdit_number").SendKeys("1#");
            AutoCAD.FindElementByAccessibilityId("textEdit_name").SendKeys("一号楼");
            AutoCAD.FindElementByName("Open").Click();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByName("公建").Coordinates);
            AutoCAD.Mouse.Click(null);
            AutoCAD.FindElementByAccessibilityId("textEdit_above_ground_storeys").SendKeys("10");
            AutoCAD.FindElementByAccessibilityId("textEdit_under_ground_storeys").SendKeys("1");
            AutoCAD.FindElementByAccessibilityId("simpleButton_OK").Click();
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 300, 100);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Keyboard.SendKeys("\n");
            Assert.AreEqual(AutoCAD.FindElementByAccessibilityId("comboBoxEdit_category").Text, "公建");

            // 第二页选择测试
            AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByAccessibilityId("tabPane1").Coordinates, 100, 15);
            AutoCAD.Mouse.Click(null);
            
            var windowsElements = AutoCAD.FindElementsByName("选择 row 0");
            for(int i = windowsElements.Count - 1, j = 0; i >= 0; j += 150, i--)
            {
                windowsElements[i].Click();
                AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 450 + j, 100);
                AutoCAD.Mouse.DoubleClick(null);
                AutoCAD.Mouse.Click(null);
                AutoCAD.Keyboard.SendKeys("\n");
            }
            
            AutoCAD.FindElementByName("选择 row 1").Click();
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 1050, 100);
            AutoCAD.Mouse.DoubleClick(null);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Keyboard.SendKeys("\n");
            Assert.AreEqual(AutoCAD.FindElementByName("面积 row 0").Text, "0.03");
            Assert.AreEqual(AutoCAD.FindElementByName("面积 row 1").Text, "0.03");
            // 删除一项
            AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByName("编号 row 0").Coordinates);
            AutoCAD.Mouse.ContextClick(null);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            AutoCAD.Mouse.MouseMove(null, 20, 10);
            AutoCAD.Mouse.Click(null);
            // 添加楼层
            AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByName("c1").Coordinates, 10, -10);
            AutoCAD.Mouse.ContextClick(null);
            AutoCAD.Mouse.MouseMove(null, 20, 10);
            AutoCAD.Mouse.Click(null);
            AutoCAD.FindElementByAccessibilityId("textBox_storey").SendKeys("2");
            AutoCAD.FindElementByAccessibilityId("button_ok").Click();
            
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
        
        [Test]
        public void Test_c_THBPPtest()
        {
            // 画布窗口
            WindowsElement paintplate = AutoCAD.FindElementByAccessibilityId("59648");

            // 打开图纸
            AutoCAD.FindElementByAccessibilityId("2").SendKeys("OPEN\n");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByName("打印到PDF测试图纸").Coordinates);
            AutoCAD.Mouse.DoubleClick(null);
            // SHX选择
            Thread.Sleep(TimeSpan.FromSeconds(1));
            if (AutoCAD.FindElementsByName("缺少 SHX 文件").Count != 0)
            {
                AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByName("忽略缺少的 SHX 文件并继续").Coordinates);
                AutoCAD.Mouse.Click(null);
            }
            // 代理图形选择
            Thread.Sleep(TimeSpan.FromSeconds(1));
            if (AutoCAD.FindElementsByName("代理信息").Count != 0)
            {
                AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByName("显示代理图形(S)").Coordinates);
                AutoCAD.Mouse.Click(null);
                AutoCAD.FindElementByName("确定").Click();
            }
            // Click or SendKeys
            AutoCAD.FindElementByAccessibilityId("2").SendKeys("THBPP\n");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            AutoCAD.FindElementByAccessibilityId("radioLeft2Right").Click();
            // 输入值1
            AutoCAD.FindElementByAccessibilityId("comboPPTLayer").Click();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByAccessibilityId("comboPPTLayer").Coordinates, AutoCAD.FindElementByAccessibilityId("comboPPTLayer").Size.Width / 2, 230);
            AutoCAD.Mouse.Click(null);
            // 输入值2
            AutoCAD.FindElementByAccessibilityId("comboPrintImage").Click();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByAccessibilityId("comboPrintImage").Coordinates, AutoCAD.FindElementByAccessibilityId("comboPrintImage").Size.Width / 2, 240);
            AutoCAD.Mouse.Click(null);
            // 输入值3
            AutoCAD.FindElementByAccessibilityId("comboPrintTextLayer").Click();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            AutoCAD.Mouse.MouseMove(AutoCAD.FindElementByAccessibilityId("comboPrintTextLayer").Coordinates, AutoCAD.FindElementByAccessibilityId("comboPrintTextLayer").Size.Width / 2, 250);
            AutoCAD.Mouse.Click(null);
            // 选择
            AutoCAD.FindElementByAccessibilityId("btnSelectPrintProfile").Click();
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 100, 100);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 1800, 700);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Keyboard.SendKeys("\n");
            // 确定
            Thread.Sleep(TimeSpan.FromSeconds(1));
            AutoCAD.FindElementByAccessibilityId("btnOK").Click();
            // 等待打印完成
            while (AutoCAD.FindElementsByName("批量打印PPT").Count != 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
