using System;
using System.Runtime.InteropServices;

namespace ThColumnInfo
{
    public class WpfTopMostTool
    {
        public static int SW_SHOW = 5;
        public static int SW_NORMAL = 1;
        public static int SW_MAX = 3;
        public static int SW_HIDE = 0;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);    //窗体置顶
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);    //取消窗体置顶
        public const uint SWP_NOMOVE = 0x0002;    //不调整窗体位置
        public const uint SWP_NOSIZE = 0x0001;    //不调整窗体大小
        public bool isFirst = true;

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// <summary>
        /// 查找子窗口
        /// </summary>
        /// <param name="hwndParent"></param>
        /// <param name="hwndChildAfter"></param>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);

        /// <summary>
        /// 窗体置顶，可以根据需要传入不同的值(需要置顶的窗体的名字Title)
        /// </summary>
        public static void SetTopUpgradeWindow(string frameName)
        {
            IntPtr frm = FindWindow(null, frameName);    // 程序中需要置顶的窗体的名字
            if (frm != IntPtr.Zero)
            {
                SetWindowPos(frm, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

                var child = FindWindowEx(frm, IntPtr.Zero, null, frameName);
            }
        }

        public static void SetTopmost(IntPtr handle)
        {
            SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }
    }
}
