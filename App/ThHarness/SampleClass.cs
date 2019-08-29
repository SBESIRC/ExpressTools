using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;

namespace SampleClass
{
    [TestFixture]
    class ThUiTest
    {
        
        [Test]
        public void Basicaltest()
        {
            // Launch the AutoCAD
            DesiredCapabilities capabilities = new DesiredCapabilities();
            capabilities.SetCapability("app", @"C:\Program Files\Autodesk\AutoCAD 2012 - Simplified Chinese\acad.exe");
            WindowsDriver<WindowsElement> AutoCAD = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), capabilities);

            // Click or SendKeys
            AutoCAD.Keyboard.SendKeys("STYLE\n");
            while (AutoCAD.FindElementsByName("文字样式").Count == 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
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
            AutoCAD.FindElementByAccessibilityId("ID_Line").Click();
            WindowsElement paintplate = AutoCAD.FindElementByAccessibilityId("59648");
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 100, 100);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 200, 200);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 100, 200);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Keyboard.SendKeys("C\n");

            // 关闭CAD
            AutoCAD.FindElementByName("关闭").Click();
            // 是否保存——否
            AutoCAD.FindElementByAccessibilityId("7").Click();
        }
        
        [Test]
        public void THBPStest()
        {
            // Launch the AutoCAD
            DesiredCapabilities capabilities = new DesiredCapabilities();
            capabilities.SetCapability("app", @"C:\Program Files\Autodesk\AutoCAD 2012 - Simplified Chinese\acad.exe");
            WindowsDriver<WindowsElement> AutoCAD = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), capabilities);

            // 画布窗口
            WindowsElement paintplate = AutoCAD.FindElementByAccessibilityId("59648");

            // 画线
            AutoCAD.FindElementByAccessibilityId("2").SendKeys("PLINE\n");
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 100, 100);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 200, 100);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 100, 200);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Keyboard.SendKeys("C\n");

            // Click or SendKeys
            AutoCAD.Keyboard.SendKeys("THBPS\n");
            while (AutoCAD.FindElementsByName("天华单体框线图层规整").Count == 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }

            // 输入值
            AutoCAD.FindElementByAccessibilityId("textEdit_number").SendKeys("1#");
            AutoCAD.FindElementByAccessibilityId("textEdit_name").SendKeys("一号楼");
            //AutoCAD.FindElementByAccessibilityId("")
            AutoCAD.FindElementByAccessibilityId("textEdit_above_ground_storeys").SendKeys("10");
            AutoCAD.FindElementByAccessibilityId("textEdit_under_ground_storeys").SendKeys("1");

            AutoCAD.FindElementByAccessibilityId("simpleButton_OK").Click();
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 100, 100);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Keyboard.SendKeys("\n");

            // 关闭CAD
            AutoCAD.FindElementByName("关闭").Click();
            // 是否保存——否
            AutoCAD.FindElementByAccessibilityId("7").Click();
        }

        [Test]
        public void THBPPtest()
        {
            // Launch the AutoCAD
            DesiredCapabilities capabilities = new DesiredCapabilities();
            capabilities.SetCapability("app", @"C:\Program Files\Autodesk\AutoCAD 2012 - Simplified Chinese\acad.exe");
            WindowsDriver<WindowsElement> AutoCAD = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), capabilities);

            // 画布窗口
            WindowsElement paintplate = AutoCAD.FindElementByAccessibilityId("59648");

            // 打开图纸


            // Click or SendKeys
            AutoCAD.Keyboard.SendKeys("THBPP\n");
            while (AutoCAD.FindElementsByName("批量打印到PPT").Count == 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }

            // 输入值

            // 确定
            AutoCAD.FindElementByAccessibilityId("btnOK").Click();
            // 关闭CAD
            AutoCAD.FindElementByName("关闭").Click();
            // 是否保存——否
            AutoCAD.FindElementByAccessibilityId("7").Click();
        }
    }
}
