using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AudioBand
{
    public static class DpiHelper
    {
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);

        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor, [In]DpiType dpiType, [Out]out uint dpiX, [Out]out uint dpiY);

        public enum DpiType
        {
            Effective,
            Angular,
            Raw,
            Default
        }

        private static void GetDpi(Control control, out uint dpiX, out uint dpiY)
        {
            var monitor = MonitorFromPoint(control.PointToScreen(Point.Empty), 2/*MONITOR_DEFAULTTONEAREST*/);
            GetDpiForMonitor(monitor, DpiType.Effective, out dpiX, out dpiY);
        }

        public static float GetScalingFactor(Control control)
        {
            GetDpi(control, out var dpiX, out var dpiY);
            return dpiX / 96f;
        }
    }
}
