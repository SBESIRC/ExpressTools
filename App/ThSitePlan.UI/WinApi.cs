using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ThSitePlan.UI
{
    public static class WinApi
    {

        /// <summary>
        /// 该函数检索指定坐标点的像素的RGB颜色值。
        /// </summary>
        /// <param name="hDC">设备环境句柄。</param>
        /// <param name="nXPos">指定要检查的像素点的逻辑X轴坐标。</param>
        /// <param name="nYPos">指定要检查的像素点的逻辑Y轴坐标。</param>
        /// <returns>返回值是该象像点的RGB值。如果指定的像素点在当前剪辑区之外；那么返回值是CLR_INVALID。</returns>
        [DllImport("gdi32")]
        public static extern uint GetPixel(IntPtr hDC, int nXPos, int nYPos);

        /// <summary>
        /// 该函数检索一指定窗口的客户区域或整个屏幕的显示设备上下文环境的句柄，
        /// 以后可以在GDI函数中使用该句柄来在设备上下文环境中绘图。
        /// </summary>
        /// <param name="hWnd">设备上下文环境被检索的窗口的句柄，如果该值为NULL，GetDC则检索整个屏幕的设备上下文环境。</param>
        /// <returns>如果成功，返回指定窗口客户区的设备上下文环境；如果失败，返回值为Null。</returns>
        [DllImport("user32")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        /// <summary>
        /// 该函数释放设备上下文环境（DC）供其他应用程序使用。函数的效果与设备上下文环境类型有关。
        /// 它只释放公用的和设备上下文环境，对于类或私有的则无效。
        /// </summary>
        /// <param name="hWnd">指向要释放的设备上下文环境所在的窗口的句柄。</param>
        /// <param name="hDC">指向要释放的设备上下文环境的句柄。</param>
        /// <returns>如果释放成功，则返回值为1；如果没有释放成功，则返回值为0。</returns>
        [DllImport("user32")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


        /// <summary>
        /// 2014-12-5 21:43:51
        /// 获取一个RGB颜色值中的红色强度值。
        /// </summary>
        /// <param name="rgb">指定的RGB颜色值。</param>
        /// <returns></returns>
        public static byte GetRValue(uint rgb)
        {
            return (byte)rgb;
        }

        /// <summary>
        /// 2014-12-5 21:51:24
        /// 获取一个RGB颜色值中的绿色强度值。
        /// </summary>
        /// <param name="rgb">指定的RGB颜色值。</param>
        /// <returns></returns>
        public static byte GetGValue(uint rgb)
        {
            return (byte)(((ushort)(rgb)) >> 8);
        }

        /// <summary>
        /// 2014-12-5 21:52:37
        /// 获取一个RGB颜色值中的蓝色强度值。
        /// </summary>
        /// <param name="rgb">指定的RGB颜色值。</param>
        /// <returns></returns>
        public static byte GetBValue(uint rgb)
        {
            return (byte)(rgb >> 16);
        }


        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        //安装钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        //调用下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

    }
}
