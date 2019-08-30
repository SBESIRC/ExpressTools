﻿using System;
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
        private WindowsDriver<WindowsElement> AutoCAD;

        [SetUp]
        public void Setup()
        {
            // Launch the AutoCAD
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

            // Click or SendKeys
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
            WindowsElement paintplate = AutoCAD.FindElementByAccessibilityId("59648");
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 100, 100);
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

            // 画线
            AutoCAD.FindElementByAccessibilityId("2").SendKeys("PLINE\n");
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 300, 100);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 400, 100);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Mouse.MouseMove(paintplate.Coordinates, 300, 200);
            AutoCAD.Mouse.Click(null);
            AutoCAD.Keyboard.SendKeys("C\n");

            // Click or SendKeys
            AutoCAD.FindElementByAccessibilityId("2").SendKeys("THBPS\n");
            Thread.Sleep(TimeSpan.FromSeconds(3));

            // 输入值
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

            //
            Assert.AreEqual(AutoCAD.FindElementByAccessibilityId("comboBoxEdit_category").Text, "公建");
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
