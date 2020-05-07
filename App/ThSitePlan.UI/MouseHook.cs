using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThSitePlan.UI
{
    public class MouseHook
    {
        private Point m_Point;
        private Point _Point
        {
            get { return m_Point; }
            set
            {
                if (m_Point != value)
                {
                    m_Point = value;
                    if (MouseMoveEvent != null)
                    {
                        var e = new MouseEventArgs(MouseButtons.Left, 0, m_Point.X, m_Point.Y, 0);
                        MouseMoveEvent(this, e);
                    }
                }
            }
        }
        private int m_Hook;
        public const int m_WH_MOUSE_LL = 14;
        public WinApi.HookProc m_Proc;
        public MouseHook()
        {
            this._Point = new Point();
        }
        public int SetHook()
        {
            m_Proc = new WinApi.HookProc(MouseHookProc);
            m_Hook = WinApi.SetWindowsHookEx(m_WH_MOUSE_LL, m_Proc, IntPtr.Zero, 0);
            return m_Hook;
        }
        public void UnHook()
        {
            WinApi.UnhookWindowsHookEx(m_Hook);
        }
        private int MouseHookProc(int _Code, IntPtr _Param, IntPtr _lParam)
        {
            WinApi.MouseHookStruct MyMouseHookStruct = (WinApi.MouseHookStruct)Marshal.PtrToStructure(_lParam, typeof(WinApi.MouseHookStruct));
            if (_Code < 0)
            {
                return WinApi.CallNextHookEx(m_Hook, _Code, _Param, _lParam);
            }
            else
            {
                if (_Param.ToString("X2") == "201")
                {
                    MouseButtons _Button = MouseButtons.Left;

                    var e = new MouseEventArgs(_Button, 1, m_Point.X, m_Point.Y, 0);

                    MouseClickEvent?.Invoke(this, e);

                    //m_Point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);

                    return 1;

                }

                if (_Param.ToString("X2") == "204")
                {
                    MouseButtons _Button = MouseButtons.Left;

                    var e = new MouseEventArgs(_Button, 1, m_Point.X, m_Point.Y, 0);

                    MouseRightClickEvent?.Invoke(this, e);

                    //m_Point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);

                    return 1;
                }

                if (_Param.ToString("X2") == "200")
                {
                    this._Point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);
                }

                return WinApi.CallNextHookEx(m_Hook, _Code, _Param, _lParam);
            }



        }
        //委托+事件（把钩到的消息封装为事件，由调用者处理）
        public delegate void MouseMoveHandler(object sender, MouseEventArgs e);
        public event MouseMoveHandler MouseMoveEvent;

        public delegate void MouseClickHandler(object sender, MouseEventArgs e);
        public event MouseClickHandler MouseClickEvent;

        public delegate void MouseRightClickHandler(object sender, MouseEventArgs e);
        public event MouseRightClickHandler MouseRightClickEvent;

    }
}
