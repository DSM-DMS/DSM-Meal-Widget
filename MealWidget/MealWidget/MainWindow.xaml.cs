using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MealWidget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }
        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        internal void SetBlur(bool enableBlur)
        {
            var windowHelper = new WindowInteropHelper(this);
            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            
            if(enableBlur)
                accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            else
                accent.AccentState = AccentState.ACCENT_DISABLED;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };
            SetWindowCompositionAttribute(windowHelper.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;
        const UInt32 SWP_NOZORDER = 0x0004;
        const int WM_ACTIVATEAPP = 0x001C;
        const int WM_ACTIVATE = 0x0006;
        const int WM_SETFOCUS = 0x0007;
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const int WM_WINDOWPOSCHANGING = 0x0046;

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
           int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd,
           IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr BeginDeferWindowPos(int nNumWindows);

        [DllImport("user32.dll")]
        static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);

        [DllImport("User32.dll")]
        static extern Int32 FindWindow(String lpClassName, String lpWindowName);

        [DllImport("user32.dll")]
        static extern int SetParent(int hWndChild, int hWndNewParent);

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SETFOCUS)
            {
                IntPtr HWnd = new WindowInteropHelper(this).Handle;
                SetWindowPos(HWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

                handled = true;
            }
            return IntPtr.Zero;
        }

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                SetBlur(true);

                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

                IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
                HwndSource src = HwndSource.FromHwnd(windowHandle);
                src.AddHook(new HwndSourceHook(WndProc));

                Left = System.Windows.SystemParameters.WorkArea.Width - Width;
                Top = System.Windows.SystemParameters.WorkArea.Height - Height;
            };

            Closing += (s, e) =>
            {
                IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
                HwndSource src = HwndSource.FromHwnd(windowHandle);
                src.RemoveHook(new HwndSourceHook(this.WndProc));
            };
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(e.ButtonState== System.Windows.Input.MouseButtonState.Pressed)
                SetBlur(false);

            Shrink(border, true);

            DragMove();
        }

        private void Window_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Released)
                SetBlur(true);

            Shrink(border, false);
        }

        private void Shrink(Border target, bool isShrink)
        {
            DoubleAnimation anim;

            if (isShrink)
            {
                anim = new DoubleAnimation(1, 0.9, TimeSpan.FromMilliseconds(150));
                asdf.ScaleX = 1;
                asdf.ScaleY = 1;
            }
            else
            {
                anim = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(150));
                asdf.ScaleX = 0.9;
                asdf.ScaleY = 0.9;
            }

            anim.FillBehavior = FillBehavior.HoldEnd;

            asdf.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            asdf.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }
    }
}
